using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using ESRI.ArcGIS.Client.Geometry;

namespace MapPrintingControls
{
	/// <summary>
	/// Log converter 
	/// </summary>
	public sealed class LogConverter : IValueConverter
	{
		/// <summary>
		/// Modifies the source data before passing it to the target for display in the UI.
		/// </summary>
		/// <param name="value">The source data being passed to the target.</param>
		/// <param name="targetType">The <see cref="T:System.Type"/> of data expected by the target dependency property.</param>
		/// <param name="parameter">An optional parameter to be used in the converter logic.</param>
		/// <param name="culture">The culture of the conversion.</param>
		/// <returns>
		/// The value to be passed to the target dependency property.
		/// </returns>
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			double input = 0.0;

			if (value is double || value is int)
			{
				input = (double)value;
			}
			else if (value is string)
			{
				input = double.Parse((string)value);
			}
			return (input <= 0.0 ? 0 : Math.Log10(input));
		}

		/// <summary>
		/// Modifies the target data before passing it to the source object.  This method is called only in <see cref="F:System.Windows.Data.BindingMode.TwoWay"/> bindings.
		/// </summary>
		/// <param name="value">The target data being passed to the source.</param>
		/// <param name="targetType">The <see cref="T:System.Type"/> of data expected by the source object.</param>
		/// <param name="parameter">An optional parameter to be used in the converter logic.</param>
		/// <param name="culture">The culture of the conversion.</param>
		/// <returns>
		/// The value to be passed to the source object.
		/// </returns>
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			double input = 0.0;

			if (value is double || value is int)
			{
				input = (double)value;
			}
			else if (value is string)
			{
				input = double.Parse((string)value);
			}

			// Back of Log10 is Pow
			double result = Math.Pow(10, input);

			// Round to 2 digits
			double scale = Math.Pow(10, Math.Floor(input + 1));
			return scale * Math.Round(result/scale, 2);
		}
	}

	/// <summary>
	/// Converter from any object to Visibility
	/// If there is a parameter the conversion is reversed
	/// </summary>
	public sealed class ToVisibilityConverter : IValueConverter
	{
		/// <summary>
		/// Modifies the source data before passing it to the target for display in the UI.
		/// </summary>
		/// <param name="value">The source data being passed to the target.</param>
		/// <param name="targetType">The <see cref="T:System.Type"/> of data expected by the target dependency property.</param>
		/// <param name="parameter">An optional parameter to be used in the converter logic.</param>
		/// <param name="culture">The culture of the conversion.</param>
		/// <returns>
		/// The value to be passed to the target dependency property.
		/// </returns>
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			bool visible;
			if (value is bool)
			{
				visible = (bool)value;
			}
			else if (value is bool?)
			{
				var nullable = (bool?)value;
				visible = nullable.HasValue ? nullable.Value : false;
			}
			else if (value is int)
			{
				visible = ((int)value != 0);
			}
			else if (value is double)
			{
				visible = ((double)value != 0);
			}
			else if (value is string)
			{
				visible = (!string.IsNullOrEmpty((string)value));
			}
			else
				visible = (value != null);

			if (parameter != null)
				visible = !visible;
			return (visible ? Visibility.Visible : Visibility.Collapsed);
		}

		/// <summary>
		/// Modifies the target data before passing it to the source object.  This method is called only in <see cref="F:System.Windows.Data.BindingMode.TwoWay"/> bindings.
		/// </summary>
		/// <param name="value">The target data being passed to the source.</param>
		/// <param name="targetType">The <see cref="T:System.Type"/> of data expected by the source object.</param>
		/// <param name="parameter">An optional parameter to be used in the converter logic.</param>
		/// <param name="culture">The culture of the conversion.</param>
		/// <returns>
		/// The value to be passed to the source object.
		/// </returns>
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return ((value is Visibility) && (((Visibility)value) == Visibility.Visible));
		}
	}

	/// <summary>
	/// Converter expanding an envelope
	/// </summary>
	public sealed class ExpandConverter : IValueConverter
	{
		/// <summary>
		/// Modifies the source data before passing it to the target for display in the UI.
		/// </summary>
		/// <param name="value">The source data being passed to the target.</param>
		/// <param name="targetType">The <see cref="T:System.Type"/> of data expected by the target dependency property.</param>
		/// <param name="parameter">An optional parameter to be used in the converter logic.</param>
		/// <param name="culture">The culture of the conversion.</param>
		/// <returns>
		/// The value to be passed to the target dependency property.
		/// </returns>
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is Envelope)
			{
				double factor = System.Convert.ToDouble(parameter, culture);
				return (value as Envelope).Expand(factor);
			}
			return value;
		}

		/// <summary>
		/// Modifies the target data before passing it to the source object.  This method is called only in <see cref="F:System.Windows.Data.BindingMode.TwoWay"/> bindings.
		/// </summary>
		/// <param name="value">The target data being passed to the source.</param>
		/// <param name="targetType">The <see cref="T:System.Type"/> of data expected by the source object.</param>
		/// <param name="parameter">An optional parameter to be used in the converter logic.</param>
		/// <param name="culture">The culture of the conversion.</param>
		/// <returns>
		/// The value to be passed to the source object.
		/// </returns>
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new Exception("Not implemented");
		}
	}

}
