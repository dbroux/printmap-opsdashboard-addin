using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
#pragma warning disable 1591 // Missing XML comment for publicly visible type or member

namespace MapPrintingControls
{
	/// <summary>
	///  Represents a dynamic collection of keys and values that provides notifications when items
	///  get added, removed, or when the entire list is refreshed.
	/// </summary>
	/// <typeparam name="TKey"> The type of the keys in the dictionary.</typeparam>
	/// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
	public class ObservableDictionary<TKey, TValue> : ObservableCollection<KeyValuePair<TKey, TValue>>, IDictionary<TKey, TValue> where TKey : class 
	{

		#region  IDictionary<TKey, TValue>

		// Summary:
		//     Adds an element with the provided key and value to the System.Collections.Generic.IDictionary<TKey,TValue>.
		//
		// Parameters:
		//   key:
		//     The object to use as the key of the element to add.
		//
		//   value:
		//     The object to use as the value of the element to add.
		//
		// Exceptions:
		//   System.ArgumentNullException:
		//     key is null.
		//
		//   System.ArgumentException:
		//     An element with the same key already exists in the System.Collections.Generic.IDictionary<TKey,TValue>.
		//
		//   System.NotSupportedException:
		//     The System.Collections.Generic.IDictionary<TKey,TValue> is read-only.
		public void Add(TKey key, TValue value)
		{
			if (key == null)
				throw (new ArgumentNullException());
			if (ContainsKey(key))
				throw (new ArgumentException());
			Add(new KeyValuePair<TKey, TValue>(key, value));
		}

		// Summary:
		//     Determines whether the System.Collections.Generic.IDictionary<TKey,TValue>
		//     contains an element with the specified key.
		//
		// Parameters:
		//   key:
		//     The key to locate in the System.Collections.Generic.IDictionary<TKey,TValue>.
		//
		// Returns:
		//     true if the System.Collections.Generic.IDictionary<TKey,TValue> contains
		//     an element with the key; otherwise, false.
		//
		// Exceptions:
		//   System.ArgumentNullException:
		//     key is null.
		public bool ContainsKey(TKey key)
		{
			if (key == null)
				throw (new ArgumentNullException());
			return this.Any(kvp => kvp.Key.Equals(key));
		}

		// Summary:
		//     Gets an System.Collections.Generic.ICollection<T> containing the keys of
		//     the System.Collections.Generic.IDictionary<TKey,TValue>.
		//
		// Returns:
		//     An System.Collections.Generic.ICollection<T> containing the keys of the object
		//     that implements System.Collections.Generic.IDictionary<TKey,TValue>.
		public ICollection<TKey> Keys
		{
			get
			{
				return this.Select(kvp => kvp.Key).ToList();
			}
		}

		// Summary:
		//     Removes the element with the specified key from the System.Collections.Generic.IDictionary<TKey,TValue>.
		//
		// Parameters:
		//   key:
		//     The key of the element to remove.
		//
		// Returns:
		//     true if the element is successfully removed; otherwise, false. This method
		//     also returns false if key was not found in the original System.Collections.Generic.IDictionary<TKey,TValue>.
		//
		// Exceptions:
		//   System.ArgumentNullException:
		//     key is null.
		//
		//   System.NotSupportedException:
		//     The System.Collections.Generic.IDictionary<TKey,TValue> is read-only.
		public bool Remove(TKey key)
		{
			if (key == null)
				throw (new ArgumentNullException());
			return ContainsKey(key) ? Remove(GetKeyValuePair(key)) : false;
		}

		//
		// Summary:
		//     Gets the value associated with the specified key.
		//
		// Parameters:
		//   key:
		//     The key whose value to get.
		//
		//   value:
		//     When this method returns, the value associated with the specified key, if
		//     the key is found; otherwise, the default value for the type of the value
		//     parameter. This parameter is passed uninitialized.
		//
		// Returns:
		//     true if the object that implements System.Collections.Generic.IDictionary<TKey,TValue>
		//     contains an element with the specified key; otherwise, false.
		//
		// Exceptions:
		//   System.ArgumentNullException:
		//     key is null.
		public bool TryGetValue(TKey key, out TValue value)
		{
			if (key == null)
				throw (new ArgumentNullException());

			bool ret;
			if (ContainsKey(key))
			{
				value = GetKeyValuePair(key).Value;
				ret = true;
			}
			else
			{
				value = default(TValue);
				ret = false;
			}
			return ret;
		}

		// Summary:
		//     Gets an System.Collections.Generic.ICollection<T> containing the values in
		//     the System.Collections.Generic.IDictionary<TKey,TValue>.
		//
		// Returns:
		//     An System.Collections.Generic.ICollection<T> containing the values in the
		//     object that implements System.Collections.Generic.IDictionary<TKey,TValue>.
		public ICollection<TValue> Values
		{
			get
			{
				return this.Select(kvp => kvp.Value).ToList();
			}
		}

		// Summary:
		//     Gets or sets the element with the specified key.
		//
		// Parameters:
		//   key:
		//     The key of the element to get or set.
		//
		// Returns:
		//     The element with the specified key.
		//
		// Exceptions:
		//   System.ArgumentNullException:
		//     key is null.
		//
		//   System.Collections.Generic.KeyNotFoundException:
		//     The property is retrieved and key is not found.
		//
		//   System.NotSupportedException:
		//     The property is set and the System.Collections.Generic.IDictionary<TKey,TValue>
		//     is read-only.
		public TValue this[TKey key]
		{
			get
			{
				if (key == null)
					throw(new ArgumentNullException());
				// defaultvalue allows to create a new attribute by binding   e.g : {Binding [Name], Mode=TwoWay} will create the attribute when the binded value changes
				return ContainsKey(key) ? GetKeyValuePair(key).Value : default(TValue);
			}
			set 
			{
				if (ContainsKey(key))
					Remove(key);
				Add(key, value);
			}
		} 
		#endregion

		#region private KeyValuePair<TKey, TValue> GetKeyValuePair(TKey key)

		private KeyValuePair<TKey, TValue> GetKeyValuePair(TKey key)
		{
			return this.FirstOrDefault(kvp => kvp.Key.Equals(key));
		}
 
		#endregion
	}
}
#pragma warning restore 1591 // Missing XML comment for publicly visible type or member

