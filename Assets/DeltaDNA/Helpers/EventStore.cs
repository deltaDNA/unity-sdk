using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DeltaDNA
{

	/// <summary>
	/// The Event Store queues game events until they are ready to be sent to Collect.  It is a 
	/// double buffer queue, events are written to one queue whilst being read from another.  
	/// Mostly files are used to hold the events as UTF8 json strings.  For platforms that don't 
	/// support filesystem, such as Webplayer in memory files are used instead.
	/// </summary>
    public class EventStore : IDisposable
    {
        private static readonly string PF_KEY_IN_FILE = "DDSDK_EVENT_IN_FILE";
        private static readonly string PF_KEY_OUT_FILE = "DDSDK_EVENT_OUT_FILE";

        private static readonly string FILE_A = "A";
        private static readonly string FILE_B = "B";

        private static readonly long MAX_FILE_SIZE = 4 * 1024 * 1024;	// 4MB

        private bool _initialised = false;
        private bool _disposed = false;
        private Stream _infs = null;
        private Stream _outfs = null;

        private static object _lock = new object();

		/// <summary>
		/// Initializes a new instance of the <see cref="DeltaDNA.EventStore"/> class.
		/// </summary>
		/// <param name="dir">Full path on the filesystem where to create the files.</param>
        public EventStore(string dir)
        {
            Logger.LogInfo("Creating Event Store");

            if (InitialiseFileStreams(dir))
            {
                _initialised = true;
            }
            else
            {
                Logger.LogError("Failed to initialise event store in " + dir);
            }
        }

		/// <summary>
		/// Add a new event to the in buffer.
		/// </summary>
        public bool Push(string obj)
        {
            lock (_lock)
            {
                if (!_initialised)
                {
                    Logger.LogError("Event Store not initialised");
                    return false;
                }

                if (_infs.Length < MAX_FILE_SIZE)
                {
                    PushEvent(obj, _infs);
                    return true;
                }
                else
                {
                    Logger.LogWarning("Event Store full");
                    return false;
                }
            }
        }

		/// <summary>
		/// Swap the in and out buffers over.
		/// </summary>
        public bool Swap()
        {
            lock (_lock)
            {
                // Only swap if the out buffer is empty
                // -- So what really happens if the out buffer is full on start up??
                if (_initialised && _outfs.Length == 0)
                {
                    SwapStreams(ref _infs, ref _outfs);

                    // Swap the filenames
                    string inFile = PlayerPrefs.GetString(PF_KEY_IN_FILE);
					string outFile = PlayerPrefs.GetString(PF_KEY_OUT_FILE);
					
					if (String.IsNullOrEmpty(inFile)) {
                    	Logger.LogError("Event Store corruption, PlayerPrefs in file key is missing");
                    }
                    else if (String.IsNullOrEmpty(outFile)) {
						Logger.LogError("Event Store corruption, PlayerPrefs out file key is missing");
					}
					else {
						PlayerPrefs.SetString(PF_KEY_IN_FILE, outFile);
						PlayerPrefs.SetString(PF_KEY_OUT_FILE, inFile);
					}
					
                    return true;
                }

                return false;
            }
        }

		/// <summary>
		/// Reads events from the out buffer.
		/// </summary>
        public List<string> Read()
        {
            lock (_lock)
            {
                List<string> events = new List<string>();
                try
                {
                    if (_initialised) ReadEvents(_outfs, events);
                }
                catch (Exception e)
                {
                    Logger.LogError("Problem reading events: " + e.Message);
					ClearStream(_outfs);
					return null;
				}
				return events;
            }
        }

		/// <summary>
		/// Clears the out buffer.
		/// </summary>
        public void ClearOut()
        {
            lock (_lock)
            {
                if (_initialised) ClearStream(_outfs);
            }
        }

		/// <summary>
		/// Clears both in and out buffers.
		/// </summary>
        public void ClearAll()
        {
            lock (_lock)
            {
                if (_initialised)
                {
                    ClearStream(_infs);
                    ClearStream(_outfs);
                }
            }
        }

		/// <summary>
		/// Flushs the buffers to disk.
		/// </summary>
        public void FlushBuffers()
        {
            lock (_lock)
            {
                if (_initialised)
                {
                    _infs.Flush();
                    _outfs.Flush();
                }
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
        	lock (_lock)
        	{
	            try
	            {
	                if (!_disposed)
	                {
	                    if (disposing)
	                    {
	                        if (_infs != null) 
	                        	_infs.Dispose();
	                        	
	                        if (_outfs != null)
	                        	_outfs.Dispose();
	                    }
	                }
	            }
	            catch (Exception e) 
	            {
	            	Logger.LogError("Failed to dispose EventStore cleanly. "+e.Message);
	            }
	            finally
	            {
	                _disposed = true;
	            }
	       	}
        }

        private bool InitialiseFileStreams(string dir)
        {
            try
            {
            	string inPath = null;
            	string outPath = null;
				string inFilename = PlayerPrefs.GetString(PF_KEY_IN_FILE, FILE_A);
				string outFilename = PlayerPrefs.GetString(PF_KEY_OUT_FILE, FILE_B);
				
				if (!String.IsNullOrEmpty(dir)) 
            	{
					if (!Utils.DirectoryExists(dir))
					{
						Logger.LogDebug("Directory not found, creating "+dir);
						Utils.CreateDirectory(dir);
					}
					
					inPath = Path.Combine(dir, inFilename);
					outPath = Path.Combine(dir, outFilename);
				}
				
				// NB as seperate call after creation resets the files
                _infs = Utils.CreateStream(inPath);
                _infs.Seek(0, SeekOrigin.End);
                _outfs = Utils.CreateStream(outPath);
                _outfs.Seek(0, SeekOrigin.Begin);

                PlayerPrefs.SetString(PF_KEY_IN_FILE, inFilename);
                PlayerPrefs.SetString(PF_KEY_OUT_FILE, outFilename);

                return true;
            }
            catch (Exception e)
            {
                Logger.LogError("Failed to initialise file stream: " + e.Message);
            }
            return false;
        }

        public static void PushEvent(string obj, Stream stream)
        {
            byte[] record = Encoding.UTF8.GetBytes(obj);
            byte[] length = BitConverter.GetBytes(record.Length);

            var bytes = new List<byte>();
            bytes.AddRange(length);
            bytes.AddRange(record);
            byte[] byteArray = bytes.ToArray();

            stream.Write(byteArray, 0, byteArray.Length);
        }

        public static void ReadEvents(Stream stream, IList<string> events)
        {
            byte[] lengthField = new byte[4];
            while (stream.Read(lengthField, 0, lengthField.Length) > 0)
            {
                Int32 eventLength = BitConverter.ToInt32(lengthField, 0);
                byte[] recordField = new byte[eventLength];
                stream.Read(recordField, 0, recordField.Length);
                string record = Encoding.UTF8.GetString(recordField, 0, recordField.Length);
                events.Add(record);
            }
            stream.Seek(0, SeekOrigin.Begin);	// let us read it again next time
        }

        public static void SwapStreams(ref Stream sin, ref Stream sout)
        {
            // Close off our write stream
            sin.Flush();
            // Swap the file handles
            Stream tmp = sin;
            sin = sout;
            sout = tmp;
            // Clear write stream
            sin.Seek(0, SeekOrigin.Begin);
            sin.SetLength(0);
            // Prepare read stream
            sout.Seek(0, SeekOrigin.Begin);
        }

        public static void ClearStream(Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);
            stream.SetLength(0);
        }

    }

}