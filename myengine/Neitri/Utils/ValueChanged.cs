using System;

namespace Neitri
{
	public class ValueChanged<T> : EventArgs
	{
		public T OldValue { get; set; }
		public T NewValue { get; set; }

		public ValueChanged(T oldValue, T newValue)
		{
			this.OldValue = oldValue;
			this.NewValue = newValue;
		}
	}
}