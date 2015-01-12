using System;
using System.Collections.Generic;

namespace DeltaDNA
{
	public class WebplayerEventStore : IEventStore
	{
		private Queue<string> inEvents = new Queue<string>();
		private Queue<string> outEvents = new Queue<string>();

		private bool disposed = false;

		private static object _lock = new object();

		public WebplayerEventStore()
		{

		}

		public bool Push(string obj)
		{
			lock (_lock)
			{
				inEvents.Enqueue(obj);
				return true;
			}
		}

		public bool Swap()
		{
			lock (_lock)
			{
				if (outEvents.Count == 0)
				{
					var temp = outEvents;
					outEvents = inEvents;
					inEvents = temp;
					return true;
				}
				return false;
			}
		}

		public List<string> Read()
		{
			lock (_lock)
			{
				List<string> results = new List<string>();
				foreach (string r in outEvents)
				{
					results.Add(r);
				}
				return results;
			}
		}

		public void ClearOut()
		{
			lock (_lock)
			{
				outEvents.Clear();
			}
		}
		
		public void ClearAll()
		{
			lock (_lock)
			{
				inEvents.Clear();
				outEvents.Clear();
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		~WebplayerEventStore()
		{
			Dispose(false);
		}

		protected virtual void Dispose(bool disposing)
		{
			if(!disposed)
			{
				if(disposing)
				{
					// Manual release of managed resources.
				}
				// Release unmanaged resources.
				disposed = true;
			}
		}
	}
}
