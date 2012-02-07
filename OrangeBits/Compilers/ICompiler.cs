using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OrangeBits.Compilers
{
	interface ICompiler
	{
		void Compile(string inPath, string outPath);
	}
}
