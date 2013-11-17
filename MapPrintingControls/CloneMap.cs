using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Linq;
using ESRI.ArcGIS.Client;

namespace MapPrintingControls
{
	/// <summary>
	/// Clone a map.
	/// </summary>
	public static class CloneMap
	{
		#region Attached property Map
		/// <summary>
		/// Map to clone.
		/// </summary>
		/// <param name="obj">The obj.</param>
		/// <returns></returns>
		public static String GetMap(DependencyObject obj)
		{
			return (String)obj.GetValue(MapProperty);
		}

		/// <summary>
		/// Sets the map.
		/// </summary>
		/// <param name="obj">The obj.</param>
		/// <param name="value">The value.</param>
		public static void SetMap(DependencyObject obj, String value)
		{
			obj.SetValue(MapProperty, value);
		}

		/// /// <summary>
		/// Identifies the <see cref="Map"/> attached property.
		/// This property attached to a map allows to clone another map.
		/// Typical use = <code><![CDATA[<esri:Map mapPrinting:CloneMap.Map={Binding ElementName=MapToClone}>]]></code>
		/// </summary>
		public static readonly DependencyProperty MapProperty =
				DependencyProperty.RegisterAttached("Map", typeof(Map), typeof(CloneMap), new PropertyMetadata(null, OnMapChanged));

		// Flag set to cloned layers in order to knwon which layers have to be cleared
		private static readonly DependencyProperty _clonedProperty =
				DependencyProperty.RegisterAttached("_cloned", typeof(String), typeof(CloneMap), new PropertyMetadata(null));

		private static void OnMapChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var map = d as Map;
			if (map == null)
				return;


			var mapToClone = (Map)e.NewValue;

			ClearClonedLayers(map);

			if (mapToClone != null)
			{
				Clone(map, mapToClone);
			}
		}
		#endregion

		#region private static void ClearClonedLayers(Map map)
		private const string clonedTag = "cloned";
		private static void ClearClonedLayers(Map map)
		{
			Debug.Assert(map != null);

			if (map.Layers == null)
				return;

			List<Layer> layersToRemove = map.Layers.Where(layer => clonedTag.Equals(layer.GetValue(_clonedProperty))).ToList(); // create a list of layer to remove before removing them
			foreach (var layer in layersToRemove)
				map.Layers.Remove(layer);
		} 
		#endregion

		#region private static void Clone(Map map, Map mapToClone)

		private static void Clone(Map map, Map mapToClone)
		{
			Debug.Assert(map != null);
			Debug.Assert(mapToClone != null);

			map.MinimumResolution = mapToClone.MinimumResolution;
			map.MaximumResolution = mapToClone.MaximumResolution;
			map.TimeExtent = mapToClone.TimeExtent;
			map.UseAcceleratedDisplay = false; // not working with accelerated display due to custom symbols in PrintOverviewLayer
			map.Extent = mapToClone.Extent; // Init SR

			map.WrapAround = mapToClone.WrapAround;
			int index = 0;

			// Clone layers
			foreach (Layer layer in mapToClone.Layers)
			{
				var toLayer = CloneLayer(layer);

				if (toLayer != null)
				{
					toLayer.SetValue(_clonedProperty, clonedTag);
					toLayer.InitializationFailed += (s, e) => { }; // to avoid crash if bad layer
					map.Layers.Insert(index++, toLayer); // use index in order to keep existing layers after cloned layers
				}
			}
		}


		// For graphics layer (except feature layer), a clone of the graphic collection is done and is frozen
		private static Layer CloneLayer(Layer layer)
		{
			Layer toLayer;
			var featureLayer = layer as FeatureLayer;

			if (layer is GraphicsLayer && (featureLayer == null || featureLayer.Url == null || featureLayer.Mode != FeatureLayer.QueryMode.OnDemand))
			{
				var fromLayer = layer as GraphicsLayer;
				var printLayer = new GraphicsLayer
				{
					Renderer = fromLayer.Renderer,
					Clusterer = fromLayer.Clusterer == null ? null : fromLayer.Clusterer.Clone(),
					ShowLegend = fromLayer.ShowLegend,
					RendererTakesPrecedence = fromLayer.RendererTakesPrecedence,
					ProjectionService = fromLayer.ProjectionService
				};
				toLayer = printLayer;

				var graphicCollection = new GraphicCollection();
				foreach (var graphic in fromLayer.Graphics)
				{
					var clone = new Graphic();

					foreach (var kvp in graphic.Attributes)
					{
						if (kvp.Value is DependencyObject)
						{
							// If the attribute is a dependency object --> clone it
							var clonedkvp = new KeyValuePair<string, object>(kvp.Key, (kvp.Value as DependencyObject).Clone());
							clone.Attributes.Add(clonedkvp);
						}
						else
							clone.Attributes.Add(kvp);
					}
					clone.Geometry = graphic.Geometry;
					clone.Symbol = graphic.Symbol;
					clone.Selected = graphic.Selected;
					clone.TimeExtent = graphic.TimeExtent;
					graphicCollection.Add(clone);
				}

				printLayer.Graphics = graphicCollection;

				toLayer.ID = layer.ID;
				toLayer.Opacity = layer.Opacity;
				toLayer.Visible = layer.Visible;
				toLayer.MaximumResolution = layer.MaximumResolution;
				toLayer.MinimumResolution = layer.MinimumResolution;
			}
			else
			{
				toLayer = layer.Clone();
				//Issue when "cloning" all elements of a ArcGISLocalFeatureLayer. The LayerID should not be set (to be set when loading).
				//if (layer is ArcGISLocalFeatureLayer)
				//{
				//	if (((ArcGISLocalFeatureLayer)toLayer).LayerName != null)
				//		((ArcGISLocalFeatureLayer)toLayer).LayerId = null;
				//}

				if (layer is GroupLayerBase)
				{
					// Clone sublayers (not cloned in Clone() to avoid issue with graphicslayer)
					var childLayers = new LayerCollection();
					foreach (Layer subLayer in (layer as GroupLayerBase).ChildLayers)
					{
						var toSubLayer = CloneLayer(subLayer);

						if (toSubLayer != null)
						{
							toSubLayer.InitializationFailed += (s, e) => { }; // to avoid crash if bad layer
							childLayers.Add(toSubLayer);
						}
					}
					((GroupLayerBase) toLayer).ChildLayers = childLayers;
				}
			}
			return toLayer;
		}

		#endregion

	}
}
