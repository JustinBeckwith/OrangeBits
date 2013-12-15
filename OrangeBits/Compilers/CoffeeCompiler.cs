using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using OrangeBits;

using SassAndCoffee.Core;
using SassAndCoffee.JavaScript;
using SassAndCoffee.JavaScript.CoffeeScript;

namespace OrangeBits.Compilers
{
    internal class CoffeeCompiler : NodeCompilerBase
    {
        public CoffeeCompiler()
            : base("/c .\\bin\\coffee.cmd -c \"{0}\"")
        {
        }
    }
}
