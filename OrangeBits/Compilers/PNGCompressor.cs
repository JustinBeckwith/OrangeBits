using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using OrangeBits;

namespace OrangeBits.Compilers
{
	public class PNGCompressor : ICompiler
	{
		public CompileResults Compile(string inPath, string outPath)
		{
			var success = false;
			var message = "Optimization Failed.";
			var details = "";

			var info = new ProcessStartInfo();
			info.UseShellExecute = false;
			info.FileName = "cmd.exe";
			info.WindowStyle = ProcessWindowStyle.Hidden;
			info.CreateNoWindow = true;
            info.Arguments = "/c " + Directory.GetParent(Assembly.GetExecutingAssembly().Location) + "\\Tools\\optipng.exe -clobber \"" + inPath + "\"";
			info.RedirectStandardError = true;

			// Use Process for the application.
			using (var p = Process.Start(info))
			{
				// for reasons beyond my understanding, optipng only seems to write to StandardError, not StandardOutput ::shrugs::
				details = p.StandardError.ReadToEnd();
				if (String.IsNullOrEmpty(details))
				{
					message = "Optimization failed.";
				}
				else if (details.Contains("already optimized"))
				{
					message = "Image already optmized.";
				}
				else
				{
					var lines = details.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
					if (lines.Length > 0)
					{
						message = "Image Optimized:  " + lines[lines.Length - 1];
						success = true;
					}
				}

				p.WaitForExit();
			}

			return new CompileResults()
			{
				InputPath = inPath,
				OutputPath = outPath,
				Success = success,
				IsNewFile = false,
				Message = message,
				Details = details
			};
		}
	}
}