using System;
using System.Windows.Input;

namespace PrintMapAddIn
{
	/// <summary>
	/// DelegateCommand class implements ICommand from 2 delegates : canExecute and execute
	/// </summary>
	internal class DelegateCommand : ICommand
	{
		private readonly Predicate<object> _canExecute;
		private readonly Action<object> _execute;

		/// <summary>
		/// Initializes a new instance of the <see cref="DelegateCommand" /> class.
		/// </summary>
		/// <param name="execute">The execute action.</param>
		public DelegateCommand(Action<object> execute)
			: this(execute, null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DelegateCommand"/> class.
		/// </summary>
		/// <param name="execute">The execute action.</param>
		/// <param name="canExecute">The can execute predicate.</param>
		public DelegateCommand(Action<object> execute,
					   Predicate<object> canExecute)
		{
			_execute = execute;
			_canExecute = canExecute;
		}

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
			if (_canExecute == null)
			{
				return true;
			}

			return _canExecute(parameter);
		}

		/// <summary>
		/// Defines the method that determines whether the command can execute in its current state.
		/// </summary>
		/// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.</param>
		/// <returns>
		/// true if this command can be executed; otherwise, false.
		/// </returns>
		public void Execute(object parameter)
		{
			_execute(parameter);
		}

		/// <summary>
		/// Raises the CanExecuteChanged event.
		/// </summary>
		public void RaiseCanExecuteChanged()
		{
			if (CanExecuteChanged != null)
			{
				CanExecuteChanged(this, EventArgs.Empty);
			}
		}
	}
}


