using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace OrangeBits.Compilers
{
    /// <summary>
    /// Top level compiler for all of the file types we support
    /// </summary>
    public class OrangeCompiler
	{
		#region Variables

		/// <summary>
		/// list of supported file extensions
		/// </summary>
		public static string[] supportedExtensions = new string[] { ".less", ".sass", ".scss", ".coffee" };

		#endregion

		#region IsSupportedFileType
		/// <summary>
		/// check if a file at the given path is a supported type 
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static bool IsSupportedFileType(string path)
		{
			FileInfo f = new FileInfo(path);
			return (OrangeCompiler.supportedExtensions.Contains(f.Extension.ToLower()));
		}
		#endregion

		#region GetOutputFilePath
		/// <summary>
		/// for a given input file, figure out what the compiled output path would be
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static string GetOutputFilePath(string path)
		{
			// ensure we got a valid path
			if (String.IsNullOrEmpty(path))
				throw new Exception("The path passed to Orange Compiler must be a valid LESS, CoffeeScript, or Sass file");

			// get the file extension
			FileInfo f = new FileInfo(path);
			string outPath = path.Substring(0, path.LastIndexOf('.'));
			string outExt = (f.Extension.ToLower() == ".coffee") ? ".js" : ".css";
			return string.Format("{0}{1}", outPath, outExt);
		}

		#endregion

		#region Compile
		/// <summary>
        /// based on the extension, create the required compiler and compile
        /// </summary>
        /// <param name="path"></param>
        public static CompileResults Compile(string path)
        {
            ICompiler compiler = null;
            
            // ensure we got a valid path
            if (String.IsNullOrEmpty(path))
                throw new Exception("The path passed to Orange Compiler must be a valid LESS, CoffeeScript, or Sass file");
            
            // get the file extension
            FileInfo f = new FileInfo(path);
            string outPath = path.Substring(0, path.LastIndexOf('.'));
			string outExt = (f.Extension.ToLower() == ".coffee") ? ".js" : ".css";
            switch (f.Extension.ToLower()) 
            {
                case ".less":
                    compiler = new LessCompiler();
                    break;                               
                case ".sass":
                case ".scss":
                    compiler = new SassCompiler();
                    break;
                case ".coffee":
                    compiler = new CoffeeCompiler();                    
                    break;
                default:
                    throw new NotImplementedException();
            }
            
            // create the compiled source
			outPath += outExt;
			bool exists = File.Exists(outPath);

            compiler.Compile(path, outPath);
			
			return new CompileResults()
			{
				Success = true,
				InputPath = path,
				OutputPath = outPath,
				IsNewFile = !exists
			};
		}
		#endregion
	}
}
