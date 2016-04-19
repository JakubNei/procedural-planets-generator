using System;
using System.Collections.Generic;

namespace MyEngine
{
	public class TimeoutList<T> : List<T>
	{
	    public delegate void OnRemoved(T item);
	    public event OnRemoved onRemoved = delegate {};
	    //  .onRemoved += delegate(T item) {  };
	
	
	    List<double> times = new List<double>();
	    public void Add(T item, float timeoutInSeconds)
	    {
	        base.Add(item);
	        times.Add(timeoutInSeconds);
	    }
	
	
	    public void Update(float timeoutInSeconds)
	    {
	        List<int> toRemove = new List<int>();
	        for (int i = 0; i < times.Count; i++)
	        {
	            times[i] -= timeoutInSeconds;
	            if (times[i] <= 0) toRemove.Add(i);
	        }
	        foreach (var i in toRemove)
	        {
	            onRemoved(this[i]);
	            times.RemoveAt(i);
	            base.RemoveAt(i);
	        }
	    }
	
	}
}