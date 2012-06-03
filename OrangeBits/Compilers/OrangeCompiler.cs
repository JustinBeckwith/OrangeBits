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
		public static string[] supportedCompileExtensions = new string[] { ".less", ".sass", ".scss", ".coffee" };
		public static string[] supportedMinifyExtensions = new string[] { ".js", ".css" };
		public static string[] supportedOptimizeExtensions = new string[] { ".png", ".bmp", ".gif" };
		public static string[] supportedDataURIExtensions = new string[] { ".png", ".bmp", ".gif", ".jpg", ".jpeg" };

		#endregion

		#region CanCompile
		/// <summary>
		/// check if a file at the given path is a supported type 
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static bool CanCompile(string path)
		{
			FileInfo f = new FileInfo(path);
			return (OrangeCompiler.supportedCompileExtensions.Contains(f.Extension.ToLower()));
		}
		#endregion

		#region CanMinify
		/// <summary>
		/// check if a file at the given path is a supported type 
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static bool CanMinify(string path)
		{
			FileInfo f = new FileInfo(path);
			return (OrangeCompiler.supportedMinifyExtensions.Contains(f.Extension.ToLower()));
		}
		#endregion

		#region CanOptimize
		/// <summary>
		/// check if a file at the given path is a supported type 
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static bool CanOptimize(string path)
		{
			FileInfo f = new FileInfo(path);
			return (OrangeCompiler.supportedOptimizeExtensions.Contains(f.Extension.ToLower()));
		}
		#endregion

		#region CanGetDataURI
		/// <summary>
		/// check if a file at the given path is a supported type 
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static bool CanGetDataURI(string path)
		{
			FileInfo f = new FileInfo(path);
			return (OrangeCompiler.supportedDataURIExtensions.Contains(f.Extension.ToLower()));
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

		#region Process
		/// <summary>
		/// based on the extension, create the required compiler and compile
		/// </summary>
		/// <param name="path"></param>
		public static CompileResults Process(OrangeJob job)
		{
			ICompiler compiler = null;
			
			// ensure we got a valid path
			if (String.IsNullOrEmpty(job.Path))
				throw new Exception("The path passed to Orange Compiler must be a valid LESS, CoffeeScript, or Sass file");
			
			// get the file extension
			FileInfo f = new FileInfo(job.Path);
			string outPath = job.Path.Substring(0, job.Path.LastIndexOf('.'));
			string outExt = "";
			switch (job.Type)
			{
				case OrangeJob.JobType.Compile:
					outExt = (f.Extension.ToLower() == ".coffee") ? ".js" : ".css";
					break;
				case OrangeJob.JobType.Minify:
					outExt = (f.Extension.ToLower() == ".css") ? ".min.css" : ".min.js";
					break;
				case OrangeJob.JobType.Optimize:
					outExt = f.Extension.ToLower();
					break;
			}

			
			switch (f.Extension.ToLower()) 
			{
				case ".js":
					compiler = new JsMinifier();
					break;
				case ".css":
					compiler = new CssMinifier();
					break;
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
				case ".png":
				case ".gif":
				case ".tiff":
				case ".bmp":
					compiler = new PNGCompressor();
					break;
				case ".jpg":
				case ".jpeg":
					compiler = new JPGCompressor();
					break;
				default:
					throw new NotImplementedException();
			}
			
			// create the compiled source
			outPath += outExt;
			bool exists = File.Exists(outPath);

			var results = compiler.Compile(job.Path, outPath);
			if (results == null)
			{
				results = new CompileResults()
				{
					Success = true,
					InputPath = job.Path,
					OutputPath = outPath,
					IsNewFile = !exists,
					Message = "Compiled"
				};
			}
			return results;
			
		}
		#endregion
	}
}
