using System;
using System.Collections;
using System.Collections.Generic;

namespace DeltaDNA
{
	static class Utils
	{
		public static Dictionary<K,V> HashtableToDictionary<K,V> (Hashtable table)
		{
			Dictionary<K,V> dict = new Dictionary<K,V>();
			foreach(DictionaryEntry kvp in table)
				dict.Add((K)kvp.Key, (V)kvp.Value);
			return dict;
		}

		public static Dictionary<K,V> HashtableToDictionary<K,V> (Dictionary<K,V> dictionary)
		{
			return dictionary;
		}
	}
}
