using System.Windows;
using System.Windows.Controls;

namespace MapPrintingControls
{
	/// <summary>
	/// Centered content control. 
	/// </summary>
	public class CenteredContentControl : ContentControl
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CenteredContentControl"/> class.
		/// </summary>
		public CenteredContentControl()
		{
			DefaultStyleKey = typeof(CenteredContentControl);
		}

		/// <summary>
		/// Provides the behavior for the Measure. Classes can override this method to define their own Measure pass behavior.
		/// Set the margin in order the control to be centered.
		/// </summary>
		/// <param name="availableSize">The available size that this object can give to child objects. Infinity (<see cref="F:System.Double.PositiveInfinity"/>) can be specified as a value to indicate that the object will size to whatever content is available.</param>
		/// <returns>
		/// The size that this object determines it needs during layout, based on its calculations of the allocated sizes for child objects; or based on other considerations, such as a fixed container size.
		/// </returns>
		protected override Size MeasureOverride(Size availableSize)
		{
			Size size = base.MeasureOverride(availableSize);
			Margin = new Thickness(-size.Width/2, -size.Height/2, 0, 0);
			return size;
		}
		
	}
}
