using UnityEngine;
using System.Collections;


namespace Neitri
{
	    public static class RectTransformExtensions
	    {
	        
	        public static float GetLocalHeight(this RectTransform rect)
	        {
	            var corners = new Vector3[4];
	            rect.GetLocalCorners(corners);
	            return Mathf.Abs(corners[0].y - corners[2].y);
	        }
	
	
	        public static float GetLocalWidth(this RectTransform rect)
	        {
	            var corners = new Vector3[4];
	            rect.GetLocalCorners(corners);
	            return Mathf.Abs(corners[0].x - corners[2].x);
	        }
	    }
	
	
}