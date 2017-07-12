using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Moras.Net.Compression.LZMA;

namespace Moras.Net.Compression.RangeCoder
{
    // unit
    internal static class URangeEncoder
    {
        public const short kNumBitPriceShiftBits = 6;
        public const int kTopMask = ~((1 << 24) - 1);
        public const short kNumBitModelTotalBits = 11;
        public const short kBitModelTotal = (1 << kNumBitModelTotalBits);
        public const short kNumMoveBits = 5;
        public const short kNumMoveReducingBits = 2;

        public static TRangeEncoder RangeEncoder;

        internal class TRangeEncoder
        {
            private int[] ProbPrices = new int[kBitModelTotal >> kNumMoveReducingBits];
            public Stream Stream;
            public Int64 Low, Position;
            public int Range, cacheSize, cache;

            public void SetStream(Stream stream)
            {
                this.Stream = stream;
            }

            public void ReleaseStream()
            {
                Stream = null;
            }

            public void Init()
            {
                Position = 0;
                Low = 0;
                Range = -1;
                cacheSize = 1;
                cache = 0;
            }

            public void FlushData()
            {
                int i;
                for (i = 0; i < 5; i++)
                    ShiftLow();
            }

            public void FlushStream()
            {
                //Stream.Flush();
            }

            public void ShiftLow()
            {
                int LowHi;
                int temp;
                LowHi = (int)(Low >> 32);
                if (LowHi != 0 || Low < (Int64)0xFF000000)
                {
                    Position = Position + cacheSize;
                    temp = cache;
                    do
                    {
                        ULZMACommon.WriteByte(Stream, (byte)(temp + LowHi));
                        temp = 0xFF;
                        cacheSize--;
                    } while (cacheSize != 0);
                    cache = (int)(Low >> 24);
                }
                cacheSize++;
                Low = (Low & (int)0xFFFFFF) << 8;
            }

            public void EncodeDirectBits(int v, int numTotalBits)
            {
                int i;
                for (i = numTotalBits - 1; i >= 0; i--)
                {
                    Range = Range >> 1;
                    if (((v >> i) & 1) == 1)
                        Low = Low + Range;
                    if ((Range & kTopMask) == 0)
                    {
                        Range = Range << 8;
                        ShiftLow();
                    }
                }
            }

            public Int64 GetProcessedSizeAdd()
            {
                return cacheSize + Position + 4;
            }

            public static void InitBitModels(short[] probs)
            {
                int i;
                for (i = 0; i < probs.Length; i++)
                    probs[i] = kBitModelTotal >> 1;
            }

            public void Encode(short[] probs, int index, int symbol)
            {
                short prob;
                int newBound;
                prob = probs[index];
                newBound = (Range >> kNumBitModelTotalBits) * prob;
                if (symbol == 0)
                {
                    Range = newBound;
                    probs[index] = (short)(prob + ((kBitModelTotal - prob) >> kNumMoveBits));
                }
                else
                {
                    Low = Low + (newBound & (Int64)0xFFFFFFFF);
                    Range = Range - newBound;
                    probs[index] = (short)(prob - ((prob) >> kNumMoveBits));
                }
                if ((Range & kTopMask) == 0)
                {
                    Range = Range << 8;
                    ShiftLow();
                }
            }

            public TRangeEncoder()
            {
                int kNumBits;
                int i, j, start, _end;
                kNumBits = (kNumBitModelTotalBits - kNumMoveReducingBits);
                for (i = kNumBits - 1; i >= 0; i--)
                {
                    start = 1 << (kNumBits - i - 1);
                    _end = 1 << (kNumBits - i);
                    for (j = start; j < _end; j++)
                        ProbPrices[j] = (i << kNumBitPriceShiftBits) +
                            (((_end - j) << kNumBitPriceShiftBits) >> (kNumBits - i - 1));
                }
            }

            public int GetPrice(int Prob, int symbol)
            {
                return ProbPrices[(((Prob - symbol) ^ ((-symbol))) & (kBitModelTotal - 1)) >> kNumMoveReducingBits];
            }

            public int GetPrice0(int Prob)
            {
                return ProbPrices[Prob >> kNumMoveReducingBits];
            }

            public int GetPrice1(int Prob)
            {
                return ProbPrices[(kBitModelTotal - Prob) >> kNumMoveReducingBits];
            }
        }

        static URangeEncoder()
        {
            RangeEncoder = new TRangeEncoder();
        }
    }
}