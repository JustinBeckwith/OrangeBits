using System;
using System.IO;
using System.Timers;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using OrangeBits.Compilers;
using System.Linq;
using System.Windows.Threading;

using Microsoft.WebMatrix.Extensibility;

namespace OrangeBits
{
	/// <summary>
	/// class that monitors the queue of file change events, and invokes the 
	/// compiler when neccessary
	/// </summary>
	public class Worker
	{
		//--------------------------------------------------------------------------
		//
		//	Properties
		//
		//--------------------------------------------------------------------------

		#region Properties

		/// <summary>
		/// queue that holds all of the files that need to be processed
		/// </summary>
		protected Queue<FileChangeItem> queue = new Queue<FileChangeItem>();		

		/// <summary>
		/// timer for our background thread, this polls the queue
		/// </summary>
		protected Timer timer = new Timer();

		/// <summary>
		/// reference to the host that contains the worker
		/// </summary>
		protected IWebMatrixHost host;

		/// <summary>
		/// 
		/// </summary>
		protected Dispatcher mainDispatcher = Dispatcher.CurrentDispatcher;

		#endregion


		//--------------------------------------------------------------------------
		//
		//	Constructors
		//
		//--------------------------------------------------------------------------

		#region Constructor
		/// <summary>
		/// create a new timer, and check the queue periodically for new requests
		/// </summary>
		public Worker(IWebMatrixHost host)
		{
			this.host = host;
			timer.Interval = 500;
			timer.Elapsed += new ElapsedEventHandler(t_Elapsed);
			timer.AutoReset = false;
			timer.Start();
		}
		#endregion


		//--------------------------------------------------------------------------
		//
		//	Event Handlers
		//
		//--------------------------------------------------------------------------

		#region t_Elapsed
		/// <summary>
		/// if there are any items in the queue which are at least 100 ms old, process them
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void t_Elapsed(object sender, ElapsedEventArgs e)
		{			
			while ((queue.Count > 0) && (DateTime.Now > queue.Peek().Time.AddMilliseconds(100)))
			{
				var item = queue.Dequeue();				
				ProcessItem(item);	
			}
			timer.Start();
		}
		#endregion


		//--------------------------------------------------------------------------
		//
		//	Methods
		//
		//--------------------------------------------------------------------------

		#region AddItem
		/// <summary>
		/// add a new file to process into the queue
		/// </summary>
		/// <param name="path">object that contains the path and times</param>
		public void AddItem(string path)
		{
			// only add to the queue if the file isn't already in the queue
			var exists = queue.Where(x => x.Path.ToLower() == path.ToLower()).Count() > 0;
			if (!exists)
				this.queue.Enqueue(new FileChangeItem() { Path = path, Time = DateTime.Now });
		}
		#endregion

		#region ProcessItem
		/// <summary>
		/// process a file, generating the compiled output
		/// </summary>
		/// <param name="item"></param>
		protected void ProcessItem(FileChangeItem item)
		{
			try
			{									
				// do the actual compilation work
				CompileResults results = OrangeCompiler.Compile(item.Path);

				// show the notification bar to notify the user it happened
				host.ShowNotification("compiled! " + results.OutputPath);

				// refresh the tree so the new file (if created) shows up
				if (results.IsNewFile)
				{
					mainDispatcher.Invoke(new Action(() =>
								{
									var refreshCommand = host.HostCommands.GetCommand(CommonCommandIds.GroupId, (int)CommonCommandIds.Ids.Refresh);									
									if (refreshCommand.CanExecute(null))
										refreshCommand.Execute(null);
								}));
				}
			}
			catch (Exception ex)
			{				
				host.ShowNotification("There was an error compiling " + item.Path);
			}
		}
		#endregion

		protected void OpenFile(object i)
		{
		}
	}
}