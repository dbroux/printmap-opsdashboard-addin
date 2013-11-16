using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using ESRI.ArcGIS.Client;

namespace MapPrintingControls
{
	/// <summary>
	/// Clone recursively a dependency object
	/// Very limited implementation based on CLR properties
	/// Attached properties are not taken incare except specific case for this sample
	/// 
	/// Is used to clone Layers and Elements of ElementLayer
	/// </summary>
	public static class CloneExtension
	{
		//  
		/// <summary>
		/// Clones a dependency object.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="source">The source to clone.</param>
		/// <returns></returns>
		public static T Clone<T>(this T source) where T : DependencyObject
		{
			Type t = source.GetType(); // can be different from typeof(T)
			var clone = (T)Activator.CreateInstance(t);
			//Debug.WriteLine("Begin Clone {0}", t.ToString());

			// Loop on CLR properties (except name and parent)
			foreach(PropertyInfo propertyInfo in t.GetProperties())
			{
				if (propertyInfo.Name == "Name" || propertyInfo.Name == "Parent" || propertyInfo.Name == "Graphics" || propertyInfo.Name == "ChildLayers" ||
						!propertyInfo.CanRead || propertyInfo.GetGetMethod() == null ||
						propertyInfo.GetIndexParameters().Length > 0)
					continue;

				if (propertyInfo.Name == "Watermark" || propertyInfo.Name == "InputScope") // avoid exception with these unimplemented properties
					continue;

				if (propertyInfo.Name == "Resources")
					continue;

				try
				{
					Object value = propertyInfo.GetValue(source, null);
					if (value != null)
					{
						if (propertyInfo.PropertyType.GetInterface("IList", true) != null && !propertyInfo.PropertyType.IsArray)
						{
							// Collection ==> loop on items and clone them (we suppose the collection itself is already initialized!)
							var count = (int)propertyInfo.PropertyType.InvokeMember("get_Count", BindingFlags.InvokeMethod, null, value, null);
							propertyInfo.PropertyType.InvokeMember("Clear", BindingFlags.InvokeMethod, null, propertyInfo.GetValue(clone, null), null); // without this line, text can be duplicated due to inlines objects added after text is set

							//Debug.WriteLine("IList property {0} count = {1}", propertyInfo.Name, count);
							for (int index = 0; index < count; index++)
							{
								//Debug.WriteLine("IList  {0} item = {1}", propertyInfo.Name, index);
								object itemValue = propertyInfo.PropertyType.InvokeMember("get_Item", BindingFlags.InvokeMethod, null, propertyInfo.GetValue(source, null), new object[] { index });
								propertyInfo.PropertyType.InvokeMember("Add", BindingFlags.InvokeMethod, null, propertyInfo.GetValue(clone, null), new[] { CloneIfDO(itemValue) });
							}
						}
						else if (propertyInfo.CanWrite && propertyInfo.GetSetMethod() != null)
						{
							//Debug.WriteLine("Init property {0} value:{1}", propertyInfo.Name, value.ToString());

							propertyInfo.SetValue(clone, CloneIfDO(value), null);
						}
					}

				}
				catch (Exception e)
				{
					Debug.WriteLine("Exception during init of property {0} : {1}", propertyInfo.Name, e.Message);
				}
			}


			// Copy some useful attached properties (not done by reflection at this time)
			if (source is UIElement)
			{
				DependencyProperty attachedProperty = ElementLayer.EnvelopeProperty; // needed for ElementLayer
				SetDP(attachedProperty, source, clone);
			}

			return clone;
		}

		static private object CloneIfDO(object source)
		{
			return source is DependencyObject && !(source is ControlTemplate) ? (source as DependencyObject).Clone() : source;
		}

		static private void SetDP(DependencyProperty dp, DependencyObject source, DependencyObject clone)
		{
			Object value = source.GetValue(dp);

			if (value != null)
			{
				try
				{
					clone.SetValue(dp, CloneIfDO(value));
				}
				catch (Exception e)
				{
					Debug.WriteLine("Exception in setDP {0}", e.Message);
				}
			}
		}
	}

}
