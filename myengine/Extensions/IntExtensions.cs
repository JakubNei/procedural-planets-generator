namespace MyEngine
{
	    public static class IntExtensions
	    {
	        public static int Abs(this int val)
	        {
	            if (val >= 0) return val;
	            return -val;
	        }
	    }
	
}