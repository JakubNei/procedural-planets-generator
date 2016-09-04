using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


namespace Neitri
{
	public static class ComponentExtensions
	{
	
	    /*
	    /// <summary>
	    /// Gets all components in the component's transfrom hiearchy
	    /// </summary>
	    /// <param name="component"></param>
	    /// <param name="type">Can be any interface or UnityEngine.Component</param>
	    /// <returns></returns>
	    public static TYPE[] GetAllComponents<TYPE>(this Component component) where TYPE : class
	    {
	        var removeMe = component.Entity.root;
	        if (typeof(TYPE).IsInterface)
	        {            
	            List<TYPE> ret = new List<TYPE>();
	            TYPE add;
	            foreach (var c in removeMe.GetComponentsInChildren<Component>())
	            {
	                add = c as TYPE;
	                if (add != null) ret.Add(add);
	            }
	            return ret.ToArray();
	        }
	        else return removeMe.GetComponentsInChildren<TYPE>();
	    }
	
	    /// <summary>
	    /// Gets first component in the component's transfrom hiearchy
	    /// </summary>
	    /// <param name="component"></param>
	    /// <param name="type">Can be any interface or UnityEngine.Component</param>
	    /// <returns></returns>
	    public static TYPE GetFirstComponent<TYPE>(this Component component) where TYPE : class
	    {
	        var removeMe = component.Entity.root;
	        if (typeof(TYPE).IsInterface)
	        {
	            List<TYPE> ret = new List<TYPE>();
	            foreach (var c in removeMe.GetComponentsInChildren<Component>())
	            {
	                if (c is TYPE) return c as TYPE;
	            }
	            return null;
	        }
	        else return removeMe.GetComponentInChildren<TYPE>();
	    }
	        
	    
	    public static TYPE GetOrAddComponent<TYPE>(this Component c) where TYPE : Component
	    {
	        TYPE component = c.GetComponent<TYPE>();
	        if (component == null) component = c.AddComponent<TYPE>();
	        return component;
	    }
	
	    */
	    
	   
	}
	
}