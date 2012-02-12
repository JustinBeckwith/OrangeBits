using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Windows.Media.Imaging;
using System.IO;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Security.Principal;

using Microsoft.WebMatrix.Extensibility;
using Microsoft.WebMatrix.Extensibility.Editor;

using OrangeBits.Compilers;

namespace OrangeBits
{
	/// <summary>
	/// This is a WebMatrix extension designed to automatically compile less, sass, coffeescript, and stylus.  
	/// </summary>
	[Export(typeof(ExtensionBase))]
	public class OrangeBits : ExtensionBase
	{
		//--------------------------------------------------------------------------
		//
		//	Variables
		//
		//--------------------------------------------------------------------------

		#region Variables
		
		/// <summary>
		/// item that monitors the file system for changes to supported file types by extension
		/// </summary>
		protected FileSystemWatcher _fileSystemWatcher = new FileSystemWatcher();

		/// <summary>
		/// background process that manages the queue of compilation requests
		/// </summary>
		protected Worker _worker;       

		#endregion

		//--------------------------------------------------------------------------
		//
		//	Constructors
		//
		//--------------------------------------------------------------------------

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the OrangeBits class.
		/// </summary>
		public OrangeBits()
			: base("OrangeBits")
		{
			_fileSystemWatcher.IncludeSubdirectories = true;
			_fileSystemWatcher.Changed += new FileSystemEventHandler(SourceFileChanged);			
		}

		#endregion

		//--------------------------------------------------------------------------
		//
		//	Event Handlers
		//
		//--------------------------------------------------------------------------
		 
		#region SourceFileChanged
		/// <summary>
		/// raised any time any file changes - filter for supported file types, and add jobs to the queue
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void SourceFileChanged(object source, FileSystemEventArgs e)
		{
            try
            {
                var ext = (new FileInfo(e.FullPath)).Extension;
                if (Regex.IsMatch(ext, @"\.(less|scss|sass|coffee)", RegexOptions.IgnoreCase))
                {
                    if (e.ChangeType == WatcherChangeTypes.Changed)
                    {
						if (_worker != null)
							_worker.AddItem(e.FullPath);
                    }
                }
            }
            catch (Exception ex)
            {
                WebMatrixHost.ShowNotification("There was an error compiling your file:  " + ex.ToString());
            }
		}

		#endregion

		#region OnWebMatrixHostChanged
		/// <summary>
		/// Called when the WebMatrixHost property changes.
		/// </summary>
		/// <param name="oldValue">Old value.</param>
		/// <param name="newValue">New value.</param>
		protected override void OnWebMatrixHostChanged(IWebMatrixHost oldValue, IWebMatrixHost newValue)
		{
			// Clear old values
			if (null != oldValue)
			{
				oldValue.WorkspaceChanged -= new EventHandler<WorkspaceChangedEventArgs>(WebMatrixHost_WorkspaceChanged);
			}

			base.OnWebMatrixHostChanged(oldValue, newValue);

			// Get new values
			if (null != newValue)
			{
				newValue.WorkspaceChanged += new EventHandler<WorkspaceChangedEventArgs>(WebMatrixHost_WorkspaceChanged);
				_worker = new Worker(newValue);

				newValue.ContextMenuOpening += new EventHandler<ContextMenuOpeningEventArgs>(host_ContextMenuOpening);				
			}
		}

		#endregion 

		#region host_ContextMenuOpening
		/// <summary>
		/// my exalted joy
		/// ISiteFileSystemItem
		/// now the code will work
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void host_ContextMenuOpening(object sender, ContextMenuOpeningEventArgs e)
		{
			// we only show the context menu if every item selected in the tree is valid to be compiled 
			IList<string> paths = new List<string>();
			var showContextMenu = e.Items.Count > 0;
			foreach (ISiteItem item in e.Items) 
			{
				var fsi = item as ISiteFileSystemItem;
				if (fsi != null)
				{
					if (!OrangeCompiler.IsSupportedFileType(fsi.Path))
					{
						showContextMenu = false;
						break;
					}
					else
					{
						paths.Add(fsi.Path);
					}
				}
			}

			// if all of the files in the list are valid, show the compile option
			if (showContextMenu)
			{
				var menuItem = new ContextMenuItem("Compile", null, new DelegateCommand(new Action<object>(AddJob)), paths);
				e.AddMenuItem(menuItem);
			}
		}

		#endregion

		#region AddJob
		/// <summary>
		/// simple handler to add a path to the queue to be processed
		/// </summary>
		/// <param name="e"></param>
		protected void AddJob(object e)
		{
			IList<string> paths = (IList<string>)e;
			if (_worker != null)
			{
				foreach (string path in paths)
					_worker.AddItem(path);
			}

			
		}

		#endregion

		#region WebMatrixHost_WorkspaceChanged
		/// <summary>
		/// Called when the WebMatrixHost's WorkspaceChanged event fires.
		/// </summary>
		/// <param name="sender">Event source.</param>
		/// <param name="e">Event arguments.</param>
		private void WebMatrixHost_WorkspaceChanged(object sender, WorkspaceChangedEventArgs e)
		{
			_fileSystemWatcher.Path = this.WebMatrixHost.WebSite.Path;

            // lame sauce.  this thing can't do multiple file types, so I'm watching them all and
            // parsing the extension on the source change event

			//_fileSystemWatcher.Filter = "*.less, *.stylus, *.sass, *.scss, *.coffee";
			_fileSystemWatcher.EnableRaisingEvents = true;			
		}
		#endregion


        #region GetHeader
        /// <summary>
        /// create a header that will work for both js and css
        /// </summary>
        /// <param name="inPath"></param>
        /// <returns></returns>
        public static string GetHeader(string inPath)
        {
             string header = @"
/* -------------------------------------------------------------------------
 * !!! AUTOMATICALLY GENERATED CODE !!!
 * -------------------------------------------------------------------------
 * This file was automatically generated by the OrangeBits compiler.  
 * Compiled on:  {0}
 * Compiled by: {1}
 * Source: {2}      
 * -------------------------------------------------------------------------*/

";

             return string.Format(header, DateTime.Now, WindowsIdentity.GetCurrent().Name, inPath);

        }
        #endregion


    }
}
