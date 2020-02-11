using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Moras.Net
{
    public class IFStreamWrapper : StreamReader
    {
        public IFStreamWrapper(FileStream stream)
            : base(stream, Encoding.Default)
        {

        }

        public IFStreamWrapper(string path)
            : base(new FileStream(path, FileMode.Open, FileAccess.Read), Encoding.Default)
        {

        }

        public IFStreamWrapper(string path, FileMode mode, FileAccess access)
            : base(new FileStream(path, mode, access), Encoding.Default)
        {

        }

        // fstream opens files in text mode, which means it converts \n to
        // the actual line endings for the current os. There is a binary mode,
        // but it's not the default, so do the same here.

        public override int Read()
        {
            int result = base.Read();
            if (result == -1)
                return result;
            char c = (char)result;
            if (c == '\r')
                return Peek() == '\n' ? base.Read() : '\n';
            if (c == '\n' && Peek() == '\r')
                base.Read();
            return c;
        }
    }

    public class FStreamWrapper : StreamWriter
    {
        public FStreamWrapper(FileStream stream)
            : base(stream, Encoding.Default)
        {

        }

        public FStreamWrapper(string path, FileMode mode, FileAccess access)
            : base(new FileStream(path, mode, access), Encoding.Default)
        {

        }

        public static FStreamWrapper operator +(FStreamWrapper lhs, char rhs)
        {
            lhs.Write(rhs);
            return lhs;
        }

        public static FStreamWrapper operator +(FStreamWrapper lhs, int rhs)
        {
            lhs.Write(rhs);
            return lhs;
        }

        public static FStreamWrapper operator +(FStreamWrapper lhs, string rhs)
        {
            lhs.Write(rhs);
            return lhs;
        }

        // fstream opens files in text mode, which means it converts \n to
        // the actual line endings for the current os. There is a binary mode,
        // but it's not the default, so do the same here.

        public override void Write(char value)
        {
            if (value == '\n' || value == '\r')
                base.Write(CoreNewLine);
            else

                base.Write(value);
        }

        private char[] ConvertNewLine(char[] buffer, int index, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("index", "Index must not be negative.");
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", "Count must not be negative.");
            }
            if ((buffer.Length - index) < count)
            {
                throw new ArgumentException("Index and count must be inside the dimensions of buffer.");
            }

            char[] temp = new char[count * 2];
            int newcount = 0;
            int end = count + index;
            for (int i = index; i < end; i++)
            {
                if (buffer[i] == '\r')
                {
                    if ((i + 1) < end && buffer[i + 1] == '\n')
                        i++;
                    Array.Copy(CoreNewLine, 0, temp, newcount, CoreNewLine.Length);
                    newcount += CoreNewLine.Length;
                }
                else if (buffer[i] == '\n')
                {
                    if ((i + 1) < end && buffer[i + 1] == '\r')
                        i++;
                    Array.Copy(CoreNewLine, 0, temp, newcount, CoreNewLine.Length);
                    newcount += CoreNewLine.Length;
                }
                else
                    temp[newcount++] = buffer[i];
            }
            char[] result = new char[newcount];
            Array.Copy(temp, result, newcount);
            return result;
        }

        public override void Write(char[] buffer)
        {
            buffer = ConvertNewLine(buffer, 0, buffer.Length);
            base.Write(buffer);
        }

        public override void Write(char[] buffer, int index, int count)
        {
            buffer = ConvertNewLine(buffer, index, count);
            base.Write(buffer);
        }

        public override void Write(string value)
        {
            base.Write(Utils.NormalizeLineEndings(value, NewLine));
        }
    }
}
