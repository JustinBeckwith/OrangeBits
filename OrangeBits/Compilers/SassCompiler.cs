using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SassAndCoffee.Core.Sass;

namespace OrangeBits.Compilers
{
    public class SassCompiler : ICompiler
    {
        public void Compile(string inPath, string outPath)
        {            
          SassAndCoffee.Core.Sass.SassCompiler compiler = new SassAndCoffee.Core.Sass.SassCompiler();          
          string output = compiler.Compile(inPath);
          using (StreamWriter sw = new StreamWriter(outPath))
          {
            sw.Write(output);
          }          
        }
    }
}
