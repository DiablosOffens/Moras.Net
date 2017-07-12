using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Moras.Net.Compression.LZMA;

namespace Moras.Net.Compression.RangeCoder
{
    // unit
    internal static class URangeDecoder
    {
        const int kTopMask = ~((1 << 24) - 1);
        const short kNumBitModelTotalBits = 11;
        const short kBitModelTotal = (1 << kNumBitModelTotalBits);
        const short kNumMoveBits = 5;

        internal class TRangeDecoder
        {
            public int Range, Code;
            public Stream Stream;

            public void SetStream(Stream Stream)
            {
                this.Stream = Stream;
            }

            public void ReleaseStream()
            {
                Stream = null;
            }

            public void Init()
            {
                int i;
                Code = 0;
                Range = -1;
                for (i = 0; i < 5; i++)
                {
                    Code = (Code << 8) | ULZMACommon.ReadByte(Stream);
                }
            }

            public int DecodeDirectBits(int numTotalBits)
            {
                int i, t;
                int result = 0;
                for (i = numTotalBits; i >= 1; i--)
                {
                    Range = Range >> 1;
                    t = ((Code - Range) >> 31);
                    Code = Code - Range & (t - 1);
                    result = (result << 1) | (1 - t);
                    if ((Range & kTopMask) == 0)
                    {
                        Code = (Code << 8) | ULZMACommon.ReadByte(Stream);
                        Range = Range << 8;
                    }
                }
                return result;
            }

            public int DecodeBit(short[] probs, int index)
            {
                short prob;
                int newBound;
                prob = probs[index];
                newBound = (Range >> kNumBitModelTotalBits) * prob;
                if ((Code ^ unchecked((int)0x80000000)) < (newBound ^ unchecked((int)0x80000000)))
                {
                    Range = newBound;
                    probs[index] = (short)(prob + ((kBitModelTotal - prob) >> kNumMoveBits));
                    if ((Range & kTopMask) == 0)
                    {
                        Code = (Code << 8) | ULZMACommon.ReadByte(Stream);
                        Range = Range << 8;
                    }
                    return 0;
                }
                else
                {
                    Range = Range - newBound;
                    Code = Code - newBound;
                    probs[index] = (short)(prob - ((prob) >> kNumMoveBits));
                    if ((Range & kTopMask) == 0)
                    {
                        Code = (Code << 8) | ULZMACommon.ReadByte(Stream);
                        Range = Range << 8;
                    }
                    return 1;
                }
            }
        }

        public static void InitBitModels(short[] probs)
        {
            int i;
            for (i = 0; i < probs.Length; i++)
                probs[i] = kBitModelTotal >> 1;
        }
    }
}