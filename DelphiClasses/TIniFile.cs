using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace DelphiClasses
{
    public class TIniFile : IDisposable
    {
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool WritePrivateProfileString(string lpAppName, string lpKeyName, string lpString, string lpFilename);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, StringBuilder lpReturnString, int nSize, string lpFilename);

        //[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        //private static extern int GetPrivateProfileSection(string lpAppName, StringBuilder lpReturnedString, int nSize, string lpFileName);

        //[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        //private static extern int GetPrivateProfileSectionNames(StringBuilder lpReturnedString, int nSize, string lpFileName);

        //[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        //private static extern int WritePrivateProfileSection(string lpAppName, string lpString, string lpFilename);

        private string path;

        public string FileName { get { return path; } }

        public TIniFile(string iniFilePath)
        {
            path = iniFilePath;
        }

        ~TIniFile()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        public void WriteString(string Section, string Ident, string Value)
        {
            if (Section == null)
                throw new ArgumentNullException("Section");
            if (Ident == null)
                throw new ArgumentNullException("Ident");
            if (!WritePrivateProfileString(Section, Ident, Value, this.path))
                throw new Win32Exception();
        }

        public string ReadString(string Section, string Ident, string Default)
        {
            if (Section == null)
                throw new ArgumentNullException("Section");
            if (Ident == null)
                throw new ArgumentNullException("Ident");
            const int len = 255;
            StringBuilder temp = new StringBuilder(len);
            int i = GetPrivateProfileString(Section, Ident, Default, temp,
                                            len, this.path);
            return temp.ToString();

        }

    }
}
