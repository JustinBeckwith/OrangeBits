using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.IO;
using Microsoft.WebMatrix.Extensibility;
using System.Reflection;


namespace OrangeBits.UI
{
	public class OptionViewModel : ViewModelBase
	{
		//--------------------------------------------------------------------------
		//
		//	Properties
		//
		//--------------------------------------------------------------------------

		#region Properties

		
		public string[] Paths { get; set; }
		public string CombinedPath
		{
			get
			{
				return String.Join(", ", this.Paths);
			}
		}

        public bool OverwriteChildSettings { get; set; }
		
		[DefaultValue(true)]
		public bool? AutoCompileLess { get; set; }

		[DefaultValue(true)]
		public bool? AutoCompileSass { get; set; }

		[DefaultValue(true)]
		public bool? AutoCompileCoffee { get; set; }

		[DefaultValue(false)]
		public bool? AutoMinifyCSS { get; set;  }

		[DefaultValue(false)]
		public bool? AutoMinifyJS { get; set; }

		[DefaultValue(@".\")]
		public string OutputPath { get; set; }
		
		#endregion
	
	}
}
