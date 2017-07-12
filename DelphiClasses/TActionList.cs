using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Forms;
using System.Drawing.Design;
using System.ComponentModel.Design;

namespace DelphiClasses
{
    public class ActionEventAgs : EventArgs
    {
        public TBasicAction Action { get; private set; }
        public bool Result { get; set; }

        public ActionEventAgs(TBasicAction action, bool result)
        {
            Action = action;
            Result = result;
        }
    }

    public enum TActionListState { asNormal, asSuspended, asSuspendedEnabled }

    [ProvideProperty("Action", typeof(Component))]
    [DefaultProperty("Actions")]
    [DefaultEvent("Execute")]
    public class TActionList : Component, IExtenderProvider
    {
        private static Dictionary<Component, TBasicActionLink> compToActionLinks = new Dictionary<Component, TBasicActionLink>();
        private TContainedActionCollection actions;
        private ImageList imageList;
        private TActionListState state;

        [Category("Appearance"), DefaultValue((string)null)]
        public ImageList ImageList
        {
            get { return imageList; }
            set
            {
                if (this.imageList != value)
                {
                    //TODO: subscribe also ChangeHandle, but it's internal
                    //EventHandler changeHandle = new EventHandler(this.ImageListChange);
                    EventHandler recreateHandle = new EventHandler(this.ImageListRecreateHandle);
                    if (this.imageList != null)
                    {
                        this.imageList.RecreateHandle -= recreateHandle;
                    }
                    this.imageList = value;
                    if (value != null)
                    {
                        value.RecreateHandle += recreateHandle;
                    }
                    OnChange(EventArgs.Empty);
                }
            }
        }

        [Category("Behavior"), DefaultValue(TActionListState.asNormal)]
        public TActionListState State { get { return state; } set { SetState(value); } }

        [DefaultValue(null)]
        public ToolTip ToolTipExtender { get; set; }
        [DefaultValue(null)]
        public HelpProvider HelpExtender { get; set; }

        [Category("Data")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [MergableProperty(false)]
        public TContainedActionCollection Actions { get { return actions; } }

        public event EventHandler Change;
        public event EventHandler<ActionEventAgs> Execute;
        public event EventHandler<ActionEventAgs> Update;

        public TActionList()
        {
            actions = new TContainedActionCollection(this);
        }

        public TActionList(IContainer cont)
            : this()
        {
            cont.Add(this);
        }

        private void ImageListRecreateHandle(object sender, EventArgs e)
        {
            if (sender == ImageList) OnChange(EventArgs.Empty);
        }

        private void ImageListChange(object sender, EventArgs e)
        {
            if (sender == ImageList) OnChange(EventArgs.Empty);
        }

        private void SetState(TActionListState value)
        {
            if (state == value) return;
            state = value;
            if (state == TActionListState.asSuspended) return;
            for (int i = 0; i < actions.Count; i++)
            {
                TCustomAction Action = actions[i] as TAction;
                if (Action is TCustomAction)
                {
                    switch (value)
                    {
                        case TActionListState.asNormal:
                            {
                                if (state == TActionListState.asSuspendedEnabled)
                                    Action.Enabled = Action.SavedEnabledState;
                                Action.OnUpdate(EventArgs.Empty);
                            } break;
                        case TActionListState.asSuspendedEnabled:
                            if (value == TActionListState.asSuspendedEnabled)
                            {
                                Action.SavedEnabledState = Action.Enabled;
                                Action.Enabled = true;
                            } break;
                    }
                }
            }
        }

        protected internal virtual void OnChange(EventArgs e)
        {
            if (Change != null)
                Change(this, e);

            foreach (var act in actions)
            {
                act.OnChange(e);
            }
        }

        public virtual bool ExecuteAction(TBasicAction action)
        {
            ActionEventAgs args = new ActionEventAgs(action, false);
            if (Execute != null)
                Execute(this, args);
            return args.Result;
        }

        public virtual bool UpdateAction(TBasicAction action)
        {
            ActionEventAgs args = new ActionEventAgs(action, false);
            if (Update != null)
                Update(this, args);
            return args.Result;
        }

        #region Extender Accessor Methods

        private TBasicActionLink CreateActionLink(Component client)
        {
            //insert other derived types above
            if (client is ToolStripMenuItem)
                return new TMenuActionLink(client, this);
            if (client is ToolStripControlHost)
                return new TToolControlActionLink(client, this);
            if (client is ToolStripButton)
                return new TToolButtonActionLink(client, this);
            if (client is ToolStripItem)
                return new TToolItemActionLink(client, this);
            if (client is Control)
                return new TControlActionLink(client, this);
            throw new NotSupportedException(string.Format("The type '{0}' is not supported by this action list.", client.GetType()));
        }

        internal static TBasicActionLink GetActionLink(Component self)
        {
            TBasicActionLink actionLink;
            lock (compToActionLinks) compToActionLinks.TryGetValue(self, out actionLink);
            return actionLink;
        }

        //https://msdn.microsoft.com/en-us/library/ms171834.aspx
        //https://msdn.microsoft.com/en-us/library/ms973818.aspx#custcodegen_topic4
        //http://www.codeproject.com/Articles/9220/Fixing-the-IExtenderProvider-in-Visual-Studio-s-AS
        //http://www.timvw.be/2007/08/21/bending-the-code-generation-of-iextenderprovider-to-your-will/
        [Category("Behavior"), DefaultValue((string)null), RefreshProperties(RefreshProperties.All)]
        public TBasicAction GetAction(Component self)
        {
            TBasicActionLink actionLink = GetActionLink(self);
            if (actionLink != null)
                return actionLink.Action;
            return null;
        }

        public void SetAction(Component self, TBasicAction value)
        {
            TBasicActionLink actionLink = GetActionLink(self);
            if (actionLink != null && actionLink.Action == value)
                return;

            if (value == null)
            {
                actionLink.Dispose();
                lock (compToActionLinks) compToActionLinks.Remove(self);
            }
            else
            {
                if (actionLink == null)
                {
                    actionLink = CreateActionLink(self);
                    lock (compToActionLinks) compToActionLinks.Add(self, actionLink);
                    actionLink.Change += (sender, e) => DoActionChange(self, sender, e);
                }
                actionLink.Action = value;

                bool isloading = false;
                ISite site = value.Site ?? self.Site;
                if (site != null)
                {
                    IDesignerHost designHost = site.GetService(typeof(IDesignerHost)) as IDesignerHost;
                    if (designHost != null)
                        isloading = designHost.Loading;
                }
                ActionChange(self, value, isloading);
            }
        }

        private void ActionChange(Component self, object sender, bool checkDefaults)
        {
            if (self is ButtonBase)
                ActionChangeButton((ButtonBase)self, sender, checkDefaults);
            else if (self is Control)
                ActionChangeControl((Control)self, sender, checkDefaults);
            else if (self is ToolStripButton)
                ActionChangeToolStripButton((ToolStripButton)self, sender, checkDefaults);
            else if (self is ToolStripMenuItem)
                ActionChangeToolStripMenuItem((ToolStripMenuItem)self, sender, checkDefaults);
            else if (self is ToolStripItem)
                ActionChangeToolStripItem((ToolStripItem)self, sender, checkDefaults);
        }

        private void ActionChangeControl(Control ctl, object sender, bool checkDefaults)
        {
            if (sender is TCustomAction)
            {
                TCustomAction action = (TCustomAction)sender;
                if (!checkDefaults ||
                 string.IsNullOrEmpty(ctl.Text) || ctl.Text == ctl.Name)
                    ctl.Text = action.Caption;
                if (!checkDefaults || ctl.Enabled)
                    ctl.SetShadowProperty(() => ctl.Enabled, action.Enabled);
                if (ToolTipExtender != null)
                {
                    if (!checkDefaults || string.IsNullOrEmpty(ToolTipExtender.GetToolTip(ctl)))
                        ToolTipExtender.SetToolTip(ctl, action.Hint);
                }
                if (!checkDefaults || ctl.Visible)
                    ctl.Visible = action.Visible;
                if (!checkDefaults || ctl.GetClickEvent() == null)
                {
                    ctl.Click += action.ExecuteDelegate;
                    //TBasicActionLink link = compToActionLinks[ctl];
                    //ctl.Click += (s, e) => link.OnExecute(ctl, e);
                }
                if (HelpExtender != null)
                {
                    DelphiHelpProvider helpext = HelpExtender as DelphiHelpProvider;
                    string helpkey = HelpExtender.GetHelpKeyword(ctl);
                    int helpcontext = helpext == null ? -1 : helpext.GetHelpContext(ctl);
                    HelpNavigator helpnav = HelpExtender.GetHelpNavigator(ctl);
                    int helpkeyctxt;
                    if (helpcontext != -1 && int.TryParse(helpkey, out helpkeyctxt) &&
                        helpkeyctxt == helpcontext)
                        helpkey = null;

                    if (!checkDefaults || helpcontext == -1)
                    {
                        if (helpext != null)
                            helpext.SetHelpContext(ctl, action.HelpContext);
                        else
                            HelpExtender.SetHelpKeyword(ctl, action.HelpContext.ToString());
                    }
                    if (!checkDefaults || helpkey == null)
                        HelpExtender.SetHelpKeyword(ctl, action.HelpKeyword);
                    if (!checkDefaults || helpnav == HelpNavigator.AssociateIndex)
                        HelpExtender.SetHelpNavigator(ctl, action.HelpType);
                }
            }
        }

        private void ActionChangeButton(ButtonBase btn, object sender, bool checkDefaults)
        {
            ActionChangeControl(btn, sender, checkDefaults);
            if (sender is TCustomAction)
            {
                TCustomAction action = (TCustomAction)sender;
                if (!checkDefaults || (btn.Image == null && this.ImageList != null &&
                    action.ImageIndex >= 0 && action.ImageIndex < this.ImageList.Images.Count))
                {
                    btn.ImageList = this.ImageList;
                    btn.ImageIndex = action.ImageIndex;
                }
            }
        }

        private void ActionChangeToolStripItem(ToolStripItem item, object sender, bool checkDefaults)
        {
            if (sender is TCustomAction)
            {
                TCustomAction action = (TCustomAction)sender;
                if (!checkDefaults ||
                 string.IsNullOrEmpty(item.Text) || item.Text == item.Name)
                    item.Text = action.Caption;
                if (!checkDefaults || item.Enabled)
                    item.Enabled = action.Enabled;
                if (!checkDefaults || string.IsNullOrEmpty(item.ToolTipText))
                    item.ToolTipText = action.Hint;
                if (!checkDefaults || item.Visible)
                    item.Visible = action.Visible;
                if (!checkDefaults || item.ImageIndex == -1)
                    item.ImageIndex = action.ImageIndex;
                if (!checkDefaults || item.GetClickEvent() == null)
                {
                    item.Click += action.ExecuteDelegate;
                    //TBasicActionLink link = compToActionLinks[item];
                    //item.Click += (s, e) => link.OnExecute(item, e);
                }

                DelphiHelpProvider helpext = HelpExtender as DelphiHelpProvider;
                if (helpext != null)
                {
                    string helpkey = helpext.GetHelpKeyword(item);
                    int helpcontext = helpext.GetHelpContext(item);
                    HelpNavigator helpnav = helpext.GetHelpNavigator(item);
                    int helpkeyctxt;
                    if (helpcontext != -1 && int.TryParse(helpkey, out helpkeyctxt) &&
                        helpkeyctxt == helpcontext)
                        helpkey = null;

                    if (!checkDefaults || helpcontext == -1)
                        helpext.SetHelpContext(item, action.HelpContext);
                    if (!checkDefaults || helpkey == null)
                        helpext.SetHelpKeyword(item, action.HelpKeyword);
                    if (!checkDefaults || helpnav == HelpNavigator.AssociateIndex)
                        helpext.SetHelpNavigator(item, action.HelpType);
                }
            }
        }

        private void ActionChangeToolStripButton(ToolStripButton button, object sender, bool checkDefaults)
        {
            ActionChangeToolStripItem(button, sender, checkDefaults);
            if (sender is TCustomAction)
            {
                TCustomAction action = (TCustomAction)sender;
                if (!checkDefaults || !button.Checked)
                    button.Checked = action.Checked;
            }
        }

        private void ActionChangeToolStripMenuItem(ToolStripMenuItem item, object sender, bool checkDefaults)
        {
            ActionChangeToolStripItem(item, sender, checkDefaults);
            if (sender is TCustomAction)
            {
                TCustomAction action = (TCustomAction)sender;
                if (!checkDefaults || !item.CheckOnClick)
                    item.CheckOnClick = action.AutoCheck;
                if (!checkDefaults || !item.Checked)
                    item.Checked = action.Checked;
                if (!checkDefaults || item.ShortcutKeys == Keys.None)
                    item.ShortcutKeys = action.ShortCut;
            }
        }

        private void DoActionChange(Component self, object sender, EventArgs e)
        {
            if (sender == GetAction(self))
                ActionChange(self, sender, false);
        }

        #endregion

        #region IExtenderProvider Members

        bool IExtenderProvider.CanExtend(object extendee)
        {
            return extendee is Component && !(extendee is TActionList) && !(extendee is TBasicAction);
        }

        #endregion
    }
}
