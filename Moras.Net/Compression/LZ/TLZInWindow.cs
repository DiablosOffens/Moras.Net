using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Moras.Net.Compression.LZ
{
    internal static class ULZInWindow
    {
        internal class TLZInWindow
        {
            public byte[] bufferBase;// pointer to buffer with data
            public Stream stream;
            public int posLimit; // offset (from _buffer) of first byte when new block reading must be done
            public bool streamEndWasReached; // if (true) then _streamPos shows real end of stream

            public int pointerToLastSafePosition;

            public int bufferOffset;

            public int blockSize;  // Size of Allocated memory block
            public int pos;             // offset (from _buffer) of curent byte
            public int keepSizeBefore;  // how many BYTEs must be kept in buffer before _pos
            public int keepSizeAfter;   // how many BYTEs must be kept buffer after _pos
            public int streamPos;   // offset (from _buffer) of first not read byte from Stream

            public void MoveBlock()
            {
                int offset, numBytes, i;
                offset = bufferOffset + pos - keepSizeBefore;
                // we need one additional byte, since MovePos moves on 1 byte.
                if (offset > 0)
                    offset--;

                numBytes = bufferOffset + streamPos - offset;

                // check negative offset ????
                for (i = 0; i < numBytes; i++)
                    bufferBase[i] = bufferBase[offset + i];
                bufferOffset = bufferOffset - offset;
            }

            public void ReadBlock()
            {
                int size, numReadBytes, pointerToPostion;
                if (streamEndWasReached)
                    return;
                while (true)
                {
                    size = (0 - bufferOffset) + blockSize - streamPos;
                    if (size == 0)
                        return;
                    numReadBytes = stream.Read(bufferBase, bufferOffset + streamPos, size);
                    if (numReadBytes == 0)
                    {
                        posLimit = streamPos;
                        pointerToPostion = bufferOffset + posLimit;
                        if (pointerToPostion > pointerToLastSafePosition)
                            posLimit = pointerToLastSafePosition - bufferOffset;
                        streamEndWasReached = true;
                        return;
                    }
                    streamPos = streamPos + numReadBytes;
                    if (streamPos >= pos + keepSizeAfter)
                        posLimit = streamPos - keepSizeAfter;
                }
            }

            public void _Free()
            {
                bufferBase = null;
            }

            public virtual void _Create(int keepSizeBefore, int keepSizeAfter, int keepSizeReserv)
            {
                int blockSize;
                this.keepSizeBefore = keepSizeBefore;
                this.keepSizeAfter = keepSizeAfter;
                blockSize = keepSizeBefore + keepSizeAfter + keepSizeReserv;
                if (bufferBase == null || bufferBase.Length == 0 || this.blockSize != blockSize)
                {
                    _Free();
                    this.blockSize = blockSize;
                    bufferBase = new byte[this.blockSize];
                }
                pointerToLastSafePosition = this.blockSize - keepSizeAfter;
            }

            public void SetStream(Stream stream)
            {
                this.stream = stream;
            }

            public void ReleaseStream()
            {
                stream = null;
            }

            public virtual void Init()
            {
                bufferOffset = 0;
                pos = 0;
                streamPos = 0;
                streamEndWasReached = false;
                ReadBlock();
            }

            public virtual void MovePos()
            {
                int pointerToPostion;
                pos++;
                if (pos > posLimit)
                {
                    pointerToPostion = bufferOffset + pos;
                    if (pointerToPostion > pointerToLastSafePosition)
                        MoveBlock();
                    ReadBlock();
                }
            }

            public byte GetIndexByte(int index)
            {
                return bufferBase[bufferOffset + pos + index];
            }

            // index + limit have not to exceed _keepSizeAfter;
            public int GetMatchLen(int index, int distance, int limit)
            {
                int pby, i;
                if (streamEndWasReached)
                    if ((pos + index) + limit > streamPos)
                        limit = streamPos - (pos + index);
                distance++;
                // Byte *pby = _buffer + (size_t)_pos + index;
                pby = bufferOffset + pos + index;

                i = 0;
                while ((i < limit) && (bufferBase[pby + i] == bufferBase[pby + i - distance]))
                {
                    i++;
                }
                return i;
            }

            public int GetNumAvailableBytes()
            {
                return streamPos - pos;
            }

            public void ReduceOffsets(int subValue)
            {
                bufferOffset = bufferOffset + subValue;
                posLimit = posLimit - subValue;
                pos = pos - subValue;
                streamPos = streamPos - subValue;
            }
        }
    }
}