using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace MapPrintingControls
{
	/// <summary>
	/// A control to provide a visual indicator when an application is busy due to printing
	/// </summary>
	public class MapPrinterIndicator : Control
	{
		#region Constructor
		static MapPrinterIndicator()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(MapPrinterIndicator),
				new FrameworkPropertyMetadata(typeof(MapPrinterIndicator)));
		}
		#endregion

		#region DependencyProperty MapPrinter

		/// <summary>
		/// Gets or sets the map printer.
		/// </summary>
		/// <value>The map printer.</value>
		[Category("Printing Properties")]
		public MapPrinter MapPrinter
		{
			get { return (MapPrinter)GetValue(MapPrinterProperty); }
			set { SetValue(MapPrinterProperty, value); }
		}

		/// /// <summary>
		/// Identifies the <see cref="MapPrinter"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty MapPrinterProperty =
				DependencyProperty.Register("MapPrinter", typeof(MapPrinter), typeof(MapPrinterIndicator), new PropertyMetadata(null, OnMapPrinterChanged));

		private static void OnMapPrinterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var mapPrinterIndicator = d as MapPrinterIndicator;
			if (mapPrinterIndicator != null)
				mapPrinterIndicator.OnMapPrinterChanged(e.OldValue as MapPrinter, e.NewValue as MapPrinter);
		}

		private void OnMapPrinterChanged(MapPrinter oldMapPrinter, MapPrinter newMapPrinter)
		{
			//mapPrinterIndicator.DataContext = e.NewValue;
			if (oldMapPrinter != null)
				oldMapPrinter.PrintProgress -= MapPrinterPrintProgress;
			if (newMapPrinter != null)
				newMapPrinter.PrintProgress += MapPrinterPrintProgress;
		}

		void MapPrinterPrintProgress(object sender, PrintProgressEventArgs e)
		{
			PrintProgress = e;
		}

		#endregion

		#region DependencyProperty PrintProgress

		/// <summary>
		/// Gets or sets the print progress.
		/// </summary>
		/// <value>The print progress.</value>
		[Category("Printing Properties")]
		public PrintProgressEventArgs PrintProgress
		{
			get { return (PrintProgressEventArgs)GetValue(PrintProgressProperty); }
			set { SetValue(PrintProgressProperty, value); }
		}

		/// /// <summary>
		/// Identifies the <see cref="PrintProgress"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty PrintProgressProperty =
				DependencyProperty.Register("PrintProgress", typeof(PrintProgressEventArgs), typeof(MapPrinterIndicator), null);

		#endregion

	}
}
