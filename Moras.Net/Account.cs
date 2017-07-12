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
using System.Linq;
using System.Text;
using DelphiClasses;
using dxgettext;
using System.Reflection;

namespace Moras.Net
{
    //---------------------------------------------------------------------------
    // Diese Klasse verwaltet die Accounts
    public struct SAccount
    {
        public String Name;
        public int nPlayers;	// Anzahl der Character in dem Account
        public DynamicArray<CPlayer> arPlayers;
    };

    public class Account
    {
        private static string _(string szMsgId) { return TGnuGettextInstance.gettext(szMsgId); }

        public Account()
        {   // Es gibt nen default account für "Keinen Account". Den nun initialisieren
            nAccounts = 1;
            arAccounts.Length = 1;
            arAccounts.Array[0].Name = _("<keiner>");
            arAccounts.Array[0].nPlayers = 0;
        }
        //---------------------------------------------------------------------------

        ~Account()
        {
            for (int i = 0; i < nAccounts; i++)
            {
                for (int j = 0; j < arAccounts[i].nPlayers; j++)
                {
                    arAccounts.Array[i].arPlayers[j] = null;
                }
                arAccounts.Array[i].arPlayers.Length = 0;
            }
            arAccounts.Length = 0;
        }

        // Im Moment nicht mehr machen, als den default-Player erstellen und die Adresse zurück geben
        public CPlayer Init()
        {
            return NewPlayer();	// Den default-Player erzeugen
        }
        // Erstelle einen neuen Account-Eintrag, wenn es ihn noch nicht gibt
        // Gib die Id dann auch zurück
        public int NewAccount(String Name)
        {
            for (int i = 1; i < nAccounts; i++)
            {
                if (arAccounts[i].Name.Equals(Name, StringComparison.CurrentCultureIgnoreCase)) return i;
            }
            arAccounts.Length++;
            arAccounts.Array[nAccounts].Name = Name;
            arAccounts.Array[nAccounts].nPlayers = 0;
            nAccounts++;
            return nAccounts - 1;
        }

        // Erstelle einen neuen Spieler Eintrag
        // Wird erstmal dem default-Account zugewiesen
        public CPlayer NewPlayer()
        {
            CPlayer pl = new CPlayer();
            if (pl != null)
            {
                nAllPlayers++;
                arAccounts.Array[0].arPlayers.Length++;
                arAccounts.Array[0].arPlayers[arAccounts[0].nPlayers] = pl;
                arAccounts.Array[0].nPlayers++;
                pl.Init();
            }
            return pl;
        }

        private String GetAccountName(int account)
        {
            if ((account >= 0) && (account < nAccounts))
                return arAccounts[account].Name;
            else
                return "";
        }

        private int GetNPlayers(int account)
        {
            if ((account >= 0) && (account < nAccounts))
                return arAccounts[account].nPlayers;
            else
                return 0;
        }

        private CPlayer GetPlayer(int account, int number)
        {
            if ((account >= 0) && (account < nAccounts) && (number >= 0) && (number < arAccounts[account].nPlayers))
                return arAccounts[account].arPlayers[number];
            else
                return null;
        }

        // Gibt die id des übergebenen Account-namen zurück.
        // id 0 ist kein Account
        public int GetAccountId(String Name)
        {
            for (int i = 1; i < nAccounts; i++)
            {
                if (arAccounts[i].Name.Equals(Name, StringComparison.CurrentCultureIgnoreCase)) return i;
            }
            return 0;
        }

        // Alle Spieler nachschauen, ob sie beim richtigen Account zugeordnet sind
        // Wenn nicht, dann machen :)
        public void AssignAccounts()
        {
            for (int a = 0; a < nAccounts; a++)
            {
                for (int p = 0; p < arAccounts[a].nPlayers; p++)
                {
                    int aa = arAccounts[a].arPlayers[p].Account;
                    if (aa != a)
                    {	// Zuerst dem richtigen Account hinzurechnen
                        arAccounts.Array[aa].arPlayers.Length++;
                        arAccounts.Array[aa].arPlayers[arAccounts[aa].nPlayers] = arAccounts[a].arPlayers[p];
                        arAccounts.Array[aa].nPlayers++;
                        // Und nun beim aktuellen entfernen
                        --arAccounts.Array[a].arPlayers.Length;
                        --arAccounts.Array[a].nPlayers;
                    }
                }
            }
        }

        //public int Load(String Filename);
        //public int Save(String Filename);

        private DynamicArray<SAccount> arAccounts;
        private int nAccounts;
        private int nAllPlayers;

        #region Properties
        public int NAccounts { get { return nAccounts; } }
        public int AllPlayers { get { return nAllPlayers; } }
        private IndexerProperty<String, int> _name;
        public IndexerProperty<String, int> Name { get { return _name ?? (_name = new IndexerProperty<String, int> { read = GetAccountName }); } }
        private IndexerProperty<int, int> _nPlayers;
        public IndexerProperty<int, int> NPlayers { get { return _nPlayers ?? (_nPlayers = new IndexerProperty<int, int> { read = GetNPlayers }); } }
        private IndexerProperty<CPlayer, int, int> _player;
        public IndexerProperty<CPlayer, int, int> Player { get { return _player ?? (_player = new IndexerProperty<CPlayer, int, int> { read = GetPlayer }); } }
        #endregion
    }
}
