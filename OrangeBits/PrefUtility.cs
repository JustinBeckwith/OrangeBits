using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using Microsoft.WebMatrix.Extensibility;
using Microsoft.WebMatrix.Extensibility.Editor;
using System.IO;
using System.Reflection;
using OrangeBits.UI;
using System.ComponentModel;

namespace OrangeBits
{
    public class PrefUtility
    {
        //--------------------------------------------------------------------------
        //
        //	Properties
        //
        //--------------------------------------------------------------------------

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


        //--------------------------------------------------------------------------
        //
        //	Methods
        //
        //--------------------------------------------------------------------------


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
        /// <param name="prop"></param>
        /// <returns></returns>
        public bool? GetPref(string itemPath, string prop, bool def)
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
                    return def;
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

        #region PathHasValue
        /// <summary>
        /// determines if any options are set on the current node
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool PathHasValue(string path)
        {
            var props = typeof(OptionViewModel).GetProperties().Where(x => Attribute.IsDefined(x, typeof(DefaultValueAttribute)));
            foreach (var prop in props)
            {
                var key = this.getPathKey(path, prop.Name);
                if (this.SitePreferences.ContainsValue(key))
                    return true;
            }
            return false;
        }
        #endregion

        #region LoadOptions
        /// <summary>
        /// 
        /// </summary>
        /// <param name="paths"></param>
        /// <param name="prefs"></param>
        public void LoadOptions(OptionViewModel vm)
        {
            // get all of the properties on this class with the DefaultValue attribute
            var props = typeof(OptionViewModel).GetProperties().Where(x => Attribute.IsDefined(x, typeof(DefaultValueAttribute)));
            foreach (var prop in props)
            {
                // what's the default value for this property?
                bool def = (bool)(prop.GetCustomAttributes(typeof(DefaultValueAttribute), false)[0] as DefaultValueAttribute).Value;
                bool? firstValue = null;

                foreach (var path in vm.Paths)
                {
                    bool? prefValue = this.GetPref(path, prop.Name, def);
                    if (firstValue == null)
                        firstValue = prefValue;

                    if (prefValue != firstValue)
                        prop.SetValue(vm, null, null);
                }
                prop.SetValue(vm, firstValue == null ? def : firstValue, null);
            }
        }
        #endregion

        #region SaveOptions
        /// <summary>
        /// save the true/false options for the given set of paths, clearing any previous downstream values
        /// </summary>
        /// <param name="prefs"></param>
        public void SaveOptions(OptionViewModel vm)
        {
            // clear any settings for the current path or paths that are downstream
            var props = typeof(OptionViewModel).GetProperties().Where(x => Attribute.IsDefined(x, typeof(DefaultValueAttribute)));
            if (vm.OverwriteChildSettings)
            {
                foreach (var path in vm.Paths)
                {
                    this.ClearPrefs(path, props);
                }
            }

            // save new settings for each path and property
            foreach (var path in vm.Paths)
            {
                foreach (var prop in props)
                {
                    var key = this.getPathKey(path, prop.Name);
                    var value = prop.GetValue(vm, null) as bool?;
                    if (value.HasValue)
                    {
                        this.SitePreferences.SetValue(key, value.Value.ToString());
                    }
                }
            }
            this.SitePreferences.Save();
        }
        #endregion
    }
}
