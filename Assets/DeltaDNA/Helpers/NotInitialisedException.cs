using System;

namespace DeltaDNA
{
	public class NotInitialisedException : Exception
	{
		public NotInitialisedException() 
		{}
		
		public NotInitialisedException(string message)
			: base(message)
		{}
		
		public NotInitialisedException(string message, Exception inner)
			: base(message, inner)	
		{}
	}
}
