using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using Microsoft.WebMatrix.Extensibility;
using Microsoft.WebMatrix.Extensibility.Editor;
using System.IO;
using System.Reflection;

namespace OrangeBits
{
    public class PrefUtility
    {
        #region Properties

        /// <summary>
        /// 
        /// </summary>
        public IPreferences SitePreferences { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string SitePath { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string ExtensionName { get; set; }

        #endregion


        


        #region getPathKey
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public string getPathKey(string path, string propertyName)
        {
            // remove the root path of the site, just give me the relative path
            var smallPath = path.Replace(SitePath, "");
            return string.Format("[{0}].[{1}].{2}", ExtensionName, smallPath, propertyName);
        }
        #endregion

        #region GetPref
        /// <summary>
        /// Get the preference value for a given property and path, walking up the tree
        /// </summary>
        /// <param name="item"></param>
        /// <param name="prefs"></param>
        /// <param name="prop"></param>
        /// <param name="def"></param>
        /// <returns></returns>
        public bool? GetPref(string itemPath, string prop, string def)
        {
            var path = itemPath;
            do
            {
                var key = this.getPathKey(path, prop);
                if (this.SitePreferences.ContainsValue(key))
                {
                    var value = this.SitePreferences.GetValue(key);
                    if (value != null)
                        return bool.Parse(value);
                }

                if (path.ToLowerInvariant() == this.SitePath.ToLowerInvariant())
                {
                    break;
                }
                else
                {
                    path = Directory.GetParent(path).FullName;
                }

            } while (true);

            return null;
        }
        #endregion

        #region ClearPrefs
        /// <summary>
        /// clear all of the preferences for a given file path and it's children
        /// </summary>
        /// <param name="item"></param>
        /// <param name="prefs"></param>
        /// <returns></returns>
        public void ClearPrefs(string path, IEnumerable<PropertyInfo> props)
        {
            // clear all preferences for the current path
            foreach (var prop in props)
            {
                var key = this.getPathKey(path, prop.Name);
                this.SitePreferences.ClearValue(key);
            }

            // if this is a directory, clear the settings for an child object recursively
            bool isDir = (File.GetAttributes(path) & FileAttributes.Directory) == FileAttributes.Directory;
            if (isDir)
            {
                var children = (new DirectoryInfo(path)).GetFileSystemInfos();
                foreach (var child in children)
                {
                    ClearPrefs(child.FullName, props);
                }
            }
        }
        #endregion
    }
}
