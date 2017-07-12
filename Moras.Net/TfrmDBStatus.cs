/*****************************************************************************/
/*                                                                           */
/* Moras Ausrüstungsplaner für Dark Age of Camelot                           */
/* Copyright (C) 2003 - 2004  Mora                                           */
/*                                                                           */
/* This program is free software; you can redistribute it and/or modify      */
/* it under the terms of the GNU General Public License as published by      */
/* the Free Software Foundation; either version 2 of the License, or         */
/* (at your option) any later version.                                       */
/*                                                                           */
/* This program is distributed in the hope that it will be useful,           */
/* but WITHOUT ANY WARRANTY; without even the implied warranty of            */
/* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the             */
/* GNU General Public License for more details.                              */
/*                                                                           */
/* You should have received a copy of the GNU General Public License         */
/* along with this program; if not, write to the Free Software               */
/* Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA */
/*                                                                           */
/*****************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DelphiClasses;
using dxgettext;
using System.IO;

namespace Moras.Net
{
    public partial class TfrmDBStatus : TCustomForm
    {
        private static string _(string szMsgId) { return TGnuGettextInstance.gettext(szMsgId); }
        protected override IContainer GetChildContainer() { return components ?? base.GetChildContainer(); }
        //---------------------------------------------------------------------------
        public TfrmDBStatus()
        {
            InitializeComponent();
            ((Bitmap)btClose.Image).MakeTransparent();
            ((Bitmap)btVacuum.Image).MakeTransparent();
            ZQuery.Connection = Unit.frmMain.ZConnection;
        }
        //---------------------------------------------------------------------------
        private void TfrmDBStatus_FormCreate(object sender, EventArgs e)
        {
            TGnuGettextInstance.TranslateComponent(this);
            string sql = "select alb.cnt as alb, hib.cnt as hib, mid.cnt as mid, arte.cnt as arte, alle.cnt as alle";
            sql = sql + " from (select count(*) as cnt from items where realm = 1) as alb,";
            sql = sql + " (select count(*) as cnt from items where realm = 2) as hib,";
            sql = sql + " (select count(*) as cnt from items where realm = 4) as mid,";
            sql = sql + " (select count(*) as cnt from items where maxlevel > 0) as arte,";
            sql = sql + " (select count(*) as cnt from items) as alle";
            ZQuery.Close();
            ZQuery.CommandText = sql;
            ZQuery.Open();
            lbAlbion.Text = ZQuery.FieldByName("alb").AsString;
            lbHibernia.Text = ZQuery.FieldByName("hib").AsString;
            lbMidgard.Text = ZQuery.FieldByName("mid").AsString;
            lbArtifacts.Text = ZQuery.FieldByName("arte").AsString;
            lbSum.Text = ZQuery.FieldByName("alle").AsString;

            lbDBVersion.Text = SQLiteUtils.SQLiteDBVersion().ToString();
            lbDBVersion.SetHint(ZQuery.Connection.GetDataSource());
            int size = (int)(new FileInfo(ZQuery.Connection.GetDataSource()).Length / 1024.0);
            lbDBSize.Text = string.Format("{0:N0} KByte", size);
            lbSysLang.Text = Unit.xml_config.sysLanguage;
        }
        //---------------------------------------------------------------------------
        private void btVacuumClick(object sender, EventArgs e)
        {
            Unit.frmMain.acDBVacuum.OnExecute(EventArgs.Empty);
        }
        //---------------------------------------------------------------------------
    }

    static partial class Unit
    {
        internal static TfrmDBStatus frmDBStatus;
    }
}
