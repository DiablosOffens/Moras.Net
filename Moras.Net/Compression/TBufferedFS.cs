using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Moras.Net.Compression
{
    internal static class UBufferedFS
    {
        public const int BufferSize = 0x10000;

        internal class TBufferedFS : FileStream
        {

            public TBufferedFS(string path, FileMode mode, FileAccess access, FileShare share)
                : base(path, mode, access, share, BufferSize)
            { }
        }
    }
}
