using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Principal;

using SassAndCoffee.Core;

namespace OrangeBits.Compilers
{
    public class SassCompiler : ICompiler
    {
        public CompileResults Compile(string inPath, string outPath)
        {
            SassAndCoffee.Ruby.Sass.SassCompiler compiler = new SassAndCoffee.Ruby.Sass.SassCompiler();
            string output = compiler.Compile(inPath, false, null);
            using (StreamWriter sw = new StreamWriter(outPath))
            {
                sw.WriteLine(OrangeBits.GetHeader(inPath));
                sw.Write(output);
            }
			return null;
        }
    }
}
