using System;
using System.IO;

namespace Neitri
{
	public interface IBinarySerializer
	{
		T Deserialize<T>(byte[] sourceBytes, T value = null) where T : class;

		T Deserialize<T>(MemoryStream source, T value = null) where T : class;

		object Deserialize(Type type, byte[] source);

		void Serialize<T>(MemoryStream dest, T value);

		byte[] Serialize<T>(T value);
	}
}