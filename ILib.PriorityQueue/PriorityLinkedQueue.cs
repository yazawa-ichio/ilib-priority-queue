using System.Collections.Generic;

namespace ILib.PriorityQueue
{
	public enum QueueSearchMode
	{
		First,
		Last,
		Prev,
	}

	public enum DuplicateOperation
	{
		Ignore,
		AllowDuplicate,
		Override,
	}

	public partial class PriorityLinkedQueue<TKey, TValue>
	{

		public delegate DuplicateOperation DuplicateOperationDelegate(TKey key, TValue existing, TValue value);

		internal sealed class Node
		{
			public TKey Key;
			public TValue Value;
			public Node Prev;
			public Node Next;
		}

		readonly IComparer<TKey> m_Comparer;
		readonly NodePool m_Pool;
		Node m_First;
		Node m_Last;
		Node m_Prev;

		public DuplicateOperationDelegate OnDuplicate;

		public bool IsEmpty => m_First == null;

		public PriorityLinkedQueue() : this(Comparer<TKey>.Default, NodePool.Shared) { }

		public PriorityLinkedQueue(IComparer<TKey> comparer) : this(comparer, NodePool.Shared) { }

		public PriorityLinkedQueue(IComparer<TKey> comparer, NodePool pool)
		{
			m_Comparer = comparer;
			m_Pool = pool;
		}

		public void SetAllowDuplicateMode()
		{
			OnDuplicate = (key, e, v) => DuplicateOperation.AllowDuplicate;
		}

		public void SetDuplicateIgnoreMode()
		{
			OnDuplicate = null;
		}

		public void Enqueue(TKey key, in TValue value, QueueSearchMode mode = QueueSearchMode.Last)
		{
			if (m_First == null)
			{
				m_Last = m_Prev = m_First = m_Pool.Pop(key, in value);
				return;
			}
			var cur = m_Last;
			switch (mode)
			{
				case QueueSearchMode.First:
					cur = m_First;
					break;
				case QueueSearchMode.Prev:
					cur = m_Prev;
					break;
			}
			var comp = m_Comparer.Compare(key, cur.Key);
			while (true)
			{
				if (comp == 0)
				{
					var op = OnDuplicate?.Invoke(key, cur.Value, value) ?? DuplicateOperation.Ignore;
					switch (op)
					{
						case DuplicateOperation.AllowDuplicate:
							while (cur.Next != null)
							{
								if (m_Comparer.Compare(key, cur.Next.Key) != 0)
								{
									break;
								}
								cur = cur.Next;
							}
							var node = m_Pool.Pop(key, in value);
							node.Prev = cur;
							node.Next = cur.Next;
							cur.Next = node;
							if (node.Next != null)
							{
								node.Next.Prev = node;
							}
							break;
						case DuplicateOperation.Override:
							cur.Value = value;
							break;
					}
					return;
				}
				else if (comp > 0)
				{
					if (cur.Next == null)
					{
						var node = m_Pool.Pop(key, in value);
						node.Prev = cur;
						cur.Next = node;
						m_Last = m_Prev = node;
						return;
					}
					comp = m_Comparer.Compare(key, cur.Next.Key);
					if (comp < 0)
					{
						var node = m_Pool.Pop(key, in value);
						node.Prev = cur;
						node.Next = cur.Next;
						node.Next.Prev = node;
						cur.Next = node;
						m_Prev = node;
						return;
					}
					else
					{
						cur = cur.Next;
					}
				}
				else
				{
					if (cur.Prev == null)
					{
						var node = m_Pool.Pop(key, in value);
						node.Next = cur;
						cur.Prev = node;
						m_First = m_Prev = node;
					}
					comp = m_Comparer.Compare(key, cur.Prev.Key);
					if (comp > 0)
					{
						var node = m_Pool.Pop(key, in value);
						node.Next = cur;
						node.Prev = cur.Prev;
						cur.Prev.Next = node;
						cur.Prev = node;
						m_Prev = node;
						return;
					}
					else
					{
						cur = cur.Prev;
					}
				}
			}

		}

		public (TKey key, TValue value) Peek()
		{
			return (m_First.Key, m_First.Value);
		}

		public (TKey key, TValue value) Dequeue()
		{
			var ret = m_First;
			try
			{
				if (ret == m_Last)
				{
					m_Last = null;
				}
				if (ret == m_Prev)
				{
					m_Prev = m_Last;
				}
				m_First = ret.Next;
				if (m_First != null)
				{
					m_First.Prev = null;
				}
				return (ret.Key, ret.Value);
			}
			finally
			{
				m_Pool.Push(ret);
			}
		}

		public bool Contains(TKey key, QueueSearchMode mode)
		{
			if (m_First == null)
			{
				return false;
			}
			var cur = m_Last;
			switch (mode)
			{
				case QueueSearchMode.First:
					cur = m_First;
					break;
				case QueueSearchMode.Prev:
					cur = m_Prev;
					break;
			}
			var comp = m_Comparer.Compare(key, cur.Key);
			while (true)
			{
				if (comp == 0)
				{
					return true;
				}
				else if (comp > 0)
				{
					if (cur.Next == null)
					{
						return false;
					}
					comp = m_Comparer.Compare(key, cur.Next.Key);
					if (comp < 0)
					{
						return false;
					}
					else
					{
						cur = cur.Next;
					}
				}
				else
				{
					if (cur.Prev == null)
					{
						return false;
					}
					comp = m_Comparer.Compare(key, cur.Prev.Key);
					if (comp > 0)
					{
						return false;
					}
					else
					{
						cur = cur.Prev;
					}
				}
			}
		}

	}
}