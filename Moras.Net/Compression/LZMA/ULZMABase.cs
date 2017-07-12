using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Moras.Net.Compression.LZMA
{
    // unit
    internal static class ULZMABase
    {
        public const int kNumRepDistances = 4;
        public const int kNumStates = 12;
        public const int kNumPosSlotBits = 6;
        public const int kDicLogSizeMin = 0;
        //public const int    kDicLogSizeMax = 28;
        //public const int    kDistTableSizeMax = kDicLogSizeMax * 2;

        public const int kNumLenToPosStatesBits = 2; // it's for speed optimization
        public const int kNumLenToPosStates = 1 << kNumLenToPosStatesBits;

        public const int kMatchMinLen = 2;

        public const int kNumAlignBits = 4;
        public const int kAlignTableSize = 1 << kNumAlignBits;
        public const int kAlignMask = (kAlignTableSize - 1);

        public const int kStartPosModelIndex = 4;
        public const int kEndPosModelIndex = 14;
        public const int kNumPosModels = kEndPosModelIndex - kStartPosModelIndex;

        public const int kNumFullDistances = 1 << (kEndPosModelIndex / 2);

        public const int kNumLitPosStatesBitsEncodingMax = 4;
        public const int kNumLitContextBitsMax = 8;

        public const int kNumPosStatesBitsMax = 4;
        public const int kNumPosStatesMax = (1 << kNumPosStatesBitsMax);
        public const int kNumPosStatesBitsEncodingMax = 4;
        public const int kNumPosStatesEncodingMax = (1 << kNumPosStatesBitsEncodingMax);

        public const int kNumLowLenBits = 3;
        public const int kNumMidLenBits = 3;
        public const int kNumHighLenBits = 8;
        public const int kNumLowLenSymbols = 1 << kNumLowLenBits;
        public const int kNumMidLenSymbols = 1 << kNumMidLenBits;
        public const int kNumLenSymbols = kNumLowLenSymbols + kNumMidLenSymbols + (1 << kNumHighLenBits);
        public const int kMatchMaxLen = kMatchMinLen + kNumLenSymbols - 1;

        public static int StateInit()
        {
            return 0;
        }

        public static int StateUpdateChar(int index)
        {
            if (index < 4)
                return 0;
            else
                if (index < 10)
                    return index - 3;
                else
                    return index - 6;
        }

        public static int StateUpdateMatch(int index)
        {
            if (index < 7) return 7;
            else return 10;
        }

        public static int StateUpdateRep(int index)
        {
            if (index < 7) return 8;
            else return 11;
        }

        public static int StateUpdateShortRep(int index)
        {
            if (index < 7) return 9;
            else return 11;
        }

        public static bool StateIsCharState(int index)
        {
            return index < 7;
        }

        public static int GetLenToPosState(int len)
        {
            len = len - kMatchMinLen;
            if (len < kNumLenToPosStates)
                return len;
            else return (kNumLenToPosStates - 1);
        }
    }
}
