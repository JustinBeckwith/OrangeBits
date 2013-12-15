using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace OrangeBits.Compilers
{
    /// <summary>
    /// use node to compile a typescript file
    /// </summary>
    internal class TypeScriptCompiler : NodeCompilerBase
    {
        public TypeScriptCompiler()
            : base("/c .\\bin\\tsc.cmd \"{0}\"")
        {
        }
    }
}
