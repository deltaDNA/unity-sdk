using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
#if NETFX_CORE
using UnityEngine.Windows;
using Windows.Storage;
using System.Threading.Tasks;
using Windows.Storage.Streams;
#endif

namespace DeltaDNA
{

    public class EventStore : IDisposable
    {
        private static readonly string PF_KEY_IN_FILE = "DDSDK_EVENT_IN_FILE";
        private static readonly string PF_KEY_OUT_FILE = "DDSDK_EVENT_OUT_FILE";

        private static readonly string FILE_A = "A";
        private static readonly string FILE_B = "B";

        private static readonly long MAX_FILE_SIZE = 40 * 1024 * 1024;

        private bool _initialised = false;
        private bool _disposed = false;
        private Stream _infs = null;
        private Stream _outfs = null;

        private static object _lock = new object();

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
                    PlayerPrefs.SetString(PF_KEY_IN_FILE, outFile);
                    PlayerPrefs.SetString(PF_KEY_OUT_FILE, inFile);

                    return true;
                }

                return false;
            }
        }

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
                }
                return events;
            }
        }

        public void ClearOut()
        {
            lock (_lock)
            {
                if (_initialised) ClearStream(_outfs);
            }
        }

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
            Logger.LogDebug("Disposing EventStore " + this);
            try
            {
                if (!_disposed)
                {
                    if (disposing)
                    {
                        Logger.LogDebug("Disposing filestreams");
                        _infs.Dispose();
                        _outfs.Dispose();
                    }
                }
            }
            finally
            {
                _disposed = true;
            }
        }

        private bool InitialiseFileStreams(string dir)
        {
        	#if !UNITY_WEBPLAYER
            if (!Directory.Exists(dir))
            {
                Logger.LogDebug("Directory not found, creating");
                Utils.CreateDirectory(dir);
            }
			#endif

            try
            {
                string inFilename = PlayerPrefs.GetString(PF_KEY_IN_FILE, FILE_A);
                string outFilename = PlayerPrefs.GetString(PF_KEY_OUT_FILE, FILE_B);

                string inPath = Path.Combine(dir, inFilename);
                string outPath = Path.Combine(dir, outFilename);

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