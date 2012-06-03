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
    public class CoffeeCompiler : ICompiler
    {       
        public CompileResults Compile(string inPath, string outPath)
        {
            CoffeeScriptCompiler compiler = new CoffeeScriptCompiler(new SassAndCoffee.Core.InstanceProvider<IJavaScriptRuntime>(() => new IEJavaScriptRuntime()));
            using (StreamReader sr = new StreamReader(inPath))
            {               
                string content = sr.ReadToEnd();
                string output = compiler.Compile(content);
                using (StreamWriter sw = new StreamWriter(outPath))
                {
                    sw.WriteLine(OrangeBits.GetHeader(inPath));
                    sw.Write(output);
                }
            }
			return null;
        }
    }
}
