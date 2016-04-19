using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyEngine
{
    public class Lazy<T>
    {
        T value;
        Func<T> loader;

        public Lazy(T value) { this.value = value; }
        public Lazy(Func<T> loader) { this.loader = loader; }

        T Value
        {
            get
            {
                if (loader != null)
                {
                    value = loader();
                    loader = null;
                }

                return value;
            }
        }

        public static implicit operator T(Lazy<T> lazy)
        {
            return lazy.Value;
        }

        public static implicit operator Lazy<T>(T value)
        {
            return new Lazy<T>(value);
        }
    }
}
