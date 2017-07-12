using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Moras.Net.Compression.RangeCoder
{
    using TRangeDecoder = URangeDecoder.TRangeDecoder;
    // unit
    internal static class UBitTreeDecoder
    {
        internal class TBitTreeDecoder
        {
            public short[] Models;
            public int NumBitLevels;

            public TBitTreeDecoder(int numBitLevels)
            {
                this.NumBitLevels = numBitLevels;
                Models = new short[1 << numBitLevels];
            }

            public void Init()
            {
                URangeDecoder.InitBitModels(Models);
            }

            public int Decode(TRangeDecoder rangeDecoder)
            {
                int m, bitIndex;
                m = 1;
                for (bitIndex = NumBitLevels; bitIndex >= 1; bitIndex--)
                {
                    m = m << 1 + rangeDecoder.DecodeBit(Models, m);
                }
                return m - (1 << NumBitLevels);
            }

            public int ReverseDecode(TRangeDecoder rangeDecoder)
            {
                int m, symbol, bitindex, bit;
                m = 1;
                symbol = 0;
                for (bitindex = 0; bitindex < NumBitLevels; bitindex++)
                {
                    bit = rangeDecoder.DecodeBit(Models, m);
                    m = (m << 1) + bit;
                    symbol = symbol | (bit << bitindex);
                }
                return symbol;
            }
        }

        public static int ReverseDecode(short[] Models, int startIndex, TRangeDecoder rangeDecoder, int NumBitLevels)
        {
            int m, symbol, bitindex, bit;
            m = 1;
            symbol = 0;
            for (bitindex = 0; bitindex < NumBitLevels; bitindex++)
            {
                bit = rangeDecoder.DecodeBit(Models, startIndex + m);
                m = (m << 1) + bit;
                symbol = symbol | bit << bitindex;
            }
            return symbol;
        }
    }
}