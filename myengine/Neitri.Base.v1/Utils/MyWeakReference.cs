using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neitri.Base
{
    public class MyWeakReference : WeakReference, IEquatable<MyWeakReference>
    {
        public MyWeakReference(object instance) : base(instance)
        {

        }
        public override int GetHashCode()
        {
            if (Target == null) return 0;
            return Target.GetHashCode();
        }
        public static bool operator !=(MyWeakReference a, MyWeakReference b)
        {
            return !(a == b);
        }
        public static bool operator ==(MyWeakReference a, MyWeakReference b)
        {
            if (object.ReferenceEquals(a, b)) return true;
            if (object.ReferenceEquals(a, null)) return false;
            return a.Equals(b);
        }
        public override bool Equals(object other)
        {
            return Equals(other as MyWeakReference);
        }
        public bool Equals(MyWeakReference other)
        {
            if (other == null) return false;
            return this.Target == other.Target;
        } 
    }

    public class MyWeakReference<T> : IEquatable<MyWeakReference<T>> where T : class
    {
        WeakReference wr;

        public T Target
        {
            get
            {
                T target = (T)wr.Target;
                return target;
            }
        }

        public bool IsAlive
        {
            get
            {
                return wr.IsAlive;
            }
        }
        public MyWeakReference(T target)
        {
            wr = new WeakReference(target);
        }
        public override int GetHashCode()
        {
            if (IsAlive == false) return 0;
            return Target.GetHashCode();
        }
        public static bool operator !=(MyWeakReference<T> a, MyWeakReference<T> b)
        {
            return !(a == b);
        }
        public static bool operator ==(MyWeakReference<T> a, MyWeakReference<T> b)
        {
            if (object.ReferenceEquals(a, b)) return true;
            if (object.ReferenceEquals(a, null)) return false;
            return a.Equals(b);
        }
        public override bool Equals(object other)
        {
            return Equals(other as MyWeakReference<T>);
        }
        public bool Equals(MyWeakReference<T> other)
        {
            if (other == null) return false;
            return this.Target == other.Target;
        }
        
    }
}
