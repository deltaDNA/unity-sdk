using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace DeltaDNA
{
	/// <summary>
	/// The Engage archive holds previously requested engage responses. The responses
	/// can be saved to disk.
	/// </summary>
	internal sealed class EngageArchive
	{
		private Hashtable _table = new Hashtable();
		private object _lock = new object();

		private static readonly string FILENAME = "ENGAGEMENTS";
		private string _path = "";

		/// <summary>
		/// Creates a new EnagageArchive, loading any previously saved Engagements from
		/// a file at <param name="path">.
		/// </summary>
		public EngageArchive(string path)
		{
			if (!String.IsNullOrEmpty(path)) {
				Load(path);
				this._path = path;
			}
		}

		/// <summary>
		/// Returns true if the archive contains a previous response for the
		/// decision point.
		/// </summary>
		public bool Contains(string decisionPoint)
		{
			Logger.LogDebug("Does Engage contain "+decisionPoint);
			return _table.ContainsKey(decisionPoint);
		}

		/// <summary>
		/// Gets or sets the <see cref="DeltaDNA.EngageArchive"/> with the specified decisionPoint.
		/// </summary>
		public string this[string decisionPoint]
		{
			get
			{
				return _table[decisionPoint] as string;
			}
			set
			{
				lock(_lock)
				{
					_table[decisionPoint] = value;
				}
			}
		}

		/// <summary>
		/// Loads an existing archive from disk.
		/// </summary>
		private void Load(string path)
		{
			lock(_lock)
			{
				try
				{
					string filename = Path.Combine(path, FILENAME);
					Logger.LogDebug("Loading Engage from "+filename);
					if (Utils.FileExists(filename))
					{
						using (Stream fs = Utils.OpenStream(filename))
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
									key = Encoding.UTF8.GetString(valueField, 0, valueField.Length);
								}
								else
								{
									value = Encoding.UTF8.GetString(valueField, 0, valueField.Length);
									_table.Add(key, value);
								}
								read++;
							}
						}
					}
				}
				catch (Exception e)
				{
					Logger.LogWarning("Unable to load Engagement archive: "+e.Message);
				}
			}
		}

		/// <summary>
		/// Save the archive to disk.
		/// </summary>
		public void Save()
		{
			lock(_lock)
			{
				try
				{
					if (!Utils.DirectoryExists(this._path))
					{
						Utils.CreateDirectory(this._path);
					}

					var bytes = new List<byte>();

					foreach (DictionaryEntry entry in _table)
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

					string filename = Path.Combine(this._path, FILENAME);

					using (Stream fs = Utils.CreateStream(filename))
					{
                        fs.SetLength(0);
                        fs.Seek(0, SeekOrigin.Begin);
						fs.Write(byteArray, 0, byteArray.Length);
					}
				}
				catch (Exception e)
				{
					Logger.LogWarning("Unable to save Engagement archive: "+e.Message);
				}
			}
		}

		/// <summary>
		/// Clears the archive.
		/// </summary>
		public void Clear()
		{
			lock(_lock)
			{
				_table.Clear();
			}
		}
	}
}
