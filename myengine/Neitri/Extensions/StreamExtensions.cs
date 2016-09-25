using System.IO;

namespace Neitri
{
	public static class StreamExtensions
	{
		public static string ReadTextToEnd(this Stream s)
		{
			using (var sr = new StreamReader(s))
			{
				return sr.ReadToEnd();
			}
		}
	}
}