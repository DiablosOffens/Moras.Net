using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Globalization;
using System.Threading;
using DelphiClasses;
using System.ComponentModel;
using System.Windows.Forms;
using System.Diagnostics;
using System.Collections;
using System.Runtime.Serialization;


namespace dxgettext
{
    public delegate void TTranslator(Object obj);
    public delegate void TOnDebugLine(Object Sender, string Line, ref Boolean Discard);
    public delegate int TGetPluralForm(int Number);
    public delegate void TDebugLogger(string line);

    public static class TGnuGettextInstance
    {
        internal const string DefaultTextDomain = "default";
        internal const bool PreferExternal = false;
        internal const bool UseMemoryMappedFiles = true;
        internal const bool ReReadMoFileOnSameLanguage = false;
        // Subversion source code version control version information
        internal const string VCSVersion = "$LastChangedRevision: 220 $";


        internal static readonly string ExecutableFilename;
        private static readonly ReaderWriterLock ResourceStringDomainListCS;
        private static readonly TStringList ResourceStringDomainList;

        private static TOnDebugLine fOnDebugLine;

        private static string curlang;
        private static TGetPluralForm curGetPluralForm;
        internal static string curmsgdomain;
        private static ReaderWriterLock savefileCS;
        //private static TStringList savememory;
        private static readonly string DefaultDomainDirectory;
        private static TStringList domainlist;
        private static TStringList TP_IgnoreList;
        private static List<TClassMode> TP_ClassHandling;
        private static List<TClassMode> TP_GlobalClassHandling;
        private static List<TClassMode> TP_InterfaceHandling;
        private static TExecutable TP_Retranslator;
        private static TInterfaceList fWhenNewLanguageListeners;
#if DXGETTEXTDEBUG
        //private static ReaderWriterLock DebugLogCS;
        private static FilteredStreamWriterTraceListener DebugLog;
        private static bool DebugLogOutputPaused;
#endif

        public static bool Enabled;
        public static int DesignTimeCodePage;
        public static bool SearchAllDomains;

        internal class FilteredStreamWriterTraceListener : TextWriterTraceListener
        {
            internal class OnDebugLineFilter : TraceFilter
            {
                public override bool ShouldTrace(TraceEventCache cache, string source, TraceEventType eventType, int id, string formatOrMessage, object[] args, object data1, object[] data)
                {
                    if (fOnDebugLine != null)
                    {
                        bool discard = true;
                        fOnDebugLine(null, formatOrMessage, ref discard);
                        return !discard;
                    }
                    return true;
                }
            }

            public FilteredStreamWriterTraceListener(StreamWriter writer)
                : base(writer)
            {
                Filter = new OnDebugLineFilter();
            }

            public new StreamWriter Writer { get { return (StreamWriter)base.Writer; } set { base.Writer = value; } }

            public override void Write(string message)
            {
                if (Filter == null || Filter.ShouldTrace(null, "Trace", TraceEventType.Information, 0, message, null, null, null))
                {
                    base.Write(message);
                }
            }

            public override void WriteLine(string message)
            {
                if (Filter == null || Filter.ShouldTrace(null, "Trace", TraceEventType.Information, 0, message, null, null, null))
                {
                    base.WriteLine(message);
                }
            }
        }

        static TGnuGettextInstance()
        {
            ExecutableFilename = Application.ExecutablePath;
            ResourceStringDomainList = new TStringList();
            ResourceStringDomainList.Add(DefaultTextDomain);
            ResourceStringDomainListCS = new ReaderWriterLock();

#if DXGETTEXTDEBUG
            //DebugLogCS = new ReaderWriterLock();
            DebugLog = new FilteredStreamWriterTraceListener(new StreamWriter(new MemoryStream(), Encoding.Default));
            Debug.Listeners.Add(DebugLog);
            Debug.WriteLine("Debug log started " + DateTime.Now.ToString());
            Debug.WriteLine("GNU gettext module version: " + VCSVersion);
            Debug.WriteLine("");
#endif
            curGetPluralForm = Utils.GetPluralForm2EN;
            Enabled = true;
            SearchAllDomains = false;
            curmsgdomain = DefaultTextDomain;
            savefileCS = new ReaderWriterLock();
            domainlist = new TStringList();
            TP_IgnoreList = new TStringList();
            TP_IgnoreList.Sorted = true;
            TP_GlobalClassHandling = new List<TClassMode>();
            TP_ClassHandling = new List<TClassMode>();
            TP_InterfaceHandling = new List<TClassMode>();
            fWhenNewLanguageListeners = new TInterfaceList();

            // Set some settings
            DefaultDomainDirectory = Path.Combine(Path.GetDirectoryName(ExecutableFilename), "locale");

            UseLanguage("");

            bindtextdomain(DefaultTextDomain, DefaultDomainDirectory);
            textdomain(DefaultTextDomain);

            // Add default properties to ignore
            TP_IgnoreInterfaceProperty(typeof(ISite), "Name");
            TP_GlobalIgnoreClassProperty(typeof(Control), "Name");
            TP_GlobalIgnoreClassProperty(typeof(TCollection<>), "PropName");
        }

        private static string GetWindowsLanguage()
        {
            CultureInfo.CurrentCulture.ClearCachedData(); // also resets RegionInfo.CurrentRegion in .Net Framework 4.0
            string LanguageName = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
            string CountryName = RegionInfo.CurrentRegion.TwoLetterISORegionName;
            string LangCode = LanguageName;
            if (LangCode.ToLower() == "no") LangCode = "nb";
            LangCode += "_" + CountryName;
            return LangCode;
        }

        public static void UseLanguage(string LanguageCode)
        {
            int i, p;
            TDomain dom;
            string l2;
#if DXGETTEXTDEBUG
            Debug.WriteLine("UseLanguage('" + LanguageCode + "'); called");
#endif

            if (LanguageCode == "")
            {
                LanguageCode = Environment.GetEnvironmentVariable("LANG");
#if DXGETTEXTDEBUG
                Debug.WriteLine("LANG env variable is '" + LanguageCode + "'.");
#endif
                if (string.IsNullOrEmpty(LanguageCode))
                {
                    LanguageCode = GetWindowsLanguage();
#if DXGETTEXTDEBUG
                    Debug.WriteLine("Found Windows language code to be '" + LanguageCode + "'.");
#endif
                }
                p = LanguageCode.IndexOf('.');
                if (p != -1)
                    LanguageCode = LanguageCode.Substring(0, p);
#if DXGETTEXTDEBUG
                Debug.WriteLine("Language code that will be set is '" + LanguageCode + "'.");
#endif
            }

            curlang = LanguageCode;
            for (i = 0; i <= domainlist.Count - 1; i++)
            {
                dom = (TDomain)domainlist.Objects[i];
                dom.SetLanguageCode(curlang);
            }

            l2 = curlang.Substring(0, 2).ToLower();
            if (l2 == "en" || l2 == "de") curGetPluralForm = Utils.GetPluralForm2EN;
            else if (l2 == "hu" || l2 == "ko" || l2 == "zh" || l2 == "ja" || l2 == "tr") curGetPluralForm = Utils.GetPluralForm1;
            else if (l2 == "fr" || l2 == "fa" || (curlang).ToLower() == "pt_br") curGetPluralForm = Utils.GetPluralForm2FR;
            else if (l2 == "lv") curGetPluralForm = Utils.GetPluralForm3LV;
            else if (l2 == "ga") curGetPluralForm = Utils.GetPluralForm3GA;
            else if (l2 == "lt") curGetPluralForm = Utils.GetPluralForm3LT;
            else if (l2 == "ru" || l2 == "uk" || l2 == "hr") curGetPluralForm = Utils.GetPluralForm3RU;
            else if (l2 == "cs" || l2 == "sk") curGetPluralForm = Utils.GetPluralForm3SK;
            else if (l2 == "pl") curGetPluralForm = Utils.GetPluralForm3PL;
            else if (l2 == "sl") curGetPluralForm = Utils.GetPluralForm4SL;
            else
            {
                curGetPluralForm = Utils.GetPluralForm2EN;
#if DXGETTEXTDEBUG
                Debug.WriteLine("Plural form for the language was not found. English plurality system assumed.");
#endif
            }

            WhenNewLanguage(curlang);

#if DXGETTEXTDEBUG
            Debug.WriteLine("");
#endif
        }

        public static void GetListOfLanguages(string domain, TStringList list)
        {
            Getdomain(domain, DefaultDomainDirectory, curlang).GetListOfLanguages(list);
        }

        public static string gettext(string szMsgId)
        {
            string domain;

            string Result = dgettext(curmsgdomain, szMsgId);
            if (SearchAllDomains)
            {
                for (int domainIndex = 0; Result == szMsgId && domainIndex < domainlist.Count; domainIndex++)
                {
                    domain = domainlist[domainIndex];
                    Result = dgettext(domain, szMsgId);
                }
            }
            return Result;
        }

        public static string gettext_NoExtract(string szMsgId)
        {
            // This one is very useful for translating text in variables.
            // This can sometimes be necessary, and by using this function,
            // the source code scanner will not trigger warnings.
            return gettext(szMsgId);
        }

        public static string gettext_NoOp(string szMsgId)
        {
            //*** With this function Strings can be added to the po-file without beeing
            //    ResourceStrings (dxgettext will add the string and this function will
            //    return it without a change)
            //    see gettext manual
            //      4.7 - Special Cases of Translatable Strings
            //      http://www.gnu.org/software/hello/manual/gettext/Special-cases.html#Special-cases
            return szMsgId;
        }

        public static string ngettext(string singular, string plural, int Number)
        {
            string Result = dngettext(curmsgdomain, singular, plural, Number);
            if (SearchAllDomains)
            {
                for (int domainIndex = 0; Result != singular && Result != plural && domainIndex < domainlist.Count; domainIndex++)
                {
                    string domain = domainlist[domainIndex];
                    Result = dngettext(domain, singular, plural, Number);
                }
            }
            return Result;
        }

        public static string ngettext_NoExtract(string singular, string plural, int Number)
        {
            // This one is very useful for translating text in variables.
            // This can sometimes be necessary, and by using this function,
            // the source code scanner will not trigger warnings.
            return ngettext(singular, plural, Number);
        }

        public static string GetCurrentLanguage()
        {
            return curlang;
        }

        public static string GetTranslationProperty(string Propertyname)
        {
            return Getdomain(curmsgdomain, DefaultDomainDirectory, curlang).GetTranslationProperty(Propertyname);
        }

        public static string GetTranslatorNameAndEmail()
        {
            return GetTranslationProperty("LAST-TRANSLATOR");
        }

        public static void TP_Ignore(Object AnObject, string name)
        {
            TP_IgnoreList.Add(name.ToUpper());
#if DXGETTEXTDEBUG
            Debug.WriteLine("On object with class name " + AnObject.GetType().Name + ", ignore is set on " + name);
#endif
        }

        public static void TP_IgnoreClass(Type IgnClass)
        {
            TClassMode cm;
            for (int i = 0; i < TP_ClassHandling.Count; i++)
            {
                cm = TP_ClassHandling[i];
                if (cm.HClass == IgnClass)
                    throw new EGGProgrammingError("You cannot add a class to the ignore list that is already on that list: " + IgnClass.Name + '.');
                if (IgnClass.InheritsFrom(cm.HClass))
                {
                    // This is the place to insert this class
                    cm = new TClassMode();
                    cm.HClass = IgnClass;
                    TP_ClassHandling.Insert(i, cm);
#if DXGETTEXTDEBUG
                    Debug.WriteLine("Locally, class " + IgnClass.Name + " is being ignored.");
#endif
                    return;
                }
            }
            cm = new TClassMode();
            cm.HClass = IgnClass;
            TP_ClassHandling.Add(cm);
#if DXGETTEXTDEBUG
            Debug.WriteLine("Locally, class " + IgnClass.Name + " is being ignored.");
#endif
        }

        public static void TP_IgnoreClassProperty(Type IgnClass, string propertyname)
        {
            TClassMode cm;
            propertyname = propertyname.ToUpper();
            for (int i = 0; i < TP_ClassHandling.Count; i++)
            {
                cm = TP_ClassHandling[i];
                if (cm.HClass == IgnClass)
                {
                    if (cm.SpecialHandler != null)
                        throw new EGGProgrammingError("You cannot ignore a class property for a class that has a handler set.");
                    cm.PropertiesToIgnore.Add(propertyname);
#if DXGETTEXTDEBUG
                    Debug.WriteLine("Globally, the " + propertyname + " property of class " + IgnClass.Name + " is being ignored.");
#endif
                    return;
                }
                if (IgnClass.InheritsFrom(cm.HClass))
                {
                    // This is the place to insert this class
                    cm = new TClassMode();
                    cm.HClass = IgnClass;
                    cm.PropertiesToIgnore.Add(propertyname);
                    TP_ClassHandling.Insert(i, cm);
#if DXGETTEXTDEBUG
                    Debug.WriteLine("Locally, the " + propertyname + " property of class " + IgnClass.Name + " is being ignored.");
#endif
                    return;
                }
            }
            cm = new TClassMode();
            cm.HClass = IgnClass;
            cm.PropertiesToIgnore.Add(propertyname);
            TP_GlobalClassHandling.Add(cm);
#if DXGETTEXTDEBUG
            Debug.WriteLine("Locally, the " + propertyname + " property of class " + IgnClass.Name + " is being ignored.");
#endif
        }

        public static Boolean TP_TryGlobalIgnoreClass(Type IgnClass)
        {
            TClassMode cm;
            for (int i = 0; i < TP_GlobalClassHandling.Count; i++)
            {
                cm = TP_GlobalClassHandling[i];
                if (cm.HClass == IgnClass)
                    return false; // class already in ignore list
                if (IgnClass.InheritsFrom(cm.HClass))
                {
                    // This is the place to insert this class
                    cm = new TClassMode();
                    cm.HClass = IgnClass;
                    TP_GlobalClassHandling.Insert(i, cm);
#if DXGETTEXTDEBUG
                    Debug.WriteLine("Globally, class " + IgnClass.Name + " is being ignored.");
#endif
                    return true;
                }
            }
            cm = new TClassMode();
            cm.HClass = IgnClass;
            TP_GlobalClassHandling.Add(cm);
#if DXGETTEXTDEBUG
            Debug.WriteLine("Globally, class " + IgnClass.Name + " is being ignored.");
#endif
            return true;
        }

        public static void TP_GlobalIgnoreClass(Type IgnClass)
        {
            if (!TP_TryGlobalIgnoreClass(IgnClass))
                throw new EGGProgrammingError("You cannot add a class to the ignore list that is already on that list: " + IgnClass.Name + ". You should keep all TP_Global functions in one place in your source code.");
        }

        public static void TP_GlobalIgnoreClassProperty(Type IgnClass, string propertyname)
        {
            TClassMode cm;
            propertyname = propertyname.ToUpper();
            for (int i = 0; i < TP_GlobalClassHandling.Count; i++)
            {
                cm = TP_GlobalClassHandling[i];
                if (cm.HClass == IgnClass)
                {
                    if (cm.SpecialHandler != null)
                        throw new EGGProgrammingError("You cannot ignore a class property for a class that has a handler set.");
                    int idx;
                    if (!cm.PropertiesToIgnore.Find(propertyname, out idx))
                        cm.PropertiesToIgnore.Add(propertyname);
#if DXGETTEXTDEBUG
                    Debug.WriteLine("Globally, the " + propertyname + " property of class " + IgnClass.Name + " is being ignored.");
#endif
                    return;
                }
                if (IgnClass.InheritsFrom(cm.HClass))
                {
                    // This is the place to insert this class
                    cm = new TClassMode();
                    cm.HClass = IgnClass;
                    cm.PropertiesToIgnore.Add(propertyname);
                    TP_GlobalClassHandling.Insert(i, cm);
#if DXGETTEXTDEBUG
                    Debug.WriteLine("Globally, the " + propertyname + " property of class " + IgnClass.Name + " is being ignored.");
#endif
                    return;
                }
            }
            cm = new TClassMode();
            cm.HClass = IgnClass;
            cm.PropertiesToIgnore.Add(propertyname);
            TP_GlobalClassHandling.Add(cm);
#if DXGETTEXTDEBUG
            Debug.WriteLine("Globally, the " + propertyname + " property of class " + IgnClass.Name + " is being ignored.");
#endif
        }

        public static void TP_GlobalHandleClass(Type HClass, TTranslator Handler)
        {
            TClassMode cm;
            for (int i = 0; i < TP_GlobalClassHandling.Count; i++)
            {
                cm = TP_GlobalClassHandling[i];
                if (cm.HClass == HClass)
                    throw new EGGProgrammingError("You cannot set a handler for a class that has already been assigned otherwise.");
                if (HClass.InheritsFrom(cm.HClass))
                {
                    // This is the place to insert this class
                    cm = new TClassMode();
                    cm.HClass = HClass;
                    cm.SpecialHandler = Handler;
                    TP_GlobalClassHandling.Insert(i, cm);
#if DXGETTEXTDEBUG
                    Debug.WriteLine("A handler was set for class " + HClass.Name + '.');
#endif
                    return;
                }
            }
            cm = new TClassMode();
            cm.HClass = HClass;
            cm.SpecialHandler = Handler;
            TP_GlobalClassHandling.Add(cm);
#if DXGETTEXTDEBUG
            Debug.WriteLine("A handler was set for class " + HClass.Name + '.');
#endif
        }

        public static void TP_IgnoreInterface(Type IgnInterface)
        {
            TClassMode cm;
            for (int i = 0; i < TP_InterfaceHandling.Count; i++)
            {
                cm = TP_InterfaceHandling[i];
                if (cm.HClass == IgnInterface)
                    throw new EGGProgrammingError("You cannot add a interface to the ignore list that is already on that list: " + IgnInterface.Name + '.');
                if (IgnInterface.ImplementsInterface(cm.HClass))
                {
                    // This is the place to insert this class
                    cm = new TClassMode();
                    cm.HClass = IgnInterface;
                    TP_InterfaceHandling.Insert(i, cm);
#if DXGETTEXTDEBUG
                    Debug.WriteLine("Locally, interface " + IgnInterface.Name + " is being ignored.");
#endif
                    return;
                }
            }
            cm = new TClassMode();
            cm.HClass = IgnInterface;
            TP_InterfaceHandling.Add(cm);
#if DXGETTEXTDEBUG
            Debug.WriteLine("Locally, interface " + IgnInterface.Name + " is being ignored.");
#endif
        }

        public static void TP_IgnoreInterfaceProperty(Type IgnInterface, string propertyname)
        {
            TClassMode cm;
            propertyname = propertyname.ToUpper();
            for (int i = 0; i < TP_InterfaceHandling.Count; i++)
            {
                cm = TP_InterfaceHandling[i];
                if (cm.HClass == IgnInterface)
                {
                    if (cm.SpecialHandler != null)
                        throw new EGGProgrammingError("You cannot ignore a interface property for a interface that has a handler set.");
                    cm.PropertiesToIgnore.Add(propertyname);
#if DXGETTEXTDEBUG
                    Debug.WriteLine("Locally, the " + propertyname + " property of interface " + IgnInterface.Name + " is being ignored.");
#endif
                    return;
                }
                if (IgnInterface.ImplementsInterface(cm.HClass))
                {
                    // This is the place to insert this class
                    cm = new TClassMode();
                    cm.HClass = IgnInterface;
                    cm.PropertiesToIgnore.Add(propertyname);
                    TP_InterfaceHandling.Insert(i, cm);
#if DXGETTEXTDEBUG
                    Debug.WriteLine("Locally, the " + propertyname + " property of interface " + IgnInterface.Name + " is being ignored.");
#endif
                    return;
                }
            }
            cm = new TClassMode();
            cm.HClass = IgnInterface;
            cm.PropertiesToIgnore.Add(propertyname);
            TP_InterfaceHandling.Add(cm);
#if DXGETTEXTDEBUG
            Debug.WriteLine("Locally, the " + propertyname + " property of interface " + IgnInterface.Name + " is being ignored.");
#endif
        }

        public static void TP_Remember(Object AnObject, string PropName, string OldValue)
        {
            if (TP_Retranslator != null)
                ((TTP_Retranslator)TP_Retranslator).Remember(AnObject, PropName, OldValue);
            else
                throw new EGGProgrammingError("You can only call TP_Remember when doing the initial translation (TP_Retranslator is not set).");
        }

        public static void TranslateProperties(Object AnObject, string textdomain = "")
        {
            TStringList TodoList; // List of Name/TObject's that is to be processed
            HashSet<object> DoneList; // hashset of objects that have been done
            PropertyInfo[] PropList;
            string UPropName;
            PropertyInfo PropInfo;
            TClassMode cm,
            currentcm; // currentcm is nil or contains special information about how to handle the current object
            TStringList ObjectPropertyIgnoreList;
            string Name;
            Object ActionProperty;

#if DXGETTEXTDEBUG
            Debug.WriteLine("----------------------------------------------------------------------");
            Debug.WriteLine("TranslateProperties() was called for an object of class " + AnObject.GetType().Name + " with domain \"" + textdomain + "\".");
#endif

            if (TP_Retranslator != null)
                if (textdomain == "")
                    ((TTP_Retranslator)TP_Retranslator).TextDomain = curmsgdomain;
                else
                    ((TTP_Retranslator)TP_Retranslator).TextDomain = textdomain;
            DoneList = new HashSet<object>();
            TodoList = new TStringList();
            ObjectPropertyIgnoreList = new TStringList();
            try
            {
                TodoList.AddObject("", AnObject);
                ObjectPropertyIgnoreList.Sorted = true;
                ObjectPropertyIgnoreList.Duplicates = TDuplicates.dupIgnore;
                ObjectPropertyIgnoreList.CaseSensitive = false;

                while (TodoList.Count != 0)
                {
                    AnObject = TodoList.Objects[0];
                    Name = TodoList.Strings[0];
                    TodoList.RemoveAt(0);
                    // In c# all things can be persisted by reflection. And WinForms has no concept of an abstract form file.
                    // But WPF supports both (form data is stored as XML), so reimplement this for WPF if there is a need for.
                    if (AnObject != null /*&& AnObject.CanPersist()*/)
                    {
                        // Make sure each object is only translated once
                        if (!DoneList.Add(AnObject))
                        {
                            continue;
                        }

                        ObjectPropertyIgnoreList.Clear();

                        // Find out if there is special handling of this object
                        currentcm = null;
                        // First check the local handling instructions
                        for (int j = 0; j < TP_ClassHandling.Count; j++)
                        {
                            cm = TP_ClassHandling[j];
                            if (AnObject.InheritsFrom(cm.HClass))
                            {
                                if (cm.PropertiesToIgnore.Count != 0)
                                {
                                    ObjectPropertyIgnoreList.AddStrings(cm.PropertiesToIgnore);
                                }
                                else
                                {
                                    // Ignore the entire class
                                    currentcm = cm;
                                    break;
                                }
                            }
                        }
                        // Then check the global handling instructions
                        if (currentcm == null)
                            for (int j = 0; j < TP_GlobalClassHandling.Count; j++)
                            {
                                cm = TP_GlobalClassHandling[j];
                                if (AnObject.InheritsFrom(cm.HClass))
                                {
                                    if (cm.PropertiesToIgnore.Count != 0)
                                    {
                                        ObjectPropertyIgnoreList.AddStrings(cm.PropertiesToIgnore);
                                    }
                                    else
                                    {
                                        // Ignore the entire class
                                        currentcm = cm;
                                        break;
                                    }
                                }
                            }
                        if (currentcm == null)
                            for (int j = 0; j < TP_InterfaceHandling.Count; j++)
                            {
                                cm = TP_InterfaceHandling[j];
                                if (AnObject.ImplementsInterface(cm.HClass))
                                {
                                    if (cm.PropertiesToIgnore.Count != 0)
                                    {
                                        ObjectPropertyIgnoreList.AddStrings(cm.PropertiesToIgnore); //TODO: craft special interface properties list
                                    }
                                    else
                                    {
                                        // Ignore the entire interface
                                        currentcm = cm;
                                        break;
                                    }
                                }
                            }
                        if (currentcm != null)
                        {
                            ObjectPropertyIgnoreList.Clear();
                            // Ignore or use special handler
                            if (currentcm.SpecialHandler != null)
                            {
                                currentcm.SpecialHandler(AnObject);
#if DXGETTEXTDEBUG
                                Debug.WriteLine("Special handler activated for " + AnObject.GetType().Name);
#endif
                            }
                            else
                            {
#if DXGETTEXTDEBUG
                                Debug.WriteLine("Ignoring object " + AnObject.GetType().Name);
#endif
                            }
                            continue;
                        }

                        // BindingFlags.Instance | BindingFlags.Public is used here, because delphi returns only published properties, which can't be static
                        int Count = (PropList = Utils.GetPropList(AnObject)).Length;
                        try
                        {
                            if (Utils.ObjectHasAssignedAction(AnObject, PropList, Count, out ActionProperty) && !ClassIsIgnored(ActionProperty.GetType()))
                                continue;

                            for (int j = 0; j < Count; j++)
                            {
                                PropInfo = PropList[j];
                                if (PropInfo.GetIndexParameters().Length != 0)
                                    continue;
                                if (!(Type.GetTypeCode(PropInfo.PropertyType)).InSet(TypeCode.String, TypeCode.Object))
                                    continue;
                                UPropName = PropInfo.Name.ToUpper();
                                int i;
                                // Ignore properties that are meant to be ignored
                                if ((currentcm == null || !currentcm.PropertiesToIgnore.Find(UPropName, out i)) &&
                                   !TP_IgnoreList.Find(Name + "." + UPropName, out i) &&
                                   !ObjectPropertyIgnoreList.Find(UPropName, out i))
                                {
                                    TranslateProperty(AnObject, PropInfo, TodoList, textdomain);
                                }  // if
                            }  // for
                        }
                        finally
                        {
                            if (Count != 0)
                                PropList = null;
                        }
                        if (AnObject is TStringList)
                        {
                            if (((TStringList)AnObject).Text != "" && TP_Retranslator != null)
                                (TP_Retranslator as TTP_Retranslator).Remember(AnObject, "Text", ((TStringList)AnObject).Text);
                            TranslateStrings((TStringList)AnObject, textdomain);
                        }
                        // Check for TCollection
                        if (AnObject is IList)
                        {
                            // Only add the object if it's not totally ignored already
                            if (currentcm == null || !AnObject.GetType().InheritsFrom(currentcm.HClass))
                            {
                                for (int i = 0; i < ((IList)AnObject).Count; i++)
                                {
                                    object obj = ((IList)AnObject)[i];
                                    if (obj != null)
                                    {
                                        // Translate certain types of objects, because the CLR can only use IList instead of TStringList
                                        switch (Type.GetTypeCode(obj.GetType()))
                                        {
                                            case TypeCode.String:
                                                {
#if DXGETTEXTDEBUG
                                                    Debug.WriteLine("Translating " + Name);
#endif
                                                    string old = obj as string;
#if DXGETTEXTDEBUG
                                                    if (string.IsNullOrEmpty(old))
                                                        Debug.WriteLine("(Empty, not translated)");
                                                    else
                                                        Debug.WriteLine("Old value: \"" + old + "\"");
#endif
                                                    if (!string.IsNullOrEmpty(old))
                                                    {
                                                        string ws;
                                                        if (textdomain == "")
                                                            ws = Utils.ComponentGettext(old);
                                                        else
                                                            ws = dgettext(textdomain, old);
                                                        if (ws != old)
                                                            ((IList)AnObject)[i] = ws;
                                                    }
                                                }// { case item }
                                                break;
                                            case TypeCode.Object:
                                                {
                                                    if (obj is Component)
                                                    {
                                                        IComponent compmarker = ((Component)obj).FindComponent("GNUgettextMarker");
                                                        if (compmarker != null)
                                                            return;
                                                    }
                                                    TodoList.AddObject("", obj);
                                                }// { case item }
                                                break;
                                        } //{ case };
                                    }
                                }
                            }
                        }
                        if (AnObject is Component)
                        {
                            var components = ((Component)AnObject).GetComponents();
                            if (components == null)
                                continue;
                            for (int i = 0; i < components.Count; i++)
                            {
                                IComponent comp = components[i];
                                int j;
                                string compname = comp.GetName() ?? string.Empty; //HACK: WinForms supports unnamed components
                                if (!TP_IgnoreList.Find(compname.ToUpper(), out j))
                                {
                                    // Only add the object if it's not totally ignored or translated already
                                    if (currentcm == null || !AnObject.GetType().InheritsFrom(currentcm.HClass))
                                    {
                                        IComponent compmarker = comp.FindComponent("GNUgettextMarker");
                                        if (compmarker == null)
                                            TodoList.AddObject(compname.ToUpper(), comp);
                                    }
                                }
                            }
                        }
                    } //{ if AnObject<>nil }
                } //{ while todolist.count<>0 }
            }
            finally
            {
                TodoList = null;
                ObjectPropertyIgnoreList = null;
                DoneList = null;
            }
            FreeTP_ClassHandlingItems();
            TP_IgnoreList.Clear();
            TP_Retranslator = null;
#if DXGETTEXTDEBUG
            Debug.WriteLine("----------------------------------------------------------------------");
#endif
        }

        public static void TranslateComponent(Component AnObject, string TextDomain = "")
        {
#if DXGETTEXTDEBUG
            Debug.WriteLine("======================================================================");
            Debug.WriteLine("TranslateComponent() was called for a component with name " + (AnObject.Site != null ? AnObject.Site.Name : "") + ".");
#endif
            TGnuGettextComponentMarker comp = (TGnuGettextComponentMarker)AnObject.FindComponent("GNUgettextMarker");
            if (comp == null)
            {
                comp = new TGnuGettextComponentMarker();
                comp.Retranslator = TP_CreateRetranslator();
                TranslateProperties(AnObject, TextDomain);
                AnObject.InsertComponent(comp, "GNUgettextMarker");
#if DXGETTEXTDEBUG
                Debug.WriteLine("This is the first time, that this component has been translated. A retranslator component has been created for this component.");
#endif
            }
            else
            {
#if DXGETTEXTDEBUG
                Debug.WriteLine("This is not the first time, that this component has been translated.");
#endif
                if (comp.LastLanguage != curlang)
                {
#if DXGETTEXTDEBUG
                    Debug.WriteLine("ERROR: TranslateComponent() was called twice with different languages. This indicates an attempt to switch language at runtime, but by using TranslateComponent every time. This API has changed - please use RetranslateComponent() instead.");
#endif
                    MessageBox.Show(null, "This application tried to switch the language, but in an incorrect way. The programmer needs to replace a call to TranslateComponent with a call to RetranslateComponent(). The programmer should see the changelog of gnugettext.pas for more information.", "Error", MessageBoxButtons.OK);
                }
                else
                {
#if DXGETTEXTDEBUG
                    Debug.WriteLine("ERROR: TranslateComponent has been called twice, but with the same language chosen. This is a mistake, but in order to prevent that the application breaks, no exception is raised.");
#endif
                }
            }
            comp.LastLanguage = curlang;
#if DXGETTEXTDEBUG
            Debug.WriteLine("======================================================================");
#endif
        }
        public static void RetranslateComponent(Component AnObject, string TextDomain = "")
        {
            TGnuGettextComponentMarker comp;
#if DXGETTEXTDEBUG
            Debug.WriteLine("======================================================================");
            Debug.WriteLine("RetranslateComponent() was called for a component with name " + AnObject.GetName() + '.');
#endif
            comp = AnObject.FindComponent("GNUgettextMarker") as TGnuGettextComponentMarker;
            if (comp == null)
            {
#if DXGETTEXTDEBUG
                Debug.WriteLine("Retranslate was called on an object that has not been translated before. An Exception is being raised.");
#endif
                throw new EGGProgrammingError("Retranslate was called on an object that has not been translated before. Please use TranslateComponent() before RetranslateComponent().");
            }
            else
            {
                //*** if param ReReadMoFileOnSameLanguage is set, use the ReTranslate
                //    function nevertheless if the current language is the same like the
                //    new (-> reread the current .mo-file from the file system).
                if (ReReadMoFileOnSameLanguage ||
                   (comp.LastLanguage != curlang))
                {
#if DXGETTEXTDEBUG
                    Debug.WriteLine("The retranslator is being executed.");
#endif
                    comp.Retranslator.Execute();
                }
                else
                {
#if DXGETTEXTDEBUG
                    Debug.WriteLine("The language has not changed. The retranslator is not executed.");
#endif
                }
            }
            comp.LastLanguage = curlang;

#if DXGETTEXTDEBUG
            Debug.WriteLine("======================================================================");
#endif
        }

        public static string dgettext(string szDomain, string szMsgId)
        {
            if (!Enabled)
            {
#if DXGETTEXTDEBUG
                Debug.WriteLine("Translation has been disabled. Text is not being translated: " + szMsgId);
#endif
                return szMsgId;
            }
            string result = Utils.EnsureLineBreakInTranslatedString(Getdomain(szDomain, DefaultDomainDirectory, curlang).gettext(Utils.StripCRRawMsgId(szMsgId)));
#if DXGETTEXTDEBUG
            if (szMsgId != "" && result == "")
                Debug.WriteLine(string.Format("Error: Translation of %s was an empty string. This may never occur.", szMsgId));
#endif

            return result;
        }

        public static string dgettext_NoExtract(string szDomain, string szMsgId)
        {
            // This one is very useful for translating text in variables.
            // This can sometimes be necessary, and by using this function,
            // the source code scanner will not trigger warnings.
            return dgettext(szDomain, szMsgId);
        }

        public static string dgettext_NoOp(string szDomain, string szMsgId)
        {
            return gettext_NoOp(szMsgId);
        }

        public static string dngettext(string szDomain, string singular, string plural, int Number)
        {
#if DXGETTEXTDEBUG
            Debug.WriteLine("dngettext translation (domain " + szDomain + ", number is " + Number.ToString() + ") of " + singular + '/' + plural);
#endif
            string org = singular + '\0' + plural;
            string trans = dgettext(szDomain, org);
            int idx;
            if (org == trans)
            {
#if DXGETTEXTDEBUG
                Debug.WriteLine("Translation was equal to english version. English plural forms assumed.");
#endif
                idx = Utils.GetPluralForm2EN(Number);
            }
            else
                idx = curGetPluralForm(Number);
#if DXGETTEXTDEBUG
            Debug.WriteLine("Index " + idx.ToString() + " will be used");
#endif
            while (true)
            {
                int p = trans.IndexOf('\0');
                if (p == -1)
                {
#if DXGETTEXTDEBUG
                    Debug.WriteLine("Last translation used: " + trans);
#endif
                    return trans;
                }
                if (idx == 0)
                {
#if DXGETTEXTDEBUG
                    Debug.WriteLine("Translation found: " + trans);
#endif
                    return trans.Substring(0, p);
                }
                trans = trans.Substring(0, p + 1);
                --idx;
            }
        }

        public static string dngettext_NoExtract(string szDomain, string singular, string plural, int Number)
        {
            // This one is very useful for translating text in variables.
            // This can sometimes be necessary, and by using this function,
            // the source code scanner will not trigger warnings.
            return dngettext(szDomain, singular, plural, Number);
        }

        public static void textdomain(string szDomain)
        {
#if DXGETTEXTDEBUG
            Debug.WriteLine("Changed text domain to \"" + szDomain + '\"');
#endif
            curmsgdomain = szDomain;
            //WhenNewDomain(szDomain);
        }

        public static string getcurrenttextdomain()
        {
            return curmsgdomain;
        }

        public static void bindtextdomain(string szDomain, string szDirectory)
        {
            string dir;
            dir = Utils.IncludeTrailingPathDelimiter(szDirectory);
#if DXGETTEXTDEBUG
            Debug.WriteLine("Text domain '" + szDomain + "' is now located at '" + dir + "'");
#endif
            Getdomain(szDomain, DefaultDomainDirectory, curlang).Directory = dir;
            //WhenNewDomainDirectory(szDomain, szDirectory);
        }

        public static void bindtextdomainToFile(string szDomain, string filename) // Also works with files embedded in exe file
        {
#if DXGETTEXTDEBUG
            Debug.WriteLine("Text domain \"" + szDomain + "\" is now bound to file named \"" + filename + '"');
#endif
            Getdomain(szDomain, DefaultDomainDirectory, curlang).SetFilename(filename);
        }

        public static void DebugLogToFile(string filename, Boolean append = false)
        {
#if DXGETTEXTDEBUG
            FileMode mode = FileMode.Open;
            // Create the file if needed
            if (!File.Exists(filename) || !append)
                mode = FileMode.Create;

            // Open file
            FileStream fs = new FileStream(filename, mode, FileAccess.Write, FileShare.Read | FileShare.Delete);
            StreamWriter writer = new StreamWriter(fs, Encoding.Default);
            if (append)
                fs.Seek(0, SeekOrigin.End);
            string sLineBreak = Environment.NewLine;
            // Write header if appending
            if (fs.Position != 0)
            {
                string marker = sLineBreak + "===========================================================================" + sLineBreak;
                writer.Write(marker);
            }

            // Copy the memorystream contents to the file
            if (DebugLog.Writer != null)
            {
                DebugLog.Writer.BaseStream.Seek(0, SeekOrigin.Begin);
                DebugLog.Writer.BaseStream.CopyTo(fs);

                DebugLog.Writer.Dispose();
            }
            // Make DebugLog point to the filestream
            DebugLog.Writer = writer;
#endif
        }

        public static void DebugLogPause(Boolean PauseEnabled)
        {
#if DXGETTEXTDEBUG
            DebugLogOutputPaused = PauseEnabled;
#endif
        }

        public static event TOnDebugLine OnDebugLine { add { fOnDebugLine += value; } remove { fOnDebugLine -= value; } } // If set, all debug output goes here

        public static void RegisterWhenNewLanguageListener(IGnuGettextInstanceWhenNewLanguageListener Listener)
        {
            fWhenNewLanguageListeners.Add(Listener);
        }
        public static void UnregisterWhenNewLanguageListener(IGnuGettextInstanceWhenNewLanguageListener Listener)
        {
            fWhenNewLanguageListeners.Remove(Listener);
        }

        internal static void TranslateStrings(TStringList sl, string TextDomain)
        {
            string line;
            TStringList tempSL;
#if dx_StringList_has_OwnsObjects
            TStringList slAsTStringList;
            bool originalOwnsObjects;
#endif //dx_StringList_has_OwnsObjects
            if (sl.Count > 0)
            {
#if dx_StringList_has_OwnsObjects
    // From D2009 onward, the TStringList class has an OwnsObjects property, just like
    // TObjectList has. This means that if we call Clear on the given
    // list in the sl parameter, we could destroy the objects it contains.
    // To avoid this we must disable OwnsObjects while we replace the strings, but
    // only if sl is a TStringList instance and if using Delphi 2009 or later.
    originalOwnsObjects = false; // avoid warning
    if (sl is TStringList)
      slAsTStringList = sl as TStringList;
    else
      slAsTStringList = null;
#endif //dx_StringList_has_OwnsObjects

                sl.BeginUpdate();
                try
                {
                    tempSL = new TStringList();
                    try
                    {
                        // don't use Assign here as it will propagate the Sorted property (among others)
                        // in versions of Delphi from Delphi XE onward
                        tempSL.AddStrings(sl);

                        for (int i = 0; i < tempSL.Count; i++)
                        {
                            line = tempSL.Strings[i];
                            if (line != "")
                                if (TextDomain == "")
                                    tempSL.Strings[i] = Utils.ComponentGettext(line);
                                else
                                    tempSL.Strings[i] = dgettext(TextDomain, line);
                        }

                        //DH Fix 2013-09-19: Only refill sl if changed
                        if (sl.Text != tempSL.Text)
                        {
#if dx_StringList_has_OwnsObjects
          if (slAsTStringList!=null) {
            originalOwnsObjects = slAsTStringList.OwnsObjects;
            slAsTStringList.OwnsObjects = false;
          }
#endif //dx_StringList_has_OwnsObjects
                            try
                            {
#if dx_StringList_has_OwnsObjects
            if (slAsTStringList!=null && slAsTStringList.Sorted)
            {
              // TStringList doesn't release the objects in PutObject, so we use this to get
              // sl.Clear to not destroy the objects in classes that inherit from TStringList
              // but do a ClearObject in Clear.
              //
              // todo: Check whether this should be
              //   if sl is TStringList then
              // instead.
              if (sl.GetType() != typeof(TStringList)
                for (int I = 0;I< sl.Count;I++)
                  sl.Objects[I] = null;

              // same here, we don't use assign because we don't want to modify the properties of the orignal string list
              sl.Clear();
              sl.AddStrings(tempSL);
            }
            else
#endif //dx_StringList_has_OwnsObjects
                                {
                                    for (int i = 0; i < sl.Count; i++)
                                        sl[i] = tempSL[i];
                                }
                            }
                            finally
                            {
#if dx_StringList_has_OwnsObjects
            if (slAsTStringList!=null)
              slAsTStringList.OwnsObjects = originalOwnsObjects;
#endif //dx_StringList_has_OwnsObjects
                            }
                        }
                    }
                    finally
                    {
                        tempSL.Clear();
                    }
                }
                finally
                {
                    sl.EndUpdate();
                }
            }
        }

        // Override to know when language changes
        internal static void WhenNewLanguage(string LanguageID)
        {
            for (int I = 0; I < fWhenNewLanguageListeners.Count; I++)
                (fWhenNewLanguageListeners[I] as IGnuGettextInstanceWhenNewLanguageListener).WhenNewLanguage(LanguageID);
        }

        // Override to know when text domain changes. Directory is purely informational
        //internal static void WhenNewDomain(string TextDomain)
        //{
        //}

        // Override to know when any text domain's directory changes. It won't be called if a domain is fixed to a specific file.
        //internal static void WhenNewDomainDirectory(string TextDomain, string Directory)
        //{
        //}

        private static TExecutable TP_CreateRetranslator()
        {
            var Result = TP_Retranslator = new TTP_Retranslator();
#if DXGETTEXTDEBUG
            Debug.WriteLine("A retranslator was created.");
#endif
            return Result;
        }

        private static void FreeTP_ClassHandlingItems()
        {
            TP_ClassHandling.Clear();
        }

        private static bool ClassIsIgnored(Type AClass)
        {
            TClassMode cm;
            for (int i = 0; i < TP_GlobalClassHandling.Count; i++)
            {
                cm = TP_GlobalClassHandling[i];
                if (AClass.InheritsFrom(cm.HClass) && cm.PropertiesToIgnore.Count == 0)
                {
                    return true;
                }
            }
            for (int i = 0; i < TP_ClassHandling.Count; i++)
            {
                cm = TP_ClassHandling[i];
                if (AClass.InheritsFrom(cm.HClass))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool InterfaceIsIgnored(Type AInterface)
        {
            TClassMode cm;
            for (int i = 0; i < TP_InterfaceHandling.Count; i++)
            {
                cm = TP_InterfaceHandling[i];
                if (AInterface.ImplementsInterface(cm.HClass) && cm.PropertiesToIgnore.Count == 0)
                {
                    return true;
                }
            }
            return false;
        }

        // Translates a single property of an object
        private static void TranslateProperty(object AnObject, PropertyInfo PropInfo, TStringList TodoList, string TextDomain)
        {
            string Propname = PropInfo.Name;
            try
            {
                // Translate certain types of properties
                switch (Type.GetTypeCode(PropInfo.PropertyType))
                {
                    // All dfm files returning tkUString
                    case TypeCode.String:
                        {
#if DXGETTEXTDEBUG
                            Debug.WriteLine("Translating " + AnObject.GetType().Name + "." + Propname);
#endif
                            string old = PropInfo.GetValue(AnObject, null) as string;
#if DXGETTEXTDEBUG
                            if (string.IsNullOrEmpty(old))
                                Debug.WriteLine("(Empty, not translated)");
                            else
                                Debug.WriteLine("Old value: \"" + old + "\"");
#endif
                            if (!string.IsNullOrEmpty(old) && Utils.IsWriteProp(PropInfo))
                            {
                                if (TP_Retranslator != null)
                                    ((TTP_Retranslator)TP_Retranslator).Remember(AnObject, Propname, old);
                                string ws;
                                if (TextDomain == "")
                                    ws = Utils.ComponentGettext(old);
                                else
                                    ws = dgettext(TextDomain, old);
                                if (ws != old)
                                {
                                    PropertyInfo ppi = AnObject.GetType().GetProperty(Propname);
                                    if (ppi != null)
                                    {
                                        ppi.SetValue(AnObject, ws, null);
                                    }
                                    else
                                    {
#if DXGETTEXTDEBUG
                                        Debug.WriteLine("ERROR: Property disappeared: " + Propname + " for object of type " + AnObject.GetType().Name);
#endif
                                    }
                                }
                            }
                        }// { case item }
                        break;
                    case TypeCode.Object:
                        {
                            Object obj;
                            try
                            {
                                obj = PropInfo.GetValue(AnObject, null); // possibly throws exceptions
                            }
                            catch
                            {
                                obj = null;
                            }
                            if (obj != null)
                            {
                                if (obj is Component)
                                {
                                    IComponent compmarker = ((Component)obj).FindComponent("GNUgettextMarker");
                                    if (compmarker != null)
                                        return;
                                }
                                TodoList.AddObject("", obj);
                            }
                        }// { case item }
                        break;
                } //{ case };
            }
            catch (Exception E)
            {
                throw new EGGComponentError("Property cannot be translated." + Environment.NewLine +
                  "Add TP_GlobalIgnoreClassProperty(" + AnObject.GetType().Name + ",''" + Propname + "'') to your source code or use" + Environment.NewLine +
                  "TP_Ignore (self,''." + Propname + "'') to prevent this message." + Environment.NewLine +
                  "Reason: " + E.Message);
            }
        }
        private static TDomain Getdomain(string domain, string DefaultDomainDirectory, string CurLang)
        {
            // Retrieves the TDomain object for the specified domain.
            // Creates one, if none there, yet.
            int idx;
            TDomain Result;
            idx = domainlist.IndexOf(domain);
            if (idx == -1)
            {
                Result = new TDomain();
#if DXGETTEXTDEBUG
                Result.DebugLogger = s => Debug.WriteLine(s);
#endif
                Result.Domain = domain;
                Result.Directory = DefaultDomainDirectory;
                Result.SetLanguageCode(curlang);
                domainlist.AddObject(domain, Result);
            }
            else
            {
                Result = (TDomain)domainlist.Objects[idx];
            }
            return Result;
        }
    }
}
