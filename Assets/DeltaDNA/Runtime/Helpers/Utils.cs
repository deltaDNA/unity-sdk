//
// Copyright (c) 2016 deltaDNA Ltd. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

#if NETFX_CORE
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
#endif

namespace DeltaDNA
{
    public static class Utils
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

        public static byte[] ComputeMD5Hash(byte[] buffer)
        {
            #if UNITY_WINRT

            return UnityEngine.Windows.Crypto.ComputeMD5Hash(buffer);

            #else

            // Use MD5CryptoServiceProvider instead of MD5 class with iOS stripping level set to micro mscorlib.
            var md5Hasher = new System.Security.Cryptography.MD5CryptoServiceProvider();
            return md5Hasher.ComputeHash(buffer);

            #endif
        }

        public static bool IsDirectoryWritable(string path) {
            try {
            if (!DirectoryExists(path)) {
                    CreateDirectory(path);
                }
                string file = Path.Combine(path, Path.GetRandomFileName());
                using (FileStream fs = File.Create(file, 1)) {}
                File.Delete(file);
                return true;
            } catch (Exception) {
                return false;
            }
        }

        public static bool FileExists(string path)
        {
            if (DDNA.Instance.Settings.UseEventStore) {
                #if UNITY_WINRT
                return UnityEngine.Windows.File.Exists(path);
                #else
                return System.IO.File.Exists(path);
                #endif
            }
            return false; // since we won't actually make a file
        }

        public static bool DirectoryExists(string path)
        {
            if (DDNA.Instance.Settings.UseEventStore) {
                #if UNITY_WINRT
                return UnityEngine.Windows.Directory.Exists(path);
                #else
                return System.IO.Directory.Exists(path);
                #endif
            }
            return false; // since we won't actually make a file
        }

        public static void CreateDirectory(string path)
        {
            if (DDNA.Instance.Settings.UseEventStore) {
                #if UNITY_WINRT
                // Unity's WP8.1 version from Windows.Storage doesn't do it recursively
                path = path.Replace('/', '\\');
                string parent = path.Substring(0, path.LastIndexOf('\\'));
                if (!UnityEngine.Windows.Directory.Exists(parent)) {
                    CreateDirectory(parent);
                }
                UnityEngine.Windows.Directory.CreateDirectory(path);
                #else
                System.IO.Directory.CreateDirectory(path);
                #endif
            }
        }

        public static Stream CreateStream(string path)
        {
            if (DDNA.Instance.Settings.UseEventStore) {
                #if NETFX_CORE
                Logger.LogDebug("Creating async file stream");
                path = FixPath(path);
                var thread = CreateAsync(path);
                thread.Wait();

                if (thread.IsCompleted)
                    return thread.Result;

                throw thread.Exception;
                #else
                Logger.LogDebug("Creating file based stream");
                return new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                #endif
            } else {
                Logger.LogDebug("Creating memory based stream");
                return new MemoryStream();
            }
        }

        public static Stream OpenStream(string path)
        {
            if (DDNA.Instance.Settings.UseEventStore) {
                #if NETFX_CORE
                Logger.LogDebug("Opening async file stream");
                path = FixPath(path);
                var thread = OpenAsync(path);
                thread.Wait();

                if (thread.IsCompleted)
                    return thread.Result;

                throw thread.Exception;
                #else
                Logger.LogDebug("Opening file based stream");
                return new FileStream(path, FileMode.Open, FileAccess.Read);
                #endif
            } else {
                Logger.LogDebug("Opening memory based stream");
                return new MemoryStream();
            }
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
        
        public static string FixURL(string url) 
        {
            if (!url.ToLower().StartsWith("http://") && !url.ToLower().StartsWith("https://")) {
                return "https://" + url;
            } else if (url.ToLower().StartsWith("http://")) {
                Logger.LogWarning("Changing " + url + " to use HTTPS");
                return "https://" + url.Substring("http://".Length);
            } else {
                return url;
            }
        }

        #region Extenson Methods

        public static T GetOrDefault<T, K>(this IDictionary<K, object> dict, K key, T def) {
            return (dict != null && dict.ContainsKey(key) && dict[key] is T) ? (T) dict[key] : def;
        }

        #endregion
    }
}
