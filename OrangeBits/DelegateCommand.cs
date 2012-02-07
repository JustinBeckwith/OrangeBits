using System;
using System.Windows.Input;
using System.Diagnostics;

namespace OrangeBits
{
	/// <summary>
	/// Simple implementation of the DelegateCommand (aka RelayCommand) pattern.
	/// </summary>
	internal class DelegateCommand : ICommand
	{
		/// <summary>
		/// Stores a reference to the CanExecute Func.
		/// </summary>
		private readonly Func<object, bool> _canExecute;

		/// <summary>
		/// Stores a reference to the Execute Action.
		/// </summary>
		private readonly Action<object> _execute;

		/// <summary>
		/// Initializes a new instance of the DelegateCommand class.
		/// </summary>
		/// <param name="execute">Action to invoke when Execute is called.</param>
		public DelegateCommand(Action<object> execute)
			: this(param => true, execute)
		{
		}

		/// <summary>
		/// Initializes a new instance of the DelegateCommand class.
		/// </summary>
		/// <param name="canExecute">Func to invoke when CanExecute is called.</param>
		/// <param name="execute">Action to invoke when Execute is called.</param>
		public DelegateCommand(Func<object, bool> canExecute, Action<object> execute)
		{
			Debug.Assert(canExecute != null, "canExecute must not be null.");
			Debug.Assert(execute != null, "execute must not be null.");
			_canExecute = canExecute;
			_execute = execute;
		}

		/// <summary>
		/// Determines if the command can execute.
		/// </summary>
		/// <param name="parameter">Data used by the command.</param>
		/// <returns>True if the command can execute.</returns>
		public bool CanExecute(object parameter)
		{
			return _canExecute(parameter);
		}

		/// <summary>
		/// Executes the command.
		/// </summary>
		/// <param name="parameter">Data used by the command.</param>
		public void Execute(object parameter)
		{
			_execute(parameter);
		}

		/// <summary>
		/// Invoked when changes to the execution state of the command occur.
		/// </summary>
		public event EventHandler CanExecuteChanged;

		/// <summary>
		/// Invokes the CanExecuteChanged event.
		/// </summary>
		public void OnCanExecuteChanged()
		{
			EventHandler handler = CanExecuteChanged;
			if (null != handler)
			{
				handler(this, EventArgs.Empty);
			}
		}
	}
}
