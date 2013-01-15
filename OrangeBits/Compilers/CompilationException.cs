using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OrangeBits.Compilers
{
    public class CompilationException : Exception
    {
        public CompilationException(string message)
            : base(message)
        {

        }
    }
}
