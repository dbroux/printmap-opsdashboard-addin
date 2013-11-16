using System.Linq;
using System.Windows;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;

namespace MapPrintingControls
{
	/// <summary>
	/// Surrogate binder class. 
	/// </summary>
	public static class SurrogateBinder
	{
		#region MapExtent
		/// /// <summary>
		/// Identifies the MapExtent attached property.
		/// This attached property is useful to bind a map extent (Map.Extent is not a DP, so it's not possible directly).
		/// Ex : <code lang="XAML">
		///	&lt;esri:Map mapPrinting:SurrogateBinder.MapExtent="{Binding Extent, ElementName=otherMap}"&gt;
		/// </code>
		/// is ~ to <code lang="XAML">&lt;esri:Map Extent="{Binding Extent, ElementName=otherMap}"&gt;</code> (but not working) 
		/// </summary>
		public static readonly DependencyProperty MapExtentProperty = DependencyProperty.RegisterAttached("MapExtent", typeof(Envelope), typeof(SurrogateBinder), new PropertyMetadata(OnMapExtentChanged));

		/// <summary>
		/// Gets the map extent.
		/// </summary>
		/// <param name="map">The map.</param>
		/// <returns>The map extent</returns>
		public static double GetMapExtent(DependencyObject map)
		{
			return (double)map.GetValue(MapExtentProperty);
		}

		/// <summary>
		/// Sets the map extent.
		/// </summary>
		/// <param name="map">The map dependency object.</param>
		/// <param name="value">The map extent.</param>
		public static void SetMapExtent(DependencyObject map, Envelope value)
		{
			map.SetValue(MapExtentProperty, value);
		}

		private static void OnMapExtentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is Map && e.NewValue is Envelope)
			{
				var map = (Map)d;
				var env = (Envelope)e.NewValue; 
				if ((map.Extent != null) && (!map.Extent.Equals(env)) || ((map.Extent == null) && (env != null)))
					map.Extent = env;
			}
		}
		#endregion

		#region OverviewMapPrinter
		/// /// <summary>
		/// Identifies the OverviewMapPrinter attached property.
		/// This attached property is useful to bind the MapPrinter of the PrintOverviewLayer contained in the map.
		/// The PrintOverviewLayer being not an UIElement, a direct binding is not working.
		/// Ex : <code lang="XAML">&lt;esri:Map mapPrinting:SurrogateBinder.OverviewMapPrinter="{Binding ElementName=myMapPrinter}"&gt;
		///         &lt;mapPrinting:PrintOverviewlayer&gt;</code>	
		/// is ~ to <code lang="XAML">&lt;esri:Map&gt;
		///            &lt;mapPrinting:PrintOverviewlayer MapPrinter="{Binding ElementName=myMapPrinter}" /&gt;</code>(but not working) 
		/// </summary>
		public static readonly DependencyProperty OverviewMapPrinterProperty =
			DependencyProperty.RegisterAttached("OverviewMapPrinter", typeof(MapPrinter), typeof(SurrogateBinder), new PropertyMetadata(OnOverviewMapPrinterChanged));

		/// <summary>
		/// Gets the overview map printer.
		/// </summary>
		/// <param name="map">The map dependency object.</param>
		/// <returns></returns>
		public static double GetOverviewMapPrinter(DependencyObject map)
		{
			return (double)map.GetValue(OverviewMapPrinterProperty);
		}

		/// <summary>
		/// Sets the overview map printer.
		/// </summary>
		/// <param name="map">The map dependency object.</param>
		/// <param name="value">The map printer.</param>
		public static void SetOverviewMapPrinter(DependencyObject map, MapPrinter value)
		{
			map.SetValue(OverviewMapPrinterProperty, value);
		}

		private static void OnOverviewMapPrinterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is Map && e.NewValue is MapPrinter)
			{
				var map = d as Map;

				if (map.Layers != null)
				{
					PrintOverviewLayer overviewLayer = map.Layers.OfType<PrintOverviewLayer>().FirstOrDefault();
					var mapPrinter = e.NewValue as MapPrinter;

					if (overviewLayer != null)
						overviewLayer.MapPrinter = mapPrinter;

					map.Layers.CollectionChanged += (s, args) => LayersCollectionChanged(args, mapPrinter);
				}
			}
		}

		static void LayersCollectionChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e, MapPrinter mapPrinter)
		{
			if (e.OldItems != null)
			{
				foreach (var overviewLayer in e.OldItems.OfType<PrintOverviewLayer>())
					overviewLayer.MapPrinter = null;
			}
			if (e.NewItems != null)
			{
				foreach (var overviewLayer in e.NewItems.OfType<PrintOverviewLayer>())
					overviewLayer.MapPrinter = mapPrinter;
			}
		}
		
		#endregion
	} 

}
