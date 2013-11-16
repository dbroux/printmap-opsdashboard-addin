using System;
using ESRI.ArcGIS.OperationsDashboard;
using System.ComponentModel.Composition;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Controls;

namespace PrintMapAddIn
{
	/// <summary>
	/// A MapTool is an extension to ArcGIS Operations Dashboard which can be configured to appear in the toolbar 
	/// of the map widget, providing a custom map tool.
	/// </summary>
	[Export("ESRI.ArcGIS.OperationsDashboard.MapTool")]
	[ExportMetadata("DisplayName", "Print Map")]
	[ExportMetadata("Description", "Print map at scale.")]
	[ExportMetadata("ImagePath", "/PrintMapAddIn;component/Images/PrintMap.png")]
	[DataContract]
	public partial class PrintMapTool : UserControl, IMapTool
	{
		public PrintMapTool()
		{
			InitializeComponent();
		}

		public PrintMapToolbar PrintMapToolbar
		{
			get { return (PrintMapToolbar)GetValue(PrintMapToolbarProperty); }
			set { SetValue(PrintMapToolbarProperty, value); }
		}

		public static readonly DependencyProperty PrintMapToolbarProperty =
			DependencyProperty.Register("PrintMapToolbar", typeof(PrintMapToolbar), typeof(PrintMapTool), null);

		[DataMember(Name = "PrintStyles")] // serializable attribute
		public StylesManager StylesManager { get; private set; }

		#region IMapTool

		/// <summary>
		/// The MapWidget property is set by the MapWidget that hosts the map tools. The application ensures that this property is set when the
		/// map widget containing this map tool is initialized.
		/// </summary>
		public MapWidget MapWidget { get; set; }

		/// <summary>
		/// OnActivated is called when the map tool is added to the toolbar of the map widget in which it is configured to appear. 
		/// Called when the operational view is opened, and also after a custom toolbar is reverted to the configured toolbar,
		/// and during toolbar configuration.
		/// </summary>
		public void OnActivated()
		{

			if (StylesManager == null) // useful while configuration has not been saved
				StylesManager = new StylesManager();

			// Add the predefined styles to the current list of styles
			StylesManager.AddPredefinedStyles();

			PrintMapToolbar = new PrintMapToolbar(MapWidget, StylesManager);
		}

		/// <summary>
		///  OnDeactivated is called before the map tool is removed from the toolbar. Called when the operational view is closed,
		///  and also before a custom toolbar is installed, and during toolbar configuration.
		/// </summary>
		public void OnDeactivated()
		{
			PrintMapToolbar = null;
		}

		/// <summary>
		///  Determines if a Configure button is shown for the map tool.
		///  Provides an opportunity to gather user-defined settings.
		/// </summary>
		/// <value>True if the Configure button should be shown, otherwise false.</value>
		public bool CanConfigure
		{
			get { return true; }
		}

		/// <summary>
		///  Provides functionality for the map tool to be configured by the end user through a dialog.
		///  Called when the user clicks the Configure button next to the map tool.
		/// </summary>
		/// <param name="owner">The application window which should be the owner of the dialog.</param>
		/// <returns>True if the user clicks ok, otherwise false.</returns>
		public bool Configure(Window owner)
		{
			bool? result = null;
			try
			{
				var dialog = new PrintMapDialog(this) {Owner = owner};
				result = dialog.ShowDialog();
			}
			catch (Exception e)
			{
				MessageBox.Show("Errow while configuring Printmap tool: " + e.Message);
			}
			return result != null && (bool)result;
		}

		#endregion
	}
}
