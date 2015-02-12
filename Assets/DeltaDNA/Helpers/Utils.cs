using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

#if NETFX_CORE
using UnityEngine.Windows;
using Windows.Storage;
using System.Threading.Tasks;
using Windows.Storage.Streams;
#endif

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

        public static void CreateDirectory(string path)
        {
            #if NETFX_CORE
            // Unity's WP8.1 version from Windows.Storage doesn't do it recursively
            path = path.Replace('/', '\\');
            string parent = path.Substring(0, path.LastIndexOf('\\'));
            if (!Directory.Exists(parent)) {
                CreateDirectory(parent);
            }
            UnityEngine.Windows.Directory.CreateDirectory(path);
            #else
            System.IO.Directory.CreateDirectory(path);
            #endif
        }

        public static Stream CreateStream(string path)
        {
            #if NETFX_CORE
            Logger.LogDebug("Creating async file stream");
            path = FixPath(path);
            var thread = CreateAsync(path);
            thread.Wait();

            if (thread.IsCompleted)
                return thread.Result;

            throw thread.Exception;

            #elif UNITY_WEBPLAYER
            Logger.LogDebug("Creating memory based stream");
            return new MemoryStream();
            #else
            Logger.LogDebug("Creating file based stream");
            return new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            #endif
        }

        public static Stream OpenStream(string path)
        {
            #if NETFX_CORE
            Logger.LogDebug("Opening async file stream");
            path = FixPath(path);
            var thread = OpenAsync(path);
            thread.Wait();

            if (thread.IsCompleted)
                return thread.Result;

            throw thread.Exception;
            #elif UNITY_WEBPLAYER
            Logger.LogDebug("Opening memory based stream");
            return new MemoryStream();
            #else
            Logger.LogDebug("Opening file based stream");
            return new FileStream(path, FileMode.Open, FileAccess.Read);
            #endif
        }


        #if NETFX_CORE

        private static async Task<Stream> CreateAsync(string path)
        {
            var dirName = Path.GetDirectoryName(path);
            var filename = Path.GetFileName(path);

            var dir = await StorageFolder.GetFolderFromPathAsync(dirName);
            var file = await dir.CreateFileAsync(filename, CreationCollisionOption.OpenIfExists);
            return await file.OpenStreamForWriteAsync();
        }

        private static async Task<Stream> OpenAsync(string path)
        {
            var file = await StorageFile.GetFileFromPathAsync(path);
            var stream = await file.OpenStreamForReadAsync();
            return stream;
        }

        private static string FixPath(string path)
        {
            return path.Replace('/', '\\');
        }

        #endif
	}
}
