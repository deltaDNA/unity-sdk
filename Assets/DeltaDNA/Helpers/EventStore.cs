using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using UnityEngine;

namespace DeltaDNA
{
	public class EventStore : IEventStore, IDisposable
	{
		
		private static readonly string PF_KEY_IN_FILE = "DDSDK_EVENT_IN_FILE";
		private static readonly string PF_KEY_OUT_FILE = "DDSDK_EVENT_OUT_FILE";	
		private static readonly string FILE_A = "A";
		private static readonly string FILE_B = "B";
		private static readonly int FILE_BUFFER_SIZE = 4096;
		private static readonly long MAX_FILE_SIZE = 40 * 1024 * 1024;	// 40MB
	
		private FileStream infs = null;
		private FileStream outfs = null;		
		
		private bool initialised = false;
		private bool disposed = false;
		private bool debug = false;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="DeltaDNA.EventStore"/> class.
		/// </summary>
		/// <param name="path">Path to where we hold the events.</param>
		public EventStore(string path, bool reset=false, bool debug=false)
		{
			this.debug = debug;
			
			try
			{
				InitialiseFileStreams(path, reset);
				initialised = true;
			}
			catch (Exception e)
			{
				Log("Problem initialising Event Store: "+e.Message);
			}
		}
		
		/// <summary>
		/// Pushes a new object onto the event store.
		/// </summary>
		/// <param name="obj">The event to push</param>
		public bool Push(string obj)
		{
			if (initialised && infs.Length < MAX_FILE_SIZE) 
			{
				try
				{
					byte[] record = Encoding.UTF8.GetBytes(obj);
					byte[] length = BitConverter.GetBytes(record.Length);
					
					var bytes = new List<byte>();
					bytes.AddRange(length);
					bytes.AddRange(record);
					byte[] byteArray = bytes.ToArray();
					
					infs.Write(byteArray, 0, byteArray.Length);
					return true;			
				}
				catch (Exception e)
				{
					Log("Problem pushing event to Event Store: "+e.Message);
				}
			}
			return false;
		}
		
		/// <summary>
		/// Swap the in and out buffers.
		/// </summary>
		public bool Swap()
		{
			// only swap if out buffer is empty
			if (outfs.Length == 0)
			{
				// close off our write stream
				infs.Flush();
				// swap the file handles
				FileStream temp = infs;
				infs = outfs;
				outfs = temp;
				// reset write
				infs.SetLength(0);
				// reset read
				outfs.Seek(0, SeekOrigin.Begin);
				
				PlayerPrefs.SetString(PF_KEY_IN_FILE, infs.Name);
				PlayerPrefs.SetString(PF_KEY_OUT_FILE, outfs.Name);
				
				return true;
			}
			return false;
		}
		
		/// <summary>
		/// Read the contents of the out buffer as a list of string.  Can be
		/// called multiple times.
		/// </summary>
		public List<string> Read()	// get the next batch of events to send
		{
			List<string> results = new List<string>();
			try
			{
				byte[] lengthField = new byte[4];
				while (outfs.Read (lengthField, 0, lengthField.Length) > 0)
				{
					Int32 eventLength = BitConverter.ToInt32(lengthField, 0);
					byte[] recordField = new byte[eventLength];
					outfs.Read(recordField, 0, recordField.Length);
					string record = Encoding.UTF8.GetString(recordField);
					results.Add(record);
				}
				outfs.Seek(0, SeekOrigin.Begin);	// let us read it again next time
			}
			catch (Exception e)
			{
				Log("Problem reading events from Event Store: "+e.Message);
			}
		
			return results;
		}
		
		/// <summary>
		/// Clears the out buffer.
		/// </summary>
		public void Clear()
		{
			outfs.SetLength(0);
		}
		
		private void InitialiseFileStreams(string path, bool reset)
		{
			if (!Directory.Exists(path)) 
			{
				Directory.CreateDirectory(path);
			}
			
			string pathA = Path.Combine(path, FILE_A);
			string pathB = Path.Combine(path, FILE_B);

			string inPath = PlayerPrefs.GetString(PF_KEY_IN_FILE, pathA);
			string outPath = PlayerPrefs.GetString(PF_KEY_OUT_FILE, pathB);

			FileMode fileMode = reset ? FileMode.Create : FileMode.OpenOrCreate;

			if (File.Exists(inPath) && File.Exists(outPath) && !reset)
			{
				Log("Loaded existing Event Store in @ "+inPath+" out @ "+outPath);
			}
			else
			{
				Log("Creating new Event Store in @ "+path);
			}

			infs = new FileStream(inPath, fileMode, FileAccess.ReadWrite, FileShare.None, FILE_BUFFER_SIZE); 
			infs.Seek(0, SeekOrigin.End);
			outfs = new FileStream(outPath, fileMode, FileAccess.ReadWrite, FileShare.None, FILE_BUFFER_SIZE); 
			
			PlayerPrefs.SetString(PF_KEY_IN_FILE, infs.Name);
			PlayerPrefs.SetString(PF_KEY_OUT_FILE, outfs.Name);
			
//			
//						
//			// if both files exist, the the most recently modified
//			// is what we write too.
//			string inPath = pathA;
//			string outPath = pathB;
//			
//			// TODO: figure out which is the file we used as the in buffer last
//			// not sure how reliable the file write time will be across different
//			// platforms!
//			if (File.Exists(pathA) && File.Exists(pathB))
//			{
//				Log("Timestamps: "+File.GetLastWriteTime(pathA)+" "+File.GetLastWriteTime(pathB));
//				
//				if (!(File.GetLastWriteTime(pathA) > File.GetLastWriteTime(pathB)))
//				{
//					inPath = pathB;
//					outPath = pathA;
//				}
//				
//				Log("Loaded existing Event Store in @ "+inPath+" out @ "+outPath);
//			}
//			else
//			{
//				Log("Creating new Event Store @ "+path);
//			}
//			
			
		}
		
		// TODO: Do I need to put a dispose method on this class, and close the file handles explicity??
		
		private void Log(string message)
		{
			if (this.debug)
			{
				Debug.Log ("[DDSDK EventStore] "+message);
			}
		}
		
		~EventStore()
		{
			Dispose(false);
		}
		
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		
		protected virtual void Dispose(bool disposing)
		{
			Log("Disposing on EventStore...");
			try
			{
				if (!this.disposed)
				{
					if (disposing)
					{
						Log("Disposing filestreams");
						this.infs.Dispose();
						this.outfs.Dispose();
					}
				}
			}
			finally
			{
				this.disposed = true;
			}
		}
	}
}
