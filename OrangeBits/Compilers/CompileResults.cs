using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OrangeBits.Compilers
{
	/// <summary>
	/// basic structure to store results of a compilation
	/// </summary>
	public class CompileResults
	{
		/// <summary>
		/// notes the compilation was successful
		/// </summary>
		public bool Success { get; set; }

		/// <summary>
		/// notes if the new file was created for the first time
		/// </summary>
		public bool IsNewFile { get; set; }

		/// <summary>
		/// original source input file path
		/// </summary>
		public string InputPath { get; set; }

		/// <summary>
		/// new compiled output file path
		/// </summary>
		public string OutputPath { get; set; }
	}
}
