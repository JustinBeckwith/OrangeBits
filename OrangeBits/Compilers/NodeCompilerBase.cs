using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;


namespace OrangeBits.Compilers
{
    /// <summary>
    /// 
    /// </summary>
    internal abstract class NodeCompilerBase : BaseCompiler
    {
        private readonly string _arguments;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="arguments"></param>
        public NodeCompilerBase(string arguments)
        {
            _arguments = arguments;
        }

        //--------------------------------------------------------------------------
        //
        //	ICompiler Methods
        //
        //--------------------------------------------------------------------------

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inPath"></param>
        /// <param name="outPath"></param>
        public override CompileResults Compile(string inPath, string outPath)
        {
            var workingDir = Directory.GetParent(Assembly.GetExecutingAssembly().Location) + @"\Tools\";
           
            using (Process process = new Process())
            {
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.WorkingDirectory = workingDir;
                process.StartInfo.Arguments = string.Format(_arguments, inPath, outPath);
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;

                StringBuilder output = new StringBuilder();
                StringBuilder error = new StringBuilder();

                using (AutoResetEvent outputWaitHandle = new AutoResetEvent(false))
                using (AutoResetEvent errorWaitHandle = new AutoResetEvent(false))
                {
                    process.OutputDataReceived += (sender, e) =>
                    {
                        if (e.Data == null)
                        {
                            outputWaitHandle.Set();
                        }
                        else
                        {
                            output.AppendLine(e.Data);
                            this.OnOutputDataReceived(new OutputReceivedEventArgs(e.Data, OutputReceivedEventArgs.DataType.STANDARD));                            
                        }
                    };
                    process.ErrorDataReceived += (sender, e) =>
                    {
                        if (e.Data == null)
                        {
                            errorWaitHandle.Set();
                        }
                        else
                        {
                            error.AppendLine(e.Data);
                            this.OnOutputDataReceived(new OutputReceivedEventArgs(e.Data, OutputReceivedEventArgs.DataType.ERROR));
                        }
                    };

                    process.Start();

                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    if (process.WaitForExit(60000) &&
                        outputWaitHandle.WaitOne(60000) &&
                        errorWaitHandle.WaitOne(60000))
                    {
                        if (process.ExitCode != 0)
                        {
                            throw new CompilationException("Compilation failed: " + error.ToString());
                        }                        
                    }
                    else
                    {
                        throw new CompilationException("Compilation timed out");
                    }
                }

                return null;
            }


        }
    }
}


