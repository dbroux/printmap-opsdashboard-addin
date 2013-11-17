using System.Linq;
using ESRI.ArcGIS.OperationsDashboard;
using System.Windows;
using System.Windows.Controls;
using MapPrintingControls;

namespace PrintMapAddIn
{
	/// <summary>
	/// Interaction logic for PrintMapToolbar.xaml
	/// </summary>
	public partial class PrintMapToolbar : UserControl, IMapToolbar
	{
		public PrintMapToolbar(MapWidget mapWidget, StylesManager stylesManager)
		{
			MapWidget = mapWidget;
			StylesManager = stylesManager;

			InitializeComponent();
			MapPrinterStyle firstStyle = stylesManager.ActiveStyles.FirstOrDefault();
			if (firstStyle != null)
				MapPrinter.Style = firstStyle.Style;
			MapPrinter.PropertyChanged += (sender, args) =>
			{
				var mapPrinter = sender as MapPrinter;
				if (args.PropertyName == "IsPrinting" && mapPrinter != null && mapPrinter.IsPrinting)
					PreviewSize = null;
			};
		}

		public MapWidget MapWidget { get; private set; }

		public StylesManager StylesManager { get; private set; }

		#region IsOpen DP

		public bool IsOpen
		{
			get { return (bool)GetValue(IsOpenProperty); }
			set { SetValue(IsOpenProperty, value); }
		}

		public static readonly DependencyProperty IsOpenProperty =
			DependencyProperty.Register("IsOpen", typeof(bool), typeof(PrintMapToolbar), new PropertyMetadata(false, OnIsOpenChanged));

		private static void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((PrintMapToolbar)d).OnIsOpenChanged((bool)e.NewValue);
		}

		private void OnIsOpenChanged(bool isOpen)
		{
			if (isOpen)
			{
				if (MapWidget != null)
					MapWidget.SetToolbar(this);
			}
			else
			{
				if (MapWidget != null)
					MapWidget.SetToolbar(null);
			}
		}
		#endregion

		#region PreviewSize DP

		public PreviewSize PreviewSize
		{
			get { return (PreviewSize)GetValue(PreviewSizeProperty); }
			set { SetValue(PreviewSizeProperty, value); }
		}

		public static readonly DependencyProperty PreviewSizeProperty =
			DependencyProperty.Register("PreviewSize", typeof(PreviewSize), typeof(PrintMapToolbar), new PropertyMetadata(null, OnPreviewSizeChanged));

		private static void OnPreviewSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((PrintMapToolbar)d).OnPreviewSizeChanged((PreviewSize)e.NewValue);
		}

		private void OnPreviewSizeChanged(PreviewSize previewSize)
		{
			if (previewSize != null && MapPrinter != null)
				MapPrinter.SetPrintableArea(previewSize.Height, previewSize.Width);
		}
		#endregion

		#region IMapToolbar

		/// <summary>
		/// OnActivated is called when the toolbar becomes the current toolbar. 
		/// </summary>
		public void OnActivated()
		{
		}

		/// <summary>
		///  OnDeactivated is called before the toolbar is closed,
		/// </summary>
		public void OnDeactivated()
		{
		}

		#endregion
	}
}
