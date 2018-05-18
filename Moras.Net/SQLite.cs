using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.Common;
using System.Data.SQLite;
using dxgettext;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Diagnostics;
using System.Reflection;

namespace Moras.Net
{
    //https://github.com/sqlitebrowser/sqlitebrowser/issues/414
    //http://stackoverflow.com/questions/24031267/fix-sqlite-encodings
    public class SQLiteField
    {
        private DataRow _row;
        private DataColumn _col;

        internal SQLiteField(DataRow row, DataColumn col)
        {
            _row = row;
            _col = col;
        }

        public int AsInteger
        {
            get { return Convert.ToInt32(_row[_col]); }
        }

        public long AsLong
        {
            get { return Convert.ToInt64(_row[_col]); }
        }

        public string AsString
        {
            get
            {
                object value = _row[_col];
                if (Convert.IsDBNull(value))
                    throw new InvalidCastException("Can not convert DBNull to string.");
                if (_col.DataType == typeof(byte[]))
                    return Encoding.Default.GetString((byte[])value);
                return Convert.ToString(value);
            }
        }

        public bool IsNull
        {
            get { return _row.IsNull(_col); }
        }
    }

    static class SQLiteUtils
    {
        private class RecordSet : DataAdapter
        {
            private SQLiteDataReader m_reader;
            private DataTable m_recordsBuffer;
            private DataSet m_set;
            private int m_currentRow;

            public RecordSet(SQLiteDataReader reader)
            {
                if (reader == null)
                    throw new ArgumentNullException("reader");
                m_reader = reader;
                m_set = new DataSet();
                m_recordsBuffer = new DataTable();
                m_set.Tables.Add(m_recordsBuffer);
                m_currentRow = -1;
                MissingSchemaAction = System.Data.MissingSchemaAction.AddWithKey;
                m_set.EnforceConstraints = false;
                m_recordsBuffer.BeginLoadData(); //TODO: maybe make this configurable
                MoveToNextRecord();
                MissingSchemaAction = System.Data.MissingSchemaAction.Error;
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    if (m_reader != null)
                        m_reader.Dispose();
                    m_reader = null;
                    if (m_recordsBuffer != null)
                    {
                        m_recordsBuffer.Clear();
                        m_recordsBuffer.Dispose();
                    }
                    m_recordsBuffer = null;
                    m_set = null;
                }
                base.Dispose(disposing);
            }

            private void CheckDisposed()
            {
                if (m_reader == null)
                    throw new ObjectDisposedException(null, "The RecordSet was already disposed.");
            }

            public bool MoveToNextRecord()
            {
                CheckDisposed();
                if ((m_currentRow + 1) < m_recordsBuffer.Rows.Count)
                {
                    m_currentRow++;
                    return true;
                }
                DataTable[] tables = new DataTable[1] { m_recordsBuffer };
                if (m_reader.HasRows)
                {
                    if (Fill(tables, m_reader, 0, 1) == 1)
                    {
                        m_currentRow++;
                        return true;
                    }
                    else
                    {
                        m_recordsBuffer.EndLoadData(); // this should be called only once, if HasRows behaves correctly
                        m_set.EnforceConstraints = true;
                    }
                }
                return false;
            }

            internal SQLiteField FieldByName(string name)
            {
                return new SQLiteField(CurrentRow, m_recordsBuffer.Columns[name]);
            }

            internal SQLiteField FieldByIndex(int index)
            {
                return new SQLiteField(CurrentRow, m_recordsBuffer.Columns[index]);
            }

            internal int FieldCount
            {
                get { return m_recordsBuffer.Columns.Count; }
            }

            internal DataRow CurrentRow
            {
                get
                {
                    CheckDisposed();
                    return m_recordsBuffer.Rows[m_currentRow];
                }
            }

            internal bool Eof
            {
                get
                {
                    CheckDisposed();
                    return (m_currentRow + 1) >= m_recordsBuffer.Rows.Count && !m_reader.HasRows;
                }
            }

            internal int RecordCount
            {
                get
                {
                    CheckDisposed();
                    if (m_reader.HasRows)
                    {
                        DataTable[] tables = new DataTable[1] { m_recordsBuffer };
                        if (Fill(tables, m_reader, 0, 0) == 0 && m_reader.HasRows)
                            throw new DataException("Couldn't fetch all available rows.");
                    }
                    return m_recordsBuffer.Rows.Count;
                }
            }

            internal DataTable SchemaTable
            {
                get { return m_reader.GetSchemaTable(); }
            }
        }

        private static string _(string szMsgId) { return TGnuGettextInstance.gettext(szMsgId); }

        private static Dictionary<SQLiteConnection, bool> _autoCommit = new Dictionary<SQLiteConnection, bool>();
        private static Dictionary<SQLiteConnection, SQLiteTransaction> _transactions = new Dictionary<SQLiteConnection, SQLiteTransaction>();
        private static Dictionary<SQLiteConnection, IsolationLevel> _transactionIsolationLevel = new Dictionary<SQLiteConnection, IsolationLevel>();
        private static Dictionary<SQLiteCommand, RecordSet> _recordSetsCache = new Dictionary<SQLiteCommand, RecordSet>();

        static SQLiteUtils()
        {
            SQLiteConnection.Changed += new SQLiteConnectionEventHandler(SQLiteConnection_Changed);
        }

        static void SQLiteConnection_Changed(object sender, ConnectionEventArgs e)
        {
            SQLiteConnection conn = (SQLiteConnection)sender;
            switch (e.EventType)
            {
                case SQLiteConnectionEventType.Opened:
                    lock (_transactionIsolationLevel)
                    {
                        IsolationLevel level;
                        if (_transactionIsolationLevel.TryGetValue(conn, out level))
                        {
                            lock (_transactions) _transactions.Add(conn, conn.BeginTransaction(level));
                        }
                    }
                    break;
                case SQLiteConnectionEventType.Closing:
                    lock (_transactions)
                    {
                        SQLiteTransaction transaction;
                        if (_transactions.TryGetValue(conn, out transaction))
                        {
                            try
                            {
                                _transactions.Remove(conn);
                            }
                            finally
                            {
                                // Destructors are called by GC in no sequential order, so the sqlite db handle can be released just before the connection object is destroyed.
                                // The connection object does not provide an indication if the connection was closed as a result of destruction or if it was manually closed.
                                // But we need it here to decide if we can dispose the transaction.
                                StackTrace callstack = new StackTrace();
                                bool indtor = false;
                                RuntimeMethodHandle dtor = conn.GetType().GetMethod("Finalize", BindingFlags.NonPublic | BindingFlags.Instance).MethodHandle;
                                foreach (var frame in callstack.GetFrames())
                                {
                                    // http://stackoverflow.com/questions/27645408/runtimemethodinfo-equality-bug
                                    // just comparing the handles never fails
                                    if (frame.GetMethod().MethodHandle == dtor)
                                    {
                                        indtor = true;
                                        break;
                                    }
                                }
                                if (!indtor)
                                    transaction.Dispose();
                            }
                        }
                    }
                    break;
                case SQLiteConnectionEventType.ChangeDatabase:
                case SQLiteConnectionEventType.Closed:
                case SQLiteConnectionEventType.ClosedToPool:
                case SQLiteConnectionEventType.ClosingDataReader:
                case SQLiteConnectionEventType.ConnectionString:
                case SQLiteConnectionEventType.DisposingCommand:
                case SQLiteConnectionEventType.DisposingDataReader:
                case SQLiteConnectionEventType.EnlistTransaction:
                case SQLiteConnectionEventType.Invalid:
                case SQLiteConnectionEventType.NewCommand:
                case SQLiteConnectionEventType.NewCriticalHandle:
                case SQLiteConnectionEventType.NewDataReader:
                case SQLiteConnectionEventType.NewTransaction:
                case SQLiteConnectionEventType.OpenedFromPool:
                case SQLiteConnectionEventType.Opening:
                case SQLiteConnectionEventType.Unknown:
                default:
                    break;
            }
        }

        internal static bool GetAutoCommit(this SQLiteConnection conn)
        {
            if (conn.State == ConnectionState.Open)
                return conn.AutoCommit;

            lock (_autoCommit)
            {
                bool result;
                if (_autoCommit.TryGetValue(conn, out result))
                    return result;
                return true;
            }
        }

        internal static void SetAutoCommit(this SQLiteConnection conn, bool value)
        {
            lock (_autoCommit)
            {
                if (_autoCommit.ContainsKey(conn))
                    _autoCommit[conn] = value;
                else
                    _autoCommit.Add(conn, value);
            }
        }

        internal static IsolationLevel? GetTransactIsolationLevel(this SQLiteConnection conn)
        {
            if (conn.State == ConnectionState.Open)
            {
                SQLiteTransaction transaction;
                lock (_transactions)
                {
                    if (_transactions.TryGetValue(conn, out transaction))
                        return transaction.IsolationLevel;
                }
            }

            lock (_transactionIsolationLevel)
            {
                IsolationLevel level;
                if (_transactionIsolationLevel.TryGetValue(conn, out level))
                    return level;

            }
            return null;
        }

        internal static void SetTransactIsolationLevel(this SQLiteConnection conn, IsolationLevel? value)
        {
            if (conn.State == ConnectionState.Open)
            {
                lock (_transactions)
                {
                    SQLiteTransaction transaction;
                    if (_transactions.TryGetValue(conn, out transaction))
                    {
                        transaction.Rollback();
                        transaction.Dispose();
                        _transactions.Remove(conn);
                    }
                }
            }

            lock (_transactionIsolationLevel)
            {
                if (!value.HasValue)
                {
                    if (_transactionIsolationLevel.ContainsKey(conn))
                        _transactionIsolationLevel.Remove(conn);
                }
                else
                {
                    if (!_transactionIsolationLevel.ContainsKey(conn))
                        _transactionIsolationLevel.Add(conn, value.Value);
                    else
                        _transactionIsolationLevel[conn] = value.Value;
                }
            }

            if (conn.State == ConnectionState.Open && value.HasValue)
            {
                lock (_transactions) _transactions.Add(conn, conn.BeginTransaction(value.Value));
            }
        }

        internal static string GetDataSource(this SQLiteConnection conn)
        {
            SQLiteConnectionStringBuilder builder = new SQLiteConnectionStringBuilder(conn.ConnectionString);
            return builder.DataSource;
        }

        internal static void SetDataSource(this SQLiteConnection conn, string dbPath)
        {
            SQLiteConnectionStringBuilder builder = new SQLiteConnectionStringBuilder(conn.ConnectionString);
            builder.DataSource = dbPath;
            conn.ConnectionString = builder.ConnectionString;
        }

        internal static void DoCommit(this SQLiteConnection conn)
        {
            if (conn.State == ConnectionState.Open)
            {
                lock (_transactions)
                {
                    SQLiteTransaction transaction;
                    if (_transactions.TryGetValue(conn, out transaction))
                    {
                        _transactions.Remove(conn);
                        var level = transaction.IsolationLevel;
                        transaction.Commit();
                        transaction.Dispose();
                        _transactions.Add(conn, conn.BeginTransaction(level));
                    }
                }
            }
        }

        internal static void DoRollBack(this SQLiteConnection conn)
        {
            if (conn.State == ConnectionState.Open)
            {
                lock (_transactions)
                {
                    SQLiteTransaction transaction;
                    if (_transactions.TryGetValue(conn, out transaction))
                    {
                        var level = transaction.IsolationLevel;
                        transaction.Rollback();
                        transaction.Dispose();
                        _transactions.Remove(conn);
                        _transactions.Add(conn, conn.BeginTransaction(level));
                    }
                }
            }
        }

        internal static void SetActive(this SQLiteCommand cmd, bool value)
        {
            RecordSet set;
            if (!value)
            {
                if (_recordSetsCache.TryGetValue(cmd, out set))
                {
                    _recordSetsCache.Remove(cmd);
                    set.Dispose();
                }
                cmd.Reset();
            }
            else
            {
                if (!_recordSetsCache.TryGetValue(cmd, out set))
                {
                    set = new RecordSet(cmd.ExecuteReader());
                    _recordSetsCache.Add(cmd, set);
                }
            }
        }

        internal static void Close(this SQLiteCommand cmd)
        {
            SetActive(cmd, false);
        }

        internal static void Open(this SQLiteCommand cmd)
        {
            SetActive(cmd, true);
        }

        private static RecordSet GetActiveRecordSet(SQLiteCommand cmd)
        {
            RecordSet set;
            if (_recordSetsCache.TryGetValue(cmd, out set))
            {
                return set;
            }
            throw new InvalidOperationException("The command was not set active.");
        }

        internal static void Next(this SQLiteCommand cmd)
        {
            //TODO: accept changes on active record
            GetActiveRecordSet(cmd).MoveToNextRecord();
        }

        internal static SQLiteField FieldByName(this SQLiteCommand cmd, string name)
        {
            return GetActiveRecordSet(cmd).FieldByName(name);
        }

        internal static SQLiteField FieldByIndex(this SQLiteCommand cmd, int index)
        {
            return GetActiveRecordSet(cmd).FieldByIndex(index);
        }

        internal static int GetFieldCount(this SQLiteCommand cmd)
        {
            return GetActiveRecordSet(cmd).FieldCount;
        }

        internal static bool GetEof(this SQLiteCommand cmd)
        {
            return GetActiveRecordSet(cmd).Eof;
        }

        internal static int GetRecordCount(this SQLiteCommand cmd)
        {
            return GetActiveRecordSet(cmd).RecordCount;
        }

        internal static DataTable GetSchemaTable(this SQLiteCommand cmd)
        {
            return GetActiveRecordSet(cmd).SchemaTable;
        }

        internal static string SQLiteDBEncoding()
        {
            Unit.frmMain.ZQuery.CommandText = "PRAGMA encoding;";
            Unit.frmMain.ZQuery.SetActive(true);
            try
            {
                return Unit.frmMain.ZQuery.FieldByIndex(0).AsString;
            }
            finally
            {
                Unit.frmMain.ZQuery.SetActive(false);
            }
        }

        internal static bool SQLiteDBInit()
        {
            bool result = true;
            try
            {
                int version = SQLiteDBVersion(Unit.frmMain.ZQuery);
                if (version == 0)
                {
                    if (!File.Exists(Unit.frmMain.ZConnection.FileName))
                        throw new Exception("Die Datenbank konnte nicht erstellt werden.");
                    Unit.frmMain.NewDatabase = true;
                    SQLiteDBUpdate0to4();
                }
                else if (version == 4)
                {
                    Unit.frmMain.NewDatabase = false;
                }
                else if (version < 4)
                {
                    Unit.frmMain.NewDatabase = false;
                    SQLiteDBConvertCharset();
                }
                else if (version > 4)
                {
                    throw new Exception("Die Version der existierenden Datenbank kann nicht für ein Update verwendet werden.");
                }
                Unit.frmMain.ZConnection.DoCommit();

                if (version == 1)
                {
                    SQLiteDBUpdate1to4();
                }
                Unit.frmMain.ZConnection.DoCommit();

                if (version == 2)
                {
                    SQLiteDBUpdate2to4();
                }
                Unit.frmMain.ZConnection.DoCommit();
            }
            catch (Exception e)
            {
                string title = _("Datenbank-Update");
                string msg = _("Fehler beim Datenbank-Update:");
                msg = msg + "\n" + e.Message;
                result = false;
                MessageBox.Show(msg, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return result;
        }

        internal static void SQLiteDBClose(SQLiteCommand cmd)
        {
            SQLiteConnection con = cmd.Connection;
            cmd.Connection = null; // reset would leave prepared statements intact, so we need to dispose or clear the connection
            con.Close();
        }

        internal static int SQLiteDBVersion(SQLiteCommand cmd)
        {
            int result = 0;

            cmd.CommandText = "select count(*) as anzahl from sqlite_master where type='table' and name='morasversion'";
            try
            {
                cmd.SetActive(true);
                result = cmd.FieldByName("anzahl").AsInteger;

                if (result > 0)
                {
                    cmd.SetActive(false);
                    cmd.CommandText = "select dbversion from morasversion";
                    cmd.SetActive(true);
                    result = cmd.FieldByName("dbversion").AsInteger;
                    //        ShowMessage("Version " + (result).ToString());
                }
            }
            finally
            {
                cmd.SetActive(false);
            }

            return result;
        }

        internal static void SQLiteDBVacuum()
        {
            SQLiteDBCorrection();

            Unit.frmMain.ZConnection.Close();
            Unit.frmMain.ZConnection.SetTransactIsolationLevel(null);
            Unit.frmMain.ZConnection.Open();

            Unit.frmMain.ZQuery.CommandText = "vacuum;";
            Unit.frmMain.ZQuery.ExecuteNonQuery();

            Unit.frmMain.ZConnection.Close();
            Unit.frmMain.ZConnection.SetTransactIsolationLevel(IsolationLevel.ReadUncommitted);
            Unit.frmMain.ZConnection.Open();
        }

        internal static void SQLiteDBCorrection()
        {
            int i, j;
            string sql;

            // Falsche Positions ID korregieren
            for (i = 0; i < CPlayer.PLAYER_ITEMS; i++)
            {
                string id = Unit.xml_config.arItemSlots[i].strPosId;
                // Ersten ID Eintrag zu dieser Position suchen
                j = Unit.xml_config.GetSlotPosition(id);
                // Wenn ungleich => problematische ID in DB
                if (i != j)
                {
                    sql = "update items set";
                    sql = sql + " position = " + (j).ToString();
                    sql = sql + " where position = " + (i).ToString();
                    if (Unit.xml_config.bDebugSQL)
                        Utils.DebugPrint(sql);
                    Unit.frmMain.ZQuery.CommandText = sql;
                    Unit.frmMain.ZQuery.ExecuteNonQuery();
                }
            }

            // zu kleines Level korregieren
            sql = "update items set";
            sql = sql + " level = -level";
            sql = sql + " where level < 0";
            if (Unit.xml_config.bDebugSQL)
                Utils.DebugPrint(sql);
            Unit.frmMain.ZQuery.CommandText = sql;
            Unit.frmMain.ZQuery.ExecuteNonQuery();

            // zu grosses Level korregieren
            sql = "update items set";
            sql = sql + " level = min(51, level/2)";
            sql = sql + " where level > 51";
            if (Unit.xml_config.bDebugSQL)
                Utils.DebugPrint(sql);
            Unit.frmMain.ZQuery.CommandText = sql;
            Unit.frmMain.ZQuery.ExecuteNonQuery();

            Unit.frmMain.ZConnection.DoCommit();
        }

        internal static void SQLiteDBUpdate0to4()
        {
            //    ShowMessage("Update 0 auf 1");
            Unit.frmMain.ZQuery.CommandText = @"
CREATE TABLE [items] (
  [id] INTEGER PRIMARY KEY ON CONFLICT ABORT AUTOINCREMENT,
  [name] VARCHAR2(150) NOT NULL ON CONFLICT ABORT,
  [nameoriginal] VARCHAR2(150),
  [origin] CLOB,
  [description] CLOB,
  [onlineurl] VARCHAR2(250),
  [extension] VARCHAR2(10),
  [provider] VARCHAR2(100),
  [classrestrictions] VARCHAR2(250),
  [effects] VARCHAR2(250),
  [realm] INT NOT NULL ON CONFLICT ABORT,
  [position] INT NOT NULL ON CONFLICT ABORT,
  [type] INT NOT NULL ON CONFLICT ABORT,
  [level] INT,
  [quality] INT,
  [bonus] INT,
  [class] INT,
  [subclass] INT,
  [material] INT,
  [af] INT,
  [dps] INT,
  [speed] INT,
  [maxlevel] INT,
  [damagetype] INT,
  [lastupdate] INT);

CREATE INDEX [idxName] ON [items] ([name]);
CREATE INDEX [idxExtension] ON [items] ([extension]);
CREATE INDEX [idxProvider] ON [items] ([provider]);
CREATE INDEX [idxRealm] ON [items] ([realm]);
CREATE INDEX [idxPosition] ON [items] ([position]);";

            Unit.frmMain.ZQuery.ExecuteNonQuery();

            Unit.frmMain.ZQuery.CommandText = @"
CREATE TABLE [morasversion] (
  [dbversion] INT NOT NULL ON CONFLICT ABORT);";
            Unit.frmMain.ZQuery.ExecuteNonQuery();

            Unit.frmMain.ZQuery.CommandText = "insert into morasversion (dbversion) values (4)";
            Unit.frmMain.ZQuery.ExecuteNonQuery();
        }

        internal static void SQLiteDBConvertCharset()
        {
            // convert codepage charset to utf-8
            var mappings = Unit.frmMain.ZConnection.GetTypeMappings();
            var flags = Unit.frmMain.ZConnection.Flags;
            Unit.frmMain.ZConnection.ClearTypeMappings();
            object[][] items;
            DbType[] coltypes;
            string[] colnames;
            try
            {
                Unit.frmMain.ZConnection.AddTypeMapping("VARCHAR2", DbType.Binary, true);
                Unit.frmMain.ZConnection.Flags |= SQLiteConnectionFlags.UseConnectionTypes;
                Unit.frmMain.ZQuery.CommandText = "select * from items";
                Unit.frmMain.ZQuery.Open();
                items = new object[Unit.frmMain.ZQuery.GetRecordCount()][];
                int cols = Unit.frmMain.ZQuery.GetFieldCount();
                coltypes = new DbType[cols];
                colnames = new string[cols];
                DataTable schema = Unit.frmMain.ZQuery.GetSchemaTable();
                for (int j = 0; j < cols; j++)
                {
                    DataRow row = schema.Rows[j];
                    colnames[j] = (string)row[SchemaTableColumn.ColumnName];
                    coltypes[j] = (DbType)row[SchemaTableColumn.ProviderType];
                    if (coltypes[j] == DbType.Binary)
                        coltypes[j] = DbType.AnsiString;
                }
                for (int i = 0; i < items.Length; i++)
                {
                    items[i] = new object[cols];
                    for (int j = 0; j < cols; j++)
                    {
                        SQLiteField field = Unit.frmMain.ZQuery.FieldByIndex(j);
                        if (field.IsNull)
                            items[i][j] = Convert.DBNull;
                        else
                        {
                            switch (coltypes[j])
                            {
                                case DbType.AnsiString: items[i][j] = field.AsString; break;
                                case DbType.Int64: items[i][j] = field.AsLong; break;
                                case DbType.Int32: items[i][j] = field.AsInteger; break;
                                default: throw new InvalidOperationException("Die Datenbank enthält Spalten deren Typ nicht erwartet wurde.");
                            }
                        }
                    }
                    Unit.frmMain.ZQuery.Next();
                }
                if (!Unit.frmMain.ZQuery.GetEof())
                    throw new InvalidOperationException("Es konnten nicht alle Zeilen gelesen werden.");
            }
            finally
            {
                Unit.frmMain.ZQuery.Close();
                Unit.frmMain.ZConnection.ClearTypeMappings();
                Unit.frmMain.ZConnection.Flags = flags;
                foreach (var pair in mappings)
                {
                    object[] mapping = (object[])pair.Value;
                    Unit.frmMain.ZConnection.AddTypeMapping((string)mapping[0], (DbType)mapping[1], (bool)mapping[2]);
                }
            }
            string dbpath = Unit.frmMain.ZConnection.FileName;
            SQLiteDBClose(Unit.frmMain.ZQuery);

            string dbpathcopy = Path.ChangeExtension(dbpath, "db3.bak");
            bool createcopy = true;
            bool cancel = false;
            if (File.Exists(dbpathcopy))
            {
                DialogResult result = MessageBox.Show("Die Backup-Datei der Datenbank existiert bereits. Wollen Sie sie überschreiben?", "Datei existiert bereits", MessageBoxButtons.YesNoCancel);
                if (result == DialogResult.Yes)
                    File.Delete(dbpathcopy);
                else if (result == DialogResult.No)
                    createcopy = false;
                else
                {
                    createcopy = false;
                    cancel = true;
                }
            }
            if (createcopy)
                File.Move(dbpath, dbpathcopy);
            Unit.frmMain.ZQuery.Connection = Unit.frmMain.ZConnection;
            Unit.frmMain.ZConnection.Open();
            if (cancel)
                return;

            SQLiteDBUpdate0to4();
            Unit.frmMain.ZConnection.DoCommit();

            Unit.frmMain.ZQuery.CommandText = "select * from items";
            using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(Unit.frmMain.ZQuery))
            {
                adapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;
                using (SQLiteCommandBuilder builder = new SQLiteCommandBuilder(adapter))
                {
                    adapter.InsertCommand = builder.GetInsertCommand(true);
                    //TODO: let id autoincrement if it is not used anywhere else
                    adapter.InsertCommand.CommandText = adapter.InsertCommand.CommandText.Replace("] (", "] ([id], ");
                    adapter.InsertCommand.CommandText = adapter.InsertCommand.CommandText.Replace("VALUES (", "VALUES (@id, ");
                    SQLiteParameter pkParam = new SQLiteParameter("id", DbType.Int64, "id");
                    adapter.InsertCommand.Parameters.Insert(0, pkParam);
                    DataTable itemsTable = new DataTable();
                    adapter.Fill(itemsTable);
                    int[] ordinals = new int[itemsTable.Columns.Count];
                    for (int i = 0; i < ordinals.Length; i++)
                    {
                        ordinals[i] = -1;
                    }
                    for (int i = 0; i < colnames.Length; i++)
                    {
                        DataColumn col = itemsTable.Columns[colnames[i]];
                        if (col != null)
                            ordinals[col.Ordinal] = i;
                    }
                    object[] itemarray = new object[itemsTable.Columns.Count];
                    for (int i = 0; i < items.Length; i++)
                    {
                        for (int j = 0; j < itemsTable.Columns.Count; j++)
                        {
                            if (ordinals[j] != -1)
                                itemarray[j] = items[i][ordinals[j]];
                            else
                                itemarray[j] = Convert.DBNull;
                        }
                        DataRow row = itemsTable.NewRow();
                        row.ItemArray = itemarray;
                        itemsTable.Rows.Add(row);
                    }
                    adapter.Update(itemsTable);
                }
            }
            Unit.frmMain.ZConnection.DoCommit();
        }

        internal static void SQLiteDBUpdate1to4()
        {
            //    ShowMessage("Update 1 auf 2");
            Unit.frmMain.ZQuery.CommandText = "alter table items rename to items_tmp";
            Unit.frmMain.ZQuery.ExecuteNonQuery();
            Unit.frmMain.ZQuery.CommandText = "drop index idxName";
            Unit.frmMain.ZQuery.ExecuteNonQuery();
            Unit.frmMain.ZQuery.CommandText = "drop index idxExtension";
            Unit.frmMain.ZQuery.ExecuteNonQuery();
            Unit.frmMain.ZQuery.CommandText = "drop index idxProvider";
            Unit.frmMain.ZQuery.ExecuteNonQuery();
            Unit.frmMain.ZQuery.CommandText = "drop index idxRealm";
            Unit.frmMain.ZQuery.ExecuteNonQuery();
            Unit.frmMain.ZQuery.CommandText = "drop index idxPosition";
            Unit.frmMain.ZQuery.ExecuteNonQuery();
            Unit.frmMain.ZQuery.CommandText = "drop table morasversion";
            Unit.frmMain.ZQuery.ExecuteNonQuery();

            Unit.frmMain.ZConnection.DoCommit();
            SQLiteDBUpdate0to4();
            Unit.frmMain.ZConnection.DoCommit();

            Unit.frmMain.ZQuery.CommandText = @"insert into items 
 (id,name,nameoriginal,origin,description,onlineurl,extension,
 provider,classrestrictions,effects,realm,position,type,level,
 quality,bonus,class,subclass,material,af,dps,speed,maxlevel,
 damagetype,lastupdate)
 select * from items_tmp";
            Unit.frmMain.ZQuery.ExecuteNonQuery();

            Unit.frmMain.ZQuery.CommandText = "drop table items_tmp";
            Unit.frmMain.ZQuery.ExecuteNonQuery();
            Unit.frmMain.ZConnection.DoCommit();

            SQLiteDBVacuum();
        }

        internal static void SQLiteDBUpdate2to4()
        {
            //    ShowMessage("Update 1 auf 2");
            Unit.frmMain.ZQuery.CommandText = "alter table items rename to items_tmp";
            Unit.frmMain.ZQuery.ExecuteNonQuery();
            Unit.frmMain.ZQuery.CommandText = "drop index idxName";
            Unit.frmMain.ZQuery.ExecuteNonQuery();
            Unit.frmMain.ZQuery.CommandText = "drop index idxExtension";
            Unit.frmMain.ZQuery.ExecuteNonQuery();
            Unit.frmMain.ZQuery.CommandText = "drop index idxProvider";
            Unit.frmMain.ZQuery.ExecuteNonQuery();
            Unit.frmMain.ZQuery.CommandText = "drop index idxRealm";
            Unit.frmMain.ZQuery.ExecuteNonQuery();
            Unit.frmMain.ZQuery.CommandText = "drop index idxPosition";
            Unit.frmMain.ZQuery.ExecuteNonQuery();
            Unit.frmMain.ZQuery.CommandText = "drop table morasversion";
            Unit.frmMain.ZQuery.ExecuteNonQuery();

            Unit.frmMain.ZConnection.DoCommit();
            SQLiteDBUpdate0to4();
            Unit.frmMain.ZConnection.DoCommit();

            Unit.frmMain.ZQuery.CommandText = @"insert into items 
 (id,name,nameoriginal,origin,description,onlineurl,extension,
 provider,classrestrictions,effects,realm,position,type,level,
 quality,bonus,class,subclass,material,af,dps,speed,maxlevel,
 damagetype,lastupdate)
 select * from items_tmp";
            Unit.frmMain.ZQuery.ExecuteNonQuery();

            Unit.frmMain.ZQuery.CommandText = "drop table items_tmp";
            Unit.frmMain.ZQuery.ExecuteNonQuery();
            Unit.frmMain.ZConnection.DoCommit();

            SQLiteDBVacuum();
        }
    }

    [ProvideProperty("AutoCommit", typeof(SQLiteConnection))]
    [ProvideProperty("TransactIsolationLevel", typeof(SQLiteConnection))]
    public class SQLiteZeosLibPropertyExtender : Component, IExtenderProvider
    {
        public SQLiteZeosLibPropertyExtender() { }
        public SQLiteZeosLibPropertyExtender(IContainer parent) : this() { parent.Add(this); }

        #region IExtenderProvider Members

        public bool CanExtend(object extendee)
        {
            return extendee is SQLiteConnection;// || extendee is SQLiteCommand;
        }

        #endregion

        [Description("Returns non-zero if the given database connection is in autocommit mode. Autocommit mode is on by default. Autocommit mode is disabled by a BEGIN statement. Autocommit mode is re-enabled by a COMMIT or ROLLBACK.")]
        [Category("Behavior")]
        [DefaultValue(true)]
        public bool GetAutoCommit(SQLiteConnection conn)
        {
            return conn.GetAutoCommit();
        }

        public void SetAutoCommit(SQLiteConnection conn, bool value)
        {
            conn.SetAutoCommit(value);
        }

        [Description("Specifies the transaction locking behavior for the connection.")]
        [Category("Behavior")]
        [DefaultValue(null)]
        public IsolationLevel? GetTransactIsolationLevel(SQLiteConnection conn)
        {
            return conn.GetTransactIsolationLevel();
        }

        public void SetTransactIsolationLevel(SQLiteConnection conn, IsolationLevel? value)
        {
            conn.SetTransactIsolationLevel(value);
        }
    }
}