using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OrangeBits
{
    internal class OutputReceivedEventArgs : EventArgs
    {
        public string Output { get; set; }
        public DataType OutputType { get; set; }

        public OutputReceivedEventArgs(string output, DataType outputType)
        {
            this.OutputType = outputType;
            this.Output = output;
        }

        public enum DataType
        {
            ERROR,
            STANDARD
        }
    }
}
