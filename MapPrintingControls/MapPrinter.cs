using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Symbols;


namespace MapPrintingControls
{
	/// <summary>
	/// Control to print a map at scale.
	/// By changing the template of this control, one can change the printed map pages.
	/// </summary>
	[TemplatePart(Name = "PrintMap", Type = typeof(Map))]
	public class MapPrinter : Control, INotifyPropertyChanged
	{
		#region Private Fields
		internal const string BeginStatus = "Starting print";
		internal const string PrintingStatus = "Printing page {0}/{1}, {2}% complete";
		internal const string CancelStatus = "Print cancelled";
		internal const string ErrorStatus = "Error printing";
		internal const string FinishedStatus = "Print complete";
		internal const string PreviewingStatus = "Previewing...";

		private bool _isLoaded;
		private readonly MapPages _pages;
		private readonly ObservableDictionary<string, object> _dataItems;

		#endregion

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the <see cref="MapPrinter"/> class.
		/// </summary>
		public MapPrinter()
		{
			_isPrinting = false;
			_isLoaded = false;

			DefineExtentCommand = new DelegateCommand(DefineExtent, CanDefineExtent);
			PrintCommand = new DelegateCommand(Print, CanPrint);
			CancelPrintCommand = new DelegateCommand(CancelPrint, CanCancelPrint);
			_pages = new MapPages();
			_dataItems = new ObservableDictionary<string, object>();
		}

		static MapPrinter()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(MapPrinter),
				new FrameworkPropertyMetadata(typeof(MapPrinter)));
		}

		#endregion

		#region DependencyProperty Map
		/// <summary>
		/// Gets or sets the map to print.
		/// </summary>
		/// <value>The map.</value>
		[Category("Printing Properties")]
		public Map Map
		{
			get { return (Map)GetValue(MapProperty); }
			set { SetValue(MapProperty, value); }
		}

		/// /// <summary>
		/// Identifies the <see cref="Map"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty MapProperty =
				DependencyProperty.Register("Map", typeof(Map), typeof(MapPrinter), new PropertyMetadata(null, OnMapChanged));

		private static void OnMapChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var mapPrinter = d as MapPrinter;
			if (mapPrinter != null) mapPrinter.OnMapChanged(e.OldValue as Map, e.NewValue as Map);
		}

		private void OnMapChanged(Map oldMap, Map newMap)
		{
			// Hook up LayersInitialized
			if (oldMap != null)
				oldMap.Layers.LayersInitialized -= OnLayersInitialized;

			if (newMap != null)
			{
				newMap.Layers.LayersInitialized += OnLayersInitialized; // to take care of map spatial reference and map extent after the map is initialized

				if (!IsActive)
				{
					_oldMap = Map;
					Map = null; // wait for activating the control
					return;
				}
				if (_isLoaded)
				{
					// Cancel the possibly ongoing print
					CancelPrint(null);

					// Default print extent
					if (PrintExtent == null && IsActive)
						PrintExtent = newMap.Extent;
					else
						PreparePages();
				}
			}
			// Fire CanExecuteChanged
			PrintCommand.CanExecute(null);
		}

		void OnLayersInitialized(object sender, EventArgs args)
		{
			if (_isLoaded)
			{
				// take care of possible new spatial reference and map extent
				if (PrintExtent == null && IsActive)
					PrintExtent = Map.Extent;
				else
					PreparePages();

				// Zoom to current page
				SetPrintMapExtent(CurrentPage);
			}
		}
		#endregion

		#region DependencyProperty PrintExtent
		/// <summary>
		/// Gets or sets the extent to print.
		/// If the PrintExtent is not initialized, the current Map extent will be used.
		/// </summary>
		/// <value>The extent.</value>
		[Category("Printing Properties")]
		public Envelope PrintExtent
		{
			get { return (Envelope)GetValue(PrintExtentProperty); }
			set { SetValue(PrintExtentProperty, value); }
		}

		/// /// <summary>
		/// Identifies the <see cref="PrintExtent"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty PrintExtentProperty =
				DependencyProperty.Register("PrintExtent", typeof(Envelope), typeof(MapPrinter), new PropertyMetadata(null, OnPrintExtentChanged));

		private static void OnPrintExtentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var mapPrinter = d as MapPrinter;
			if (mapPrinter != null)
			{
				mapPrinter.PreparePages();
				mapPrinter.NotifyPropertyChanged("PrintExtent");
			}
		}

		#endregion

		#region DependencyProperty Scale
		/// <summary>
		/// Gets or sets the scale.
		/// </summary>
		/// <value>The scale.</value>
		[Category("Printing Properties")]
		public double Scale
		{
			get { return (double)GetValue(ScaleProperty); }
			set { SetValue(ScaleProperty, value); }
		}

		/// /// <summary>
		/// Identifies the <see cref="Scale"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty ScaleProperty =
				DependencyProperty.Register("Scale", typeof(double), typeof(MapPrinter), new PropertyMetadata(0.0, OnScaleChanged));

		private static void OnScaleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var mapPrinter = d as MapPrinter;
			if (mapPrinter != null && mapPrinter.IsScaleFixed)
				mapPrinter.PreparePages();
		}
		#endregion

		#region DependencyProperty CurrentPage

		/// <summary>
		/// Gets or sets the current page.
		/// </summary>
		/// <value>The current page.</value>
		public int CurrentPage
		{
			get { return (int)GetValue(CurrentPageProperty); }
			set { SetValue(CurrentPageProperty, value); }
		}

		/// /// <summary>
		/// Identifies the <see cref="CurrentPage"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty CurrentPageProperty =
				DependencyProperty.Register("CurrentPage", typeof(int), typeof(MapPrinter), new PropertyMetadata(0, OnCurrentPageChanged));

		private static void OnCurrentPageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var mapPrinter = d as MapPrinter;
			if (mapPrinter != null) mapPrinter.OnCurrentPageChanged((int)e.NewValue);
		}

		private void OnCurrentPageChanged(int currentPage)
		{
			NotifyPropertyChanged("CurrentPage");
			NotifyPropertyChanged("Now"); // in case time is displayed
			if (currentPage <= 0 || (currentPage > PageCount && PageCount != 0))
			{
				throw (new Exception(string.Format("CurrentPage must be between 1 and {0}", PageCount)));
			}
			SetPrintMapExtent(currentPage);
			OnPageChanged(currentPage);
		}


		/// <summary>
		/// Sets the print map extent to the extent of a page (extent can have been changed by user ==> useful even if page is the current page)
		/// </summary>
		/// <param name="page">The page.</param>
		internal void SetPrintMapExtent(int page)
		{
			PrintMap.Extent = ChangeExtentWhenRotated(_pages[page], RotateMap);
		}

		// Workaround for the extent strange behavior when the map is rotated.
		// Looks like if Map.Rotate = 90, the extent of the map after setting it is not the right one (width and height inversed)
		// This workaround is not perfectly working but seems better than if nothing was done --> to reconsider anyway 
		private static Envelope ChangeExtentWhenRotated(Envelope env, bool rotateMap)
		{
			if (rotateMap && env != null)
			{
				MapPoint center = env.GetCenter();
				return new Envelope(center.X - env.Height/2.0, center.Y - env.Width/2.0, center.X + env.Height/2.0,
				                    center.Y + env.Width/2.0) {SpatialReference = env.SpatialReference};
			}
			return env;
		}

		#endregion

		#region Property DataItems

		/// <summary>
		/// Provides the ability to specify key value pairs of data. The key should correspond to an element name in the print template
		/// which will have the value data inserted into it.
		/// </summary>
		public ObservableDictionary<string, object> DataItems
		{
			get
			{
				return _dataItems;
			}
		}

		#endregion

		#region DependencyProperty Title
		/// <summary>
		/// Gets or sets the title of the print document.
		/// </summary>
		/// <value>The title of the print document.</value>
		[Category("Printing Properties")]
		public string Title
		{
			get { return (string)GetValue(TitleProperty); }
			set { SetValue(TitleProperty, value); }
		}

		/// <summary>
		/// Identifies the <see cref="Title"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty TitleProperty =
				DependencyProperty.Register("Title", typeof(string), typeof(MapPrinter), new PropertyMetadata("Map Document"));
		#endregion

		#region DependencyProperty RotateMap

		/// <summary>
		/// Gets or sets the rotate map flag.
		/// </summary>
		/// <value>Indicates if the map is printed with a 90° rottaion.</value>
		[Category("Printing Properties")]
		public bool RotateMap
		{
			get { return (bool)GetValue(RotateMapProperty); }
			set { SetValue(RotateMapProperty, value); }
		}

		/// /// <summary>
		/// Identifies the <see cref="RotateMap"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty RotateMapProperty =
				DependencyProperty.Register("RotateMap", typeof(bool), typeof(MapPrinter), new PropertyMetadata(false, OnRotateMapChanged));

		private static void OnRotateMapChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var mapPrinter = d as MapPrinter;
			if (mapPrinter != null) mapPrinter.OnRotateMapChanged((bool)e.NewValue);
		}

		private void OnRotateMapChanged(bool rotateMap)
		{
			if (_isLoaded)
			{
				PrintMap.Rotation = (rotateMap ? -90 : 0);
				PreparePages();
			}
		}
		#endregion

		#region DependencyProperty IsScaleFixed
		/// <summary>
		/// Gets or sets a value indicating whether this instance is scale fixed.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is scale fixed; otherwise, <c>false</c>.
		/// </value>
		[Category("Printing Properties")]
		public bool IsScaleFixed
		{
			get { return (bool)GetValue(IsScaleFixedProperty); }
			set { SetValue(IsScaleFixedProperty, value); }
		}

		/// /// <summary>
		/// Identifies the <see cref="IsScaleFixed"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty IsScaleFixedProperty =
				DependencyProperty.Register("IsScaleFixed", typeof(bool), typeof(MapPrinter), new PropertyMetadata(false, OnIsScaleFixedChanged));

		private static void OnIsScaleFixedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var mapPrinter = d as MapPrinter;
			if (mapPrinter != null) mapPrinter.PreparePages();
		}

		#endregion

		#region DependencyProperty MapUnits

		/// <summary>
		/// Gets or sets the map units.
		/// </summary>
		/// <value>The map units.</value>
		[Category("Printing Properties")]
		public MapUnit MapUnits
		{
			get { return (MapUnit)GetValue(MapUnitsProperty); }
			set { SetValue(MapUnitsProperty, value); }
		}

		/// /// <summary>
		/// Identifies the <see cref="MapUnits"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty MapUnitsProperty =
				DependencyProperty.Register("MapUnitsProperty", typeof(MapUnit), typeof(MapPrinter), new PropertyMetadata(MapUnit.Undefined));

		#endregion

		#region DependencyProperty IsActive
		/// <summary>
		/// Gets or sets a value indicating whether this instance is active.
		/// </summary>
		/// <remarks>
		/// The map is cloned for printing when the IsActive property is set to true.
		/// </remarks>
		/// <value><c>true</c> if this instance is active; otherwise, <c>false</c>.</value>
		[Category("Printing Properties")]
		public bool IsActive
		{
			get { return (bool)GetValue(IsActiveProperty); }
			set { SetValue(IsActiveProperty, value); }
		}

		/// /// <summary>
		/// Identifies the <see cref="IsActive"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty IsActiveProperty =
				DependencyProperty.Register("IsActive", typeof(bool), typeof(MapPrinter), new PropertyMetadata(true, OnIsActiveChanged));

		private static void OnIsActiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var mapPrinter = d as MapPrinter;
			if (mapPrinter != null) mapPrinter.OnIsActiveChanged((bool)e.NewValue);
		}

		private Map _oldMap;
		private void OnIsActiveChanged(bool newIsActive)
		{
			if (!_isLoaded)
				return;

			if (newIsActive)
			{
				if (Map == null && _oldMap != null)
				{
					// reset the map (will clone it if needed)
					Map = _oldMap;
					_oldMap = null;
				}
				if (PrintExtent == null && Map != null)
					PrintExtent = Map.Extent; // Default print extent is current map extent

				// Zoom to current page
				SetPrintMapExtent(CurrentPage);
			}
			else
			{
				CancelPrint(null);
				PrintExtent = null; // PrintExtent will be reinitialized with current map extent when IsActive is set
				if (Map != null)
				{
					_oldMap = Map;
					Map = null; // Free cloned layers (but break the binding-> need something better!)
				}
				ResetDefineExtent();
			}

		}
		#endregion

		#region Property Status

		string _status;
		/// <summary>
		/// Human readable value of the current print status
		/// </summary>
		public string Status
		{
			get { return _status; }
			internal set
			{
				if (value != _status)
				{
					_status = value;
					NotifyPropertyChanged("Status");
				}
			}
		}
		#endregion

		#region Property IsPrinting

		bool _isPrinting;
		/// <summary>
		/// Indicates if a print task is going on.
		/// </summary>
		public bool IsPrinting
		{
			get { return _isPrinting; }
			private set
			{
				if (value != _isPrinting)
				{
					_isPrinting = value;
					NotifyPropertyChanged("IsPrinting");
					PrintCommand.CanExecute(null); // fire event CanExecuteChanged
					CancelPrintCommand.CanExecute(null); // fire event CanExecuteChanged
				}
			}
		}
		#endregion

		#region Property IsCancelingPrint

		bool _isCancelingPrint;
		/// <summary>
		/// Indicates if a print task is going on.
		/// </summary>
		public bool IsCancelingPrint
		{
			get
			{
				return _isCancelingPrint;
			}
			internal set
			{
				if (value != _isCancelingPrint)
				{
					_isCancelingPrint = value;
					if (_isCancelingPrint)
					{
						Debug.WriteLine("IsCanceling is set");
						Status = CancelStatus;
					}
					NotifyPropertyChanged("IsCancelingPrint");
				}
			}
		}
		#endregion

		#region Property PageCount

		/// <summary>
		/// Gets the number of pages.
		/// </summary>
		public int PageCount
		{
			get
			{
				return _pages.PageCount > 0 ? _pages.PageCount : 1; // PageCount==0 is an issue for the slider so...
			}
		}

		#endregion

		#region Property PagesGeometries

		/// <summary>
		/// Gets the geometries of the pages.
		/// </summary>
		public IEnumerable<Geometry> PagesGeometries
		{
			get
			{
				for (int i = 1; i <= _pages.PageCount; i++)
					yield return _pages[i];
			}
		}
		#endregion

		#region Property PrintMap

		/// <summary>
		/// Gets the print map.
		/// </summary>
		public Map PrintMap { get; private set; }

		#endregion

		#region Property Now
		/// <summary>
		/// Gets the current date/time.
		/// </summary>
		public DateTime Now
		{
			get { return DateTime.Now; }
		}
		#endregion

		#region Event OnErrorPrinting

		/// <summary>
		/// This event allows implementing classes listen for errors during printing
		/// </summary>
		public event EventHandler<ExceptionEventArgs> OnErrorPrinting;

		/// <summary>
		/// If the implementing class has defined a listener for errors then report the exception that has occured. Otherwise throw the exception
		/// </summary>
		/// <param name="ex">The exception that has occured</param>
		protected void ThrowError(Exception ex)
		{
			Status = ErrorStatus;
			Debug.WriteLine("Error during print ; {0}", ex.Message);
			var handler = OnErrorPrinting;
			if (handler != null)
				handler(this, new ExceptionEventArgs(ex));
			else
				throw ex;
		}

		#endregion

		#region Event PageChanged

		/// <summary>
		/// Occurs when the page to print changed.
		/// </summary>
		public event EventHandler<PageChangedEventArgs> PageChanged;

		private void OnPageChanged(int page)
		{
			var handler = PageChanged;
			if (handler != null)
				handler(this, new PageChangedEventArgs(page));
		}

		#endregion

		#region Event PrintProgress

		/// <summary>
		/// Occurs when the print is ongoing.
		/// </summary>
		public event EventHandler<PrintProgressEventArgs> PrintProgress;
		private void OnPrintProgress(PrintProgressEventArgs e)
		{
			var handler = PrintProgress;
			if (handler != null)
			{
				handler(this, e);
			}
		}

		#endregion

		#region Overriden OnApplyTemplate
		/// <summary>
		/// Implementation of OnApplyTemplate so that child classes can supply a map control
		/// </summary>
		public override void OnApplyTemplate()
		{
			ResetDefineExtent(); // If ongoing defineprintnextent
			if (PrintMap != null && PrintMap.Layers != null)
			{
				foreach (PrintOverviewLayer overviewLayer in PrintMap.Layers.OfType<PrintOverviewLayer>())
				{
					overviewLayer.ClearGraphics();
					overviewLayer.MapPrinter = null;
				}
				PrintMap.Layers.Clear(); // Free previous cloned layers
			}

			base.OnApplyTemplate();
			PrintMap = GetTemplateChild("PrintMap") as Map;
			if (PrintMap == null)
			{
				ThrowError(new Exception("Need a PrintMap element in template"));
				return;
			}
			_isLoaded = true;
			PrintMap.SizeChanged += (s, e) => { PreparePages(); SetPrintMapExtent(CurrentPage); }; // Calculate print pages when printMap size will be known (or changed by selecting a printer or a printer paper size)
			PrintMap.ExtentChanged += OnPrintMapExtentChanged;
			PrintMap.Rotation = (RotateMap ? -90 : 0);
			PrintMap.MapGesture += OnMapGesture;
			OnIsActiveChanged(IsActive);
			PreparePages();

			NotifyPropertyChanged("PrintMap");
		}

		void OnPrintMapExtentChanged(object sender, ExtentEventArgs e)
		{
			// If the scle is not fixed : change the user can select its print extent by zooming/panning
			if (!IsScaleFixed && !IsPrinting)
			{
				PrintExtent = e.NewExtent;
			}
		}

		void OnMapGesture(object sender, Map.MapGestureEventArgs e)
		{
			if (e.Gesture == GestureType.Flick && IsScaleFixed && PageCount > 1)
			{
				int newPage;
				double dx = RotateMap ? -e.Translate.Y : e.Translate.X;
				double dy = RotateMap ? e.Translate.X : e.Translate.Y;

				bool horizontal = Math.Abs(dx) > Math.Abs(dy);


				if (horizontal)
				{
					if (dx < 0)
						newPage = CurrentPage + 1 > PageCount ? 0 : CurrentPage + 1;
					else
						newPage = CurrentPage - 1 <= 0 ? 0 : CurrentPage - 1;
				}
				else
				{
					if (dy < 0)
						newPage = CurrentPage + _pages.NbColumn > PageCount ? 0 : CurrentPage + _pages.NbColumn;
					else
						newPage = CurrentPage - _pages.NbColumn <= 0 ? 0 : CurrentPage - _pages.NbColumn;
				}

				if (newPage != 0)
				{
					CurrentPage = newPage;
					e.Handled = true;
				}
			}
		}

		#endregion

		#region PrintCommand
		/// <summary>
		/// Gets or sets the print command.
		/// </summary>
		/// <value>The print command.</value>
		/// The optional param of the Print command is an enumeration of pages to print.
		public ICommand PrintCommand { get; private set; }

		private void Print(object param)
		{
			List<int> pagesToPrint;

			if (param is IEnumerable<int>)
				pagesToPrint = (param as IEnumerable<int>).ToList();
			else if (param is string && !string.IsNullOrEmpty((string)param))
			{
				// Decode string containing page list  e.g:
				//  1, 5, 12
				//  1-9
				// 1-9, 15, 15-17
				pagesToPrint = new List<int>();
				string[] pages = ((string)param).Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
				foreach (string page in pages)
				{
					if (page.Contains('-'))
					{
						// fromPage-toPage : Range added to the list
						int fromPage, toPage;
						string[] fromToPage = page.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
						if (fromToPage.Length >= 2 && Int32.TryParse(fromToPage[0], out fromPage) && Int32.TryParse(fromToPage.Last(), out toPage)
							  && fromPage >= 0 && toPage >= 0 && toPage >= fromPage)
						{
							pagesToPrint.AddRange(Enumerable.Range(fromPage, toPage - fromPage + 1));
						}
					}
					else
					{
						// One page added to the list
						int numPage;
						if (Int32.TryParse(page, out numPage) && numPage > 0)
							pagesToPrint.Add(numPage);
					}
				}
			}
			else
				pagesToPrint = null;

			// Create the print engine depending on WPF
			var printEngine = new WPF.WpfPrintEngine(this);

			// Hook up PrintProgress just to redirect events
			printEngine.PrintProgress += (s, e) => OnPrintProgress(e);

			// Call the print engine doing the work
			try
			{
				printEngine.Print(pagesToPrint);
			}
			catch (Exception e)
			{
				EndPrint(e);
			}
		}


		private bool CanPrint(object param)
		{
			return IsPrinting == false && (Map != null || (!IsActive && _oldMap != null)); // _oldMap will become current map during print
		}

		#endregion

		#region CancelPrintCommand
		/// <summary>
		/// Gets or sets the cancel print command.
		/// </summary>
		/// <value>The cancel print command.</value>
		public ICommand CancelPrintCommand { get; private set; }

		private void CancelPrint(object param)
		{
			if (IsPrinting)
				IsCancelingPrint = true;
		}

		private bool CanCancelPrint(object param)
		{
			return IsPrinting;
		}
		#endregion

		#region DefineExtentCommand
		private UIElement _defineExtentInstructions;
		private Draw _draw;

		/// <summary>
		/// Gets or sets the define extent command.
		/// </summary>
		/// <value>The define extent command.</value>
		public ICommand DefineExtentCommand { get; private set; }

		private void DefineExtent(object param)
		{
			if (Map == null) return;

			if (_draw == null)
			{
				_draw = new Draw(PrintMap);
				_draw.DrawComplete += DrawDrawComplete;
			}

			var fillSymbol = (PrintOverviewLayer == null ? null : PrintOverviewLayer.ExtentFillSymbol) ?? new FillSymbol();

			_draw.FillSymbol = fillSymbol;

			_defineExtentInstructions = param as UIElement;
			if (_defineExtentInstructions != null)
				_defineExtentInstructions.Visibility = Visibility.Visible;
			_draw.DrawMode = DrawMode.Rectangle;
			_draw.IsEnabled = true;
		}

		private void ResetDefineExtent()
		{
			if (_defineExtentInstructions != null)
				_defineExtentInstructions.Visibility = Visibility.Collapsed;

			if (_draw != null)
			{
				_draw.DrawMode = DrawMode.None;
				_draw.IsEnabled = false;
				_draw = null;
			}
		}

		void DrawDrawComplete(object sender, DrawEventArgs e)
		{
			ResetDefineExtent();

			var env = e.Geometry as Envelope;

			if (env != null && env.Height > 0 && env.Width > 0)
				PrintExtent = env;
		}

		private static bool CanDefineExtent(object param)
		{
			return true;
		}

		#endregion

		/// <summary>
		/// Sets the printable area.
		/// </summary>
		/// <param name="printableAreaHeight">Height of the printable area.</param>
		/// <param name="printableAreaWidth">Width of the printable area.</param>
		public void SetPrintableArea(double printableAreaHeight, double printableAreaWidth)
		{
			// Recalculate layout in order to fit printable area
			Height = printableAreaHeight;
			Width = printableAreaWidth;

			if (PrintMap != null)
			{
				// Be sure height and width of map is not fixed in template
				PrintMap.Height = double.NaN;
				PrintMap.Width = double.NaN;

				// Update map size
				UpdateLayout();
			}
		}

		#region INotifyPropertyChanged Members
		/// <summary>
		/// Occurs when a property value changes.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Notifies the property changed.
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		protected void NotifyPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
		#endregion

		#region private void PreparePages()
		/// <summary>
		/// Prepares the pages to print.
		/// Calculate the number of pages to print.
		/// Set the extent of the current page
		/// </summary>
		private void PreparePages()
		{
			if (!_isLoaded || Map == null)
				return;

			// Calculate pages to print
			_pages.PreparePages(this, PrintMap.ActualWidth, PrintMap.ActualHeight);

			if (PageCount == 0 || CurrentPage == 0)
				CurrentPage = 1;
			else if (CurrentPage > PageCount)
				CurrentPage = PageCount;

			NotifyPropertyChanged("PageCount");
			NotifyPropertyChanged("PagesGeometries");
		}
		#endregion

		#region private PrintOverviewLayer
		private PrintOverviewLayer PrintOverviewLayer
		{
			get
			{
				if (PrintMap != null && PrintMap.Layers != null)
					return PrintMap.Layers.OfType<PrintOverviewLayer>().FirstOrDefault();
				return null;
			}
		}
		#endregion

		#region internal Print methods useable by the print engine

		private bool _wasActive = true;

		internal void BeginPrint()
		{
			if (IsPrinting)
			{
				ThrowError(new Exception("Only one print task allowed at a time"));
				return;
			}
			if (PrintMap == null)
			{
				ThrowError(new Exception("MapPrinter must be loaded before printing"));
				return;
			}

			// Active the mapPrinter if it's not (useful for printing without dialog)
			_wasActive = IsActive;
			if (!IsActive)
				IsActive = true;

			Status = BeginStatus;
			IsCancelingPrint = false;
			IsPrinting = true;

			if (PrintOverviewLayer != null)
				PrintOverviewLayer.Visible = false;

			NotifyPropertyChanged("Now"); // in case time is displayed
			Debug.WriteLine("BeginPrint");
		}

		internal void EndPrint(Exception error)
		{
			Debug.WriteLine("EndPrint");
			if (PrintOverviewLayer != null)
				PrintOverviewLayer.Visible = true;

			if (error != null && !IsCancelingPrint)
			{
				ThrowError(error);
			}
			Status = FinishedStatus;

			IsPrinting = false;
			IsCancelingPrint = false;

			// Desactivate the mapprinter at the end of the print
			if (!_wasActive)
				IsActive = false;
		}

		#endregion

	} // Class mapPrinter

	#region Enum MapUnit
	/// <summary>
	/// Unit used by the MapPrinter
	/// </summary>
	/// <remarks>The integer value of the enums corresponds to 1/10th of a millimeter</remarks>
	public enum MapUnit
	{
		/// <summary>
		/// Undefined
		/// </summary>
		Undefined = -1,
		/// <summary>
		/// Decimal degrees
		/// </summary>
		DecimalDegrees = 0,
		/// <summary>
		/// Inches
		/// </summary>
		Inches = 254,
		/// <summary>
		/// Feet
		/// </summary>
		Feet = 3048,
		/// <summary>
		/// Yards
		/// </summary>
		Yards = 9144,
		/// <summary>
		/// Miles
		/// </summary>
		Miles = 16093440,
		/// <summary>
		/// Nautical Miles
		/// </summary>
		NauticalMiles = 18520000,
		/// <summary>
		/// Millimeters
		/// </summary>
		Millimeters = 10,
		/// <summary>
		/// Centimeters
		/// </summary>
		Centimeters = 100,
		/// <summary>
		/// Decimeters
		/// </summary>
		Decimeters = 1000,
		/// <summary>
		/// Meters
		/// </summary>
		Meters = 10000,
		/// <summary>
		/// Kilometers
		/// </summary>
		Kilometers = 10000000
	}
	#endregion

	#region ExceptionEventArgs
	/// <summary>
	/// Class that allows an exception to be sent to an event listener
	/// </summary>
	public class ExceptionEventArgs : EventArgs
	{
		/// <summary>
		/// The exception.
		/// </summary>
		/// <value>The exception.</value>
		public Exception Exception { get; internal set; }
		internal ExceptionEventArgs(Exception ex)
		{
			Exception = ex;
		}
	}

	#endregion

	#region PageChangedEventArgs
	/// <summary>
	/// Holds event data for the <see cref="MapPrinter.PageChanged"/> event.
	/// </summary>
	public class PageChangedEventArgs : EventArgs
	{
		/// <summary>
		/// Gets or sets the page.
		/// </summary>
		/// <value>The page.</value>
		public int Page { get; internal set; }
		internal PageChangedEventArgs(int page)
		{
			Page = page;
		}
	}

	#endregion

	#region PrintProgressEventArgs
	/// <summary>
	/// Holds event data for the <see cref="MapPrinter.PrintProgress"/> event.
	/// </summary>
	public sealed class PrintProgressEventArgs : EventArgs
	{
		internal PrintProgressEventArgs(int pageIndex, int pageCount, int pageProgress, int printProgress)
		{
			PageIndex = pageIndex;
			PageCount = pageCount;
			PageProgress = pageProgress;
			PrintProgress = printProgress;
		}

		/// <summary>
		/// Gets or sets the page index being printed.
		/// </summary>
		/// <value>The page index between 1 and PageCount.</value>
		public int PageIndex { get; private set; }

		/// <summary>
		/// Gets or sets the number of page to print.
		/// </summary>
		/// <value>The number of page to print.</value>
		public int PageCount { get; private set; }

		/// <summary>
		/// Gets or sets the page progress.
		/// </summary>
		/// <value>The progress is a value between 0 and 100.</value>
		public int PageProgress { get; private set; }

		/// <summary>
		/// Gets or sets the global print progress.
		/// </summary>
		/// <value>The progress is a value between 0 and 100.</value>
		public int PrintProgress { get; private set; }
	} 
	#endregion


}
