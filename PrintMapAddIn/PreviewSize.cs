using System.Collections.ObjectModel;

namespace PrintMapAddIn
{

	// Collection of PreviewSize (creatable in XAML)
	public class PreviewSizes : ObservableCollection<PreviewSize> { }

	// Represents a Preview Size (creatable in XAML)
	public class PreviewSize
	{
		public string Id { get; set; }
		public double Width { get; set; }
		public double Height { get; set; }
	}
}
