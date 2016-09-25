namespace Neitri
{
	public interface ILogging
	{
		void Trace<T>(T value);

		void Info<T>(T value);

		void Warn<T>(T value);

		void Error<T>(T value);

		void Fatal<T>(T value);
	}
}