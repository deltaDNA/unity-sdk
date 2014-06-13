using System;
using System.Collections.Generic;

namespace DeltaDNA
{
	public interface IEventStore : IDisposable
	{
		bool Push(string obj);
		bool Swap();
		List<string> Read();
		void Clear();
	}
}

