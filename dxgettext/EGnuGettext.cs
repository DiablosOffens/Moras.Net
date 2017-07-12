using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dxgettext
{
    public class EGnuGettext : Exception
    {
        public EGnuGettext(string message)
            : base(message)
        {
        }
    }
    public class EGGProgrammingError : EGnuGettext
    {
        public EGGProgrammingError(string message)
            : base(message)
        {
        }
    }
    public class EGGComponentError : EGnuGettext
    {
        public EGGComponentError(string message)
            : base(message)
        {
        }
    }
    public class EGGIOError : EGnuGettext
    {
        public EGGIOError(string message)
            : base(message)
        {
        }
    }
    public class EGGAnsi2WideConvError : EGnuGettext
    {
        public EGGAnsi2WideConvError(string message)
            : base(message)
        {
        }
    }
}
