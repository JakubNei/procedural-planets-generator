using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyEngine
{
	public class DictionaryWatcher<TKey, TItem> : DictionaryWatcher<TKey, TItem, TItem>
	{
		public DictionaryWatcher()
		{
			comparisonValueSelector = (item) => item;
		}
	}

	public class DictionaryWatcher<TKey, TItem, TEqualityComparisonValue>
	{
		public event Action<TKey, TItem> OnAdded;
		/// <summary>
		/// Guarantees that OnAdded was called on same TKey before.
		/// </summary>
		public event Action<TKey, TItem> OnRemoved;
		/// <summary>
		/// Guarantees that OnAdded was called on same TKey before.
		/// </summary>
		public event Action<TKey, TItem> OnUpdated;

		public Func<TEqualityComparisonValue, TEqualityComparisonValue, bool> equalityComparer = (a, b) => a.Equals(b);
		public Func<TItem, TEqualityComparisonValue> comparisonValueSelector;


		Dictionary<TKey, TEqualityComparisonValue> currentValues = new Dictionary<TKey, TEqualityComparisonValue>();

		public void UpdateBy(IDictionary<TKey, TItem> source)
		{
			foreach (var kvp in source)
			{
				TEqualityComparisonValue sourceValue = comparisonValueSelector(kvp.Value);
				TEqualityComparisonValue currentValue;
				if (currentValues.TryGetValue(kvp.Key, out currentValue))
				{
					if (equalityComparer(currentValue, sourceValue) == false)
					{
						currentValues[kvp.Key] = sourceValue;
						OnUpdated.Raise(kvp.Key, kvp.Value);
					}
				}
				else
				{
					currentValues[kvp.Key] = sourceValue;
					OnAdded.Raise(kvp.Key, kvp.Value);
				}
			}

			var keysRemoved = currentValues.Keys.Except(source.Keys).ToArray();
			foreach (var keyRemoved in keysRemoved)
			{
				currentValues.Remove(keyRemoved);
				OnRemoved.Raise(keyRemoved, source[keyRemoved]);
			}

		}

	}
}
