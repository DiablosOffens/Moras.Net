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

//---------------------------------------------------------------------------

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

namespace Moras.Net
{
    public partial class TfrmOverWrite : TCustomForm
    {
        private static string _(string szMsgId) { return TGnuGettextInstance.gettext(szMsgId); }

        protected override IContainer GetChildContainer() { return components ?? base.GetChildContainer(); }

        public bool AnswerToAll { get; private set; }

        public TfrmOverWrite()
        {
            InitializeComponent();
        }

        private void Button1Click(object sender, EventArgs e)
        {
            AnswerToAll = true;
            switch (cbAllOptions.SelectedIndex)
            {
                case 0: DialogResult = DialogResult.Yes;
                    break;
                case 1: DialogResult = DialogResult.No;
                    break;
                default: DialogResult = DialogResult.Cancel;
                    break;
            }
        }

        private void TfrmOverWrite_FormCreate(object sender, EventArgs e)
        {
            TGnuGettextInstance.TranslateComponent(this);
            cbAllOptions.SelectedIndex = 0;
        }

        private void FormResize(object sender, EventArgs e)
        {
            GroupBox1.Width = (int)((Width / 2) - (Width - ClientWidth));
        }
    }

    static partial class Unit
    {
        internal static TfrmOverWrite frmOverWrite;
    }
}
