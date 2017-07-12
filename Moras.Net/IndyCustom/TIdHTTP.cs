using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.Sockets;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Moras.Net.IndyCustom
{
    //HINT: Delphi produces IL code with modopts, but C# doesn't understand them:
    //https://blogs.msdn.microsoft.com/samng/2008/03/05/optional-modifiers-and-overload-resolution/
    [ToolboxItem(true)]
    [DesignTimeVisible(true)]
    [Designer("Moras.Net.Design.IndyComponentDesigner, Moras.Net.Design, Version=1.0.0.0, Culture=neutral, PublicKeyToken=f9135b47417b1285")]
    public class TIdHTTP : CustomHTTP
    {
        protected internal IndyMaxLineAction FMaxLineAction;
        protected internal IndyMaxLineAction GetMaxLineAction()
        {
            if (IOHandler != null)
                return IOHandler.MaxLineAction;
            return FMaxLineAction;
        }

        protected internal void SetMaxLineAction([In]IndyMaxLineAction AValue)
        {
            FMaxLineAction = AValue;
            if (IOHandler != null)
                IOHandler.MaxLineAction = AValue;
        }

        protected override void SetIOHandler(IOStream AValue)
        {
            base.SetIOHandler(AValue);
            if (IOHandler != null)
                IOHandler.MaxLineAction = FMaxLineAction;
        }

        public TIdHTTP()
            : base(new InitializerComponent())
        {

        }

        #region events
        public new event TIdOnAuthorization OnAuthorization
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            add
            {
                base.OnAuthorization += value;
            }
            [MethodImpl(MethodImplOptions.Synchronized)]
            remove
            {
                base.OnAuthorization -= value;
            }
        }

        public new event TIdHTTPOnHeadersAvailable OnHeadersAvailable
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            add
            {
                base.OnHeadersAvailable += value;
            }
            [MethodImpl(MethodImplOptions.Synchronized)]
            remove
            {
                base.OnHeadersAvailable -= value;
            }
        }

        public new event TIdOnAuthorization OnProxyAuthorization
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            add
            {
                base.OnProxyAuthorization += value;
            }
            [MethodImpl(MethodImplOptions.Synchronized)]
            remove
            {
                base.OnProxyAuthorization -= value;
            }
        }

        public new event TIdHTTPOnRedirectEvent OnRedirect
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            add
            {
                base.OnRedirect += value;
            }
            [MethodImpl(MethodImplOptions.Synchronized)]
            remove
            {
                base.OnRedirect -= value;
            }
        }

        public new event TIdOnSelectAuthorization OnSelectAuthorization
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            add
            {
                base.OnSelectAuthorization += value;
            }
            [MethodImpl(MethodImplOptions.Synchronized)]
            remove
            {
                base.OnSelectAuthorization -= value;
            }
        }

        public new event TIdOnSelectAuthorization OnSelectProxyAuthorization
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            add
            {
                base.OnSelectProxyAuthorization += value;
            }
            [MethodImpl(MethodImplOptions.Synchronized)]
            remove
            {
                base.OnSelectProxyAuthorization -= value;
            }
        }
        #endregion

        [Browsable(true)]
        public new bool AllowCookies { get { return base.AllowCookies; } set { base.AllowCookies = value; } }
        [Browsable(true)]
        public new ZLibCompressorBase Compressor { get { return base.Compressor; } set { base.Compressor = value; } }
        [Browsable(true)]
        public new CookieJar CookieManager { get { return base.CookieManager; } set { base.CookieManager = value; } }
        [Browsable(true), DefaultValue((short)0x0)]
        public new bool HandleRedirects { get { return base.HandleRedirects; } set { base.HandleRedirects = value; } }
        [Browsable(true)]
        public new HTTPOptions HTTPOptions { get { return base.HTTPOptions; } set { base.HTTPOptions = value; } }
        [Browsable(true), DefaultValue(0x3)]
        public new int MaxAuthRetries { get { return base.MaxAuthRetries; } set { base.MaxAuthRetries = value; } }
        [Browsable(true), DefaultValue((short)0x1)]
        public new HTTPProtocolVersion ProtocolVersion { get { return base.ProtocolVersion; } set { base.ProtocolVersion = value; } }
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [TypeConverter(typeof(NestedPropertiesTypeConverter<ProxyConnectionInfo>))]
        public new ProxyConnectionInfo ProxyParams { get { return base.ProxyParams; } set { base.ProxyParams = value; } }
        [Browsable(true), DefaultValue(0xf)]
        public new int RedirectMaximum { get { return base.RedirectMaximum; } set { base.RedirectMaximum = value; } }
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [TypeConverter(typeof(NestedPropertiesTypeConverter<HTTPRequest>))]
        public new HTTPRequest Request { get { return base.Request; } set { base.Request = value; } }
        [Browsable(true)]
        public IndyMaxLineAction MaxLineAction
        {
            get { return GetMaxLineAction(); }
            [param: In]
            set { SetMaxLineAction(value); }
        }
        [Browsable(true)]
        public new int ReadTimeout { get { return base.ReadTimeout; } set { base.ReadTimeout = value; } }
    }

    internal class NestedPropertiesTypeConverter<T> : TypeConverter
    {
        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            return TypeDescriptor.GetProperties(typeof(T), attributes);
        }

        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        {
            return true;
        }
    }
}
