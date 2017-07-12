using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.Serialization;

namespace DelphiClasses.HelpIntfs
{
    public class EHelpSystemException : Exception
    {
        public EHelpSystemException()
        {
        }

        public EHelpSystemException(string message)
            : base(message)
        {
        }

        public EHelpSystemException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public EHelpSystemException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }

    public static class Unit
    {
        private class THelpSystem : IHelpSystem3, IHelpManager, ISetActiveControl
        {
            private IHelpSelector helpSelector;
            private List<ICustomHelpViewer> helpViewers = new List<ICustomHelpViewer>();
            private Control activeControl;

            #region IHelpSystem3 Members

            public string GetFilter()
            {
                throw new NotImplementedException();
            }

            public void SetFilter(string Filter)
            {
                throw new NotImplementedException();
            }

            public void ShowIndex(string Topic, string HelpFileName)
            {
                throw new NotImplementedException();
            }

            public void ShowSearch(string Topic, string HelpFileName)
            {
                throw new NotImplementedException();
            }

            public void ShowTopicHelp(string Topic, string Anchor, string HelpFileName)
            {
                throw new NotImplementedException();
            }

            #endregion

            #region IHelpSystem2 Members

            public bool UnderstandsKeyword(string HelpKeyword, string HelpFileName)
            {
                throw new NotImplementedException();
            }

            #endregion

            #region IHelpSystem Members

            public void AssignHelpSelector(IHelpSelector Selector)
            {
                helpSelector = Selector;
            }

            public bool Hook(UIntPtr Handle, string HelpFile, ushort Comand, IntPtr Data)
            {
                foreach (var viewer in helpViewers)
                {
                    if (viewer is ISpecialWinHelpViewer &&
                        ((ISpecialWinHelpViewer)viewer).CallWinHelp(Handle, HelpFile, Comand, Data.ToUIntPtr()))
                        return true;
                }
                return false;
            }

            public void ShowContextHelp(int ContextID, string HelpFileName)
            {
                bool foundViewer = false;
                foreach (var viewer in helpViewers)
                {
                    IExtendedHelpViewer extviewer = viewer as IExtendedHelpViewer;
                    if (extviewer != null)
                    {
                        foundViewer = true;
                        if (extviewer.UnderstandsContext(ContextID, HelpFileName))
                        {
                            extviewer.DisplayHelpByContext(ContextID, HelpFileName);
                            return;
                        }
                    }
                }
                if (!foundViewer)
                    throw new EHelpSystemException("No help viewer found that implements IExtendedHelpViewer.");
            }

            public void ShowHelp(string HelpKeyword, string HelpFileName)
            {
                List<ICustomHelpViewer> providesHelp = new List<ICustomHelpViewer>();
                foreach (var viewer in helpViewers)
                {
                    int topics = viewer.UnderstandsKeyword(HelpKeyword);
                    if (topics > 0)
                        providesHelp.Add(viewer);
                }

                if (providesHelp.Count == 0)
                    return;

                if (helpSelector == null || providesHelp.Count == 1)
                {
                    providesHelp[0].ShowHelp(HelpKeyword);
                }
                else
                {
                    TStringList keywords = new TStringList();
                    keywords.Duplicates = TDuplicates.dupIgnore;
                    foreach (var viewer in providesHelp)
                    {
                        TStringList temp = new TStringList(viewer.GetHelpStrings(HelpKeyword));
                        int count = temp.Count;
                        for (int i = 0; i < count; i++)
                        {
                            temp.Objects[i] = viewer;
                        }
                        keywords.AddStrings(temp);
                    }
                    int index = helpSelector.SelectKeyword(keywords);
                    if (index < 0)
                        return;
                    string helpstring = keywords[index];
                    ICustomHelpViewer selectedViewer = (ICustomHelpViewer)keywords.Objects[index];
                    selectedViewer.ShowHelp(helpstring);
                }
            }

            public void ShowTableOfContents()
            {
                List<ICustomHelpViewer> providesHelp = new List<ICustomHelpViewer>();
                foreach (var viewer in helpViewers)
                {
                    if (viewer.CanShowTableOfContents())
                        providesHelp.Add(viewer);
                }

                if (providesHelp.Count == 0)
                    return;

                if (helpSelector == null || providesHelp.Count == 1)
                {
                    providesHelp[0].ShowTableOfContents();
                }
                else
                {
                    TStringList viewerNames = new TStringList();
                    viewerNames.Duplicates = TDuplicates.dupIgnore;
                    foreach (var viewer in providesHelp)
                    {
                        viewerNames.AddObject(viewer.GetViewerName(), viewer);
                    }
                    int index = helpSelector.TableOfContents(viewerNames);
                    if (index < 0)
                        return;
                    ICustomHelpViewer selectedViewer = (ICustomHelpViewer)viewerNames.Objects[index];
                    selectedViewer.ShowTableOfContents();
                }
            }

            public void ShowTopicHelp(string Topic, string HelpFileName)
            {
                bool foundViewer = false;
                foreach (var viewer in helpViewers)
                {
                    IExtendedHelpViewer extviewer = viewer as IExtendedHelpViewer;
                    if (extviewer != null)
                    {
                        foundViewer = true;
                        if (extviewer.UnderstandsTopic(Topic))
                        {
                            extviewer.DisplayTopic(Topic);
                            return;
                        }
                    }
                }
                if (!foundViewer)
                    throw new EHelpSystemException("No help viewer found that implements IExtendedHelpViewer.");
            }

            #endregion

            #region IHelpManager Members

            public UIntPtr GetHandle()
            {
                Control ctl = GetActiveControl();
                if (ctl == null)
                    return UIntPtr.Zero;
                return ctl.Handle.ToUIntPtr();
            }

            public string GetHelpFile()
            {
                Control ctl = GetActiveControl();
                if (ctl == null)
                    return "";
                string filename;
                ctl.AccessibilityObject.GetHelpTopic(out filename);
                return filename ?? "";
            }

            public void Release(int ViewerID)
            {
                if (ViewerID < 1 || ViewerID > helpViewers.Count)
                    throw new ArgumentOutOfRangeException("ViewerID");
                helpViewers.RemoveAt(ViewerID - 1);
                for (int i = ViewerID - 1; i < helpViewers.Count; i++)
                {
                    helpViewers[i].NotifyID(i + 1);
                }
            }

            #endregion

            internal void AddViewer(ICustomHelpViewer newViewer)
            {
                helpViewers.Add(newViewer);
                newViewer.NotifyID(helpViewers.Count);
            }

            private Control GetActiveControl()
            {
                if (activeControl != null)
                    return activeControl;

                Form form = Form.ActiveForm;
                if (form == null)
                    return null;
                return form.ActiveControl;
            }

            #region ISetActiveControl Members

            void ISetActiveControl.SetActiveControl(Control ctl)
            {
                activeControl = ctl;
            }

            #endregion
        }
        private static THelpSystem HelpSystem = new THelpSystem();

        public static Boolean GetHelpSystem(out IHelpSystem System)
        {
            System = HelpSystem;
            return true;
        }

        public static Boolean GetHelpSystem(out IHelpSystem2 System)
        {
            System = HelpSystem;
            return true;
        }

        public static Boolean GetHelpSystem(out IHelpSystem3 System)
        {
            System = HelpSystem;
            return true;
        }

        public static int RegisterViewer(ICustomHelpViewer newViewer, out IHelpManager Manager)
        {
            try
            {
                HelpSystem.AddViewer(newViewer);
            }
            catch
            {
                Manager = null;
                return 0;
            }
            Manager = HelpSystem;
            return 1;
        }
    }
}
