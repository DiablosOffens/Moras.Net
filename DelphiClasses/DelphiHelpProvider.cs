using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;

namespace DelphiClasses
{
    [ProvideProperty("HelpContext", typeof(IComponent)),
    ProvideProperty("HelpString", typeof(IComponent)),
    ProvideProperty("HelpKeyword", typeof(IComponent)),
    ProvideProperty("HelpNavigator", typeof(IComponent)),
    ProvideProperty("ShowHelp", typeof(IComponent))]
    public class DelphiHelpProvider : HelpProvider
    {
        private Hashtable boundControls = new Hashtable();
        private Hashtable boundEvents = new Hashtable();
        private Hashtable bindOwners = new Hashtable();
        private Hashtable contexts = new Hashtable();
        private Hashtable helpStrings = new Hashtable();
        private Hashtable keywords = new Hashtable();
        private Hashtable navigators = new Hashtable();
        private Hashtable showHelp = new Hashtable();
        private bool useFrameworkHelpSystem;
        private bool inControlHelp;

        [Localizable(true), DefaultValue(false),
        Description("Determines wether the original .Net-Framework Help system or the customizable delphi implementation is used.")]
        public bool UseFrameworkHelpSystem
        {
            get { return useFrameworkHelpSystem; }
            set { useFrameworkHelpSystem = value; }
        }

        public override bool CanExtend(object target)
        {
            return (target is ToolStripItem) || base.CanExtend(target);
        }

        public override string GetHelpKeyword(Control ctl)
        {
            string keyword = base.GetHelpKeyword(ctl);
            if (!string.IsNullOrEmpty(keyword))
                return keyword;

            int ctxt = GetHelpContext(ctl);
            return ctxt == -1 ? null : ctxt.ToString();
        }

        public override bool GetShowHelp(Control ctl)
        {
            if (useFrameworkHelpSystem || inControlHelp)
                return base.GetShowHelp(ctl);
            return false;
        }

        [DefaultValue(-1), Localizable(true),
        Description("Determines the Help context (Topic-Id) associated with this control.")]
        public virtual int GetHelpContext(IComponent comp)
        {
            object ctxt = contexts[comp];
            return ctxt == null ? -1 : (int)ctxt;
        }

        [DefaultValue(null), Localizable(true),
        Description("")]
        public virtual string GetHelpKeyword(IComponent comp)
        {
            Control ctl = comp as Control;
            if (ctl != null)
            {
                string keyword = base.GetHelpKeyword(ctl);
                if (!string.IsNullOrEmpty(keyword))
                    return keyword;
            }
            else
            {
                string keyword = (string)keywords[comp];
                if (!string.IsNullOrEmpty(keyword))
                    return keyword;
            }
            int ctxt = GetHelpContext(comp);
            return ctxt == -1 ? null : ctxt.ToString();
        }

        [DefaultValue(HelpNavigator.AssociateIndex), Localizable(true),
        Description("")]
        public virtual HelpNavigator GetHelpNavigator(IComponent comp)
        {
            Control ctl = comp as Control;
            if (ctl != null)
                return base.GetHelpNavigator(ctl);
            object nav = navigators[comp];
            return (nav == null) ? HelpNavigator.AssociateIndex : (HelpNavigator)nav;
        }

        [DefaultValue(null), Localizable(true),
        Description("")]
        public virtual string GetHelpString(IComponent comp)
        {
            Control ctl = comp as Control;
            if (ctl != null)
                return base.GetHelpString(ctl);
            return (string)helpStrings[comp];
        }

        [Localizable(true),
        Description("")]
        public virtual bool GetShowHelp(IComponent comp)
        {
            Control ctl = comp as Control;
            if (ctl != null)
            {
                if (useFrameworkHelpSystem || inControlHelp || base.DesignMode)
                    return base.GetShowHelp(ctl);
                return false;
            }
            else
            {
                object b = showHelp[comp];
                if (b == null)
                {
                    return false;
                }
                else
                {
                    return (Boolean)b;
                }
            }
        }

        private void ShowHelp(Control ctl, string helpFile, HelpNavigator nav, string keyword, int context)
        {
            HelpIntfs.ISetActiveControl setControl = TApplication.Instance.HelpSystem as HelpIntfs.ISetActiveControl;
            if (setControl != null)
                setControl.SetActiveControl(ctl);
            try
            {
                switch (nav)
                {
                    case HelpNavigator.Find:
                        HelpIntfs.IHelpSystem3 searchHelp = TApplication.Instance.HelpSystem as HelpIntfs.IHelpSystem3;
                        if (searchHelp != null)
                            searchHelp.ShowSearch(keyword, helpFile);
                        break;
                    case HelpNavigator.AssociateIndex:
                    case HelpNavigator.Index:
                        HelpIntfs.IHelpSystem3 indexHelp = TApplication.Instance.HelpSystem as HelpIntfs.IHelpSystem3;
                        if (indexHelp != null)
                            indexHelp.ShowIndex(keyword, helpFile);
                        break;
                    case HelpNavigator.KeywordIndex:
                        TApplication.Instance.HelpSystem.ShowHelp(keyword, helpFile);
                        break;
                    case HelpNavigator.TableOfContents:
                        TApplication.Instance.HelpSystem.ShowTableOfContents();
                        break;
                    case HelpNavigator.Topic:
                        TApplication.Instance.HelpSystem.ShowTopicHelp(keyword, helpFile);
                        break;
                    case HelpNavigator.TopicId:
                        TApplication.Instance.HelpSystem.ShowContextHelp(context, helpFile);
                        break;
                    default:
                        break;
                }
            }
            finally
            {
                if (setControl != null)
                    setControl.SetActiveControl(null);
            }
        }

        private void OnControlHelp(object sender, HelpEventArgs hevent)
        {
            if (useFrameworkHelpSystem)
                return;

            inControlHelp = true;
            try
            {
                Control ctl = sender as Control;
                string helpString;
                string keyword;
                int context;
                HelpNavigator navigator;
                bool show;
                if (ctl != null)
                {
                    helpString = GetHelpString(ctl);
                    keyword = GetHelpKeyword(ctl);
                    context = GetHelpContext(ctl);
                    navigator = GetHelpNavigator(ctl);
                    show = GetShowHelp(ctl);
                }
                else
                {
                    ToolStripItem item = (ToolStripItem)sender;
                    ctl = item.Owner;
                    helpString = GetHelpString(item);
                    keyword = GetHelpKeyword(item);
                    context = GetHelpContext(item);
                    navigator = GetHelpNavigator(item);
                    show = GetShowHelp(item);
                }

                if (!show)
                {
                    return;
                }

                // If the mouse was down, we first try whats this help
                //
                if (Control.MouseButtons != MouseButtons.None && helpString != null)
                {
                    Debug.WriteLine("HelpProvider:: Mouse down w/ helpstring");

                    if (helpString.Length > 0)
                    {
                        ShowHelp(ctl, null, HelpNavigator.Topic, helpString, context);
                        hevent.Handled = true;
                    }
                }

                // If we have a help file, and help keyword we try F1 help next
                //
                if (!hevent.Handled)
                {
                    Debug.WriteLine("HelpProvider:: F1 help");
                    if (keyword != null)
                    {
                        if (keyword.Length > 0)
                        {
                            ShowHelp(ctl, HelpNamespace, navigator, keyword, context);
                            hevent.Handled = true;
                        }
                    }

                    if (!hevent.Handled)
                    {
                        ShowHelp(ctl, HelpNamespace, navigator, null, context);
                        hevent.Handled = true;
                    }
                }

                // So at this point we don't have a help keyword, so try to display
                // the whats this help
                //
                if (!hevent.Handled && helpString != null)
                {
                    Debug.WriteLine("HelpProvider:: back to helpstring");

                    if (helpString.Length > 0)
                    {
                        ShowHelp(ctl, null, HelpNavigator.Topic, helpString, context);
                        hevent.Handled = true;
                    }
                }

                // As a last resort, just popup the contents page of the help file...
                //
                if (!hevent.Handled)
                {
                    Debug.WriteLine("HelpProvider:: contents");

                    ShowHelp(ctl, HelpNamespace, HelpNavigator.TableOfContents, null, -1);
                    hevent.Handled = true;
                }
            }
            finally
            {
                inControlHelp = false;
            }
        }

        private void OnQueryAccessibilityHelp(object sender, QueryAccessibilityHelpEventArgs e)
        {
            ToolStripItem item = (ToolStripItem)sender;

            e.HelpString = GetHelpString(item);
            e.HelpKeyword = GetHelpKeyword(item);
            e.HelpNamespace = HelpNamespace;
        }

        public override void SetHelpKeyword(Control ctl, string keyword)
        {
            base.SetHelpKeyword(ctl, keyword);
            UpdateEventBinding(ctl);
        }

        public override void SetHelpNavigator(Control ctl, HelpNavigator navigator)
        {
            base.SetHelpNavigator(ctl, navigator);
            UpdateEventBinding(ctl);
        }

        public override void SetHelpString(Control ctl, string helpString)
        {
            base.SetHelpString(ctl, helpString);
            UpdateEventBinding(ctl);
        }

        public override void SetShowHelp(Control ctl, bool value)
        {
            base.SetShowHelp(ctl, value);
            UpdateEventBinding(ctl);
        }

        public virtual void SetHelpContext(IComponent comp, int context)
        {
            contexts[comp] = context;
            SetShowHelp(comp, true);
            Control ctl = comp as Control;
            if (ctl != null)
                UpdateEventBinding(ctl);
            else
                UpdateEventBinding((ToolStripItem)comp);
        }

        public virtual void SetHelpString(IComponent comp, string helpString)
        {
            Control ctl = comp as Control;
            if (ctl != null)
            {
                base.SetHelpString(ctl, helpString);
                UpdateEventBinding(ctl);
            }
            else
            {
                helpStrings[comp] = helpString;
                if (helpString != null)
                {
                    if (helpString.Length > 0)
                    {
                        SetShowHelp(comp, true);
                    }
                }
                UpdateEventBinding((ToolStripItem)comp);
            }
        }

        public virtual void SetHelpKeyword(IComponent comp, string keyword)
        {
            Control ctl = comp as Control;
            if (ctl != null)
            {
                base.SetHelpKeyword(ctl, keyword);
                UpdateEventBinding(ctl);
            }
            else
            {
                keywords[comp] = keyword;
                if (keyword != null)
                {
                    if (keyword.Length > 0)
                    {
                        SetShowHelp(comp, true);
                    }
                }
                UpdateEventBinding((ToolStripItem)comp);
            }
        }

        public virtual void SetHelpNavigator(IComponent comp, HelpNavigator navigator)
        {
            Control ctl = comp as Control;
            if (ctl != null)
            {
                base.SetHelpNavigator(ctl, navigator);
                UpdateEventBinding(ctl);
            }
            else
            {
                //valid values are 0x80000001 to 0x80000007
                if ((int)navigator < (int)HelpNavigator.Topic || (int)navigator > (int)HelpNavigator.TopicId)
                {
                    //validate the HelpNavigator enum
                    throw new InvalidEnumArgumentException("navigator", (int)navigator, typeof(HelpNavigator));
                }

                navigators[comp] = navigator;
                SetShowHelp(comp, true);
                UpdateEventBinding((ToolStripItem)comp);
            }
        }
        public virtual void SetShowHelp(IComponent comp, bool value)
        {
            Control ctl = comp as Control;
            if (ctl != null)
            {
                base.SetShowHelp(ctl, value);
                UpdateEventBinding(ctl);
            }
            else
            {
                showHelp[comp] = value;
                UpdateEventBinding((ToolStripItem)comp);
            }
        }

        internal virtual bool ShouldSerializeShowHelp(IComponent comp)
        {
            Control ctl = comp as Control;
            if (ctl != null)
                return base.GetShowHelp(ctl);
            return showHelp.ContainsKey(comp);
        }

        public virtual void ResetShowHelp(IComponent comp)
        {
            Control ctl = comp as Control;
            if (ctl != null)
                base.ResetShowHelp(ctl);
            else
                showHelp.Remove(comp);
        }

        private void UpdateEventBinding(Control ctl)
        {
            if (GetShowHelp(ctl) && !boundControls.ContainsKey(ctl))
            {
                ctl.HelpRequested += new HelpEventHandler(OnControlHelp);
                boundControls[ctl] = ctl;
            }
            else if (!GetShowHelp(ctl) && boundControls.ContainsKey(ctl))
            {
                ctl.HelpRequested -= new HelpEventHandler(OnControlHelp);
                boundControls.Remove(ctl);
            }
        }

        private void UpdateEventBinding(ToolStripItem item)
        {
            if (GetShowHelp(item) && !boundControls.ContainsKey(item))
            {
                if (bindOwners.ContainsKey(item))
                    return; //it's already on the way

                ToolStrip owner = item.Owner;
                if (owner != null)
                    OnOwnerChanged(item, EventArgs.Empty);
                else
                {
                    item.OwnerChanged += new EventHandler(OnOwnerChanged);
                    bindOwners[item] = item;
                }
            }
            else if (!GetShowHelp(item) && boundControls.ContainsKey(item))
            {
                ToolStrip owner = (ToolStrip)boundControls[item];
                HelpEventHandler handler = (HelpEventHandler)boundEvents[item];
                owner.HelpRequested -= handler;
                item.QueryAccessibilityHelp -= new QueryAccessibilityHelpEventHandler(OnQueryAccessibilityHelp);
                boundControls.Remove(item);
                boundEvents.Remove(item);
            }
        }

        private void OnOwnerChanged(object sender, EventArgs e)
        {
            ToolStripItem item = (ToolStripItem)sender;
            ToolStrip owner = item.Owner;
            HelpEventHandler handler = new HelpEventHandler((s, e2) => OnControlHelp(item, e2));
            owner.HelpRequested += handler;
            item.QueryAccessibilityHelp += new QueryAccessibilityHelpEventHandler(OnQueryAccessibilityHelp);
            item.OwnerChanged -= new EventHandler(OnOwnerChanged);
            boundControls[item] = owner;
            boundEvents[item] = handler;
            bindOwners.Remove(item);
        }
    }
}
