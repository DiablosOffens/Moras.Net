using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Moras.Net
{
    internal static class FontExtensions
    {
        public static Font CloneWithFamily(this Font font, FontFamily family)
        {
            return new Font(family, font.Size, font.Style, font.Unit);
        }

        public static Font CloneWithBold(this Font font, bool bold)
        {
            return new Font(font.FontFamily, font.Size,
                bold ? font.Style | FontStyle.Bold : (font.Style & (~FontStyle.Bold)), font.Unit);
        }

        public static Font CloneWithItalic(this Font font, bool italic)
        {
            return new Font(font.FontFamily, font.Size,
                italic ? font.Style | FontStyle.Italic : (font.Style & (~FontStyle.Italic)), font.Unit);
        }

        public static Font CloneWithUnderline(this Font font, bool underline)
        {
            return new Font(font.FontFamily, font.Size,
                underline ? font.Style | FontStyle.Underline : (font.Style & (~FontStyle.Underline)), font.Unit);
        }

        public static Font CloneWithSize(this Font font, float size)
        {
            return new Font(font.FontFamily, size, font.Style, font.Unit);
        }
    }
}
