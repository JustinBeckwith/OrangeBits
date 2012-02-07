using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OrangeBits
{
	/// <summary>
	/// simple structure to hold a file change request
	/// </summary>
    public class FileChangeItem
    {
        public string Path { get; set; }
        public DateTime Time { get; set; }
    }
}
