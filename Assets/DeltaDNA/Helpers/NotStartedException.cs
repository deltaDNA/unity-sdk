using System;

namespace DeltaDNA
{
	public class NotStartedException : Exception
	{
		public NotStartedException() 
		{}
		
		public NotStartedException(string message)
			: base(message)
		{}
		
		public NotStartedException(string message, Exception inner)
			: base(message, inner)	
		{}
	}
}
