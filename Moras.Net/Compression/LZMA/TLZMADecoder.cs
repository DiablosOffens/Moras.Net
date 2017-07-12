using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using DelphiClasses;

namespace Moras.Net.Compression.LZMA
{
    using Moras.Net.Compression.RangeCoder;
    using TLZMAProgress = Moras.Net.Compression.LZMA.ULZMACommon.TLZMAProgress;
    using TLZMAProgressAction = Moras.Net.Compression.LZMA.ULZMACommon.TLZMAProgressAction;
    using TLZOutWindow = Moras.Net.Compression.LZ.ULZOutWindow.TLZOutWindow;
    using TRangeDecoder = Moras.Net.Compression.RangeCoder.URangeDecoder.TRangeDecoder;
    using TBitTreeDecoder = Moras.Net.Compression.RangeCoder.UBitTreeDecoder.TBitTreeDecoder;

    // unit
    internal static class ULZMADecoder
    {
        internal class TLZMALenDecoder
        {
            public short[] m_Choice = new short[2];
            public TBitTreeDecoder[] m_LowCoder = new TBitTreeDecoder[ULZMABase.kNumPosStatesMax];
            public TBitTreeDecoder[] m_MidCoder = new TBitTreeDecoder[ULZMABase.kNumPosStatesMax];
            public TBitTreeDecoder m_HighCoder;
            public int m_NumPosStates;

            public TLZMALenDecoder()
            {
                m_HighCoder = new TBitTreeDecoder(ULZMABase.kNumHighLenBits);
                m_NumPosStates = 0;
            }
            /*
            ~TLZMALenDecoder()
            {
                int i;
                m_HighCoder = null;
                for (i = Extensions.Low(m_LowCoder); i <= Extensions.High(m_LowCoder); i++)
                {
                    if (m_LowCoder[i] != null) m_LowCoder[i] = null;
                    if (m_MidCoder[i] != null) m_MidCoder[i] = null;
                }
            }
            */
            public void _Create(int numPosStates)
            {
                while (m_NumPosStates < numPosStates)
                {
                    m_LowCoder[m_NumPosStates] = new TBitTreeDecoder(ULZMABase.kNumLowLenBits);
                    m_MidCoder[m_NumPosStates] = new TBitTreeDecoder(ULZMABase.kNumMidLenBits);
                    m_NumPosStates++;
                }
            }

            public void Init()
            {
                int posState;
                URangeDecoder.InitBitModels(m_Choice);
                for (posState = 0; posState < m_NumPosStates; posState++)
                {
                    m_LowCoder[posState].Init();
                    m_MidCoder[posState].Init();
                }
                m_HighCoder.Init();
            }

            public int Decode(TRangeDecoder rangeDecoder, int posState)
            {
                int symbol;
                if (rangeDecoder.DecodeBit(m_Choice, 0) == 0)
                {
                    return m_LowCoder[posState].Decode(rangeDecoder);
                }
                symbol = ULZMABase.kNumLowLenSymbols;
                if (rangeDecoder.DecodeBit(m_Choice, 1) == 0)
                    symbol = symbol + m_MidCoder[posState].Decode(rangeDecoder);
                else symbol = symbol + ULZMABase.kNumMidLenSymbols + m_HighCoder.Decode(rangeDecoder);
                return symbol;
            }
        }

        internal class TLZMADecoder2
        {
            public short[] m_Decoders = new short[0x300];

            public void Init()
            {
                URangeDecoder.InitBitModels(m_Decoders);
            }

            public byte DecodeNormal(TRangeDecoder rangeDecoder)
            {
                int symbol;
                symbol = 1;
                do
                {
                    symbol = (symbol << 1) | rangeDecoder.DecodeBit(m_Decoders, symbol);
                } while (symbol < 0x100);
                return (byte)symbol;
            }

            public byte DecodeWithMatchByte(TRangeDecoder rangeDecoder, byte matchByte)
            {
                int symbol;
                int matchBit;
                int bit;
                symbol = 1;
                do
                {
                    matchBit = (matchByte >> 7) & 1;
                    matchByte = (byte)(matchByte << 1);
                    bit = rangeDecoder.DecodeBit(m_Decoders, ((1 + matchBit) << 8) + symbol);
                    symbol = (symbol << 1) | bit;
                    if (matchBit != bit)
                    {
                        while (symbol < 0x100)
                        {
                            symbol = (symbol << 1) | rangeDecoder.DecodeBit(m_Decoders, symbol);
                        }
                        break;
                    }
                } while (symbol < 0x100);
                return (byte)symbol;
            }
        }

        internal class TLZMALiteralDecoder
        {
            public TLZMADecoder2[] m_Coders;
            public int m_NumPrevBits;
            public int m_NumPosBits;
            public int m_PosMask;

            public void _Create(int numPosBits, int numPrevBits)
            {
                int numStates, i;
                if (m_Coders.Length != 0 && m_NumPrevBits == numPrevBits && m_NumPosBits == numPosBits)
                    return;
                m_NumPosBits = numPosBits;
                m_PosMask = (1 << numPosBits) - 1;
                m_NumPrevBits = numPrevBits;
                numStates = 1 << (m_NumPrevBits + m_NumPosBits);
                m_Coders = new TLZMADecoder2[numStates];
                for (i = 0; i < numStates; i++)
                    m_Coders[i] = new TLZMADecoder2();
            }
            /*
            ~TLZMALiteralDecoder()
            {
                int i;
                for (i = Extensions.Low(m_Coders); i <= Extensions.High(m_Coders); i++)
                    if (m_Coders[i] != null) m_Coders[i] = null;
            }
            */
            public void Init()
            {
                int numStates, i;
                numStates = 1 << (m_NumPrevBits + m_NumPosBits);
                for (i = 0; i < numStates; i++)
                    m_Coders[i].Init();
            }

            public TLZMADecoder2 GetDecoder(int pos, byte prevByte)
            {
                return m_Coders[((pos & m_PosMask) << m_NumPrevBits) + ((prevByte & 0xFF) >> (8 - m_NumPrevBits))];
            }
        }

        internal class TLZMADecoder
        {
            private TLZMAProgress FOnProgress;

            public TLZOutWindow m_OutWindow;
            public TRangeDecoder m_RangeDecoder;

            public short[] m_IsMatchDecoders = new short[ULZMABase.kNumStates << ULZMABase.kNumPosStatesBitsMax];
            public short[] m_IsRepDecoders = new short[ULZMABase.kNumStates];
            public short[] m_IsRepG0Decoders = new short[ULZMABase.kNumStates];
            public short[] m_IsRepG1Decoders = new short[ULZMABase.kNumStates];
            public short[] m_IsRepG2Decoders = new short[ULZMABase.kNumStates];
            public short[] m_IsRep0LongDecoders = new short[ULZMABase.kNumStates << ULZMABase.kNumPosStatesBitsMax];

            public TBitTreeDecoder[] m_PosSlotDecoder = new TBitTreeDecoder[ULZMABase.kNumLenToPosStates];
            public short[] m_PosDecoders = new short[ULZMABase.kNumFullDistances - ULZMABase.kEndPosModelIndex];

            public TBitTreeDecoder m_PosAlignDecoder;

            public TLZMALenDecoder m_LenDecoder;
            public TLZMALenDecoder m_RepLenDecoder;

            public TLZMALiteralDecoder m_LiteralDecoder;

            public int m_DictionarySize;
            public int m_DictionarySizeCheck;

            public int m_PosStateMask;

            public TLZMADecoder()
            {
                int i;
                FOnProgress = null;
                m_OutWindow = new TLZOutWindow();
                m_RangeDecoder = new TRangeDecoder();
                m_PosAlignDecoder = new TBitTreeDecoder(ULZMABase.kNumAlignBits);
                m_LenDecoder = new TLZMALenDecoder();
                m_RepLenDecoder = new TLZMALenDecoder();
                m_LiteralDecoder = new TLZMALiteralDecoder();
                m_DictionarySize = -1;
                m_DictionarySizeCheck = -1;
                for (i = 0; i < ULZMABase.kNumLenToPosStates; i++)
                    m_PosSlotDecoder[i] = new TBitTreeDecoder(ULZMABase.kNumPosSlotBits);
            }
            /*
            ~TLZMADecoder()
            {
                int i;
                m_OutWindow = null;
                m_RangeDecoder = null;
                m_PosAlignDecoder = null;
                m_LenDecoder = null;
                m_RepLenDecoder = null;
                m_LiteralDecoder = null;
                for (i = 0; i < ULZMABase.kNumLenToPosStates; i++)
                    m_PosSlotDecoder[i] = null;
            }
            */
            public bool SetDictionarySize(int dictionarySize)
            {
                if (dictionarySize < 0)
                    return false;
                else
                {
                    if (m_DictionarySize != dictionarySize)
                    {
                        m_DictionarySize = dictionarySize;
                        m_DictionarySizeCheck = Math.Max(m_DictionarySize, 1);
                        m_OutWindow._Create(Math.Max(m_DictionarySizeCheck, (1 << 12)));
                    }
                    return true;
                }
            }

            public bool SetLcLpPb(int lc, int lp, int pb)
            {
                int numPosStates;
                if (lc > ULZMABase.kNumLitContextBitsMax || lp > 4 || pb > ULZMABase.kNumPosStatesBitsMax)
                {
                    return false;
                }
                m_LiteralDecoder._Create(lp, lc);
                numPosStates = 1 << pb;
                m_LenDecoder._Create(numPosStates);
                m_RepLenDecoder._Create(numPosStates);
                m_PosStateMask = numPosStates - 1;
                return true;
            }

            public void Init()
            {
                int i;
                m_OutWindow.Init(false);

                URangeDecoder.InitBitModels(m_IsMatchDecoders);
                URangeDecoder.InitBitModels(m_IsRep0LongDecoders);
                URangeDecoder.InitBitModels(m_IsRepDecoders);
                URangeDecoder.InitBitModels(m_IsRepG0Decoders);
                URangeDecoder.InitBitModels(m_IsRepG1Decoders);
                URangeDecoder.InitBitModels(m_IsRepG2Decoders);
                URangeDecoder.InitBitModels(m_PosDecoders);

                m_LiteralDecoder.Init();
                for (i = 0; i < ULZMABase.kNumLenToPosStates; i++)
                    m_PosSlotDecoder[i].Init();
                m_LenDecoder.Init();
                m_RepLenDecoder.Init();
                m_PosAlignDecoder.Init();
                m_RangeDecoder.Init();
            }

            public bool Code(Stream inStream, Stream outStream, Int64 outSize)
            {
                int state, rep0, rep1, rep2, rep3;
                Int64 nowPos64;
                byte prevByte;
                int posState;
                TLZMADecoder2 decoder2;
                int len, distance, posSlot, numDirectBits;
                Int64 lpos;
                Int64 progint;
                DoProgress(TLZMAProgressAction.LPAMax, outSize);
                m_RangeDecoder.SetStream(inStream);
                m_OutWindow.SetStream(outStream);
                Init();

                state = ULZMABase.StateInit();
                rep0 = 0; rep1 = 0; rep2 = 0; rep3 = 0;

                nowPos64 = 0;
                prevByte = 0;
                progint = outSize / ULZMACommon.CodeProgressInterval;
                lpos = progint;
                while (outSize < 0 || nowPos64 < outSize)
                {
                    if (nowPos64 >= lpos)
                    {
                        DoProgress(TLZMAProgressAction.LPAPos, nowPos64);
                        lpos = lpos + progint;
                    }
                    posState = (int)(nowPos64 & m_PosStateMask);
                    if (m_RangeDecoder.DecodeBit(m_IsMatchDecoders, (state << ULZMABase.kNumPosStatesBitsMax) + posState) == 0)
                    {
                        decoder2 = m_LiteralDecoder.GetDecoder((int)nowPos64, prevByte);
                        if (!ULZMABase.StateIsCharState(state))
                            prevByte = decoder2.DecodeWithMatchByte(m_RangeDecoder, m_OutWindow.GetByte(rep0));
                        else prevByte = decoder2.DecodeNormal(m_RangeDecoder);
                        m_OutWindow.PutByte(prevByte);
                        state = ULZMABase.StateUpdateChar(state);
                        nowPos64++;
                    }
                    else
                    {
                        if (m_RangeDecoder.DecodeBit(m_IsRepDecoders, state) == 1)
                        {
                            len = 0;
                            if (m_RangeDecoder.DecodeBit(m_IsRepG0Decoders, state) == 0)
                            {
                                if (m_RangeDecoder.DecodeBit(m_IsRep0LongDecoders, (state << ULZMABase.kNumPosStatesBitsMax) + posState) == 0)
                                {
                                    state = ULZMABase.StateUpdateShortRep(state);
                                    len = 1;
                                }
                            }
                            else
                            {
                                if (m_RangeDecoder.DecodeBit(m_IsRepG1Decoders, state) == 0)
                                    distance = rep1;
                                else
                                {
                                    if (m_RangeDecoder.DecodeBit(m_IsRepG2Decoders, state) == 0)
                                        distance = rep2;
                                    else
                                    {
                                        distance = rep3;
                                        rep3 = rep2;
                                    }
                                    rep2 = rep1;
                                }
                                rep1 = rep0;
                                rep0 = distance;
                            }
                            if (len == 0)
                            {
                                len = m_RepLenDecoder.Decode(m_RangeDecoder, posState) + ULZMABase.kMatchMinLen;
                                state = ULZMABase.StateUpdateRep(state);
                            }
                        }
                        else
                        {
                            rep3 = rep2;
                            rep2 = rep1;
                            rep1 = rep0;
                            len = ULZMABase.kMatchMinLen + m_LenDecoder.Decode(m_RangeDecoder, posState);
                            state = ULZMABase.StateUpdateMatch(state);
                            posSlot = m_PosSlotDecoder[ULZMABase.GetLenToPosState(len)].Decode(m_RangeDecoder);
                            if (posSlot >= ULZMABase.kStartPosModelIndex)
                            {
                                numDirectBits = (posSlot >> 1) - 1;
                                rep0 = ((2 | (posSlot & 1)) << numDirectBits);
                                if (posSlot < ULZMABase.kEndPosModelIndex)
                                    rep0 = rep0 + UBitTreeDecoder.ReverseDecode(m_PosDecoders,
                                             rep0 - posSlot - 1, m_RangeDecoder, numDirectBits);
                                else
                                {
                                    rep0 = rep0 + (m_RangeDecoder.DecodeDirectBits(
                                             numDirectBits - ULZMABase.kNumAlignBits) << ULZMABase.kNumAlignBits);
                                    rep0 = rep0 + m_PosAlignDecoder.ReverseDecode(m_RangeDecoder);
                                    if (rep0 < 0)
                                    {
                                        if (rep0 == -1)
                                            break;
                                        return false;
                                    }
                                }
                            }
                            else rep0 = posSlot;
                        }
                        if (rep0 >= nowPos64 || rep0 >= m_DictionarySizeCheck)
                        {
                            m_OutWindow.Flush();
                            return false;
                        }
                        m_OutWindow.CopyBlock(rep0, len);
                        nowPos64 = nowPos64 + len;
                        prevByte = m_OutWindow.GetByte(0);
                    }
                }
                m_OutWindow.Flush();
                m_OutWindow.ReleaseStream();
                m_RangeDecoder.ReleaseStream();
                DoProgress(TLZMAProgressAction.LPAPos, nowPos64);
                return true;
            }

            public bool SetDecoderProperties(byte[] properties)
            {
                int val, lc, remainder, lp, pb, dictionarySize, i;
                if (properties.Length < 5)
                {
                    return false;
                }
                val = properties[0] & 0xFF;
                lc = val % 9;
                remainder = val / 9;
                lp = remainder % 5;
                pb = remainder / 5;
                dictionarySize = 0;
                for (i = 0; i < 4; i++)
                    dictionarySize = dictionarySize + ((properties[1 + i]) & 0xFF) << (i * 8);
                if (!SetLcLpPb(lc, lp, pb))
                {
                    return false;
                }
                return SetDictionarySize(dictionarySize);
            }

            public TLZMAProgress OnProgress { get { return FOnProgress; } set { FOnProgress = value; } }

            private void DoProgress(TLZMAProgressAction Action, Int64 Value)
            {
                if (FOnProgress != null)
                    FOnProgress(Action, Value);
            }
        }
    }
}