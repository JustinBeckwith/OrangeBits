using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Principal;

using SassAndCoffee.Core;

namespace OrangeBits.Compilers
{   
    internal class ScssCompiler : NodeCompilerBase
    {
        public ScssCompiler()
            : base("/c .\\bin\\node-sass.cmd \"{0}\" \"{1}\"")
        {
          
        }
    }
}
