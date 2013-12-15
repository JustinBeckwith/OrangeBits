using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace OrangeBits.Compilers
{
    internal abstract class BaseCompiler : ICompiler
    {
        public event EventHandler<OutputReceivedEventArgs> OutputDataReceived;
        
        protected virtual void OnOutputDataReceived(OutputReceivedEventArgs e)
        {
            EventHandler<OutputReceivedEventArgs> handler = OutputDataReceived;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public abstract CompileResults Compile(string inPath, string outPath);
    }
}
