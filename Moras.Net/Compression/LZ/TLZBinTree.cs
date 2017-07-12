using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Moras.Net.Compression.LZ
{
    using TLZInWindow = ULZInWindow.TLZInWindow;
    // unit
    internal static class ULZBinTree
    {
        private const int kHash2Size = 1 << 10;
        private const int kHash3Size = 1 << 16;
        private const int kBT2HashSize = 1 << 16;
        private const int kStartMaxLen = 1;
        private const int kHash3Offset = kHash2Size;
        private const int kEmptyHashValue = 0;
        private const int kMaxValForNormalize = (1 << 30) - 1;
        private static int[] CRCTable = new int[256];

        internal class TLZBinTree : TLZInWindow
        {
            public int cyclicBufferPos;
            public int cyclicBufferSize;
            public int matchMaxLen;

            public int[] son;
            public int[] hash;

            public int cutValue;
            public int hashMask;
            public int hashSizeSum;

            public bool HASH_ARRAY;


            public int kNumHashDirectBytes;
            public int kMinMatchCheck;
            public int kFixHashSize;

            public TLZBinTree()
                : base()
            {
                cyclicBufferSize = 0;
                cutValue = 0xFF;
                hashSizeSum = 0;
                HASH_ARRAY = true;
                kNumHashDirectBytes = 0;
                kMinMatchCheck = 4;
                kFixHashSize = kHash2Size + kHash3Size;
            }

            public void SetType(int numHashBytes)
            {
                HASH_ARRAY = (numHashBytes > 2);
                if (HASH_ARRAY)
                {
                    kNumHashDirectBytes = 0;
                    kMinMatchCheck = 4;
                    kFixHashSize = kHash2Size + kHash3Size;
                }
                else
                {
                    kNumHashDirectBytes = 2;
                    kMinMatchCheck = 2 + 1;
                    kFixHashSize = 0;
                }
            }

            public override void Init()
            {
                base.Init();
                for (int i = 0; i < hashSizeSum; i++)
                    hash[i] = kEmptyHashValue;
                cyclicBufferPos = 0;
                ReduceOffsets(-1);
            }

            public override void MovePos()
            {
                cyclicBufferPos++;
                if (cyclicBufferPos >= cyclicBufferSize)
                    cyclicBufferPos = 0;
                base.MovePos();
                if (pos == kMaxValForNormalize)
                    Normalize();
            }

            [EditorBrowsable(EditorBrowsableState.Never)]
            public override void _Create(int keepSizeBefore, int keepSizeAfter, int keepSizeReserv)
            {
                base._Create(keepSizeBefore, keepSizeAfter, keepSizeReserv);
            }

            public bool _Create(int historySize, int keepAddBufferBefore, int matchMaxLen, int keepAddBufferAfter)
            {
                int windowReservSize;
                int cyclicBufferSize;
                int hs;
                if (historySize > kMaxValForNormalize - 256)
                {
                    return false;
                }
                cutValue = 16 + (matchMaxLen >> 1);

                windowReservSize = (historySize + keepAddBufferBefore + matchMaxLen + keepAddBufferAfter) / 2 + 256;

                base._Create(historySize + keepAddBufferBefore, matchMaxLen + keepAddBufferAfter, windowReservSize);

                this.matchMaxLen = matchMaxLen;

                cyclicBufferSize = historySize + 1;
                if (this.cyclicBufferSize != cyclicBufferSize)
                {
                    this.cyclicBufferSize = cyclicBufferSize;
                    son = new int[cyclicBufferSize * 2];
                }

                hs = kBT2HashSize;

                if (HASH_ARRAY)
                {
                    hs = historySize - 1;
                    hs = hs | (hs >> 1);
                    hs = hs | (hs >> 2);
                    hs = hs | (hs >> 4);
                    hs = hs | (hs >> 8);
                    hs = hs >> 1;
                    hs = hs | 0xFFFF;
                    if (hs > (1 << 24))
                        hs = hs >> 1;
                    hashMask = hs;
                    hs++;
                    hs = hs + kFixHashSize;
                }
                if (hs != hashSizeSum)
                {
                    hashSizeSum = hs;
                    hash = new int[hashSizeSum];
                }
                return true;
            }

            public int GetMatches(int[] distances)
            {
                int lenLimit;
                int offset, matchMinPos, cur, maxLen, hashValue, hash2Value, hash3Value;
                int temp, curMatch, curMatch2, curMatch3, ptr0, ptr1, len0, len1, count;
                int delta, cyclicPos, pby1, len;

                if (pos + matchMaxLen <= streamPos)
                    lenLimit = matchMaxLen;
                else
                {
                    lenLimit = streamPos - pos;
                    if (lenLimit < kMinMatchCheck)
                    {
                        MovePos();
                        return 0;
                    }
                }

                offset = 0;
                if (pos > cyclicBufferSize)
                    matchMinPos = (pos - cyclicBufferSize);
                else matchMinPos = 0;
                cur = bufferOffset + pos;
                maxLen = kStartMaxLen; // to avoid items for len < hashSize;
                hash2Value = 0;
                hash3Value = 0;

                if (HASH_ARRAY)
                {
                    temp = CRCTable[bufferBase[cur] & 0xFF] ^ (bufferBase[cur + 1] & 0xFF);
                    hash2Value = temp & (kHash2Size - 1);
                    temp = temp ^ ((bufferBase[cur + 2] & 0xFF) << 8);
                    hash3Value = temp & (kHash3Size - 1);
                    hashValue = (temp ^ (CRCTable[bufferBase[cur + 3] & 0xFF] << 5)) & hashMask;
                }
                else
                    hashValue = ((bufferBase[cur] & 0xFF) ^ ((bufferBase[cur + 1] & 0xFF) << 8));

                curMatch = hash[kFixHashSize + hashValue];
                if (HASH_ARRAY)
                {
                    curMatch2 = hash[hash2Value];
                    curMatch3 = hash[kHash3Offset + hash3Value];
                    hash[hash2Value] = pos;
                    hash[kHash3Offset + hash3Value] = pos;
                    if (curMatch2 > matchMinPos)
                        if (bufferBase[bufferOffset + curMatch2] == bufferBase[cur])
                        {
                            maxLen = 2;
                            distances[offset] = maxLen;
                            offset++;
                            distances[offset] = pos - curMatch2 - 1;
                            offset++;
                        }
                    if (curMatch3 > matchMinPos)
                        if (bufferBase[bufferOffset + curMatch3] == bufferBase[cur])
                        {
                            if (curMatch3 == curMatch2)
                                offset = offset - 2;
                            maxLen = 3;
                            distances[offset] = maxLen;
                            offset++;
                            distances[offset] = pos - curMatch3 - 1;
                            offset++;
                            curMatch2 = curMatch3;
                        }
                    if (offset != 0 && curMatch2 == curMatch)
                    {
                        offset = offset - 2;
                        maxLen = kStartMaxLen;
                    }
                }

                hash[kFixHashSize + hashValue] = pos;

                ptr0 = (cyclicBufferPos << 1) + 1;
                ptr1 = (cyclicBufferPos << 1);

                len0 = kNumHashDirectBytes;
                len1 = len0;

                if (kNumHashDirectBytes != 0)
                {
                    if (curMatch > matchMinPos)
                    {
                        if (bufferBase[bufferOffset + curMatch + kNumHashDirectBytes] != bufferBase[cur + kNumHashDirectBytes])
                        {
                            maxLen = kNumHashDirectBytes;
                            distances[offset] = maxLen;
                            offset++;
                            distances[offset] = pos - curMatch - 1;
                            offset++;
                        }
                    }
                }

                count = cutValue;

                while (true)
                {
                    if (curMatch <= matchMinPos || count == 0)
                    {
                        son[ptr1] = kEmptyHashValue;
                        son[ptr0] = son[ptr1];
                        break;
                    }
                    count--;
                    delta = pos - curMatch;
                    if (delta <= cyclicBufferPos)
                        cyclicPos = (cyclicBufferPos - delta) << 1;
                    else cyclicPos = (cyclicBufferPos - delta + cyclicBufferSize) << 1;

                    pby1 = bufferOffset + curMatch;
                    len = Math.Min(len0, len1);
                    if (bufferBase[pby1 + len] == bufferBase[cur + len])
                    {
                        len++;
                        while (len != lenLimit)
                        {
                            if (bufferBase[pby1 + len] != bufferBase[cur + len])
                                break;
                            len++;
                        }
                        if (maxLen < len)
                        {
                            maxLen = len;
                            distances[offset] = maxLen;
                            offset++;
                            distances[offset] = delta - 1;
                            offset++;
                            if (len == lenLimit)
                            {
                                son[ptr1] = son[cyclicPos];
                                son[ptr0] = son[cyclicPos + 1];
                                break;
                            }
                        }
                    }
                    if ((bufferBase[pby1 + len] & 0xFF) < (bufferBase[cur + len] & 0xFF))
                    {
                        son[ptr1] = curMatch;
                        ptr1 = cyclicPos + 1;
                        curMatch = son[ptr1];
                        len1 = len;
                    }
                    else
                    {
                        son[ptr0] = curMatch;
                        ptr0 = cyclicPos;
                        curMatch = son[ptr0];
                        len0 = len;
                    }
                }
                MovePos();
                return offset;
            }

            public void Skip(int num)
            {
                int lenLimit, matchMinPos, cur, hashValue, temp, hash2Value, hash3Value, curMatch;
                int ptr0, ptr1, len, len0, len1, count, delta, cyclicPos, pby1;
                do
                {
                    if (pos + matchMaxLen <= streamPos)
                        lenLimit = matchMaxLen;
                    else
                    {
                        lenLimit = streamPos - pos;
                        if (lenLimit < kMinMatchCheck)
                        {
                            MovePos();
                            num--;
                            continue;
                        }
                    }

                    if (pos > cyclicBufferSize)
                        matchMinPos = (pos - cyclicBufferSize);
                    else matchMinPos = 0;
                    cur = bufferOffset + pos;

                    if (HASH_ARRAY)
                    {
                        temp = CRCTable[bufferBase[cur] & 0xFF] ^ (bufferBase[cur + 1] & 0xFF);
                        hash2Value = temp & (kHash2Size - 1);
                        hash[hash2Value] = pos;
                        temp = temp ^ ((bufferBase[cur + 2] & 0xFF) << 8);
                        hash3Value = temp & (kHash3Size - 1);
                        hash[kHash3Offset + hash3Value] = pos;
                        hashValue = (temp ^ (CRCTable[bufferBase[cur + 3] & 0xFF] << 5)) & hashMask;
                    }
                    else
                        hashValue = ((bufferBase[cur] & 0xFF) ^ ((bufferBase[cur + 1] & 0xFF) << 8));

                    curMatch = hash[kFixHashSize + hashValue];
                    hash[kFixHashSize + hashValue] = pos;

                    ptr0 = (cyclicBufferPos << 1) + 1;
                    ptr1 = (cyclicBufferPos << 1);

                    len0 = kNumHashDirectBytes;
                    len1 = kNumHashDirectBytes;

                    count = cutValue;
                    while (true)
                    {
                        if (curMatch <= matchMinPos || count == 0)
                        {
                            son[ptr1] = kEmptyHashValue;
                            son[ptr0] = son[ptr1];
                            break;
                        }
                        else count--;

                        delta = pos - curMatch;
                        if (delta <= cyclicBufferPos)
                            cyclicPos = (cyclicBufferPos - delta) << 1;
                        else cyclicPos = (cyclicBufferPos - delta + cyclicBufferSize) << 1;

                        pby1 = bufferOffset + curMatch;
                        len = Math.Min(len0, len1);
                        if (bufferBase[pby1 + len] == bufferBase[cur + len])
                        {
                            len++;
                            while (len != lenLimit)
                            {
                                if (bufferBase[pby1 + len] != bufferBase[cur + len])
                                    break;
                                len++;
                            }
                            if (len == lenLimit)
                            {
                                son[ptr1] = son[cyclicPos];
                                son[ptr0] = son[cyclicPos + 1];
                                break;
                            }
                        }
                        if ((bufferBase[pby1 + len] & 0xFF) < (bufferBase[cur + len] & 0xFF))
                        {
                            son[ptr1] = curMatch;
                            ptr1 = cyclicPos + 1;
                            curMatch = son[ptr1];
                            len1 = len;
                        }
                        else
                        {
                            son[ptr0] = curMatch;
                            ptr0 = cyclicPos;
                            curMatch = son[ptr0];
                            len0 = len;
                        }
                    }
                    MovePos();
                    num--;
                } while (num != 0);
            }

            public void NormalizeLinks(int[] items, int numItems, int subValue)
            {
                int i, value;
                for (i = 0; i < numItems; i++)
                {
                    value = items[i];
                    if (value <= subValue)
                        value = kEmptyHashValue;
                    else value = value - subValue;
                    items[i] = value;
                }
            }

            public void Normalize()
            {
                int subValue;
                subValue = pos - cyclicBufferSize;
                NormalizeLinks(son, cyclicBufferSize * 2, subValue);
                NormalizeLinks(hash, hashSizeSum, subValue);
                ReduceOffsets(subValue);
            }

            public void SetCutValue(int cutValue)
            {
                this.cutValue = cutValue;
            }
        }

        private static void InitCRC()
        {
            int i, r, j;
            for (i = 0; i < 256; i++)
            {
                r = i;
                for (j = 0; j < 8; j++)
                    if ((r & 1) != 0)
                        r = (r >> 1) ^ unchecked((int)0xEDB88320);
                    else
                        r = r >> 1;
                CRCTable[i] = r;
            }
        }

        static ULZBinTree()
        {
            InitCRC();
        }
    }
}
