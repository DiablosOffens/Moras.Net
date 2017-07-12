using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using NGettext.Loaders;
using DelphiClasses;
using System.Diagnostics;
using System.IO.MemoryMappedFiles;

namespace dxgettext
{
    internal class TMoFile
    {
        public MoFile File;
        public int Users;
        public bool isSwappedArchitecture;

        public TMoFile(string filename,
                          Int64 Offset, Int64 Size,
                          Boolean xUseMemoryMappedFiles)
        {
            using (FileStream file = new FileStream(filename, FileMode.Open, FileAccess.Read, xUseMemoryMappedFiles ? FileShare.Read : FileShare.ReadWrite))
            {
                Stream mostream = file;
                if (xUseMemoryMappedFiles)
                {
                    using (MemoryMappedFile memfile = MemoryMappedFile.CreateFromFile(file, null, 0, MemoryMappedFileAccess.Read, null, HandleInheritability.None, true))
                    {
                        mostream = memfile.CreateViewStream(0, 0, MemoryMappedFileAccess.Read);
                    }
                }
                mostream.Position = Offset;
                File = new MoFileParser().Parse(mostream);
            }
            isSwappedArchitecture = File.BigEndian == BitConverter.IsLittleEndian;
        }

        public string gettext(string msgid, out Boolean found)
        {
            string[] result;
            found = File.Translations.TryGetValue(msgid, out result);
            if (found)
                return result[0];
            return msgid;
        }
    }

    internal static class TFileLocator
    {

        internal class TEmbeddedFileInfo
        {
            public Int64 offset, size;
        }

        internal static string basedirectory;
        internal static readonly TStringList filelist;
        private static readonly ReaderWriterLock MoFilesCS;
        private static readonly TStringList MoFiles;

        static TFileLocator()
        {
            MoFilesCS = new ReaderWriterLock();
            MoFiles = new TStringList();
            filelist = new TStringList();
            MoFiles.Sorted = true;
            MoFiles.Duplicates = TDuplicates.dupError;
            MoFiles.CaseSensitive = false;
            filelist.Duplicates = TDuplicates.dupError;
            filelist.CaseSensitive = false;
            filelist.Sorted = true;

            Analyze();
        }

        public static Int64 FindSignaturePos(string signature, FileStream str)
        {
            if (signature == "") return -1;

            const int bufsize = 100000;
            StreamReader reader = new StreamReader(str, Encoding.Default, true, bufsize);
            int offset = 0;
            str.Seek(0, SeekOrigin.Begin);
            char[] a = new char[bufsize];
            char[] b = new char[bufsize];
            reader.Read(a, 0, bufsize);

            while (true)
            {
                int rd = reader.Read(b, 0, bufsize);
                int p = (new string(a) + new string(b)).IndexOf(signature);
                if (p != -1) // do not check p < bufsize+100 here!
                    return offset + p;
                if (rd != bufsize)
                    // Prematurely ended without finding anything
                    return -1;
                a = b;
                offset = offset + bufsize;
            }
        }

        public static void Analyze()
        {
            // DetectionSignature: used solely to detect gnugettext usage by assemble
            const string DetectionSignature = "2E23E563-31FA-4C24-B7B3-90BE720C6B1A";
            // Embedded Header Begin Signature (without dynamic prefix written by assemble)
            const string BeginHeaderSignature = "BD7F1BE4-9FCF-4E3A-ABA7-3443D11AB362";
            // Embedded Header End Signature (without dynamic prefix written by assemble)
            const string EndHeaderSignature = "1C58841C-D8A0-4457-BF54-D8315D4CF49D";
            // Assemble Prefix (do not put before the Header Signatures!)
            const string SignaturePrefix = "DXG"; // written from assemble

            int HeaderSize = BeginHeaderSignature.Length;
            int PrefixSize = SignaturePrefix.Length;

            basedirectory = Utils.IncludeTrailingPathDelimiter(Path.GetDirectoryName(TGnuGettextInstance.ExecutableFilename));
            try
            {
                FileStream fs = new FileStream(TGnuGettextInstance.ExecutableFilename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                try
                {
                    // try to find new header begin and end signatures
                    long headerbeginpos = FindSignaturePos(SignaturePrefix + BeginHeaderSignature, fs);
                    long headerendpos = FindSignaturePos(SignaturePrefix + EndHeaderSignature, fs);

                    if (headerbeginpos > -1 && headerendpos > -1)
                    {
                        // adjust positions (to the end of each signature)
                        headerbeginpos = headerbeginpos + HeaderSize + PrefixSize;

                        // get file table offset (8 byte, stored directly before the end header)
                        fs.Seek(headerendpos - 8, SeekOrigin.Begin);
                        // get relative offset and convert to absolute offset during runtime
                        Int64 tableoffset = headerbeginpos + ReadInt64(fs);

                        // go to beginning of embedded block
                        fs.Seek(headerbeginpos, SeekOrigin.Begin);

                        Int64 offset = tableoffset;
                        Debug.Assert(sizeof(Int64) == 8);
                        while (true && fs.Position < headerendpos)
                        {
                            fs.Position = offset;
                            offset = ReadInt64(fs);
                            if (offset == 0)
                                return;
                            offset = headerbeginpos + offset;
                            TEmbeddedFileInfo fi = new TEmbeddedFileInfo();
                            try
                            {
                                // get embedded file info (adjusting dynamic to real offsets now)
                                fi.offset = headerbeginpos + ReadInt64(fs);
                                fi.size = ReadInt64(fs);
                                byte[] filename8bit = new byte[offset - fs.Position];
                                fs.Read(filename8bit, 0, (int)(offset - fs.Position));
                                string filename = Encoding.UTF8.GetString(filename8bit).Trim();
                                if (TGnuGettextInstance.PreferExternal && File.Exists(basedirectory + filename))
                                {
                                    // Disregard the internal version and use the external version instead
                                }
                                else
                                    filelist.AddObject(filename, fi);
                            }
                            catch
                            {
                                throw;
                            }
                        }
                    }
                }
                finally
                {
                    fs.Dispose();
                }
            }
            catch
            {
#if DXGETTEXTDEBUG
                throw;
#endif
            }
        }

        public static bool FileExists(string filename)
        {
            if (filename.StartsWith(basedirectory))
                // Cut off basedirectory if the file is located beneath that base directory
                filename = filename.Substring(basedirectory.Length);
            int idx;
            return filelist.Find(filename, out idx);
        }

        public static TMoFile GetMoFile(string filename, TDebugLogger DebugLogger)
        {
            Int64 offset, size;
            // Find real filename
            offset = 0;
            size = 0;
            string realfilename = filename;
            if (filename.StartsWith(basedirectory))
            {
                filename = filename.Substring(basedirectory.Length);
                int idx = filelist.IndexOf(filename);
                if (idx != -1)
                {
                    TEmbeddedFileInfo fi = (TEmbeddedFileInfo)filelist.Objects[idx];
                    realfilename = TGnuGettextInstance.ExecutableFilename;
                    offset = fi.offset;
                    size = fi.size;
#if DXGETTEXTDEBUG
                    DebugLogger("Instead of " + filename + ", using " + realfilename + " from offset " + (offset).ToString() + ", size " + (size).ToString());
#endif
                }
            }


#if DXGETTEXTDEBUG
            DebugLogger("Reading .mo data from file ''" + filename + "''");
#endif

            // Find TMoFile object
            TMoFile Result;
            MoFilesCS.AcquireWriterLock(Timeout.Infinite);
            try
            {
                string idxname = realfilename + " //\\ " + (offset).ToString();
                int idx;
                if (MoFiles.Find(idxname, out idx))
                {
                    Result = (TMoFile)MoFiles.Objects[idx];
                }
                else
                {
                    Result = new TMoFile(realfilename, offset, size, TGnuGettextInstance.UseMemoryMappedFiles);
                    MoFiles.AddObject(idxname, Result);
                }
                Result.Users++;
            }
            finally
            {
                MoFilesCS.ReleaseWriterLock();
            }
            return Result;
        }

        public static void ReleaseMoFile(TMoFile mofile)
        {
            Debug.Assert(mofile != null);

            MoFilesCS.AcquireWriterLock(Timeout.Infinite);
            try
            {
                mofile.Users--;
                if (mofile.Users <= 0)
                {
                    for (int i = MoFiles.Count - 1; i >= 0; i--)
                    {
                        if (MoFiles.Objects[i] == mofile)
                        {
                            MoFiles.RemoveAt(i);
                            //Dispose()?
                            break;
                        }
                    }
                }
            }
            finally
            {
                MoFilesCS.ReleaseWriterLock();
            }
        }

        private static Int64 ReadInt64(Stream str)
        {
            return new BinaryReader(str).ReadInt64();
        }
    }
}
