using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Resources;

namespace PrintMapAddIn
{
	[DataContract]
	public class StylesManager
	{
		private ObservableCollection<MapPrinterStyle> _styles = new ObservableCollection<MapPrinterStyle>();
		private readonly ObservableCollection<MapPrinterStyle> _activeStyles = new ObservableCollection<MapPrinterStyle>();

		private static IEnumerable<MapPrinterStyle> _predefinedStyles;
		public static IEnumerable<MapPrinterStyle> PredefinedStyles
		{
			get
			{
				if (_predefinedStyles == null)
				{
					_predefinedStyles = new List<MapPrinterStyle>
					                    {
						                    GetPredefinedStyle("Standard", "Standard", "Print the map with attribution and scale line."),
						                    GetPredefinedStyle("Basic", "Map Only", "Print the map only."),
						                    GetPredefinedStyle("WithLegend", "With Legend", "Print the map with a legend."),
						                    GetPredefinedStyle("WithHeaderAndFooter", "With Header and Footer", "Print the map with a header and a footer."),
						                    GetPredefinedStyle("WithOverview", "With Overview", "Print the map with a topographic map as overview.")
					                    };
				}
				return _predefinedStyles;
			}
		}

		// Get each style from a XAML resource 
		internal static MapPrinterStyle GetPredefinedStyle(string id, string name, string description)
		{
			var uri = new Uri(string.Format("/PrintMapAddIn;component/Styles/{0}MapPrinterStyle.xaml", id), UriKind.Relative);
			StreamResourceInfo info = Application.GetResourceStream(uri);
			MapPrinterStyle mapPrinterStyle = null;
			if (info != null)
			{
				using (var reader = new StreamReader(info.Stream))
				{
					var xamlStyle = reader.ReadToEnd();
					mapPrinterStyle = new MapPrinterStyle
						                  {
							                  ID = id,
							                  Name = name,
							                  Description = description,
							                  XamlStyle = xamlStyle,
							                  IsActive = true
						                  };
					mapPrinterStyle.InitStyle();
				}
			}
			return mapPrinterStyle;
		}


		public StylesManager Clone()
		{
			var styleManager = new StylesManager();
			styleManager.Copy(this);
			return styleManager;
		}

		internal void Copy(StylesManager stylesManager)
		{
			_styles.Clear();
			_activeStyles.Clear();
			if (stylesManager != null)
			{
				foreach (var style in stylesManager.Styles)
				{
					AddStyle(style.Clone());
				}
			}
		}

		public IReadOnlyCollection<MapPrinterStyle> ActiveStyles
		{
			get { return _activeStyles; }
		}

		[DataMember(Name = "Styles")] // serializable attribute (IReadOnlyCollection not serializable)
		public ObservableCollection<MapPrinterStyle> Styles
		{
			get { return _styles; }
			set { _styles = value; } // for serializer
		}

		public void AddStyle(MapPrinterStyle style)
		{
			style.PropertyChanged += OnStylePropertyChanged;
				_styles.Add(style);
			if (style.IsActive)
				_activeStyles.Add(style);
		}

		public bool RemoveStyle(MapPrinterStyle style)
		{
			if (style == null)
				return false;
			style.PropertyChanged -= OnStylePropertyChanged;
			_activeStyles.Remove(style);
			return _styles.Remove(style);
		}

		void OnStylePropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			var style = sender as MapPrinterStyle;
			if (e.PropertyName == "IsActive" && style != null)
			{
				if (style.IsActive)
					_activeStyles.Add(style);
				else
					_activeStyles.Remove(style);
			}
		}

		[OnDeserialized]
		public void OnDeserialized(StreamingContext context) // Note: needs to be public, not working with internal.
		{
			// Create the style from the xamlstring
			foreach (var style in Styles)
			{
				try
				{
					style.InitStyle();
				}
				catch (Exception)
				{
				}
				style.PropertyChanged += OnStylePropertyChanged;
				if (style.IsActive)
					_activeStyles.Add(style);
			}
		}

		public void AddPredefinedStyles()
		{
			bool isActive = !Styles.Any(); // active the predefined styles if there is no existing styles (i.e mostly during first use)

			// Add the predefined styles that are not already existing and init style of others 
			foreach (var style in PredefinedStyles)
			{
				var existingStyle = Styles.FirstOrDefault(s => s.ID == style.ID);

				if (existingStyle == null)
				{
					style.IsActive = isActive;
					AddStyle(style);
				} else if (string.IsNullOrEmpty(existingStyle.XamlStyle))
				{
					existingStyle.Style = style.Style;
				}
			}
		}


		internal void UpStyle(MapPrinterStyle style)
		{
			if (style == null)
				return;
			var ind= _styles.IndexOf(style);
			if (ind <= 0) return;

			_styles.Move(ind, ind - 1);
			//RemoveStyle(style);
			//AddStyle(style, ind -1);
		}

		internal bool CanUpStyle(MapPrinterStyle style)
		{
			if (style == null)
				return false;
			return _styles.IndexOf(style) > 0;
		}


		internal void DownStyle(MapPrinterStyle style)
		{
			if (style == null)
				return;
			var ind = _styles.IndexOf(style);
			if (ind < 0 || ind >= _styles.Count - 1) return;

			_styles.Move(ind, ind + 1);
			//RemoveStyle(style);
			//AddStyle(style, ind + 1);
		}

		internal bool CanDownStyle(MapPrinterStyle style)
		{
			if (style == null)
				return false;
			var ind = _styles.IndexOf(style);
			return ind >= 0 && ind < _styles.Count - 1;
		}
	}
}
