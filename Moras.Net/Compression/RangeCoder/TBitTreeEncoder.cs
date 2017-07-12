using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Moras.Net.Compression.RangeCoder
{
    using TRangeEncoder = URangeEncoder.TRangeEncoder;

    internal static class UBitTreeEncoder
    {
        internal class TBitTreeEncoder
        {
            short[] Models;
            int NumBitLevels;

            TBitTreeEncoder(int numBitLevels)
            {
                this.NumBitLevels = numBitLevels;
                Models = new short[1 << numBitLevels];
            }

            void Init()
            {
                URangeDecoder.InitBitModels(Models);
            }

            void Encode(TRangeEncoder rangeEncoder, int symbol)
            {
                int m, bitIndex, bit;
                m = 1;
                for (bitIndex = NumBitLevels - 1; bitIndex >= 0; bitIndex--)
                {
                    bit = (symbol >> bitIndex) & 1;
                    rangeEncoder.Encode(Models, m, bit);
                    m = (m << 1) | bit;
                }
            }

            void ReverseEncode(TRangeEncoder rangeEncoder, int symbol)
            {
                int m, i, bit;
                m = 1;
                for (i = 0; i < NumBitLevels; i++)
                {
                    bit = symbol & 1;
                    rangeEncoder.Encode(Models, m, bit);
                    m = (m << 1) | bit;
                    symbol = symbol >> 1;
                }
            }

            int GetPrice(int symbol)
            {
                int price, m, bitIndex, bit;
                price = 0;
                m = 1;
                for (bitIndex = NumBitLevels - 1; bitIndex >= 0; bitIndex--)
                {
                    bit = (symbol >> bitIndex) & 1;
                    price = price + URangeEncoder.RangeEncoder.GetPrice(Models[m], bit);
                    m = (m << 1) + bit;
                }
                return price;
            }

            int ReverseGetPrice(int symbol)
            {
                int price, m, i, bit;
                price = 0;
                m = 1;
                for (i = NumBitLevels; i >= 1; i--)
                {
                    bit = symbol & 1;
                    symbol = symbol >> 1;
                    price = price + URangeEncoder.RangeEncoder.GetPrice(Models[m], bit);
                    m = (m << 1) | bit;
                }
                return price;
            }
        }

        public static int ReverseGetPrice(short[] Models, int startIndex, int NumBitLevels, int symbol)
        {
            int price, m, i, bit;
            price = 0;
            m = 1;
            for (i = NumBitLevels; i >= 1; i--)
            {
                bit = symbol & 1;
                symbol = symbol >> 1;
                price = price + URangeEncoder.RangeEncoder.GetPrice(Models[startIndex + m], bit);
                m = (m << 1) | bit;
            }
            return price;
        }

        public static void ReverseEncode(short[] Models, int startIndex, TRangeEncoder rangeEncoder, int NumBitLevels, int symbol)
        {
            int m, i, bit;
            m = 1;
            for (i = 0; i < NumBitLevels; i++)
            {
                bit = symbol & 1;
                rangeEncoder.Encode(Models, startIndex + m, bit);
                m = (m << 1) | bit;
                symbol = symbol >> 1;
            }
        }
    }
}
