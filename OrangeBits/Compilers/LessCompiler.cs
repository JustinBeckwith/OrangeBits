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
using dotless.Core.Parser.Tree;
using dotless.Core.Parser.Infrastructure;
using dotless.Core.Exceptions;


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
		public CompileResults Compile(string inPath, string outPath)
		{
			using (StreamReader sr = new StreamReader(inPath))
			{
				string content = sr.ReadToEnd();
				LessEngine engine = new LessEngine();
				Ruleset ruleset = engine.Parser.Parse(content, inPath);
				Env env = new Env();
				env.Compress = engine.Compress;
				Env env2 = env;
				string output = ruleset.ToCSS(env2);

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


