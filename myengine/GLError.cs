using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyEngine
{
	public class GLError : Exception
	{
		public GLError(object message) : base(message.ToString())
		{

		}
		public GLError(string message) : base(message)
		{

		}
	}
}
