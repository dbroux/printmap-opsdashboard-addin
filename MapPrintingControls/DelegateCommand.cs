using System;
using System.Windows.Input;

namespace MapPrintingControls
{
	/// <summary>
	/// DelegateCommand class implements ICommand from 2 delegates : canExecute and execute
	/// </summary>
	internal class DelegateCommand : ICommand
	{
		#region Constructor
		readonly Func<object, bool> _canExecute;
		readonly Action<object> _executeAction;
		bool _canExecuteCache;

		/// <summary>
		/// Initializes a new instance of the <see cref="DelegateCommand"/> class.
		/// </summary>
		/// <param name="executeAction">The execute action.</param>
		/// <param name="canExecute">The can execute.</param>
		public DelegateCommand(Action<object> executeAction, Func<object, bool> canExecute)
		{
			_executeAction = executeAction;
			_canExecute = canExecute;
		} 
		#endregion

		#region ICommand Members
		/// <summary>
		/// Occurs when changes occur that affect whether the command should execute.
		/// </summary>
		public event EventHandler CanExecuteChanged;

		/// <summary>
		/// Defines the method that determines whether the command can execute in its current state.
		/// </summary>
		/// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.</param>
		/// <returns>
		/// true if this command can be executed; otherwise, false.
		/// </returns>
		public bool CanExecute(object parameter)
		{
			bool temp = _canExecute(parameter);
			if (_canExecuteCache != temp)
			{
				_canExecuteCache = temp;
				if (CanExecuteChanged != null)
				{
					CanExecuteChanged(this, new EventArgs());
				}
			}
			return _canExecuteCache;
		}

		/// <summary>
		/// Defines the method to be called when the command is invoked.
		/// </summary>
		/// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.</param>
		public void Execute(object parameter)
		{
			_executeAction(parameter);
		}
		#endregion
	}
}
