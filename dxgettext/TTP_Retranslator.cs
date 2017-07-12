using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using System.Reflection;
using DelphiClasses;
using System.Diagnostics;

namespace dxgettext
{
    public class TTP_Retranslator : TExecutable
    {
        private class TTP_RetranslatorItem
        {
            public WeakReference obj;
            public string Propname;
            public string OldValue;

            ~TTP_RetranslatorItem()
            {
                if (obj.IsAlive)
                    RemoveFromKnowRetranslators(obj.Target);
            }
        }

        private static List<TTP_Retranslator> KnownRetranslators = new List<TTP_Retranslator>();
        private readonly static PropertyInfo ValuesProp = typeof(ConditionalWeakTable<,>).GetProperty("Values", BindingFlags.Instance | BindingFlags.NonPublic);

        private ConditionalWeakTable<object, List<TTP_RetranslatorItem>> list;
        public string TextDomain;

        public TTP_Retranslator()
        {
            list = new ConditionalWeakTable<object, List<TTP_RetranslatorItem>>();
            KnownRetranslators.Add(this);
        }

        ~TTP_Retranslator()
        {
            list = null;
            if (KnownRetranslators != null)
                KnownRetranslators.Remove(this);
        }

        private static void RemoveFromKnowRetranslators(object obj)
        {
            foreach (var retranslator in KnownRetranslators)
            {
                retranslator.list.Remove(obj);
                //for (int itemindex = 0; itemindex < retranslator.list.Count; )
                //{
                //    var item = retranslator.list[itemindex];
                //    if (!item.obj.IsAlive || item.obj.Target == obj)
                //        retranslator.list.RemoveAt(itemindex);
                //    else
                //        itemindex++;
                //}
            }
        }

        public void Remember(Object obj, string PropName, string OldValue)
        {
            List<TTP_RetranslatorItem> value = list.GetValue(obj, k => new List<TTP_RetranslatorItem>());
            var item = new TTP_RetranslatorItem();
            item.obj = new WeakReference(obj);
            item.Propname = PropName;
            item.OldValue = OldValue;
            value.Add(item);
        }

        public override void Execute()
        {
            ICollection<List<TTP_RetranslatorItem>> items = (ICollection<List<TTP_RetranslatorItem>>)ValuesProp.GetValue(list, null);
            foreach (var item in items.Flatten())
            {
                object obj = item.obj.IsAlive ? item.obj.Target : null;
                if (obj == null)
                    continue;
                if (obj is Component)
                {
                    var comp = ((Component)obj).FindComponent("GNUgettextMarker") as TGnuGettextComponentMarker;
                    if (comp != null && this != comp.Retranslator)
                    {
                        comp.Retranslator.Execute();
                        continue;
                    }
                }
                if (obj is TStringList)
                {
                    // Since we don't know the order of items in sl, and don't have
                    // the original .Objects[] anywhere, we cannot anticipate anything
                    // about the current sl.Strings[] and sl.Objects[] values. We therefore
                    // have to discard both values. We can, however, set the original .Strings[]
                    // value into the list and retranslate that.
                    var sl = new TStringList();
                    try
                    {
                        sl.Text = item.OldValue;
                        TGnuGettextInstance.TranslateStrings(sl, TextDomain);
                        ((TStringList)obj).BeginUpdate();
                        try
                        {
                            ((TStringList)obj).Text = sl.Text;
                        }
                        finally
                        {
                            ((TStringList)obj).EndUpdate();
                        }
                    }
                    finally
                    {
                        sl.Clear();
                        sl = null;
                    }
                }
                else
                {
                    string newValue;
                    if (TextDomain == "" || TextDomain == TGnuGettextInstance.DefaultTextDomain)
                        newValue = Utils.ComponentGettext(item.OldValue);
                    else
                        newValue = TGnuGettextInstance.dgettext(TextDomain, item.OldValue);
                    var ppi = obj.GetType().GetProperty(item.Propname);
                    if (ppi != null)
                    {
                        ppi.SetValue(obj, newValue, null);
                    }
                    else
                    {
#if DXGETTEXTDEBUG
                        Debug.WriteLine("ERROR: On retranslation, property disappeared: " + item.Propname + " for object of type " + obj.GetType().Name);
#endif
                    }
                }
            }
        }
    }
}
