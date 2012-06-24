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
	class OptionViewModel : ViewModelBase
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
		
		#endregion

		//--------------------------------------------------------------------------
		//
		//	Methods
		//
		//--------------------------------------------------------------------------

		#region LoadOptions
		/// <summary>
		/// 
		/// </summary>
		/// <param name="paths"></param>
		/// <param name="prefs"></param>
		public void LoadOptions(IEnumerable<ISiteItem> paths, PrefUtility prefUtility)
		{
			// get all of the properties on this class with the DefaultValue attribute
			var props = this.GetType().GetProperties().Where(x => Attribute.IsDefined(x, typeof(DefaultValueAttribute)));
			foreach (var prop in props)
			{
				// what's the default value for this property?
				bool def = (bool)(prop.GetCustomAttributes(typeof(DefaultValueAttribute), false)[0] as DefaultValueAttribute).Value;
			    bool? firstValue = null;				
				
			    foreach (var path in paths)
			    {					
					bool? prefValue = prefUtility.GetPref((path as ISiteFileSystemItem).Path, prop.Name, def.ToString());
					if (firstValue == null)
						firstValue = prefValue;

					if (prefValue != firstValue)
						prop.SetValue(this, null, null);					
			    }
				prop.SetValue(this, firstValue == null ? def : firstValue, null);					
			}			
		}
		#endregion

        #region SaveOptions
        /// <summary>
        /// save the true/false options for the given set of paths, clearing any previous downstream values
        /// </summary>
        /// <param name="prefs"></param>
        public void SaveOptions(PrefUtility prefUtility)
        {
            // clear any settings for the current path or paths that are downstream
            var props = this.GetType().GetProperties().Where(x => Attribute.IsDefined(x, typeof(DefaultValueAttribute)));
            foreach (var path in Paths)
                prefUtility.ClearPrefs(path, props);

            // save new settings for each path and property
            foreach (var path in Paths)
            {
                foreach (var prop in props)
                {
                    var key = prefUtility.getPathKey(path, prop.Name);
                    var value = prop.GetValue(this, null) as bool?;
                    if (value.HasValue)
                    {
                        prefUtility.SitePreferences.SetValue(key, value.Value.ToString());
                    }
                }
            }
            prefUtility.SitePreferences.Save();
        }
        #endregion



	}
}
