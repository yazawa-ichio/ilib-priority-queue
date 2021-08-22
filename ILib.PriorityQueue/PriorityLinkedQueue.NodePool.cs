namespace ILib.PriorityQueue
{

	public partial class PriorityLinkedQueue<TKey, TValue>
	{
		public sealed class NodePool
		{
			public static readonly NodePool Shared = new NodePool(1024);

			public int MaxPool { get; set; }

			Node m_PoolFirst;
			Node m_PoolLast;
			int m_Count;
			readonly object m_Lock = new object();

			public NodePool(int max)
			{
				MaxPool = max;
			}

			public void Clear()
			{
				lock (m_Lock)
				{
					m_Count = 0;
					m_PoolFirst = null;
					m_PoolLast = null;
				}
			}

			internal Node Pop(TKey key, in TValue value)
			{
				lock (m_Lock)
				{
					Node node;
					if (m_PoolFirst != null)
					{
						node = m_PoolFirst;
						if (m_PoolLast == m_PoolFirst)
						{
							m_PoolLast = null;
							m_PoolFirst = null;
						}
						else
						{
							m_PoolFirst = node.Next;
						}
						node.Next = null;
						m_Count--;
					}
					else
					{
						node = new Node();
					}
					node.Key = key;
					node.Value = value;
					return node;
				}
			}

			internal void Push(Node node)
			{
				lock (m_Lock)
				{
					if (MaxPool < m_Count)
					{
						return;
					}
					m_Count++;
					node.Value = default;
					node.Next = default;
					node.Prev = default;
					if (m_PoolLast != null)
					{
						m_PoolLast.Next = node;
						m_PoolLast = node;
					}
					if (m_PoolFirst == null)
					{
						m_PoolLast = m_PoolFirst = node;
					}
				}
			}

		}
	}
}