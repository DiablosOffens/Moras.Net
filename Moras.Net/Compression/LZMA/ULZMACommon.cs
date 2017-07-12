using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Moras.Net.Compression.LZMA
{
    // unit
    internal static class ULZMACommon
    {
        public enum TLZMAProgressAction { LPAMax, LPAPos };
        public delegate void TLZMAProgress(TLZMAProgressAction Action, Int64 Value);

        public const int CodeProgressInterval = 50;//approx. number of times an OnProgress event will be fired during coding

        public static byte ReadByte(Stream stream)
        {
            int result = stream.ReadByte();
            if (result == -1)
                throw new EndOfStreamException();
            return (byte)result;
        }

        public static void WriteByte(Stream stream, byte b)
        {
            stream.WriteByte(b);
        }
    }
}
