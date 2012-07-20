using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.WebMatrix.Extensibility;
using OrangeBits.Compilers;
using OrangeBits.UI;

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

        /// <summary>
        /// Get all of the information needed to load preferences
        /// </summary>        
        protected PrefUtility prefUtility { get; set; }


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
                InitSite();
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
                if (Regex.IsMatch(ext, @"\.(less|scss|sass|coffee|js|css)", RegexOptions.IgnoreCase))
                {
                    if (e.ChangeType == WatcherChangeTypes.Changed)
                    {
                        bool? doIt = null;
                        OrangeJob.JobType jobType = OrangeJob.JobType.Compile;

                        switch (ext.ToLowerInvariant())
                        {
                            case ".less":
                                doIt = prefUtility.GetPref(e.FullPath, "AutoCompileLess", true) as bool?;
                                break;
                            case ".scss":
                                doIt = prefUtility.GetPref(e.FullPath, "AutoCompileScss", true) as bool?;
                                break;
                            case ".sass":
                                doIt = prefUtility.GetPref(e.FullPath, "AutoCompileSass", true) as bool?;
                                break;
                            case ".coffee":
                                doIt = prefUtility.GetPref(e.FullPath, "AutoCompileCoffee", true) as bool?;
                                break;
                            case ".js":
                                doIt = e.FullPath.EndsWith(".min.js") ? false : prefUtility.GetPref(e.FullPath, "AutoMinifyJS", false) as bool?;
                                jobType = OrangeJob.JobType.Minify;
                                break;
                            case ".css":
                                doIt = e.FullPath.EndsWith(".min.css") ? false : prefUtility.GetPref(e.FullPath, "AutoMinifyCSS", false) as bool?;
                                jobType = OrangeJob.JobType.Minify;
                                break;
                        }

                        if (doIt.HasValue && doIt.Value && _worker != null)
                        {
                            _worker.AddItem(new OrangeJob()
                            {
                                Path = e.FullPath,
								OutputPath = GetOutputPath(e.FullPath),
                                Type = jobType,
								Source = OrangeJob.JobSource.Save
                            });

                        }
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
            AddMultiMenu(e, OrangeCompiler.supportedCompileExtensions, OrangeJob.JobType.Compile, "Compile", "Compile");
            AddMultiMenu(e, OrangeCompiler.supportedMinifyExtensions, OrangeJob.JobType.Minify, "Minify", "Minify");
            AddMultiMenu(e, OrangeCompiler.supportedOptimizeExtensions, OrangeJob.JobType.Optimize, "Optimize Image", "Optimize Images");
            AddCopyDataURIMenu(e);
            AddOptions(e);
        }

        #endregion

        #region AddOptions
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected void AddOptions(ContextMenuOpeningEventArgs e)
        {
            // we only show the context menu if every item selected in the tree is valid to be compiled 
            var jobs = new List<OrangeJob>();
            var showMenu = e.Items.Count > 0;
            Type itemType = null;

            // determine if we're dealing with folders or files.  If it's mixed, bug out
            foreach (ISiteItem item in e.Items)
            {
                if (itemType == null)
                {
                    itemType = item.GetType();
                }
                else if (itemType != item.GetType())
                {
                    return;
                }
            }

            var menuItem = new ContextMenuItem("OrangeBits Options", null, new DelegateCommand((items) =>
            {
                var selectedItems = items as IEnumerable<ISiteItem>;
                var dialog = new OptionsUI();
                var vm = new OptionViewModel()
                {
                    Paths = selectedItems.Select(x => (x as ISiteFileSystemItem).Path).ToArray()
                };

                prefUtility.LoadOptions(vm);
                dialog.DataContext = vm;
                var result = _host.ShowDialog("OrangeBits Options", dialog);
                if (result.HasValue && result.Value)
                {
                    prefUtility.SaveOptions(vm);
                }

            }), e.Items);
            e.AddMenuItem(menuItem);
        }
        #endregion

        #region AddMultiMenu
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected void AddMultiMenu(ContextMenuOpeningEventArgs e, string[] supportedExtensions, OrangeJob.JobType jobType, string title, string multipleTitle)
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
                        var files = supportedExtensions.SelectMany(x => dir.GetFiles("*" + x, SearchOption.AllDirectories));

                        foreach (var f in files)
                        {
                            if (jobType != OrangeJob.JobType.Minify || (!f.FullName.EndsWith(".min.js") && !f.FullName.EndsWith(".min.css")))
                            {
                                jobs.Add(new OrangeJob()
                                {
                                    Path = f.FullName,
									OutputPath = GetOutputPath(f.FullName),
                                    Type = jobType,
									Source = OrangeJob.JobSource.Context
                                });
                            }
                        }
                    }
                    else if (supportedExtensions.Contains((new FileInfo(fsi.Path)).Extension.ToLower()))
                    {
                        if (jobType != OrangeJob.JobType.Minify || (!fsi.Path.EndsWith(".min.js") && !fsi.Path.EndsWith(".min.css")))
                        {
                            jobs.Add(new OrangeJob()
                            {
                                Path = fsi.Path,
								OutputPath = GetOutputPath(fsi.Path),
                                Type = jobType,
								Source = OrangeJob.JobSource.Context
                            });
                        }
                    }
                }
            }

            if (jobs.Count > 0)
            {
                var menuTitle = jobs.Count > 1 ? multipleTitle : title;
                var menuItem = new ContextMenuItem(menuTitle, null, new DelegateCommand(new Action<object>(AddJob)), jobs);
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
            if (e.Items.Count == 1)
            {
                var item = e.Items.First();
                if (item is ISiteFile)
                {
                    var path = (item as ISiteFile).Path;
                    if (OrangeCompiler.CanGetDataURI(path))
                    {
                        var menuItem = new ContextMenuItem("Copy Data URI", null, new DelegateCommand((filePath) =>
                        {
                            var scheduler = TaskScheduler.FromCurrentSynchronizationContext();
                            Task.Factory.StartNew(() =>
                            {
                                var data = "data:image/" +
                                Path.GetExtension(filePath as string).Replace(".", "") +
                                ";base64," +
                                Convert.ToBase64String(File.ReadAllBytes(filePath as string));
                                return data;
                            }).ContinueWith((x) =>
                            {
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
            InitSite();
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

        #region InitSite
        /// <summary>
        /// set up the file watcher for detecting changes
        /// </summary>
        protected void InitSite()
        {
            if (_host != null && _host.WebSite != null && !String.IsNullOrEmpty(_host.WebSite.Path))
            {
                prefUtility = new PrefUtility()
                {
                    ExtensionName = "OrangeBits",
                    SitePath = _host.WebSite.Path,
                    SitePreferences = _host.WebSite.SitePreferences
                };

                // by default, do not do anything to node_modules
                var path = Path.Combine(_host.WebSite.Path, "node_modules");
                var isSet = prefUtility.PathHasValue(path);
                if (!isSet)
                {
                    var props = typeof(OptionViewModel).GetProperties().Where(x => Attribute.IsDefined(x, typeof(DefaultValueAttribute)));
                    OptionViewModel vm = new OptionViewModel()
                    {
                        Paths = new string[] { path }
                    };

                    foreach (var prop in props)
                    {
						if (prop.PropertyType == typeof(bool?)) 
							prop.SetValue(vm, false, null);
                    }
                    prefUtility.SaveOptions(vm);
                }


                _siteFileWatcher.RegisterForSiteNotifications(WatcherChangeTypes.Changed | WatcherChangeTypes.Created, new FileSystemEventHandler(SourceFileChanged), null);
            }
            else
            {
                _siteFileWatcher.DeregisterForSiteNotifications(WatcherChangeTypes.Changed | WatcherChangeTypes.Created, new FileSystemEventHandler(SourceFileChanged), null);
            }
        }

        #endregion

		#region GetOutputPath
		/// <summary>
		/// 
		/// </summary>
		/// <param name="inputPath"></param>
		/// <returns></returns>
		private string GetOutputPath(string inputPath)
		{
			string outPath = prefUtility.GetPref(inputPath, "OutputPath", ".\\").ToString();
			var targetDir = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(inputPath), outPath));			
			return targetDir;
		}
		#endregion

	}
}
