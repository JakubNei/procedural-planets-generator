using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neitri.Logging
{
	public class LogNothing : ILog
	{
		public void Log(LogEntry logEntry)
		{
		}
	}
}
