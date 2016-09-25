using System.Collections;
using System.Collections.Generic;

namespace Neitri
{
	/// <summary>
	/// Support for deffered Add, Remove and Clear to/from ICollection. Said methods are not executed until Update is called. This way you can easily modify collection during enumeration.
	/// </summary>
	public class DeferredCollection<T, T2> : ICollection<T2> where T : ICollection<T2>, new()
	{
		public T underlyingCollection = new T();

		public int Count
		{
			get
			{
				return underlyingCollection.Count;
			}
		}

		public bool IsReadOnly
		{
			get
			{
				return underlyingCollection.IsReadOnly;
			}
		}

		Queue<IAction> deferredActions = new Queue<IAction>();

		interface IAction
		{
			void Execute(DeferredCollection<T, T2> me);
		}

		struct AddAction : IAction
		{
			public T2 item;

			public void Execute(DeferredCollection<T, T2> me)
			{
				me.underlyingCollection.Add(item);
			}
		}

		struct RemoveAction : IAction
		{
			public T2 item;

			public void Execute(DeferredCollection<T, T2> me)
			{
				me.underlyingCollection.Remove(item);
			}
		}

		struct ClearAction : IAction
		{
			public void Execute(DeferredCollection<T, T2> me)
			{
				me.underlyingCollection.Clear();
			}
		}

		public void Add(T2 item)
		{
			deferredActions.Enqueue(new AddAction() { item = item });
		}

		public bool Remove(T2 item)
		{
			deferredActions.Enqueue(new RemoveAction() { item = item });
			return underlyingCollection.Contains(item);
		}

		public void Clear()
		{
			deferredActions.Enqueue(new ClearAction());
		}

		public bool Contains(T2 item)
		{
			return underlyingCollection.Contains(item);
		}

		public void CopyTo(T2[] array, int arrayIndex)
		{
			underlyingCollection.CopyTo(array, arrayIndex);
		}

		public IEnumerator<T2> GetEnumerator()
		{
			return underlyingCollection.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return underlyingCollection.GetEnumerator();
		}

		public void ProcessDeferredAddRemoveOrClear()
		{
			while (deferredActions.Count > 0)
			{
				var a = deferredActions.Dequeue();
				a.Execute(this);
			}
		}
	}
}