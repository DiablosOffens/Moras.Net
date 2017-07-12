//*****************************************************************************/
//*                                                                           */
//* Moras Ausrüstungsplaner für Dark Age of Camelot                           */
//* Copyright (C) 2003 - 2004  Mora                                           */
//*                                                                           */
//* This program is free software; you can redistribute it and/or modify      */
//* it under the terms of the GNU General Public License as published by      */
//* the Free Software Foundation; either version 2 of the License, or         */
//* (at your option) any later version.                                       */
//*                                                                           */
//* This program is distributed in the hope that it will be useful,           */
//* but WITHOUT ANY WARRANTY; without even the implied warranty of            */
//* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the             */
//* GNU General Public License for more details.                              */
//*                                                                           */
//* You should have received a copy of the GNU General Public License         */
//* along with this program; if not, write to the Free Software               */
//* Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA */
//*                                                                           */
//*****************************************************************************/

namespace Moras.Net.Wikihelp
{


    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using DelphiClasses;
    using DelphiClasses.WinHelpViewer;
    using WinHelpViewer = DelphiClasses.WinHelpViewer.Unit;
    using DelphiClasses.HelpIntfs;
    using HelpIntfs = DelphiClasses.HelpIntfs.Unit;
    using System.Runtime.CompilerServices;
    using dxgettext;
    using System.Diagnostics;

    // use this as global module in this namespace
    internal static partial class Unit
    {
        #region Implementation


        private static void TestHelp()
        {
        }

        private class TWikiHelpTester : IWinHelpTester
        {
            public TWikiHelpTester()
                : base()
            {
            }

            //~TWikiHelpTester()
            //{
            //}

            #region IWinHelpTester Members
            public bool CanShowALink(string ALink, string FileName)
            {
                return false;
            }

            public bool CanShowTopic(string Topic, string FileName)
            {
                return false;
            }

            public bool CanShowContext(int Context, string FileName)
            {
                return false;
            }

            public TStringList GetHelpStrings(string ALink)
            {
                return null;
            }

            public string GetHelpPath()
            {
                return "";
            }

            public string GetDefaultHelpFile()
            {
                return "";
            }

            #endregion
        }

        private static string ViewerName = "WikiHelp";

        private class TWikiHelpViewer : ICustomHelpViewer, IExtendedHelpViewer, ISpecialWinHelpViewer
        {
            private int FViewerID;
            public IHelpManager FHelpManager;

            public int ViewerID { get { return FViewerID; } }
            public IHelpManager HelpManager { get { return FHelpManager; } set { FHelpManager = value; } }



            public TWikiHelpViewer()
                : base()
            {
            }

            //~TWikiHelpViewer()
            //{
            //}

            public string HelpFile(string Name)
            {
                string FileName;
                if (Name == "" && FHelpManager != null)
                    FileName = HelpManager.GetHelpFile();
                else
                    FileName = Name;
                if (FileName == "")
                    FileName = TApplication.Instance.HelpFile;
                return FileName;
            }

            public void InternalShutDown()
            {
                SoftShutDown();
                if (FHelpManager != null)
                {
                    HelpManager.Release(ViewerID);
                    if (FHelpManager != null)
                        HelpManager = null;
                }
            }

            public string GetUrl()
            {
                return "http://" + TGnuGettextInstance.GetCurrentLanguage().Substring(0, 2) + '.' + HelpFile("") + "/index.php";
            }

            #region ICustomHelpViewer Members

            public string GetViewerName()
            {
                return ViewerName;
            }

            public int UnderstandsKeyword(string HelpString)
            {
                return 1;
            }


            public TStringList GetHelpStrings(string HelpString)
            {
                TStringList Result = new TStringList();
                Result.Add(GetViewerName() + ": " + HelpString);
                return Result;
            }

            public bool CanShowTableOfContents()
            {
                return true;
            }

            public void ShowTableOfContents()
            {
                string url;
                if (HelpFile("") != "")
                {
                    url = GetUrl();
                    Extensions.ShellExecute(TApplication.Instance.Handle, "open", url, "", "", ProcessWindowStyle.Normal);
                }
            }

            public void ShowHelp(string HelpString)
            {
                string url;
                if (HelpFile("") != "")
                {
                    url = GetUrl() + '/' + HelpString;
                    Extensions.ShellExecute(TApplication.Instance.Handle, "open", url, "", "", ProcessWindowStyle.Normal);
                }
            }

            public void NotifyID(int ViewerID)
            {
                FViewerID = ViewerID;
            }

            public void SoftShutDown()
            {
            }

            public void ShutDown()
            {
                SoftShutDown();
                if (FHelpManager != null)
                    HelpManager = null;
                if (WinHelpViewer.WinHelpTester != null)
                    WinHelpViewer.WinHelpTester = null;
            }

            #endregion
            #region IExtendedHelpViewer Members

            public bool UnderstandsTopic(string Topic)
            {
                return true;
            }

            public void DisplayTopic(string Topic)
            {
                string url;
                if (HelpFile("") != "")
                {
                    url = HelpFile("") + Topic;
                    Extensions.ShellExecute(TApplication.Instance.Handle, "open", url, "", "", ProcessWindowStyle.Normal);
                }
            }

            public bool UnderstandsContext(int ContextID,
                                        string HelpFileName)
            {
                return true;
            }

            public void DisplayHelpByContext(int ContextID, string HelpFileName)
            {
                string url;
                if (HelpFile("") != "")
                {
                    url = HelpFile("") + (ContextID).ToString();
                    Extensions.ShellExecute(TApplication.Instance.Handle, "open", url, "", "", ProcessWindowStyle.Normal);
                }
            }

            #endregion
            #region ISpecialWinHelpViewer Members

            public bool CallWinHelp(UIntPtr Handle, string HelpFile,
                                ushort Command, UIntPtr Data)
            {
                return false;
            }

            #endregion
        }

        private static TWikiHelpViewer WikiHelpViewer;
        private static TWikiHelpTester WikiHelpTester;


        private class TWikiHelpSelector : IHelpSelector
        {


            public TWikiHelpSelector()
                : base()
            {
            }

            //~TWikiHelpSelector()
            //{
            //}

            #region IHelpSelector Members
            public int SelectKeyword(TStringList Keywords)
            {
                return WikiHelpViewer.ViewerID - 1;
            }

            public int TableOfContents(TStringList Contents)
            {
                return WikiHelpViewer.ViewerID - 1;
            }

            #endregion
        }

        private static TWikiHelpSelector WikiHelpSelector;

        static Unit()
        {
            TApplication.AddFinalization(Finalization);
            try
            {
                WikiHelpViewer = new TWikiHelpViewer();
                WikiHelpSelector = new TWikiHelpSelector();
                WikiHelpTester = new TWikiHelpTester();
                HelpIntfs.RegisterViewer(WikiHelpViewer, out WikiHelpViewer.FHelpManager);
                //{Changing to HTML}
                WinHelpViewer.WinHelpTester = WikiHelpTester;
                TApplication.Instance.HelpSystem.AssignHelpSelector(WikiHelpSelector);
            }
            catch
            {
            }
            TestHelp();
        }

        static void Finalization()
        {
            if (WikiHelpViewer.FHelpManager != null)
                WikiHelpViewer.InternalShutDown();
            if (WikiHelpTester != null)
                WikiHelpTester = null;
            if (WikiHelpSelector != null)
                WikiHelpSelector = null;
        }
        #endregion
    }
}
