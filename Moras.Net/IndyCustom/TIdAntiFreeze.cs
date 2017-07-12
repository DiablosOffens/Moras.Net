using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.Sockets;
using System.ComponentModel;
using System.Windows.Forms;

namespace Moras.Net.IndyCustom
{
    [ToolboxItem(true)]
    [DesignTimeVisible(true)]
    [Designer("Moras.Net.Design.IndyComponentDesigner, Moras.Net.Design, Version=1.0.0.0, Culture=neutral, PublicKeyToken=f9135b47417b1285")]
    public class TIdAntiFreeze : AntiFreezeBase
    {
        public override void Process()
        {
            //TODO: Handle ApplicationHasPriority
            Application.DoEvents();
        }

        public TIdAntiFreeze()
            : base(new InitializerComponent())
        {

        }

        [Browsable(true), DefaultValue(true)]
        public new bool Active { get { return base.Active; } set { base.Active = value; } }
        [Browsable(true), DefaultValue(true)]
        public new bool ApplicationHasPriority { get { return base.ApplicationHasPriority; } set { base.ApplicationHasPriority = value; } }
        [Browsable(true)]
        public new int IdleTimeOut { get { return base.IdleTimeOut; } set { base.IdleTimeOut = value; } }
        [Browsable(true), DefaultValue(true)]
        public new bool OnlyWhenIdle { get { return base.OnlyWhenIdle; } set { base.OnlyWhenIdle = value; } }
    }
}
