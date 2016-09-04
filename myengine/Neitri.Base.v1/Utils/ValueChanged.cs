using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neitri
{
    public class ValueChanged<T> : EventArgs
    {
        public T OldValue { get; private set; }
        public T NewValue { get; private set; }
        public ValueChanged(T oldValue, T newValue)
        {
            this.OldValue = oldValue;
            this.NewValue = newValue;
        }
    }
}
