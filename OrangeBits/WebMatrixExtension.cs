using System;
using System.Drawing;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Windows.Media.Imaging;
using System.IO;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Security.Principal;
using System.Linq;

using Microsoft.WebMatrix.Extensibility;
using Microsoft.WebMatrix.Extensibility.Editor;

using OrangeBits.Compilers;
using System.Windows;

namespace OrangeBits
{
	/// <summary>
	/// This is a WebMatrix extension designed to automatically compile less, sass, coffeescript, and stylus.  
	/// </summary>
	[Export(typeof(Extension))]
	public class OrangeBits : Extension
	{
		//--------------------------------------------------------------------------
		//
		//	Variables
		//
		//--------------------------------------------------------------------------

		#region Variables

		/// <summary>
		/// unique identifier for this extension
		/// </summary>
		private Guid ExtensionId = Guid.Parse("ee0fff40-b3f5-473b-9149-aa31bb0f90c3");

		/// <summary>
		/// item that monitors the file system for changes to supported file types by extension
		/// </summary>
		private ISiteFileWatcherService _siteFileWatcher;

		/// <summary>
		/// background process that manages the queue of compilation requests
		/// </summary>
		protected Worker _worker;

		/// <summary>
		/// 
		/// </summary>
		protected IWebMatrixHost _host;

		/// <summary>
		/// 
		/// </summary>
		[Import(typeof(ISiteFileWatcherService))]
		private ISiteFileWatcherService SiteFileWatcherService
		{
			get
			{
				return _siteFileWatcher;
			}
			set
			{
				_siteFileWatcher = value;                
			}
		}

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
		}

		#endregion

		//--------------------------------------------------------------------------
		//
		//	Event Handlers
		//
		//--------------------------------------------------------------------------

		#region Initialize
		/// <summary>
		/// get a reference to the host when the app starts
		/// </summary>
		/// <param name="host"></param>
		protected override void Initialize(IWebMatrixHost host, ExtensionInitData data)
		{
			// add host event handlers
			_host = host;
			if (host != null)
			{
				host.WebSiteChanged += new EventHandler<EventArgs>(WebMatrixHost_WebSiteChanged);
				_worker = new Worker(host);

				host.ContextMenuOpening += new EventHandler<ContextMenuOpeningEventArgs>(host_ContextMenuOpening);

				// ensure the site watcher is fired up the first time the extension is installed
				if (host.WebSite != null && !String.IsNullOrEmpty(_host.WebSite.Path))
				{
					_siteFileWatcher.RegisterForSiteNotifications(WatcherChangeTypes.All, new FileSystemEventHandler(SourceFileChanged), null);
				}
			}
		}
		#endregion

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
							_worker.AddItem(new OrangeJob()
							{
								Path = e.FullPath,
								Type = OrangeJob.JobType.Compile
							});
					}
				}
			}
			catch (Exception ex)
			{
				_host.ShowNotification("There was an error compiling your file:  " + ex.ToString());
			}
		}

		#endregion

		#region host_ContextMenuOpening
		/// <summary>
		/// add context menu options based on the capabilities of the selected files
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void host_ContextMenuOpening(object sender, ContextMenuOpeningEventArgs e)
		{
			AddCompileMenu(e);
			AddMinifyMenu(e);
			AddOptimizeMenu(e);
			AddCopyDataURIMenu(e);
		}

		#endregion

		#region AddCompileMenu
		/// <summary>
		/// 
		/// </summary>
		/// <param name="e"></param>
		protected void AddCompileMenu(ContextMenuOpeningEventArgs e)
		{
			// we only show the context menu if every item selected in the tree is valid to be compiled 
			var jobs = new List<OrangeJob>();
			var showMenu = e.Items.Count > 0;
			foreach (ISiteItem item in e.Items)
			{
				var fsi = item as ISiteFileSystemItem;
				if (fsi != null)
				{
					if (!OrangeCompiler.CanCompile(fsi.Path))
					{
						showMenu = false;
						break;
					}
					else
					{
						jobs.Add(new OrangeJob() {
							Path = fsi.Path,
							Type = OrangeJob.JobType.Compile
						});
					}
				}
			}

			// if all of the files in the list are valid, show the compile option
			if (showMenu)
			{
				var menuItem = new ContextMenuItem("Compile", null, new DelegateCommand(new Action<object>(AddJob)), jobs);
				e.AddMenuItem(menuItem);
			}
		}
		#endregion

		#region AddMinifyMenu
		/// <summary>
		/// 
		/// </summary>
		/// <param name="e"></param>
		protected void AddMinifyMenu(ContextMenuOpeningEventArgs e)
		{
			var jobs = new List<OrangeJob>();
			var showMinify = e.Items.Count > 0;
			foreach (ISiteItem item in e.Items)
			{
				var fsi = item as ISiteFileSystemItem;
				if (fsi != null)
				{
					if (!OrangeCompiler.CanMinify(fsi.Path))
					{
						showMinify = false;
						break;
					}
					else
					{
						jobs.Add(new OrangeJob()
						{
							Path = fsi.Path,
							Type = OrangeJob.JobType.Minify
						});
					}
				}
			}
			if (showMinify)
			{
				var menuItem = new ContextMenuItem("Minify", null, new DelegateCommand(new Action<object>(AddJob)), jobs);
				e.AddMenuItem(menuItem);
			}
		}
		#endregion

		#region AddOptimizeMenu
		/// <summary>
		/// 
		/// </summary>
		/// <param name="e"></param>
		protected void AddOptimizeMenu(ContextMenuOpeningEventArgs e)
		{
			var jobs = new List<OrangeJob>();
			foreach (ISiteItem item in e.Items)
			{
				var fsi = item as ISiteFileSystemItem;
				if (fsi != null)
				{
					if (fsi is ISiteFolder)
					{
						// get all of the pngs under the selected path and add a job
						var dir = new DirectoryInfo((fsi as ISiteFolder).Path);
						var files = OrangeCompiler.supportedOptimizeExtensions.SelectMany(x => dir.GetFiles("*" + x, SearchOption.AllDirectories));
						
						foreach (var f in files)
						{
							jobs.Add(new OrangeJob()
							{
								Path = f.FullName,
								Type = OrangeJob.JobType.Optimize
							});
						}
					}
					else if (OrangeCompiler.CanOptimize(fsi.Path))
					{
						jobs.Add(new OrangeJob()
						{
							Path = fsi.Path,
							Type = OrangeJob.JobType.Optimize
						});
					}
				}
			}

			if (jobs.Count > 0)
			{
				var tail = jobs.Count > 1 ? "s" : "";
				var menuItem = new ContextMenuItem("Optimize Image" + tail, null, new DelegateCommand(new Action<object>(AddJob)), jobs);
				e.AddMenuItem(menuItem);
			}
		}
		#endregion

		#region AddCopyDataURIMenu
		/// <summary>
		/// If the selected file can be compressed to a data uri, this will place it on the clipboard
		/// </summary>
		/// <param name="e"></param>
		protected void AddCopyDataURIMenu(ContextMenuOpeningEventArgs e)
		{			
			if (e.Items.Count == 1) {
				var item = e.Items.First();
				if (item is ISiteFile) {
					var path = (item as ISiteFile).Path;
					if (OrangeCompiler.CanGetDataURI(path))
					{						
						var menuItem = new ContextMenuItem("Copy Data URI", null, new DelegateCommand((filePath) => {
							var scheduler = TaskScheduler.FromCurrentSynchronizationContext();
							Task.Factory.StartNew(() =>
							{
								var data = "data:image/" +
								Path.GetExtension(filePath as string).Replace(".", "") +
								";base64," +
								Convert.ToBase64String(File.ReadAllBytes(filePath as string));
								return data;
							}).ContinueWith((x) => {
								Clipboard.SetText(x.Result);
							}, scheduler);							
						}), path);
						e.AddMenuItem(menuItem);
					}
				}
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
			IList<OrangeJob> jobs = (IList<OrangeJob>)e;
			if (_worker != null)
			{
				foreach (OrangeJob job in jobs)
					_worker.AddItem(job);
			}


		}

		#endregion

		#region WebMatrixHost_WebSiteChanged
		/// <summary>
		/// Called when the currently loaded website changes
		/// </summary>
		/// <param name="sender">Event source.</param>
		/// <param name="e">Event arguments.</param>
		private void WebMatrixHost_WebSiteChanged(object sender, EventArgs e)
		{
			if (_host != null && _host.WebSite != null && !String.IsNullOrEmpty(_host.WebSite.Path))
			{
				_siteFileWatcher.RegisterForSiteNotifications(WatcherChangeTypes.All, new FileSystemEventHandler(SourceFileChanged), null);                                
			}
			else
			{
				_siteFileWatcher.DeregisterForSiteNotifications(WatcherChangeTypes.All, new FileSystemEventHandler(SourceFileChanged), null);
			}
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
