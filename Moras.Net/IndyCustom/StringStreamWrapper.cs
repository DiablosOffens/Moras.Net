using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Indy.Sockets;
using Borland.Vcl;

namespace Moras.Net.IndyCustom
{
    public class IStringStreamWrapper : StreamReader
    {
        public IStringStreamWrapper(TStringStream stream)
            : base(stream, System.Text.Encoding.Default)
        {

        }

        public IStringStreamWrapper(string value)
            : base(new MemoryStream(), System.Text.Encoding.Unicode)
        {
            byte[] Bytes = System.Text.Encoding.Unicode.GetBytes(value);
            BaseStream.Write(Bytes, 0, (Bytes != null) ? Bytes.Length : 0x0);
            BaseStream.Position = 0;
        }
    }
}
