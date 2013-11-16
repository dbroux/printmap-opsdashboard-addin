using System;
using System.Windows;
using ESRI.ArcGIS.Client.Bing;
using ESRI.ArcGIS.Client.Geometry;

namespace MapPrintingControls
{
	/// <summary>
	/// Manage the extent by page
	/// </summary>
	internal class MapPages
	{
		#region Constructor
		private MapPoint _topLeft;
		private Size _pageExtentSize;

		public MapPages()
		{
			PageCount = 0;
		} 
		#endregion

		public int PageCount { get; private set; }

		/// <summary>
		/// Gets the <see cref="ESRI.ArcGIS.Client.Geometry.Envelope"/> of the specified page.
		/// </summary>
		/// <value></value>
		public Envelope this[int page]
		{
			get
			{
				if (NbColumn == 0) return null; // not yet initialized

				int row = (page-1) / NbColumn;
				int column = (page-1) % NbColumn;
				double x = _topLeft.X + column * _pageExtentSize.Width;
				double y = _topLeft.Y - row * _pageExtentSize.Height;
				return new Envelope(x, y - _pageExtentSize.Height, x + _pageExtentSize.Width, y) { SpatialReference = _topLeft.SpatialReference};
			}
		}

		public int NbColumn { get; private set; }

		#region PreparePages
		/// <summary>
		/// Prepares the pages to print
		/// </summary>
		/// <param name="mapPrinter">The map printer.</param>
		/// <param name="mapWidth">Width of the map.</param>
		/// <param name="mapHeight">Height of the map.</param>
		public void PreparePages(MapPrinter mapPrinter, double mapWidth, double mapHeight)
		{
			Envelope printExtent = mapPrinter.PrintExtent;
			if (printExtent == null)
			{
				NbColumn = 0;
				PageCount = 0;
				return;
			}
			if (mapWidth == 0 || mapHeight == 0)
				return;

			var mapSize = new Size(mapPrinter.RotateMap ? mapHeight : mapWidth, mapPrinter.RotateMap ? mapWidth : mapHeight);

			var mapUnit = mapPrinter.MapUnits;
			bool isWebMercator = (mapPrinter.Map != null && mapPrinter.Map.SpatialReference != null &&
			                      (mapPrinter.Map.SpatialReference.WKID == 102100 || mapPrinter.Map.SpatialReference.WKID == 102113 || mapPrinter.Map.SpatialReference.WKID == 3857));

			double ratioScaleResolution = RatioScaleResolution(mapUnit, printExtent.GetCenter().Y, isWebMercator);

			double scale = mapPrinter.Scale;
			int nbRow;
			double printResolution;
			Size neededSize;

			if (scale <= 0 || !mapPrinter.IsScaleFixed)
			{
				// Only one page with full extent ==> resolution fixed, scale calculated from resolution
				NbColumn = 1;
				nbRow = 1;

				printResolution = Math.Max(printExtent.Width / mapSize.Width, printExtent.Height / mapSize.Height);
				scale = ratioScaleResolution * printResolution;

				neededSize = new Size(printExtent.Width / printResolution, printExtent.Height / printResolution); 
			}
			else
			{
				// scale fixed ==> calculate resolution from scale, deduce nbColumn and nbRow from resolution
				const double maxPage = 1000;

				do
				{
					printResolution = scale / ratioScaleResolution;
					neededSize = new Size(printExtent.Width / printResolution, printExtent.Height / printResolution); 

					NbColumn = Math.Max((int)Math.Ceiling(neededSize.Width / mapSize.Width), 1); // Use max because 0 sometimes due to rounding
					nbRow = Math.Max((int)Math.Ceiling(neededSize.Height / mapSize.Height), 1);

					if ((double)nbRow * NbColumn > maxPage)
					{
						// Too much _pages ==> retry with higher scale (1:scale lower in fact :-)) 
						scale *= Math.Sqrt((double)nbRow * NbColumn / (maxPage - 1)); 
						// Round to 2 digits
						double s = Math.Pow(10, Math.Floor(Math.Log10(scale) - 1));
						scale = s * Math.Ceiling(scale / s);
					}
				}
				while ((double)nbRow * NbColumn > maxPage); // prevent scale with too much page

			}

			PageCount = nbRow * NbColumn;
			var marginSize = new Size(Math.Max(0, NbColumn * mapSize.Width - neededSize.Width), Math.Max(0, nbRow * mapSize.Height - neededSize.Height)); // useful to center the print on the print extent

			_pageExtentSize = new Size(mapSize.Width * printResolution, mapSize.Height * printResolution);

			_topLeft = new MapPoint(printExtent.XMin - (marginSize.Width*printResolution/2.0),
			                        printExtent.YMax + (marginSize.Height*printResolution/2.0),
			                        printExtent.SpatialReference);

			if (scale != mapPrinter.Scale)
				mapPrinter.Scale = scale;

		}

		#endregion

		#region Resolution<->Scale Conversion
		private const int dpi = 96;
		private const double toRadians = 0.017453292519943295769236907684886;
		private const double earthRadius = 6378137; //Earth radius in meters (defaults to WGS84 / GRS80)
		private const double degreeDist = earthRadius * toRadians;// distance of 1 degree at equator in meters

		private static double RatioScaleResolution(MapUnit mapUnit, double yCenter, bool isWebMercator)
		{
			double ratio;

			if (isWebMercator)
			{
				// Transform yCenter from web mercator to decimal degree
				yCenter = Math.Min(Math.Max(yCenter, -20037508.3427892), 20037508.3427892);
				MapPoint point = new MapPoint(0, yCenter);
				yCenter = point.WebMercatorToGeographic().Y;
				ratio = Math.Cos(yCenter * toRadians) * dpi * 39.37; // 39.37 = MapUnit.Meters/MapUnit.Inches
			}
			else if (mapUnit == MapUnit.DecimalDegrees || mapUnit == MapUnit.Undefined)
			{
				if (Math.Abs(yCenter) > 90)
					ratio = 0.0;
				else
					ratio = Math.Cos(yCenter * toRadians) * degreeDist * dpi * 39.37;
			}
			else
			{
				ratio = (double)dpi * (int)mapUnit / (int)MapUnit.Inches;
			}
			return ratio;
		} 
		#endregion

	}
}
