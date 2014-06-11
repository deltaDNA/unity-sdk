using System;
using System.Collections.Generic;

namespace DeltaDNA
{
	public class EventParams
	{
		private Dictionary<string, object> dict = new Dictionary<string, object>();
	
		public EventParams()
		{
		
		}
		
		/// <summary>
		/// Adds a game parameter to the event params object.  If the value is null
		/// it is ignored.
		/// </summary>
		/// <param name="key">Game parameter name.</param>
		/// <param name="value">The value of the game parameter.</param>
		public EventParams AddParam(string key, object value)
		{
			if (value == null) return this;
			
			if (value.GetType() == typeof(ProductParams))
			{
				ProductParams product = value as ProductParams;
				value = product.ToDictionary();
			}
		
			this.dict.Add(key, value);
			return this;
		}
		
		public Dictionary<string, object> ToDictionary()
		{
			return dict;
		}
	}
}

