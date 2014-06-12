using System;
using System.Collections.Generic;

namespace DeltaDNA
{
	public class EventBuilder
	{
		private Dictionary<string, object> dict = new Dictionary<string, object>();
	
		public EventBuilder()
		{
		
		}
		
		/// <summary>
		/// Adds a game parameter to the event params object.  If the value is null
		/// it is ignored.
		/// </summary>
		/// <param name="key">Game parameter name.</param>
		/// <param name="value">The value of the game parameter.</param>
		public EventBuilder AddParam(string key, object value)
		{
			if (value == null) return this;
			
			if (value.GetType() == typeof(ProductBuilder))
			{
				ProductBuilder product = value as ProductBuilder;
				value = product.ToDictionary();
			}
			else if (value.GetType() == typeof(EventBuilder))
			{
					EventBuilder eventParams = value as EventBuilder;
					value = eventParams.ToDictionary();
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

