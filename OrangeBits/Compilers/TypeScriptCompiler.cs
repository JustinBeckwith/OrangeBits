using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace OrangeBits.Compilers
{
    /// <summary>
    /// use node to compile a typescript file
    /// </summary>
    public class TypeScriptCompiler : ICompiler
    {
        public CompileResults Compile(string inPath, string outPath)
        {                                 
            var workingDir = Directory.GetParent(Assembly.GetExecutingAssembly().Location) + @"\Tools\";
            var info = new ProcessStartInfo();
            info.UseShellExecute = false;
            info.FileName = "cmd.exe";
            info.WindowStyle = ProcessWindowStyle.Hidden;
            info.CreateNoWindow = true;
            info.WorkingDirectory = workingDir;
            info.Arguments = "/c .\\.bin\\tsc.cmd \"" + inPath + "\"";
            info.RedirectStandardError = true;

            // Use Process for the application.
            using (var p = Process.Start(info))
            {                
                var output = p.StandardError.ReadToEnd();
                if (!String.IsNullOrEmpty(output))
                {
                    throw new CompilationException(output);
                }
                
                p.WaitForExit();
            }

           return null;
        }
    }
}
