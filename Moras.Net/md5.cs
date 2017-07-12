// tabs = 2
// -----------------------------------------------------------------------------------------------
//
//                                 MD5 Message-Digest for Delphi 4
//
//                                 Delphi 4 Unit implementing the
//                      RSA Data Security, Inc. MD5 Message-Digest Algorithm
//
//                          Implementation of Ronald L. Rivest's RFC 1321
//
//                      Copyright © 1997-1999 Medienagentur Fichtner & Meyer
//                                  Written by Matthias Fichtner
//
// -----------------------------------------------------------------------------------------------
//               See RFC 1321 for RSA Data Security's copyright and license notice!
// -----------------------------------------------------------------------------------------------
//
//     14-Jun-97  mf  Implemented MD5 according to RFC 1321                           RFC 1321
//     16-Jun-97  mf  Initial release of the compiled unit (no source code)           RFC 1321
//     28-Feb-99  mf  Added MD5Match function for comparing two digests               RFC 1321
//     13-Sep-99  mf  Reworked the entire unit                                        RFC 1321
//     17-Sep-99  mf  Reworked the "Test Driver" project                              RFC 1321
//     19-Sep-99  mf  Release of sources for MD5 unit and "Test Driver" project       RFC 1321
//
// -----------------------------------------------------------------------------------------------
//                   The latest release of md5.pas will always be available from
//                  the distribution site at: http://www.fichtner.net/delphi/md5/
// -----------------------------------------------------------------------------------------------
//                       Please send questions, bug reports and suggestions
//                      regarding this code to: mfichtner@fichtner-meyer.com
// -----------------------------------------------------------------------------------------------
//                        This code is provided "as is" without express or
//                     implied warranty of any kind. Use it at your own risk.
// -----------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Moras.Net
{
    using DWORD = UInt32;
    using System.Runtime.InteropServices;
    using System.IO;
    using System.IO.MemoryMappedFiles;
    internal class md5
    {
        public unsafe struct MD5Count
        {
            fixed DWORD array[2];
            public DWORD this[int index]
            {
                get { fixed (MD5Count* p = &this) { return p->array[index]; } }
                set { fixed (MD5Count* p = &this) { p->array[index] = value; } }
            }
            public static implicit operator MD5Count(DWORD[] value)
            {
                MD5Count result;
                Marshal.Copy((int[])(object)value, 0, (IntPtr)result.array, Math.Min(value.Length, 2));
                return result;
            }
            public IntPtr GetPointer(DWORD index = 0)
            {
                fixed (MD5Count* p = &this) { return (IntPtr)(p->array + index); }
            }
        }
        public unsafe struct MD5State
        {
            fixed DWORD array[4];
            public DWORD this[int index]
            {
                get { fixed (MD5State* p = &this) { return p->array[index]; } }
                set { fixed (MD5State* p = &this) { p->array[index] = value; } }
            }
            public static implicit operator MD5State(DWORD[] value)
            {
                MD5State result;
                Marshal.Copy((int[])(object)value, 0, (IntPtr)result.array, Math.Min(value.Length, 4));
                return result;
            }
            public IntPtr GetPointer(DWORD index = 0)
            {
                fixed (MD5State* p = &this) { return (IntPtr)(p->array + index); }
            }
        }
        public unsafe struct MD5Block
        {
            fixed DWORD array[16];
            public DWORD this[int index]
            {
                get { fixed (MD5Block* p = &this) { return p->array[index]; } }
                set { fixed (MD5Block* p = &this) { p->array[index] = value; } }
            }
            public static implicit operator MD5Block(DWORD[] value)
            {
                MD5Block result;
                Marshal.Copy((int[])(object)value, 0, (IntPtr)result.array, Math.Min(value.Length, 16));
                return result;
            }
            public IntPtr GetPointer(DWORD index = 0)
            {
                fixed (MD5Block* p = &this) { return (IntPtr)(p->array + index); }
            }
        }
        public unsafe struct MD5CBits
        {
            fixed byte array[8];
            public byte this[int index]
            {
                get { fixed (MD5CBits* p = &this) { return p->array[index]; } }
                set { fixed (MD5CBits* p = &this) { p->array[index] = value; } }
            }
            public static implicit operator MD5CBits(byte[] value)
            {
                MD5CBits result;
                Marshal.Copy(value, 0, (IntPtr)result.array, Math.Min(value.Length, 8));
                return result;
            }
            public IntPtr GetPointer(DWORD index = 0)
            {
                fixed (MD5CBits* p = &this) { return (IntPtr)(p->array + index); }
            }
        }
        public unsafe struct MD5Digest
        {
            fixed byte array[16];
            public byte this[int index]
            {
                get { fixed (MD5Digest* p = &this) { return p->array[index]; } }
                set { fixed (MD5Digest* p = &this) { p->array[index] = value; } }
            }
            public static implicit operator MD5Digest(byte[] value)
            {
                MD5Digest result;
                Marshal.Copy(value, 0, (IntPtr)result.array, Math.Min(value.Length, 16));
                return result;
            }
            public IntPtr GetPointer(DWORD index = 0)
            {
                fixed (MD5Digest* p = &this) { return (IntPtr)(p->array + index); }
            }
        }
        public unsafe struct MD5Buffer
        {
            fixed byte array[64];
            public byte this[int index]
            {
                get { fixed (MD5Buffer* p = &this) { return p->array[index]; } }
                set { fixed (MD5Buffer* p = &this) { p->array[index] = value; } }
            }
            public static implicit operator MD5Buffer(byte[] value)
            {
                MD5Buffer result;
                Marshal.Copy(value, 0, (IntPtr)result.array, Math.Min(value.Length, 64));
                return result;
            }
            public IntPtr GetPointer(DWORD index = 0)
            {
                fixed (MD5Buffer* p = &this) { return (IntPtr)(p->array + index); }
            }
        }
        public struct MD5Context
        {
            public MD5State State;
            public MD5Count Count;
            public MD5Buffer Buffer;
        }

        static MD5Buffer PADDING = new byte[] {
		    0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
		    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
		    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
		    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
		    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
		    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
		    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
		    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
        };

        [DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
        static extern void CopyMemory(IntPtr dest, IntPtr src, UIntPtr count);

        static DWORD F(DWORD x, DWORD y, DWORD z)
        {
            return (x & y) | ((~x) & z);
        }

        static DWORD G(DWORD x, DWORD y, DWORD z)
        {
            return (x & z) | (y & (~z));
        }

        static DWORD H(DWORD x, DWORD y, DWORD z)
        {
            return x ^ y ^ z;
        }

        static DWORD I(DWORD x, DWORD y, DWORD z)
        {
            return y ^ (x | (~z));
        }

        static void rot(ref DWORD x, byte n)
        {
            x = (x << n) | (x >> (32 - n));
        }

        static void FF(ref DWORD a, DWORD b, DWORD c, DWORD d, DWORD x, byte s, DWORD ac)
        {
            a += F(b, c, d) + x + ac;
            rot(ref a, s);
            a += b;
        }

        static void GG(ref DWORD a, DWORD b, DWORD c, DWORD d, DWORD x, byte s, DWORD ac)
        {
            a += G(b, c, d) + x + ac;
            rot(ref a, s);
            a += b;
        }

        static void HH(ref DWORD a, DWORD b, DWORD c, DWORD d, DWORD x, byte s, DWORD ac)
        {
            a += H(b, c, d) + x + ac;
            rot(ref a, s);
            a += b;
        }

        static void II(ref DWORD a, DWORD b, DWORD c, DWORD d, DWORD x, byte s, DWORD ac)
        {
            a += I(b, c, d) + x + ac;
            rot(ref a, s);
            a += b;
        }

        // Encode Count bytes at Source into (Count / 4) DWORDs at Target
        static unsafe void Encode(IntPtr Source, IntPtr Target, UInt32 Count)
        {
            UInt32 I;
            byte* S = (byte*)Source;
            DWORD* T = (DWORD*)Target;
            for (I = 1; I <= Count / 4; I++)
            {
                *T = *S;
                S++;
                *T = *T | ((DWORD)(*S) << 8);
                S++;
                *T = *T | ((DWORD)(*S) << 16);
                S++;
                *T = *T | ((DWORD)(*S) << 24);
                S++;
                T++;
            }
        }

        // Decode Count DWORDs at Source into (Count * 4) Bytes at Target
        static unsafe void Decode(IntPtr Source, IntPtr Target, UInt32 Count)
        {

            UInt32 I;
            DWORD* S = (DWORD*)Source;
            byte* T = (byte*)Target;
            for (I = 1; I <= Count; I++)
            {
                *T = (byte)(*S & 0xff);
                T++;
                *T = (byte)((*S >> 8) & 0xff);
                T++;
                *T = (byte)((*S >> 16) & 0xff);
                T++;
                *T = (byte)((*S >> 24) & 0xff);
                T++;
                S++;
            }
        }

        // Transform State according to first 64 bytes at Buffer
        static void Transform(IntPtr Buffer, ref MD5State State)
        {
            DWORD a, b, c, d;
            MD5Block Block;

            Encode(Buffer, Block.GetPointer(), 64);
            a = State[0];
            b = State[1];
            c = State[2];
            d = State[3];
            FF(ref a, b, c, d, Block[0], 7, 0xd76aa478);
            FF(ref d, a, b, c, Block[1], 12, 0xe8c7b756);
            FF(ref c, d, a, b, Block[2], 17, 0x242070db);
            FF(ref b, c, d, a, Block[3], 22, 0xc1bdceee);
            FF(ref a, b, c, d, Block[4], 7, 0xf57c0faf);
            FF(ref d, a, b, c, Block[5], 12, 0x4787c62a);
            FF(ref c, d, a, b, Block[6], 17, 0xa8304613);
            FF(ref b, c, d, a, Block[7], 22, 0xfd469501);
            FF(ref a, b, c, d, Block[8], 7, 0x698098d8);
            FF(ref d, a, b, c, Block[9], 12, 0x8b44f7af);
            FF(ref c, d, a, b, Block[10], 17, 0xffff5bb1);
            FF(ref b, c, d, a, Block[11], 22, 0x895cd7be);
            FF(ref a, b, c, d, Block[12], 7, 0x6b901122);
            FF(ref d, a, b, c, Block[13], 12, 0xfd987193);
            FF(ref c, d, a, b, Block[14], 17, 0xa679438e);
            FF(ref b, c, d, a, Block[15], 22, 0x49b40821);
            GG(ref a, b, c, d, Block[1], 5, 0xf61e2562);
            GG(ref d, a, b, c, Block[6], 9, 0xc040b340);
            GG(ref c, d, a, b, Block[11], 14, 0x265e5a51);
            GG(ref b, c, d, a, Block[0], 20, 0xe9b6c7aa);
            GG(ref a, b, c, d, Block[5], 5, 0xd62f105d);
            GG(ref d, a, b, c, Block[10], 9, 0x2441453);
            GG(ref c, d, a, b, Block[15], 14, 0xd8a1e681);
            GG(ref b, c, d, a, Block[4], 20, 0xe7d3fbc8);
            GG(ref a, b, c, d, Block[9], 5, 0x21e1cde6);
            GG(ref d, a, b, c, Block[14], 9, 0xc33707d6);
            GG(ref c, d, a, b, Block[3], 14, 0xf4d50d87);
            GG(ref b, c, d, a, Block[8], 20, 0x455a14ed);
            GG(ref a, b, c, d, Block[13], 5, 0xa9e3e905);
            GG(ref d, a, b, c, Block[2], 9, 0xfcefa3f8);
            GG(ref c, d, a, b, Block[7], 14, 0x676f02d9);
            GG(ref b, c, d, a, Block[12], 20, 0x8d2a4c8a);
            HH(ref a, b, c, d, Block[5], 4, 0xfffa3942);
            HH(ref d, a, b, c, Block[8], 11, 0x8771f681);
            HH(ref c, d, a, b, Block[11], 16, 0x6d9d6122);
            HH(ref b, c, d, a, Block[14], 23, 0xfde5380c);
            HH(ref a, b, c, d, Block[1], 4, 0xa4beea44);
            HH(ref d, a, b, c, Block[4], 11, 0x4bdecfa9);
            HH(ref c, d, a, b, Block[7], 16, 0xf6bb4b60);
            HH(ref b, c, d, a, Block[10], 23, 0xbebfbc70);
            HH(ref a, b, c, d, Block[13], 4, 0x289b7ec6);
            HH(ref d, a, b, c, Block[0], 11, 0xeaa127fa);
            HH(ref c, d, a, b, Block[3], 16, 0xd4ef3085);
            HH(ref b, c, d, a, Block[6], 23, 0x4881d05);
            HH(ref a, b, c, d, Block[9], 4, 0xd9d4d039);
            HH(ref d, a, b, c, Block[12], 11, 0xe6db99e5);
            HH(ref c, d, a, b, Block[15], 16, 0x1fa27cf8);
            HH(ref b, c, d, a, Block[2], 23, 0xc4ac5665);
            II(ref a, b, c, d, Block[0], 6, 0xf4292244);
            II(ref d, a, b, c, Block[7], 10, 0x432aff97);
            II(ref c, d, a, b, Block[14], 15, 0xab9423a7);
            II(ref b, c, d, a, Block[5], 21, 0xfc93a039);
            II(ref a, b, c, d, Block[12], 6, 0x655b59c3);
            II(ref d, a, b, c, Block[3], 10, 0x8f0ccc92);
            II(ref c, d, a, b, Block[10], 15, 0xffeff47d);
            II(ref b, c, d, a, Block[1], 21, 0x85845dd1);
            II(ref a, b, c, d, Block[8], 6, 0x6fa87e4f);
            II(ref d, a, b, c, Block[15], 10, 0xfe2ce6e0);
            II(ref c, d, a, b, Block[6], 15, 0xa3014314);
            II(ref b, c, d, a, Block[13], 21, 0x4e0811a1);
            II(ref a, b, c, d, Block[4], 6, 0xf7537e82);
            II(ref d, a, b, c, Block[11], 10, 0xbd3af235);
            II(ref c, d, a, b, Block[2], 15, 0x2ad7d2bb);
            II(ref b, c, d, a, Block[9], 21, 0xeb86d391);
            State[0] += a;
            State[1] += b;
            State[2] += c;
            State[3] += d;
        }

        // Initialize given Context
        public static void MD5Init(ref MD5Context Context)
        {

            Context.State[0] = 0x67452301;
            Context.State[1] = 0xefcdab89;
            Context.State[2] = 0x98badcfe;
            Context.State[3] = 0x10325476;
            Context.Count[0] = 0;
            Context.Count[1] = 0;
            Context.Buffer = new MD5Buffer();
        }

        // Update given Context to include Length bytes of Input
        public unsafe static void MD5Update(ref MD5Context Context, byte* Input, UInt32 Length)
        {
            UInt32 Index;
            UInt32 PartLen;
            UInt32 I;
            Index = (Context.Count[0] >> 3) & 0x3f;
            Context.Count[0] += Length << 3;
            if (Context.Count[0] < (Length << 3)) Context.Count[1]++;
            Context.Count[1] += Length >> 29;
            PartLen = 64 - Index;
            if (Length >= PartLen)
            {
                CopyMemory(Context.Buffer.GetPointer(Index), (IntPtr)Input, (UIntPtr)PartLen);
                Transform(Context.Buffer.GetPointer(), ref Context.State);
                I = PartLen;
                while (I + 63 < Length)
                {
                    Transform((IntPtr)(Input + I), ref Context.State);
                    I += 64;
                }
                Index = 0;
            }
            else I = 0;
            CopyMemory(Context.Buffer.GetPointer(Index), (IntPtr)(&Input[I]), (UIntPtr)(Length - I));
        }

        // Finalize given Context, create Digest and zeroize Context
        public unsafe static void MD5Final(ref MD5Context Context, out MD5Digest Digest)
        {
            MD5CBits Bits;
            UInt32 Index;
            UInt32 PadLen;
            Decode(Context.Count.GetPointer(), Bits.GetPointer(), 2);
            Index = (Context.Count[0] >> 3) & 0x3f;
            if (Index < 56) PadLen = 56 - Index; else PadLen = 120 - Index;
            MD5Update(ref Context, (byte*)PADDING.GetPointer(), PadLen);
            MD5Update(ref Context, (byte*)Bits.GetPointer(), 8);
            Decode(Context.State.GetPointer(), Digest.GetPointer(), 4);
            Context = new MD5Context();
        }

        // Create digest of given Message
        public unsafe static MD5Digest MD5String(string M)
        {
            MD5Context Context;
            MD5Init(ref Context);
            byte[] input = Encoding.Default.GetBytes(M);
            fixed (byte* pinput = input)
            {
                MD5Update(ref Context, pinput, (DWORD)input.Length);
            }
            MD5Digest Result;
            MD5Final(ref Context, out Result);
            return Result;
        }

        // Create digest of file with given Name
        public unsafe static MD5Digest MD5File(string N)
        {
            MD5Context Context;
            MD5Init(ref Context);
            using (FileStream fileStream = new FileStream(N, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 0x1000, FileOptions.SequentialScan))
            {
                using (MemoryMappedFile map = MemoryMappedFile.CreateFromFile(fileStream, null, 0, MemoryMappedFileAccess.Read, null, HandleInheritability.None, false))
                {
                    using (MemoryMappedViewStream viewStream = map.CreateViewStream(0, 0, MemoryMappedFileAccess.Read))
                    {
                        byte* viewPointer = null;
                        try
                        {
                            viewStream.SafeMemoryMappedViewHandle.AcquirePointer(ref viewPointer);
                            MD5Update(ref Context, viewPointer, (DWORD)fileStream.Length);
                        }
                        finally
                        {
                            if (viewPointer != null)
                                viewStream.SafeMemoryMappedViewHandle.ReleasePointer();
                        }
                    }
                }
            }
            MD5Digest Result;
            MD5Final(ref Context, out Result);
            return Result;
        }

        // Create hex representation of given Digest
        public static string MD5Print(MD5Digest D)
        {
            byte I;
            const string Digits = "0123456789abcdef";
            string Result = "";
            for (I = 0; I < 16; I++) Result = Result + Digits[(D[I] >> 4) & 0x0f] + Digits[D[I] & 0x0f];
            return Result;
        }

        // Compare two Digests
        public static bool MD5Match(MD5Digest D1, MD5Digest D2)
        {
            byte I;
            I = 0;
            bool Result = true;
            while (Result && I < 16)
            {
                Result = D1[I] == D2[I];
                I++;
            }
            return Result;
        }
    }
}
