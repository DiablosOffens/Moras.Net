using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Xml;
using System.IO;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing.Design;

namespace DelphiClasses
{
    public enum TDOMVendor
    {
        MSXML
    }

    public interface IDOMImplementation
    {
        XmlDocument CreateDocument(string namespaceURI, string qualifiedName, XmlDocumentType doctype);
        XmlDocumentType CreateDocumentType(string qualifiedName, string publicId, string systemId);
        bool HasFeature(string feature, string version);
    }

    public interface IDOMParseOptions
    {
        bool Async { get; set; }
        bool PreserveWhiteSpace { get; set; }
        bool ResolveExternals { get; set; }
        bool Validate { get; set; }
    }

    public class TAsyncEventArgs : EventArgs
    {
        internal TAsyncEventArgs(int asyncLoadState)
        {
            AsyncLoadState = asyncLoadState;
        }
        /// <summary>
        /// AsyncLoadState gibt den aktuellen Status der Analyse an. Der Wert des Parameters ist herstellerspezifisch und entspricht der Eigenschaft AsyncLoadState des Dokuments.
        /// </summary>
        public int AsyncLoadState { get; private set; }
    }

    /// <summary>
    /// TAsyncEventHandler ist der Typ der Behandlungsroutinen für Ereignisse, die beim asynchronen Analysieren eines XML-Dokuments ausgelöst werden.
    /// </summary>
    /// <param name="sender">Sender gibt das TXMLDocument-Objekt an, in dem das Ereignis aufgetreten ist.</param>
    /// <param name="e">Enthält die Ereignisdaten.</param>
    public delegate void TAsyncEventHandler(object sender, TAsyncEventArgs e);

    /// <summary>
    /// IDOMPersist ist das Interface, das für das Laden und Speichern von XML-Dokumenten verwendet wird. 
    /// </summary>
    public interface IDOMPersist
    {
        /// <summary>
        /// Mit der Methode AsyncLoadState ermitteln Sie den Status des DOM-Parsers, wenn dieser ein XML-Dokument asynchron analysiert.
        /// </summary>
        /// <returns>Gibt den Status des DOM-Parsers zurück.</returns>
        int AsyncLoadState();
        /// <summary>
        /// Mit der Methode Load laden Sie das xml aus einer im Parameter source angegebenen Datei.
        /// </summary>
        /// <param name="source">Die Datei aus der das xml geladen werden soll.</param>
        /// <returns>Die Methode load gibt True zurück, wenn das Laden erfolgreich durchgeführt werden konnte; ansonsten False.</returns>
        bool Load(object source);
        /// <summary>
        /// Mit der Methode LoadFromStream laden Sie das xml aus einem im Parameter stream angegebenen Stream.
        /// </summary>
        /// <param name="stream">Der Stream aus dem das xml geladen werden soll.</param>
        /// <returns>Die Methode loadFromStream gibt True zurück, wenn das Laden erfolgreich durchgeführt werden konnte; ansonsten False.</returns>
        bool LoadFromStream(Stream stream);
        /// <summary>
        /// Mit der Methode LoadXml laden Sie das xml aus einem im Parameter xml angegebenen String.
        /// </summary>
        /// <param name="xml">Der String aus dem das xml geladen werden soll.</param>
        /// <returns>Die Methode loadxml gibt True zurück, wenn das Laden erfolgreich durchgeführt werden konnte; ansonsten False.</returns>
        bool LoadXml(string xml);
        /// <summary>
        /// Mit der Methode Save speichern Sie das xml in eine im Parameter destination angegebene Datei.
        /// </summary>
        void Save(object destination);
        /// <summary>
        /// Mit der Methode SaveToStream speichern Sie das xml in einen im Parameter stream angegebenen Stream.
        /// </summary>
        /// <param name="stream">Der Stream in dem das xml gespeichert werden soll.</param>
        void SaveToStream(Stream stream);
        /// <summary>
        /// Registriert ein Ereignis, das beim Laden des Dokuments auftritt.
        /// </summary>
        event TAsyncEventHandler OnAsyncLoad;
        /// <summary>
        /// Die Eigenschaft Xml repräsentiert den Inhalt des XML-Dokuments.
        /// </summary>
        string Xml { get; }
    }

    public interface IDOMParseError
    {
        int ErrorCode { get; }
        int FilePos { get; }
        int Line { get; }
        int LinePos { get; }
        string Reason { get; }
        string SrcText { get; }
        string Url { get; }
    }

    public class TXMLDocument : Component, ISupportInitializeNotification
    {
        protected enum TXMLDocumentSource { xdsNone, xdsXMLProperty, xdsXMLData, xdsFile, xdsStream }

        public enum TXMLDocOption : byte { doNodeAutoCreate, doNodeAutoIndent, doAttrNull, doAutoPrefix, doNamespaceDecl, doAutoSave }

        [BitSet(typeof(TXMLDocOption), TXMLDocOption.doNodeAutoCreate, TXMLDocOption.doAutoSave)]
        public enum TXMLDocOptions
        {
            doNodeAutoCreate = 1 << TXMLDocOption.doNodeAutoCreate,
            doNodeAutoIndent = 1 << TXMLDocOption.doNodeAutoIndent,
            doAttrNull = 1 << TXMLDocOption.doAttrNull,
            doAutoPrefix = 1 << TXMLDocOption.doAutoPrefix,
            doNamespaceDecl = 1 << TXMLDocOption.doNamespaceDecl,
            doAutoSave = 1 << TXMLDocOption.doAutoSave
        }

        public enum TParseOption : byte { poResolveExternals, poValidateOnParse, poPreserveWhiteSpace, poAsyncLoad };

        [BitSet(typeof(TParseOption), TParseOption.poResolveExternals, TParseOption.poAsyncLoad)]
        public enum TParseOptions
        {
            poResolveExternals = 1 << TParseOption.poResolveExternals,
            poValidateOnParse = 1 << TParseOption.poValidateOnParse,
            poPreserveWhiteSpace = 1 << TParseOption.poPreserveWhiteSpace,
            poAsyncLoad = 1 << TParseOption.poAsyncLoad
        }

        private enum TXMLEncodingType : byte
        {
            xetUnknown, xetUCS_4BE, xetUCS_4LE, xetUCS_4Order2134, xetUCS_4Order3412,
            xetUTF_16BE, xetUTF_16LE, xetUTF_8, xetUCS_4Like, xetUTF_16BELike, xetUTF_16LELike, xetUTF_8Like, xetEBCDICLike
        }

        private class MSXMLImplementation : XmlImplementation, IDOMImplementation
        {
            public override XmlDocument CreateDocument()
            {
                return new MSXMLDocument(this);
            }

            #region IDOMImplementation Members

            public XmlDocument CreateDocument(string namespaceURI, string qualifiedName, XmlDocumentType doctype)
            {
                XmlDocument doc = CreateDocument();
                if (doctype != null)
                    doc.AppendChild(doc.ImportNode(doctype, true)); // import doctype
                if (!string.IsNullOrEmpty(qualifiedName))
                    doc.AppendChild(doc.CreateElement(qualifiedName, namespaceURI)); // create root element
                return doc;
            }

            public XmlDocumentType CreateDocumentType(string qualifiedName, string publicId, string systemId)
            {
                return CreateDocument().CreateDocumentType(qualifiedName, publicId, systemId, string.Empty);
            }

            #endregion
        }

        private class MSXMLDocument : XmlDocument, IServiceProvider, IDOMPersist, IDOMParseOptions, IDOMParseError
        {
            private struct XmlError
            {
                public int ErrorCode;
                public int FilePos;
                public int Line;
                public int LinePos;
                public string Reason;
                public string SrcText;
                public string Url;

                public void Reset()
                {
                    ErrorCode = FilePos = Line = LinePos = 0;
                    Reason = SrcText = Url = null;
                }
            }

            private XmlReaderSettings readerSettings = new XmlReaderSettings();
            private XmlError errorData;

            protected internal MSXMLDocument(MSXMLImplementation imp)
                : base(imp)
            {
                readerSettings.NameTable = NameTable;
                readerSettings.IgnoreWhitespace = true;
                readerSettings.CloseInput = false;
                readerSettings.XmlResolver = null;
                XmlResolver = null;
            }

            private void SetErrorData(Exception ex)
            {
                errorData.Reset();
                errorData.Reason = ex.Message;
                errorData.ErrorCode = Marshal.GetHRForException(ex);
                if (ex is XmlException)
                {
                    XmlException xmlex = (XmlException)ex;
                    errorData.Line = xmlex.LineNumber;
                    errorData.LinePos = xmlex.LinePosition;
                    errorData.Url = xmlex.SourceUri;
                    //errorData.SrcText=...?
                    //errorData.FilePos=...?
                }
            }

            #region IServiceProvider Members

            object IServiceProvider.GetService(Type serviceType)
            {
                if (serviceType == typeof(IDOMPersist))
                    return this;
                if (serviceType == typeof(IDOMParseOptions))
                    return this;
                if (serviceType == typeof(IDOMParseError))
                    return this;
                if (serviceType == typeof(XmlDocument))
                    return this;
                return null;
            }

            #endregion

            #region IDOMParseOptions Members

            // Async isn't implemented in the .Net Client Framework
            bool IDOMParseOptions.Async
            {
                get { return false; }
                set
                {
                    if (value)
                        throw new NotImplementedException("Async can't be set to true, because it's not implemented in .Net Framework 4 Client Profile.");
                }
            }

            bool IDOMParseOptions.PreserveWhiteSpace
            {
                get { return PreserveWhitespace; }
                set
                {
                    if (PreserveWhitespace == value)
                        return;

                    PreserveWhitespace = value;
                    readerSettings.IgnoreWhitespace = !value;
                }
            }

            private bool resolveExternals;
            bool IDOMParseOptions.ResolveExternals
            {
                get { return resolveExternals; }
                set
                {
                    if (resolveExternals == value)
                        return;
                    resolveExternals = value;
                    var resolver = value ? new XmlUrlResolver() : null;
                    readerSettings.XmlResolver = resolver;
                    XmlResolver = resolver;
                }
            }

            bool IDOMParseOptions.Validate
            {
                get { return readerSettings.ValidationType != ValidationType.None; }
                set
                {
                    if (value)
                        readerSettings.ValidationType = ValidationType.Auto;
                    else
                        readerSettings.ValidationType = ValidationType.None;
                    //TODO: set also this?
                    //readerSettings.DtdProcessing = DtdProcessing.Parse;
                    //readerSettings.DtdProcessing = DtdProcessing.Ignore;
                }
            }

            #endregion

            #region IDOMPersist Members

            // Async isn't implemented in the .Net Client Framework
            int IDOMPersist.AsyncLoadState()
            {
                throw new NotImplementedException();
            }

            bool IDOMPersist.Load(object source)
            {
                if (source is string)
                {
                    try
                    {
                        using (XmlReader reader = XmlReader.Create((string)source, readerSettings))
                        {
                            Load(reader);
                        }
                        return true;
                    }
                    catch (Exception ex)
                    {
                        SetErrorData(ex);
                        return false;
                    }
                }
                SetErrorData(new ArgumentException("Source must be a string", "source"));
                return false;
            }

            bool IDOMPersist.LoadFromStream(Stream stream)
            {
                try
                {
                    using (XmlReader reader = XmlReader.Create(stream, readerSettings))
                    {
                        Load(reader);
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    SetErrorData(ex);
                    return false;
                }
            }

            bool IDOMPersist.LoadXml(string xml)
            {
                try
                {
                    using (XmlReader reader = XmlReader.Create(new StringReader(xml), readerSettings))
                    {
                        Load(reader);
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    SetErrorData(ex);
                    return false;
                }
            }

            void IDOMPersist.Save(object destination)
            {
                if (destination is string)
                    Save((string)destination);
                else
                    throw new ArgumentException("Destination must be a string", "destination");
            }

            void IDOMPersist.SaveToStream(Stream stream)
            {
                Save(stream);
            }

            // Async isn't implemented in the .Net Client Framework
            event TAsyncEventHandler IDOMPersist.OnAsyncLoad
            {
                add { throw new NotImplementedException(); }
                remove { throw new NotImplementedException(); }
            }

            string IDOMPersist.Xml { get { return InnerXml; } }

            #endregion

            #region IDOMParseError Members

            int IDOMParseError.ErrorCode { get { return errorData.ErrorCode; } }
            int IDOMParseError.FilePos { get { return errorData.FilePos; } }
            int IDOMParseError.Line { get { return errorData.Line; } }
            int IDOMParseError.LinePos { get { return errorData.LinePos; } }
            string IDOMParseError.Reason { get { return errorData.Reason; } }
            string IDOMParseError.SrcText { get { return errorData.SrcText; } }
            string IDOMParseError.Url { get { return errorData.Url; } }

            #endregion
        }

        private const string SNotActive = "No active document";
        private const string SNodeNotFound = "Node \"{0}\" not found";
        private const string SMissingNode = "IDOMNode required";
        private const string SNoAttributes = "Attributes are not supported on this node type";
        private const string SInvalidNodeType = "Invalid node type";
        private const string SMismatchedRegItems = "Mismatched paramaters to RegisterChildNodes";
        private const string SNotSingleTextNode = "Element does not contain a single text node";
        private const string SNoDOMParseOptions = "DOM Implementation does not support IDOMParseOptions";
        private const string SNotOnHostedNode = "Invalid operation on a hosted node";
        private const string SMissingItemTag = "ItemTag property is not initialized";
        private const string SNodeReadOnly = "Node is readonly";
        private const string SUnsupportedEncoding = "'Unsupported character encoding \"%s\", try using LoadFromFile";
        private const string SNoRefresh = "Refresh is only supported if the FileName or XML properties are set";
        private const string SMissingFileName = "FileName cannot be blank";
        private const string SLine = "Line";
        private const string SUnknown = "Unknown";

        private static readonly object EVENT_AFTERCLOSE = new object();
        private static readonly object EVENT_AFTEROPEN = new object();
        private static readonly object EVENT_AFTERNODECHANGE = new object();
        private static readonly object EVENT_BEFORECLOSE = new object();
        private static readonly object EVENT_BEFOREOPEN = new object();
        private static readonly object EVENT_BEFORENODECHANGE = new object();
        private static readonly object EVENT_INITIALIZED = new object();
        private static readonly object EVENT_ONASYNCLOAD = new object();

        private byte[] FXMLData;
        private Stream FSrcStream;
        private string FXMLString;
        private TDOMVendor? FDOMVendor;
        private IDOMPersist FDOMPersist;
        private XmlDocument FDOMDocument;
        private IDOMImplementation FDOMImplementation;
        private IDOMParseOptions FDOMParseOptions;
        private TParseOptions FParseOptions;
        private string FFileName;
        private TXMLDocOptions FOptions;
        private bool FStreamedActive;
        private int FModified;
        private int FXMLStrRead;
        private TXMLDocumentSource FDocSource;
        private bool initializing;

        public static TDOMVendor DefaultDOMVendor = TDOMVendor.MSXML;

        public event EventHandler AfterClose { add { Events.AddHandler(EVENT_AFTERCLOSE, value); } remove { Events.RemoveHandler(EVENT_AFTERCLOSE, value); } }
        public event EventHandler AfterOpen { add { Events.AddHandler(EVENT_AFTEROPEN, value); } remove { Events.RemoveHandler(EVENT_AFTEROPEN, value); } }
        public event XmlNodeChangedEventHandler AfterNodeChange { add { Events.AddHandler(EVENT_AFTERNODECHANGE, value); } remove { Events.RemoveHandler(EVENT_AFTERNODECHANGE, value); } }
        public event EventHandler BeforeClose { add { Events.AddHandler(EVENT_BEFORECLOSE, value); } remove { Events.RemoveHandler(EVENT_BEFORECLOSE, value); } }
        public event EventHandler BeforeOpen { add { Events.AddHandler(EVENT_BEFOREOPEN, value); } remove { Events.RemoveHandler(EVENT_BEFOREOPEN, value); } }
        public event XmlNodeChangedEventHandler BeforeNodeChange { add { Events.AddHandler(EVENT_BEFORENODECHANGE, value); } remove { Events.RemoveHandler(EVENT_BEFORENODECHANGE, value); } }
        public event TAsyncEventHandler OnAsyncLoad
        {
            add
            {
                Events.AddHandler(EVENT_ONASYNCLOAD, value);
                if (Active)
                    DOMPersist.OnAsyncLoad += value;
            }
            remove
            {
                Events.RemoveHandler(EVENT_ONASYNCLOAD, value);
                if (Active)
                    DOMPersist.OnAsyncLoad -= value;
            }
        }

        public TXMLDocument(string AFileName = null)
        {
            FFileName = AFileName;
            FOptions = TXMLDocOptions.doNodeAutoCreate | TXMLDocOptions.doAttrNull | TXMLDocOptions.doAutoPrefix | TXMLDocOptions.doNamespaceDecl;
            if (!string.IsNullOrEmpty(FFileName))
                SetActive(true);
        }
        public TXMLDocument(IContainer cont)
            : this()
        {
            cont.Add(this);
            if (DesignMode)
                DOMVendor = DefaultDOMVendor;
        }

        #region ISupportInitialize Members

        void ISupportInitialize.BeginInit()
        {
            initializing = true;
        }

        void ISupportInitialize.EndInit()
        {
            initializing = false;
            try
            {
                if (FStreamedActive) SetActive(true);
            }
            catch (Exception ex)
            {
                if (DesignMode)
                {
                    Application.OnThreadException(ex);
                }
                else
                    throw;
            }
            OnInitialized();
        }

        #endregion

        #region ISupportInitializeNotification Members

        event EventHandler ISupportInitializeNotification.Initialized { add { base.Events.AddHandler(EVENT_INITIALIZED, value); } remove { base.Events.RemoveHandler(EVENT_INITIALIZED, value); } }

        bool ISupportInitializeNotification.IsInitialized { get { return !initializing; } }

        #endregion

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        #region Event triggers

        protected virtual void OnAfterClose(EventArgs e)
        {
            EventHandler eh = Events[EVENT_AFTERCLOSE] as EventHandler;
            if (eh != null)
            {
                eh(this, e);
            }
        }

        protected virtual void OnAfterOpen(EventArgs e)
        {
            EventHandler eh = Events[EVENT_AFTEROPEN] as EventHandler;
            if (eh != null)
            {
                eh(this, e);
            }
        }

        protected virtual void OnAfterNodeChange(XmlNodeChangedEventArgs e)
        {
            XmlNodeChangedEventHandler eh = Events[EVENT_AFTERNODECHANGE] as XmlNodeChangedEventHandler;
            if (eh != null)
            {
                eh(this, e);
            }
        }

        protected virtual void OnBeforeClose(EventArgs e)
        {
            EventHandler eh = Events[EVENT_BEFORECLOSE] as EventHandler;
            if (eh != null)
            {
                eh(this, e);
            }
        }

        protected virtual void OnBeforeOpen(EventArgs e)
        {
            EventHandler eh = Events[EVENT_BEFOREOPEN] as EventHandler;
            if (eh != null)
            {
                eh(this, e);
            }
        }

        protected virtual void OnBeforeNodeChange(XmlNodeChangedEventArgs e)
        {
            XmlNodeChangedEventHandler eh = Events[EVENT_BEFORENODECHANGE] as XmlNodeChangedEventHandler;
            if (eh != null)
            {
                eh(this, e);
            }
        }

        private void OnInitialized()
        {
            EventHandler eh = (EventHandler)base.Events[EVENT_INITIALIZED];
            if (eh != null)
            {
                eh(this, EventArgs.Empty);
            }
        }

        #endregion

        private void CheckDOM()
        {
            if (FDOMImplementation == null)
            {
                switch (FDOMVendor ?? DefaultDOMVendor)
                {
                    case TDOMVendor.MSXML: FDOMImplementation = new MSXMLImplementation(); break;
                    default: throw new NotImplementedException();
                }
            }
        }

        private IDOMParseOptions GetDOMParseOptions()
        {
            IServiceProvider provider = FDOMDocument as IServiceProvider;
            if (provider != null && FDOMParseOptions == null)
                FDOMParseOptions = (IDOMParseOptions)provider.GetService(typeof(IDOMParseOptions)); //QueryInterface?
            return FDOMParseOptions;
        }

        private IDOMPersist GetDOMPersist()
        {
            if (FDOMPersist == null)
                FDOMPersist = (IDOMPersist)FDOMDocument;
            return FDOMPersist;
        }

        protected void CheckActive()
        {
            CheckDOM();
            if (!Active)
                throw new XmlException(SNotActive);
        }

        protected void CheckAutoSave()
        {
            if (TXMLDocOption.doAutoSave.InSet(FOptions) && Modified)
                switch (DocSource)
                {
                    case TXMLDocumentSource.xdsXMLProperty: SaveToXMLString(); break;
                    case TXMLDocumentSource.xdsFile: SaveToFile(FileName); break;
                }
        }

        private void AssignParseOptions()
        {
            if (DOMParseOptions != null)
            {
                DOMParseOptions.PreserveWhiteSpace = TParseOption.poPreserveWhiteSpace.InSet(FParseOptions);
                DOMParseOptions.ResolveExternals = TParseOption.poResolveExternals.InSet(FParseOptions);
                DOMParseOptions.Validate = TParseOption.poValidateOnParse.InSet(FParseOptions);
                DOMParseOptions.Async = TParseOption.poAsyncLoad.InSet(FParseOptions);
                TAsyncEventHandler FOnAsyncLoad = Events[EVENT_ONASYNCLOAD] as TAsyncEventHandler;
                if (FOnAsyncLoad != null)
                    DOMPersist.OnAsyncLoad += FOnAsyncLoad; //TODO: protect against double assignment?
            }
            else if (FParseOptions == 0)
                throw new XmlException(SNoDOMParseOptions);
        }

        private bool GetActive()
        {
            return FDOMDocument != null;
        }

        private XmlDocument GetDOMDocument()
        {
            return FDOMDocument;
        }

        private XmlNode GetDocumentElement()
        {
            CheckActive();
            return FDOMDocument.DocumentElement;
        }

        private string GetFileName()
        {
            return FFileName;
        }

        private bool GetModified()
        {
            return FModified != 0;
        }

        private TXMLDocOptions GetOptions()
        {
            return FOptions;
        }

        private string GetXML()
        {
            // When active, make sure the string list is up to date with what is in the DOM
            if (Active)
                SaveToXMLString();
            return FXMLString;
        }

        private void SaveToXMLString()
        {
            if (FXMLStrRead != FModified)
            {
                string XMLData;
                SaveToXML(out XMLData);
                SetXML(XMLData, false);
                FXMLStrRead = FModified;
            }
        }

        private void LoadFromFile(string AFileName = null)
        {
            SetActive(false);
            if (!string.IsNullOrEmpty(AFileName))
                FileName = AFileName;
            if (string.IsNullOrEmpty(FileName))
                throw new XmlException(SMissingFileName);
            FXMLData = null;
            SetXML(null, false);
            SetActive(true);
        }

        private void SaveToFile(string AFileName = null)
        {
            CheckActive();
            if (AFileName == null)
                DOMPersist.Save(FFileName);
            else
                DOMPersist.Save(AFileName);
            SetModified(false);
        }

        private void LoadFromStream(Stream stream, TXMLEncodingType EncodingType = TXMLEncodingType.xetUnknown)
        {
            SetActive(false);
            SetXML(null, false);
            FXMLData = null;
            FSrcStream = stream;
            SetActive(true);
        }

        private void SaveToStream(Stream stream)
        {
            CheckActive();
            DOMPersist.SaveToStream(stream);
        }

        private void LoadFromXML(byte[] XML)
        {
            SetActive(false);
            SetXML(null, false);
            FXMLData = XML;
            SetActive(true);
        }

        private void LoadFromXML(string XML)
        {
            SetActive(false);
            SetXML(XML, false);
            FXMLData = null;
            SetActive(true);
        }

        private void SaveToXML(out string XMLData)
        {
            CheckActive();
            XMLData = DOMPersist.Xml;
        }

        private void SetActive(bool value)
        {
            if (initializing)
            {
                FStreamedActive = value;
                return;
            }
            if ((FDOMDocument != null) == value)
                return;

            if (value)
            {
                OnBeforeOpen(EventArgs.Empty);

                // create dom parser
                CheckDOM();

                if (FDOMImplementation == null)
                    throw new InvalidOperationException();

                FDOMDocument = FDOMImplementation.CreateDocument(string.Empty, string.Empty, null);

                try
                {
                    LoadData();
                }
                catch
                {
                    ReleaseDoc(false);
                    throw;
                }

                OnAfterOpen(EventArgs.Empty);
            }
            else
            {
                OnBeforeClose(EventArgs.Empty);
                ReleaseDoc();
                OnAfterClose(EventArgs.Empty);
            }
        }

        private void LoadData()
        {
            bool Status;
            /* Data loading precedence: 
               - XML Property (FXMLStrings) 
               - LoadFromStream/LoadFromXML (FXMLData) 
               - FileName Property/LoadFromFile (FFileName) 
            */
            DocSource = TXMLDocumentSource.xdsNone;
            FXMLStrRead = -1;
            AssignParseOptions();
            if (!string.IsNullOrEmpty(FXMLString))
            {
                Status = DOMPersist.LoadXml(FXMLString);
                DocSource = TXMLDocumentSource.xdsXMLProperty;
                FXMLStrRead = 0;
            }
            else if (FXMLData != null && FXMLData.Length != 0) // string is used as word array, without conversion
            {
                using (MemoryStream stream = new MemoryStream(FXMLData, 0, FXMLData.Length, false, true))
                {
                    Status = DOMPersist.LoadFromStream(stream);
                }
                FXMLData = null;
                if (DocSource == TXMLDocumentSource.xdsNone)
                    DocSource = TXMLDocumentSource.xdsXMLData;
            }
            else if (FSrcStream != null)
            {
                FSrcStream.Position = 0;
                Status = DOMPersist.LoadFromStream(FSrcStream);
                DocSource = TXMLDocumentSource.xdsStream;
                FSrcStream = null;
            }
            else if (!string.IsNullOrEmpty(FFileName))
            {
                Status = DOMPersist.Load(FFileName);
                DocSource = TXMLDocumentSource.xdsFile;
            }
            else
                Status = true; // No load, just create empty doc. 
            if (!Status)
            {
                DocSource = TXMLDocumentSource.xdsNone;
                IDOMParseError ParseError = (IDOMParseError)DOMDocument;
                string srcText = ParseError.SrcText;
                if (srcText == null)
                    srcText = string.Empty;
                else if (srcText.Length > 40)
                    srcText = srcText.Substring(0, 40);
                string Msg = string.Format("{0}{1}{2}: {3}{4}{5}", ParseError.Reason, Environment.NewLine, SLine,
                  ParseError.Line, Environment.NewLine, srcText);
                throw new XmlException(Msg, null, ParseError.Line, ParseError.LinePos);
            }
            SetModified(false);

            FDOMDocument.NodeChanging += new XmlNodeChangedEventHandler(FXmlDoc_NodeChanging);
            FDOMDocument.NodeInserting += new XmlNodeChangedEventHandler(FXmlDoc_NodeChanging);
            FDOMDocument.NodeRemoving += new XmlNodeChangedEventHandler(FXmlDoc_NodeChanging);
            FDOMDocument.NodeChanged += new XmlNodeChangedEventHandler(FXmlDoc_NodeChanged);
            FDOMDocument.NodeInserted += new XmlNodeChangedEventHandler(FXmlDoc_NodeChanged);
            FDOMDocument.NodeRemoved += new XmlNodeChangedEventHandler(FXmlDoc_NodeChanged);
        }

        private void ReleaseDoc(bool checkSave = true)
        {
            if (checkSave)
                CheckAutoSave();
            FDOMPersist = null;
            if (FDOMDocument != null)
            {
                FDOMDocument.NodeChanging -= new XmlNodeChangedEventHandler(FXmlDoc_NodeChanging);
                FDOMDocument.NodeInserting -= new XmlNodeChangedEventHandler(FXmlDoc_NodeChanging);
                FDOMDocument.NodeRemoving -= new XmlNodeChangedEventHandler(FXmlDoc_NodeChanging);
                FDOMDocument.NodeChanged -= new XmlNodeChangedEventHandler(FXmlDoc_NodeChanged);
                FDOMDocument.NodeInserted -= new XmlNodeChangedEventHandler(FXmlDoc_NodeChanged);
                FDOMDocument.NodeRemoved -= new XmlNodeChangedEventHandler(FXmlDoc_NodeChanged);
                FDOMDocument = null;
            }
            FDOMParseOptions = null;
            //FPrefixID = 0;
            SetModified(false);
            if (DocSource != TXMLDocumentSource.xdsXMLProperty)
                SetXML(null, false);
        }

        void FXmlDoc_NodeChanged(object sender, XmlNodeChangedEventArgs e)
        {
            SetModified(true);
            OnBeforeNodeChange(e);
        }

        void FXmlDoc_NodeChanging(object sender, XmlNodeChangedEventArgs e)
        {
            OnAfterNodeChange(e);
        }

        private void SetDOMDocument(XmlDocument value)
        {
            SetActive(false);
            OnBeforeOpen(EventArgs.Empty);
            FDOMDocument = value;
            OnAfterOpen(EventArgs.Empty);
        }

        private void SetDocumentElement(XmlNode value)
        {
            CheckActive();

            XmlNode OldDocElement = FDOMDocument.DocumentElement;
            if (OldDocElement != null)
                FDOMDocument.ReplaceChild(value, OldDocElement);
            else
                FDOMDocument.AppendChild(value);
        }

        private void SetDOMImplementation(IDOMImplementation value)
        {
            if (FDOMImplementation == value)
                return;
            SetActive(false);
            FDOMImplementation = value;
        }

        private void SetDOMVendor(TDOMVendor? value)
        {
            if (FDOMVendor == value)
                return;
            FDOMVendor = value;
            SetDOMImplementation(null);
        }

        private void SetFileName(string value)
        {
            if (string.Equals(FFileName, value, StringComparison.InvariantCultureIgnoreCase))
                return;

            if (Active && DocSource == TXMLDocumentSource.xdsFile)
                SetActive(false);
            else
                SetXML(null, false);

            FFileName = value;
            // embarcadero says that the XML property is automatically set on runtime
            //if (!DesignMode)
            //{
            //    SetXML(File.ReadAllText(value), false);
            //}
        }

        private void SetModified(bool value)
        {
            if (value)
                FModified++;
            else
                FModified = 0;
        }

        private void SetOptions(TXMLDocOptions value)
        {
            FOptions = value;
        }

        private void SetXML(string value, bool reset = true)
        {
            if (reset)
            {
                if (!initializing && Active)
                    SetActive(false);
                FFileName = null;
            }
            FXMLString = value;
        }

        private bool ShouldSerializeXML()
        {
            return (Active && (DocSource == TXMLDocumentSource.xdsXMLProperty)) ||
                  (!Active && !string.IsNullOrEmpty(FXMLString));
        }

        private void ResetXML()
        {
            XML = null;
        }

        protected TXMLDocumentSource DocSource { get { return FDocSource; } set { FDocSource = value; } }
        protected IDOMParseOptions DOMParseOptions { get { return GetDOMParseOptions(); } }
        protected IDOMPersist DOMPersist { get { return GetDOMPersist(); } }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public XmlDocument DOMDocument { get { return GetDOMDocument(); } set { SetDOMDocument(value); } }
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public XmlNode DocumentElement { get { return GetDocumentElement(); } set { SetDocumentElement(value); } }
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IDOMImplementation DOMImplementation { get { return FDOMImplementation; } set { SetDOMImplementation(value); } }
        [Browsable(false)]
        public bool Modified { get { return GetModified(); } }

        #region published properties

        [DefaultValue(false)]
        public bool Active { get { return GetActive(); } set { SetActive(value); } }
        public string FileName { get { return GetFileName(); } set { SetFileName(value); } }
        public TDOMVendor? DOMVendor { get { return FDOMVendor; } set { SetDOMVendor(value); } }
        [DefaultValue(TXMLDocOptions.doNodeAutoCreate | TXMLDocOptions.doAttrNull | TXMLDocOptions.doAutoPrefix | TXMLDocOptions.doNamespaceDecl)]
        public TXMLDocOptions Options { get { return GetOptions(); } set { SetOptions(value); } }
        [DefaultValue((TParseOptions)0)]
        public TParseOptions ParseOptions { get { return FParseOptions; } set { FParseOptions = value; } }
        [Editor("System.ComponentModel.Design.MultilineStringEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public /*TStringList*/ string XML { get { return GetXML(); } set { SetXML(value); } }

        #endregion
    }
}
