using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Globalization;
using System.Threading;
using System.ComponentModel;
using DelphiClasses;
using System.Windows.Forms;
using System.Diagnostics;
using NGettext.Loaders;

namespace dxgettext
{
    internal static class Utils
    {
        private static readonly ReaderWriterLock ComponentDomainListCS;
        private static readonly TStringList ComponentDomainList;

        static Utils()
        {
            ComponentDomainList = new TStringList();
            ComponentDomainList.Add(TGnuGettextInstance.DefaultTextDomain);
            ComponentDomainListCS = new ReaderWriterLock();
        }

        internal static string EnsureLineBreakInTranslatedString(string s)
        {
            int i = 1;
            while (i < s.Length)
            {
                Debug.Assert(Environment.NewLine == "\r\n");
                if (s[i] == '\r' && s[i - 1] != '\n')
                {
                    s = s.Insert(i, "\n");
                    i += 2;
                }
                else
                    i++;
            }
            return s;
        }

        internal static string IncludeTrailingPathDelimiter(string path)
        {
            if (path.EndsWith(Path.DirectorySeparatorChar.ToString()) || path.EndsWith(Path.AltDirectorySeparatorChar.ToString()))
                return path;

            if (path.Contains(Path.AltDirectorySeparatorChar))
                return path + Path.AltDirectorySeparatorChar;
            return path + Path.DirectorySeparatorChar;
        }

        internal static string StripCRRawMsgId(string s)
        {
            int i = 1;
            while (i < s.Length)
            {
                if (s[i] == 0x13) s = s.Remove(i, 1); else i++;
            }
            return s;
        }

        internal static Boolean IsWriteProp(PropertyInfo Info)
        {
            return (Info != null && Info.CanWrite); //delphi: Info.GetSetMethod() != null
        }

        private static class PropListCache<T>
        {
            public static readonly PropertyInfo[] Value = CreatePropList();

            private static PropertyInfo[] CreatePropList()
            {
                Type type = typeof(Type);
                PropertyInfo[] PropList;
                // BindingFlags.Instance | BindingFlags.Public is used here, because delphi returns only published properties, which can't be static
                int Count = (PropList = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public)).Length;
                List<PropertyInfo> result = new List<PropertyInfo>(Count);
                for (int i = 0; i < Count; i++)
                {
                    PropertyInfo PropInfo = PropList[i];
                    //HACK: PropInfo.GetCustomAttributes ingores the inherit parameter, so instead use Attribute.GetCustomAttribute
                    //http://stackoverflow.com/questions/2520035/inheritance-of-custom-attributes-on-abstract-properties
                    BrowsableAttribute attrBrowsable = (BrowsableAttribute)Attribute.GetCustomAttribute(PropInfo, typeof(BrowsableAttribute), true);
                    if (attrBrowsable != null && !attrBrowsable.Browsable) // use BrowsableAttribute.No to mimic non-published properties
                        continue;
                    result.Add(PropInfo);
                }
                return result.ToArray();
            }
        }

        internal static PropertyInfo[] GetPropList<T>() { return PropListCache<T>.Value; }
        private static readonly MethodInfo propListValueFunc = typeof(Utils).GetMethod("GetPropList", BindingFlags.NonPublic | BindingFlags.Static, null, Type.EmptyTypes, null);

        internal static PropertyInfo[] GetPropList(object AnObject)
        {
            if (AnObject == null)
                throw new ArgumentNullException("AnObject");
            MethodInfo cacheType = propListValueFunc.MakeGenericMethod(AnObject.GetType());
            return (PropertyInfo[])cacheType.Invoke(null, null);
        }

        internal static Boolean ObjectHasAssignedAction(Object AnObject, PropertyInfo[] PropList, int Count, out Object ActionProperty)
        {
            PropertyInfo PropInfo;
            Object Obj;
            Boolean Result = false;
            ActionProperty = null;
            for (int I = 0; !Result && I < Count; I++)
            {
                PropInfo = PropList[I];
                if (PropInfo.GetIndexParameters().Length != 0)
                    continue;
                // check property type with reflection, before we try to get the value
                if (Type.GetTypeCode(PropInfo.PropertyType) == TypeCode.Object &&
                    typeof(TBasicAction).IsAssignableFrom(PropInfo.PropertyType))
                {
                    try
                    {
                        Obj = PropInfo.GetValue(AnObject, null); // possibly throws exceptions
                    }
                    catch
                    {
                        Obj = null;
                    }
                    Result = Obj != null;//is TBasicAction;
                    if (Result)
                        ActionProperty = Obj;
                }
            }
            return Result;
        }

        public static string ComponentGettext(string MsgId)
        {
            if (MsgId == "" || ComponentDomainListCS == null)
            {
                // This only happens during very complicated program startups that fail,
                // or when Msgid=''
                return MsgId;
            }

            // First, get the value from the default domain
            string Result;
            Result = TGnuGettextInstance.dgettext(TGnuGettextInstance.curmsgdomain, MsgId);
            if (Result != MsgId)
                return Result;

            // If it was not in the default domain, then go through the others
            ComponentDomainListCS.AcquireReaderLock(Timeout.Infinite);
            try
            {
                for (int i = 0; i < ComponentDomainList.Count; i++)
                {
                    Result = TGnuGettextInstance.dgettext(ComponentDomainList.Strings[i], MsgId);
                    if (Result != MsgId)
                        break;
                }
            }
            finally
            {
                ComponentDomainListCS.ReleaseLock();
            }
            return Result;
        }

        public static int GetPluralForm2EN(int Number)
        {
            Number = Math.Abs(Number);
            return (Number == 1) ? 0 : 1;
        }

        public static int GetPluralForm1(int Number)
        {
            return 0;
        }

        public static int GetPluralForm2FR(int Number)
        {
            Number = Math.Abs(Number);
            return (Number == 1 || Number == 0) ? 0 : 1;
        }

        public static int GetPluralForm3LV(int Number)
        {
            Number = Math.Abs(Number);
            if ((Number % 10) == 1 && (Number % 100) != 11)
                return 0;
            else if (Number != 0) return 1;
            else return 2;
        }

        public static int GetPluralForm3GA(int Number)
        {
            Number = Math.Abs(Number);
            if (Number == 1) return 0;
            else if (Number == 2) return 1;
            else return 2;
        }

        public static int GetPluralForm3LT(int Number)
        {
            byte n1, n2;
            Number = Math.Abs(Number);
            n1 = (byte)(Number % 10);
            n2 = (byte)(Number % 100);
            if (n1 == 1 && n2 != 11)
                return 0;
            else if (n1 >= 2 && (n2 < 10 || n2 >= 20)) return 1;
            else return 2;
        }

        public static int GetPluralForm3PL(int Number)
        {
            byte n1, n2;
            Number = Math.Abs(Number);
            n1 = (byte)(Number % 10);
            n2 = (byte)(Number % 100);

            if (Number == 1) return 0;
            else if (n1 >= 2 && n1 <= 4 && (n2 < 10 || n2 >= 20)) return 1;
            else return 2;
        }

        public static int GetPluralForm3RU(int Number)
        {
            byte n1, n2;
            Number = Math.Abs(Number);
            n1 = (byte)(Number % 10);
            n2 = (byte)(Number % 100);
            if (n1 == 1 && n2 != 11)
                return 0;
            else if (n1 >= 2 && n1 <= 4 && (n2 < 10 || n2 >= 20)) return 1;
            else return 2;
        }

        public static int GetPluralForm3SK(int Number)
        {
            Number = Math.Abs(Number);
            if (Number == 1) return 0;
            else if (Number < 5 && Number != 0) return 1;
            else return 2;
        }

        public static int GetPluralForm4SL(int Number)
        {
            byte n2;
            Number = Math.Abs(Number);
            n2 = (byte)(Number % 100);
            if (n2 == 1) return 0;
            else if (n2 == 2) return 1;
            else if (n2 == 3 || n2 == 4) return 2;
            else return 3;
        }
    }
}
