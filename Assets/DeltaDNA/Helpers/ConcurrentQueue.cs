using System;
using System.Collections.Generic;

namespace DeltaDNA {

	internal class ConcurrentQueue<T> {
		
		private readonly object queueLock = new object();	
		private Queue<T> queue = new Queue<T>();
		
		public int Count 
		{
			get 
			{
				lock(queueLock)
				{
					return queue.Count;
				}
			}
		}
		
		public T Peek()
		{
			lock(queueLock)
			{
				return queue.Peek();
			}
		}
		
		public void Enqueue(T obj)
		{
			lock(queueLock) 
			{
				queue.Enqueue(obj);
			}
		}
		
		public T Dequeue()
		{
			lock(queueLock)
			{
				return queue.Dequeue();
			}
		}
		
		public void Clear()
		{
			lock(queueLock)
			{
				queue.Clear();
			}
		}
	}
}
