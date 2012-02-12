using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.ComponentModel;
using System.Collections.Specialized;
using System.IO;
using System.Security.Principal;

using dotless.Core;

namespace OrangeBits.Compilers
{
    /// <summary>
    /// 
    /// </summary>
	public class LessCompiler : ICompiler
	{
        /// <summary>
        /// 
        /// </summary>
        /// <param name="inPath"></param>
        /// <param name="outPath"></param>
		public void Compile(string inPath, string outPath) 
		{            
			using (StreamReader sr = new StreamReader(inPath))
			{
				string content = sr.ReadToEnd();
				string output = Less.Parse(content);				
				using (StreamWriter sw = new StreamWriter(outPath))
				{
                    sw.WriteLine(OrangeBits.GetHeader(inPath));
					sw.Write(output);
				}
			}
		}
	}
}
