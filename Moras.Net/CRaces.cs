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
using System.Linq;
using System.Text;
using DelphiClasses;
using dxgettext;

namespace Moras.Net
{

    /*public struct SRace : INeedsInitialization
    {
        public string id;
        public string Name;
        public int[] arResists { get; private set; }

        public void Init()
        {
            arResists = new int[9];
        }
    }*/

    public class CRaces
    {
        private static string _(string szMsgId) { return TGnuGettextInstance.gettext(szMsgId); }

        private DynamicArray<SRace> arRaces;	// Das Array mit den Rassendaten
        private int curRace;	// ID der aktuellen Rasse
        private int nRaces;		// Anzahl der gespeicherten Rassen
        //---------------------------------------------------------------------------

        public CRaces()
        {
            arRaces.Length = 1;
            nRaces = 1;
            arRaces.Array[0].id = "NONE";
            arRaces.Array[0].Name = _("<keine>");
            curRace = 0;
            for (int i = 0; i < 9; i++)
                arRaces[0].arResists[i] = 0;
        }

        /*~CRaces()
        {
        }*/

        public int NewRace(string strID)
        {
            int i;
            curRace = 0;
            // Testen, ob es die Id nicht doch schon gibt
            for (i = 0; i < nRaces; i++)
            {
                if (arRaces[i].id == strID) curRace = i;
            }
            if (curRace == 0)
            {
                curRace = nRaces;
                nRaces++;
                arRaces.Length = nRaces;
                arRaces.Array[curRace].id = strID;
                arRaces.Array[curRace].Name = "!Kein Rassenname!";
                for (i = 0; i < 9; i++)
                    arRaces[curRace].arResists[i] = 0;
            }
            return curRace;
        }

        public void SetName(string strName)
        {
            if (curRace > 0)
                arRaces.Array[curRace].Name = strName;
        }

        public void SetResistance(int idResistance, int Value)
        {
            if ((curRace > 0) && (Value >= 0))
                arRaces[curRace].arResists[Value] = Value;
        }

        public int GetRaceId(string strId)
        {	// liefert die id des übergebenen Rassen-Strings
            // 0 wenn nicht gefunden
            curRace = 0;
            for (int i = 0; i < nRaces; i++)
            {
                if (arRaces[i].id == strId)
                {
                    curRace = i;
                    break;
                }
            }
            return curRace;
        }
    }
}
