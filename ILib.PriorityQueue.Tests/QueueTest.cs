using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace ILib.PriorityQueue.Tests
{
	[TestClass]
	public class QueueTest
	{
		[TestMethod]
		public void LinkedQueuePriorityTest()
		{
			var linkedQueue = new PriorityLinkedQueue<double, int>();
			var sortedList = new SortedList<double, int>();

			var random = new Random();
			var duplicateCheck = new HashSet<double>();
			// 指定モードで繰り返す
			foreach (QueueSearchMode mode in Enum.GetValues(typeof(QueueSearchMode)))
			{
				sortedList.Clear();
				duplicateCheck.Clear();

				for (int i = 0; i < 1000; i++)
				{
					var key = random.NextDouble();
					if (duplicateCheck.Add(key))
					{
						linkedQueue.Enqueue(key, i, mode);
						sortedList.Add(key, i);
					}
				}

				for (int i = 0; i < duplicateCheck.Count; i++)
				{
					Assert.IsTrue(linkedQueue.Contains(sortedList.Keys[i], mode));

					var (key, value) = linkedQueue.Dequeue();

					Assert.IsFalse(linkedQueue.Contains(sortedList.Keys[i], mode));

					Assert.AreEqual(sortedList.Keys[i], key);
					Assert.AreEqual(sortedList.Values[i], value);
					Console.WriteLine(key);
				}

				Assert.IsTrue(linkedQueue.IsEmpty);
			}

		}

		[TestMethod]
		public void LinkedQueueDuplicateTest()
		{
			{
				// 重複は認めない
				var queue = new PriorityLinkedQueue<int, int>();
				queue.Enqueue(1, 10);
				queue.Enqueue(1, 20);
				Assert.AreEqual(10, queue.Dequeue().value);
				Assert.IsTrue(queue.IsEmpty);
			}
			{
				// 重複を許可
				var queue = new PriorityLinkedQueue<int, int>();
				queue.SetAllowDuplicateMode();
				queue.Enqueue(1, 10);
				queue.Enqueue(1, 20, QueueSearchMode.First);
				queue.Enqueue(1, 30, QueueSearchMode.First);
				Assert.AreEqual(10, queue.Dequeue().value);
				Assert.AreEqual(20, queue.Dequeue().value);
				Assert.AreEqual(30, queue.Dequeue().value);
				Assert.IsTrue(queue.IsEmpty);
			}
			{
				// 指定条件で上書き
				var queue = new PriorityLinkedQueue<int, int>()
				{
					OnDuplicate = (key, prev, cur) =>
					{
						if (prev < cur)
						{
							return DuplicateOperation.Override;
						}
						return DuplicateOperation.Ignore;
					}
				};
				queue.Enqueue(1, 10);
				queue.Enqueue(1, 30, QueueSearchMode.First);
				queue.Enqueue(1, 20, QueueSearchMode.First);
				Assert.AreEqual(30, queue.Dequeue().value);
				Assert.IsTrue(queue.IsEmpty);
			}
		}
	}

}