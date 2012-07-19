using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OrangeBits
{
	/// <summary>
	/// simple structure to hold a file change request
	/// </summary>
	public class OrangeJob
	{
		public enum JobType
		{
			Compile,
			Minify,
			Optimize
		}

		public enum JobSource
		{
			Save,
			Context
		}

		public JobType Type { get; set; }
		public JobSource Source { get; set; }
		public string Path { get; set; }
		public DateTime Time { get; set; }

		public OrangeJob()
		{
			this.Time = DateTime.Now;
		}
	}
}
