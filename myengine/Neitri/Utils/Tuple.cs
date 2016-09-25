namespace Neitri
{
	public class Tuple<T1, T2>
	{
		public readonly T1 Item1;
		public readonly T2 Item2;

		public Tuple(T1 Item1, T2 Item2)
		{
			this.Item1 = Item1;
			this.Item2 = Item2;
		}

		public override int GetHashCode()
		{
			return Item1.GetHashCode() ^ Item2.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			return base.Equals(obj);
		}
	}
}