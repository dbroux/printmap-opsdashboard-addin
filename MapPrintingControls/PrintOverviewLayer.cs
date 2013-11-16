using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Symbols;
using Geometry = ESRI.ArcGIS.Client.Geometry;

namespace MapPrintingControls
{
	/// <summary>
	/// GraphicsLayer displaying the print grid and the print extent of a map printer.
	/// </summary>
	public class PrintOverviewLayer : GraphicsLayer
	{
		#region Constructor
		private const string TextTemplate = @"
			<ControlTemplate xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
							 xmlns:controls=""clr-namespace:MapPrintingControls;assembly=MapPrintingControls"">
				<Grid>
					<controls:CenteredContentControl Content=""{Binding Attributes[Page]}"" FontSize=""{Binding Symbol.FontSize}"" Foreground=""{Binding Symbol.Foreground}""/>
				</Grid>
			</ControlTemplate>";
		private const string PageSymbolTemplate = @"
			<ControlTemplate xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""  xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
				<Path x:Name=""Element""
					  Stroke=""Blue""
					  StrokeStartLineCap=""Round""
					  StrokeThickness=""2.0""
					  StrokeLineJoin=""Round""
					  StrokeEndLineCap=""Round""
					  Fill=""#050000E0""
					 >
					<VisualStateManager.VisualStateGroups>
						<VisualStateGroup x:Name=""SelectionStates"">
							<VisualState x:Name=""Unselected"" />
							<VisualState x:Name=""Selected"">
								<Storyboard>
									<ColorAnimationUsingKeyFrames Storyboard.TargetName=""Element""
																   Storyboard.TargetProperty=""(Path.Fill).(SolidColorBrush.Color)"">
										<DiscreteColorKeyFrame KeyTime=""0:0:0"" Value=""#152000E0"" />
									</ColorAnimationUsingKeyFrames>
									<ColorAnimationUsingKeyFrames Storyboard.TargetName=""Element""
																   Storyboard.TargetProperty=""(Path.Stroke).(SolidColorBrush.Color)"">
										<DiscreteColorKeyFrame KeyTime=""0:0:0"" Value=""Cyan"" />
									</ColorAnimationUsingKeyFrames>
									<DoubleAnimationUsingKeyFrames Storyboard.TargetName=""Element""
										Storyboard.TargetProperty=""(Path.StrokeThickness)"">
										<DiscreteDoubleKeyFrame KeyTime=""0:0:0"" Value=""4.0"" />
									</DoubleAnimationUsingKeyFrames>
								</Storyboard>
							</VisualState>
						</VisualStateGroup>
					</VisualStateManager.VisualStateGroups>
				</Path>
			</ControlTemplate>";


		static ControlTemplate CreateControlTemplate(string template)
		{
			ControlTemplate controlTemplate;
			using (var templateStream = new System.IO.MemoryStream(System.Text.Encoding.Default.GetBytes(template)))
			{
				controlTemplate = System.Windows.Markup.XamlReader.Load(templateStream) as ControlTemplate;
			}
			return controlTemplate;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PrintOverviewLayer"/> class.
		/// </summary>
		public PrintOverviewLayer()
		{
			PageFillSymbol = new FillSymbol
			{
				ControlTemplate = CreateControlTemplate(PageSymbolTemplate),
				//BorderBrush = new SolidColorBrush(Colors.Blue),
				//BorderThickness = 2.0,
				//Fill = new SolidColorBrush(new Color { A = 10, B = 0xE0, G = 0, R = 0 })
			};

			ExtentFillSymbol = new FillSymbol
			{
				BorderBrush = new SolidColorBrush(new Color(){A=0xA0}),
				BorderThickness = 1.0,
				//Fill = new SolidColorBrush(new Color { A = 10, R = 0xFF, G = 0, B = 0 })
			};

			TextSymbol = new TextSymbol
			{
				ControlTemplate = CreateControlTemplate(TextTemplate),
				Foreground = new SolidColorBrush(new Color { A = 0x80, B = 0xFF, G = 0, R = 0 }),
				FontSize = 72
			};
		}

		#endregion

		#region DependencyProperty MapPrinter

		/// <summary>
		/// Gets or sets the map printer the overview layer is buddied to.
		/// </summary>
		/// <value>The map printer.</value>
		public MapPrinter MapPrinter
		{
			get { return (MapPrinter)GetValue(MapPrinterProperty); }
			set { SetValue(MapPrinterProperty, value); }
		}

		/// /// <summary>
		/// Identifies the <see cref="MapPrinter"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty MapPrinterProperty =
				DependencyProperty.Register("MapPrinter", typeof(MapPrinter), typeof(PrintOverviewLayer), new PropertyMetadata(null, OnMapPrinterChanged));

		private static void OnMapPrinterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((PrintOverviewLayer) d).OnMapPrinterChanged(e.OldValue as MapPrinter, e.NewValue as MapPrinter);
		}

		private void OnMapPrinterChanged(MapPrinter oldMapPrinter, MapPrinter newMapPrinter)
		{
			if (oldMapPrinter != null)
			{
				oldMapPrinter.PropertyChanged -= MapPrinterPropertyChanged;
			}
			if (newMapPrinter != null)
			{
				newMapPrinter.PropertyChanged += MapPrinterPropertyChanged;
				if (newMapPrinter.Map != null)
					SpatialReference = newMapPrinter.Map.SpatialReference;
			}
			AddPages();
			AddPrintExtent();
		}

		void MapPrinterPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case "PagesGeometries":
					AddPages();
					break;
				case "PrintExtent":
					AddPrintExtent();
					break;
				case "CurrentPage":
					ChangeCurrentPage();
					break;
			}
		}

		#endregion

		#region DependencyProperty ExtentFillSymbol

		/// <summary>
		/// Gets or sets the extent fill symbol used to draw the print extent.
		/// </summary>
		/// <value>The extent fill symbol.</value>
		[Category("PrintOverviewLayer Symbols")]
		public FillSymbol ExtentFillSymbol
		{
			get { return (FillSymbol)GetValue(ExtentFillSymbolProperty); }
			set { SetValue(ExtentFillSymbolProperty, value); }
		}

		/// /// <summary>
		/// Identifies the <see cref="ExtentFillSymbol"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty ExtentFillSymbolProperty =
				DependencyProperty.Register("ExtentFillSymbol", typeof(FillSymbol), typeof(PrintOverviewLayer), new PropertyMetadata(OnExtentFillSymbolChanged));

		private static void OnExtentFillSymbolChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((PrintOverviewLayer)d).AddPrintExtent();
		}
		#endregion

		#region DependencyProperty PageFillSymbol

		/// <summary>
		/// Gets or sets the symbol used to draw the extent of a page.
		/// </summary>
		/// <value>The page fill symbol.</value>
		[Category("PrintOverviewLayer Symbols")]
		public FillSymbol PageFillSymbol
		{
			get { return (FillSymbol)GetValue(PageFillSymbolProperty); }
			set { SetValue(PageFillSymbolProperty, value); }
		}

		/// /// <summary>
		/// Identifies the <see cref="Map"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty PageFillSymbolProperty =
				DependencyProperty.Register("PageFillSymbol", typeof(FillSymbol), typeof(PrintOverviewLayer), new PropertyMetadata(OnPageFillSymbolChanged));

		private static void OnPageFillSymbolChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((PrintOverviewLayer)d).AddPages();
		}
		#endregion

		#region DependencyProperty TextSymbol

		/// <summary>
		/// Gets or sets the text symbol used to display the page number.
		/// </summary>
		/// <value>The text symbol.</value>
		[Category("PrintOverviewLayer Symbols")]
		public Symbol TextSymbol
		{
			get { return (Symbol)GetValue(TextSymbolProperty); }
			set { SetValue(TextSymbolProperty, value); }
		}

		/// /// <summary>
		/// Identifies the <see cref="TextSymbol"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty TextSymbolProperty =
				DependencyProperty.Register("TextSymbol", typeof(Symbol), typeof(PrintOverviewLayer), new PropertyMetadata(OnTextSymbolChanged));

		private static void OnTextSymbolChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((PrintOverviewLayer)d).AddPages();
		}
		#endregion

		/// <summary>
		/// Show Pages even If Scale Not Fixed.
		/// </summary>
		public bool ShowPagesIfScaleNotFixed { get; set; }

		private Graphic PrintExtentGraphic { get; set; }

		#region private void AddPages()
		/// <summary>
		/// Adds all graphics displaying the print _pages.
		/// </summary>
		private void AddPages()
		{
			Graphics.Clear();

			if (MapPrinter != null && MapPrinter.PagesGeometries != null && (MapPrinter.IsScaleFixed || ShowPagesIfScaleNotFixed))
			{
				int page = 0;
				foreach (Geometry.Geometry geometry in MapPrinter.PagesGeometries.Where(geometry => geometry != null))
				{
					page++;
					Symbol symbol = PageFillSymbol;

					if (symbol != null)
					{
						var graphic = new Graphic { Geometry = geometry, Symbol = symbol };
						graphic.Attributes.Add(new KeyValuePair<string, object>("Page", page));
						graphic.Attributes.Add(new KeyValuePair<string, object>("PageCount", MapPrinter.PageCount));
						Graphics.Add(graphic);
					}

					if (TextSymbol != null && geometry.Extent != null)
					{
						var graphic = new Graphic { Geometry = geometry.Extent.GetCenter(), Symbol = TextSymbol };
						graphic.Attributes.Add(new KeyValuePair<string, object>("Page", page));
						graphic.Attributes.Add(new KeyValuePair<string, object>("PageCount", MapPrinter.PageCount));
						Graphics.Add(graphic);
					}
				}
			}

			if (PrintExtentGraphic != null)
				Graphics.Add(PrintExtentGraphic);
			ChangeCurrentPage();
			OnPropertyChanged("FullExtent");
		}
		#endregion

		#region private void AddPrintExtent()
		/// <summary>
		/// Adds the graphic displaying the print extent.
		/// </summary>
		private void AddPrintExtent()
		{
			if (DesignerProperties.GetIsInDesignMode(this))
				return; // to avoid crash in design. Why?

			Geometry.Envelope printExtent = (MapPrinter == null ? null : MapPrinter.PrintExtent);

			if (printExtent == null || (!MapPrinter.IsScaleFixed && !ShowPagesIfScaleNotFixed))
			{
				if (PrintExtentGraphic != null)
				{
					Graphics.Remove(PrintExtentGraphic);
					PrintExtentGraphic = null;
				}
			}
			else
			{
				if (PrintExtentGraphic == null)
				{
					PrintExtentGraphic = new Graphic();
					Graphics.Add(PrintExtentGraphic);
				}
				PrintExtentGraphic.Geometry = printExtent;
				PrintExtentGraphic.Symbol = ExtentFillSymbol;
			}
			OnPropertyChanged("FullExtent");
		}

		#endregion

		#region private void ChangeCurrentPage()
		private void ChangeCurrentPage()
		{
			foreach (var graphic in SelectedGraphics.ToList())
			{
				graphic.Selected = false;
				graphic.SetZIndex(0);
			}

			foreach (var graphic in Graphics.Where(g => g.Attributes != null && g.Attributes.ContainsKey("Page") && MapPrinter.CurrentPage.Equals(g.Attributes["Page"])))
			{
				graphic.Select();
				graphic.SetZIndex(100);
			}
		}
		#endregion

	}
}



