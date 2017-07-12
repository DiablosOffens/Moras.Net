using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Moras.Net.Compression.LZ
{
    // unit
    internal static class ULZOutWindow
    {
        internal class TLZOutWindow
        {
            public byte[] buffer;
            public int pos;
            public int windowSize;
            public int streamPos;
            public Stream stream;

            public void _Create(int windowSize)
            {
                if (buffer == null || buffer.Length == 0 || this.windowSize != windowSize)
                    buffer = new byte[windowSize];
                this.windowSize = windowSize;
                pos = 0;
                streamPos = 0;
            }

            public void SetStream(Stream stream)
            {
                ReleaseStream();
                this.stream = stream;
            }

            public void ReleaseStream()
            {
                Flush();
                this.stream = null;
            }

            public void Init(bool solid)
            {
                if (!solid)
                {
                    streamPos = 0;
                    pos = 0;
                }
            }

            public void Flush()
            {
                int size;
                size = pos - streamPos;
                if (size == 0)
                    return;
                stream.Write(buffer, streamPos, size);
                if (pos >= windowSize)
                    pos = 0;
                streamPos = pos;
            }

            public void CopyBlock(int distance, int len)
            {
                int pos;
                pos = this.pos - distance - 1;
                if (pos < 0)
                    pos = pos + windowSize;
                while (len != 0)
                {
                    if (pos >= windowSize)
                        pos = 0;
                    buffer[this.pos] = buffer[pos];
                    this.pos++;
                    pos++;
                    if (this.pos >= windowSize)
                        Flush();
                    len--;
                }
            }

            public void PutByte(byte b)
            {
                buffer[pos] = b;
                pos++;
                if (pos >= windowSize)
                    Flush();
            }

            public byte GetByte(int distance)
            {
                int pos;
                pos = this.pos - distance - 1;
                if (pos < 0)
                    pos = pos + windowSize;
                return buffer[pos];
            }
        }
    }
}