using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace PrintMapAddIn
{
	/// <summary>
	/// Interaction logic for PrintMapDialog.xaml
	/// </summary>
	internal partial class PrintMapDialog : Window
	{
		private readonly PrintMapTool _printMapTool;
		private readonly Style _currentStyle;

		public PrintMapDialog(PrintMapTool printMapTool)
		{
			// Clone the styles to avoid changing them while it's not validated
			StylesManager = printMapTool.PrintMapToolbar.StylesManager.Clone();
			_printMapTool = printMapTool;
			_currentStyle = _printMapTool.PrintMapToolbar.MapPrinter.Style;

			OkCommand = new DelegateCommand(Ok);
			EditStyleCommand = new DelegateCommand(EditStyle);
			AddStyleCommand = new DelegateCommand(AddStyle);
			UpStyleCommand = new DelegateCommand(UpStyle, CanUpStyle);
			DownStyleCommand = new DelegateCommand(DownStyle, CanDownStyle);
			RemoveStyleCommand = new DelegateCommand(RemoveStyle);

			InitializeComponent();
		}

		#region SelectedStyle DP
		public MapPrinterStyle SelectedStyle
		{
			get { return (MapPrinterStyle)GetValue(SelectedStyleProperty); }
			set { SetValue(SelectedStyleProperty, value); }
		}

		public static readonly DependencyProperty SelectedStyleProperty =
			DependencyProperty.Register("SelectedStyle", typeof(MapPrinterStyle), typeof(PrintMapDialog), new PropertyMetadata(null, OnSelectedStyleChanged));

		private static void OnSelectedStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((PrintMapDialog)d).OnSelectedStyleChanged();
		}

		private void OnSelectedStyleChanged()
		{
			((DelegateCommand)UpStyleCommand).RaiseCanExecuteChanged();
			((DelegateCommand)DownStyleCommand).RaiseCanExecuteChanged();
			((DelegateCommand)RemoveStyleCommand).RaiseCanExecuteChanged();
			((DelegateCommand)EditStyleCommand).RaiseCanExecuteChanged();
		}
		
		#endregion

		public StylesManager StylesManager { get; private set; }

		#region OkCommand
		public ICommand OkCommand { get; private set; }

		private void Ok(object parameter)
		{
			// reinit PrintToolbar.StyleManager with the modified style
			_printMapTool.PrintMapToolbar.StylesManager.Copy(StylesManager);

			// Reset the previous current style if still active
			bool stillActive = _printMapTool.PrintMapToolbar.StylesManager.ActiveStyles.Any(s => s.Style == _currentStyle && s.IsActive);
			if (stillActive)
				_printMapTool.PrintMapToolbar.MapPrinter.Style = _currentStyle;
			else
			{
				var firstMapPrinterStyle = _printMapTool.PrintMapToolbar.StylesManager.ActiveStyles.FirstOrDefault();
				if (firstMapPrinterStyle != null)
					_printMapTool.PrintMapToolbar.MapPrinter.Style = firstMapPrinterStyle.Style;
			}

			DialogResult = true;
			Close();
		}

		#endregion

		#region EditStyleCommand
		public ICommand EditStyleCommand { get; private set; }

		private void EditStyle(object parameter)
		{
			var style = parameter as MapPrinterStyle ?? SelectedStyle;
			if (style == null)
				return;

			// Show edit style dialog
			var dialog = new EditStyleDialog(style) { Owner = this };
			dialog.ShowDialog();
		}

		#endregion

		#region AddStyleCommand
		public ICommand AddStyleCommand { get; private set; }

		private void AddStyle(object parameter)
		{
			var style = StylesManager.GetPredefinedStyle("New", "New Style", null);

			// Show edit style dialog
			var dialog = new EditStyleDialog(style) { Owner = this };
			bool? result = dialog.ShowDialog();

			if (result.HasValue && result.Value)
			{
				StylesManager.AddStyle(style);
			}
		}

		#endregion

		#region UpStyleCommand
		public ICommand UpStyleCommand { get; private set; }

		private void UpStyle(object parameter)
		{
			var style = parameter as MapPrinterStyle ?? SelectedStyle;
			StylesManager.UpStyle(style);
			SelectedStyle = style;
			((DelegateCommand) UpStyleCommand).RaiseCanExecuteChanged();
			((DelegateCommand)DownStyleCommand).RaiseCanExecuteChanged();
		}

		private bool CanUpStyle(object parameter)
		{
			var style = parameter as MapPrinterStyle ?? SelectedStyle;
			return StylesManager.CanUpStyle(style);
		}
		#endregion

		#region DownStyleCommand
		public ICommand DownStyleCommand { get; private set; }

		private void DownStyle(object parameter)
		{
			var style = parameter as MapPrinterStyle ?? SelectedStyle;
			StylesManager.DownStyle(style);
			((DelegateCommand)UpStyleCommand).RaiseCanExecuteChanged();
			((DelegateCommand)DownStyleCommand).RaiseCanExecuteChanged();
		}

		private bool CanDownStyle(object parameter)
		{
			var style = parameter as MapPrinterStyle ?? SelectedStyle;
			return StylesManager.CanDownStyle(style);
		}
		#endregion

		#region RemoveStyleCommand
		public ICommand RemoveStyleCommand { get; private set; }

		private void RemoveStyle(object parameter)
		{
			var style = parameter as MapPrinterStyle ?? SelectedStyle;
			StylesManager.RemoveStyle(style);
			((DelegateCommand)UpStyleCommand).RaiseCanExecuteChanged();
			((DelegateCommand)DownStyleCommand).RaiseCanExecuteChanged();
		}

		#endregion

	}
}
