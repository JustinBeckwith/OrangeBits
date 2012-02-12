using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SassAndCoffee.Core;
using SassAndCoffee.Core.CoffeeScript;
using SassAndCoffee.Core.Pipeline;
using SassAndCoffee.Core.Pooling;
using OrangeBits;

namespace OrangeBits.Compilers
{
    public class CoffeeCompiler : ICompiler
    {       
        public void Compile(string inPath, string outPath)
        {                        
            SassAndCoffee.Core.CoffeeScript.CoffeeScriptCompiler compiler = new CoffeeScriptCompiler(new InstanceProvider<IJavaScriptRuntime>(() => new IEJavaScriptRuntime()));
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
        }
    }
}
