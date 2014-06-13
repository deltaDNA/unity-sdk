using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace DeltaDNA
{
	internal sealed class EngageArchive
	{
		private Hashtable table = new Hashtable();
		
		#if !UNITY_WEBPLAYER
		private string FILENAME = "ENGAGEMENTS";
		private string path;
		#endif 
	
		public EngageArchive(string path, bool reset=false)
		{
			if (!reset)
			{
				Load(path);			
			}
			
			#if !UNITY_WEBPLAYER
			this.path = path;
			#endif
		}
		
		public bool Contains(string decisionPoint)
		{
			Debug.Log ("Does Engage contain "+decisionPoint);
			return table.ContainsKey(decisionPoint);
		}
		
		public string this[string decisionPoint]
		{
			get
			{
				return table[decisionPoint] as string;
			}
			set
			{
				table[decisionPoint] = value;
			}
		}
		
		private void Load(string path)
		{
			#if !UNITY_WEBPLAYER
			try
			{
				string filename = Path.Combine(path, FILENAME);
				Debug.Log("Loading Engage from "+filename);
				if (File.Exists(filename))
				{
					using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
					{
						string key = null;
						string value = null;
						int read = 0;
						byte[] length = new byte[4];
						while (fs.Read(length, 0, length.Length) > 0)
						{
							Int32 valueLength = BitConverter.ToInt32(length, 0);
							byte[] valueField = new byte[valueLength];
							fs.Read(valueField, 0, valueField.Length);
							if (read % 2 == 0)
							{
								key = Encoding.UTF8.GetString(valueField);
							}
							else
							{
								value = Encoding.UTF8.GetString(valueField);
								table.Add(key, value);
							}
							read++;
						}
					}
				}
			}
			catch (Exception e)
			{
				Debug.LogWarning("Unable to load Engagement archive: "+e.Message);
			}
			#endif
		}
		
		public void Save()
		{
			#if !UNITY_WEBPLAYER
			try
			{
				if (!Directory.Exists(this.path)) 
				{
					Directory.CreateDirectory(this.path);
				}
				
				var bytes = new List<byte>();
				
				foreach (DictionaryEntry entry in table)
				{
					byte[] key = Encoding.UTF8.GetBytes(entry.Key as string);
					byte[] keyLength = BitConverter.GetBytes(key.Length);
					byte[] value = Encoding.UTF8.GetBytes(entry.Value as string);
					byte[] valueLength = BitConverter.GetBytes(value.Length);
					
					bytes.AddRange(keyLength);
					bytes.AddRange(key);
					bytes.AddRange(valueLength);
					bytes.AddRange(value);
				}
				
				byte[] byteArray = bytes.ToArray();
				
				string filename = Path.Combine(this.path, FILENAME);
			
				using (FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write))
				{
					fs.Write(byteArray, 0, byteArray.Length);
				}
			}
			catch (Exception e)
			{
				Debug.LogWarning("Unable to save Engagement archive: "+e.Message);
			}
			#endif
		}
	}
}
