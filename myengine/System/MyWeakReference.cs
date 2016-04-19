using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyEngine
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
}
