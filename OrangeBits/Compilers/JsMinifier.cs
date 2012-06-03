using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using OrangeBits;

using Yahoo.Yui.Compressor;

namespace OrangeBits.Compilers
{
    public class JsMinifier : ICompiler
    {       
        public CompileResults Compile(string inPath, string outPath)
        {
            using (StreamReader sr = new StreamReader(inPath))
            {               
                string content = sr.ReadToEnd();
                string output = JavaScriptCompressor.Compress(content);
                using (StreamWriter sw = new StreamWriter(outPath))
                {
                    //sw.WriteLine(OrangeBits.GetHeader(inPath));
                    sw.Write(output);
                }
            }
			return null;
        }
    }
}
