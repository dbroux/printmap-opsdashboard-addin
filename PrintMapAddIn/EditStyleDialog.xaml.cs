using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;

namespace PrintMapAddIn
{
	/// <summary>
	/// Interaction logic for EditStyleDialog.xaml
	/// </summary>
	internal partial class EditStyleDialog : Window
	{
		private MapPrinterStyle _initialMapPrinterStyle;
		private readonly MapPrinterStyle _editedMapPrinterStyle;

		public EditStyleDialog(MapPrinterStyle mapPrinterStyle)
		{
			_initialMapPrinterStyle = mapPrinterStyle;

			// Clone the styles to avoid changing it while it's not validated
			_editedMapPrinterStyle = mapPrinterStyle.Clone();

			InitializeComponent();

			if (!string.IsNullOrEmpty(_editedMapPrinterStyle.XamlStyle))
				new TextRange(RichTextBox.Document.ContentStart, RichTextBox.Document.ContentEnd).Text = _editedMapPrinterStyle.XamlStyle;
			OkCommand = new DelegateCommand(Ok);
			DataContext = _editedMapPrinterStyle;
		}

		#region OkCommand
		public ICommand OkCommand { get; private set; }

		private void Ok(object parameter)
		{
			string errorMessage = null;

			// Check the Name
			if (string.IsNullOrEmpty(_editedMapPrinterStyle.Name))
			{
				errorMessage = "Name mandatory";
			}
			else
			{
				var xaml = new TextRange(RichTextBox.Document.ContentStart, RichTextBox.Document.ContentEnd).Text;
				Style style;
				Exception error = null;
				try
				{
					style = MapPrinterStyle.CreateStyle(xaml);
				}
				catch (Exception e)
				{
					error = e;
					style = null;
				}
				if (style != null)
				{
					_editedMapPrinterStyle.XamlStyle = xaml;
					_editedMapPrinterStyle.Style = style;
				}
				else
				{
					if (error == null)
						errorMessage = "Unable to create style";
					else
					{
						errorMessage = error.Message;
						if (error.InnerException != null && error.Message != error.InnerException.Message)
						{
							errorMessage += "\n";
							errorMessage += error.InnerException.Message;
						}
					}
				}
			}


			// Validation if no error
			if (errorMessage == null)
			{
				_initialMapPrinterStyle.Copy(_editedMapPrinterStyle);
				DialogResult = true;
				Close();
			}
			else
				MessageBox.Show(errorMessage, "Incorrect Style");
		}

		#endregion



	}
}
