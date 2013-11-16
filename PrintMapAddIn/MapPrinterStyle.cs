using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Windows;
using MapPrintingControls;

namespace PrintMapAddIn
{
	[DataContract]
	public class MapPrinterStyle :INotifyPropertyChanged
	{
		private string _name;
		private string _xamlStyle;
		private Style _style;
		private bool _isActive;
		private string _description;

		public void Copy(MapPrinterStyle style)
		{
			ID = style.ID;
			Name = style.Name;
			XamlStyle = style.XamlStyle;
			Style = style.Style;
			IsActive = style.IsActive;
			Description = style.Description;
		}

		public MapPrinterStyle Clone()
		{
			var clone = new MapPrinterStyle();
			clone.Copy(this);
			return clone;
		}

		[DataMember(Name = "ID")]
		public string ID { get; set; }

		[DataMember(Name = "Name")]
		public string Name
		{
			get { return _name; }
			set { SetProperty(ref _name, value); }
		}

		[DataMember(Name = "Description")]
		public string Description
		{
			get { return _description; }
			set { SetProperty(ref _description, value); }
		}

		[DataMember(Name = "XamlStyle")]
		public string XamlStyle
		{
			get { return _xamlStyle; }
			set { SetProperty(ref _xamlStyle, value); }
		}

		[DataMember(Name = "IsActive")]
		public bool IsActive
		{
			get { return _isActive; }
			set { SetProperty(ref _isActive, value); }
		}

		public Style Style
		{
			get { return _style; }
			set { SetProperty(ref _style, value); }
		}


		internal void InitStyle()
		{
			Style = CreateStyle(XamlStyle);
		}

		internal static Style CreateStyle(string xaml)
		{
			Style style = null;
			if (!string.IsNullOrEmpty(xaml))
			{
				using (var stream = new System.IO.MemoryStream(System.Text.Encoding.Default.GetBytes(xaml)))
				{
					var dict = System.Windows.Markup.XamlReader.Load(stream) as ResourceDictionary;
					if (dict != null)
					{
						style = dict.Values.OfType<Style>().FirstOrDefault(s => s.TargetType == typeof(MapPrinter));
					}
				}
			}
			return style;
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
		protected void NotifyPropertyChanged(string propertyName = null)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		protected void SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
		{
			if (!Equals(storage, value))
			{
				storage = value;
				NotifyPropertyChanged(propertyName);
			}
		}
		#endregion
	}
}
