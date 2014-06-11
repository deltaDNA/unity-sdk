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
		
		public EventParams AddParam(string key, object value)
		{
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

