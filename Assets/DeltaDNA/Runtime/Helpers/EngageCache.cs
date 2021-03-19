//
// Copyright (c) 2018 deltaDNA Ltd. All rights reserved.
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace DeltaDNA {

    internal class EngageCache {

        private const string TIMES = "times";
        private static object LOCK = new object();

        private readonly Settings settings;

        private readonly string location =
            Application.temporaryCachePath + "/deltadna/engagements/";
        private readonly IDictionary<string, string> cache;
        private readonly IDictionary<string, DateTime> times;

        internal EngageCache(Settings settings){
            this.settings = settings;

            lock (LOCK) {
                // Handle the case where the disk is full or can't be written to
                // IOException: Disk full. Path /var/mobile/Containers/Data/Application/[HASH]/Library/Caches/deltadna
                //   at System.IO.Directory.CreateDirectoriesInternal (System.String path)
                //   at System.IO.Directory.CreateDirectoriesInternal (System.String path)
                //   at DeltaDNA.EngageCache..ctor (DeltaDNA.Settings settings)
                //   at DeltaDNA.DDNAImpl..ctor (DeltaDNA.DDNA ddna)
                //   at DeltaDNA.DDNA.Awake ()
                //   at UnityEngine.GameObject.AddComponent[T] ()
                //   at DeltaDNA.Singleton`1[T].get_Instance ()
                try {
                    CreateDirectory();
                } catch (Exception ex) {
                    Logger.LogWarning("Unable to create directory " + location + ": " + ex);
                    cache = new Dictionary<string, string>();
                    times = new Dictionary<string, DateTime>();
                    return;
                }

                // Handle the case where cached data is not in the expected format for some reason
                // IndexOutOfRangeException: Index was outside the bounds of the array.
                //   at DeltaDNA.EngageCache+<>c.<.ctor>b__6_3 (System.String e)
                //   at System.Func`2[T,TResult].Invoke (T arg)
                //   at System.Linq.Enumerable.ToDictionary[TSource,TKey,TElement] (System.Collections.Generic.IEnumerable`1[T] source, System.Func`2[T,TResult] keySelector, System.Func`2[T,TResult] elementSelector, System.Collections.Generic.IEqualityComparer`1[T] comparer)
                //   at System.Linq.Enumerable.ToDictionary[TSource,TKey,TElement] (System.Collections.Generic.IEnumerable`1[T] source, System.Func`2[T,TResult] keySelector, System.Func`2[T,TResult] elementSelector)
                //   at DeltaDNA.EngageCache..ctor (DeltaDNA.Settings settings)
                //   at DeltaDNA.DDNAImpl..ctor (DeltaDNA.DDNA ddna)
                //   at DeltaDNA.DDNA.Awake ()
                //   at UnityEngine.GameObject.AddComponent[T] ()
                //   at DeltaDNA.Singleton`1[T].get_Instance ()
                try {
                    cache = Directory
                        .GetFiles(location)
                        .ToDictionary(e => Path.GetFileName(e), e => File.ReadAllText(e));
                    if (File.Exists(location + TIMES)) {
                        times = File
                            .ReadAllLines(location + TIMES)
                            .ToDictionary(
                                e => e.Split(' ')[0],
                                e => new DateTime(Convert.ToInt64(e.Split(' ')[1])));
                    } else {
                        times = new Dictionary<string, DateTime>();
                    }
                } catch (Exception ex) {
                    Logger.LogError("Unable to deserialize cache: " + ex);
                    cache = new Dictionary<string, string>();
                    times = new Dictionary<string, DateTime>();
                }
            }
        }

        internal void Put(string decisionPoint, string flavour, string data) {
            if (string.IsNullOrEmpty(decisionPoint)) {
                Logger.LogWarning(
                    "Failed inserting " + data + " into cache due to null or empty decision point");
                return;
            }

            var key = Key(decisionPoint, flavour);

            lock (LOCK) {
                cache[key] = data;
                times[key] = DateTime.UtcNow;
            }
        }

        internal string Get(string decisionPoint, string flavour) {
            if (string.IsNullOrEmpty(decisionPoint)) {
                Logger.LogWarning(
                    "Failed retrieving from cache due to null or empty decision point");
                return null;
            } else if (settings.EngageCacheExpirySeconds == 0) {
                return null;
            }

            var key = Key(decisionPoint, flavour);

            lock (LOCK) {
                if (cache.ContainsKey(key)){
                    var age = TimeSpan.Zero;
                    if (times.ContainsKey(key)){
                       age = DateTime.UtcNow - times[key];
                    } else {
                        times[key] = DateTime.UtcNow;
                    }
                    if (age.TotalSeconds < settings.EngageCacheExpirySeconds) {
                        return cache[key];
                    }

                    cache.Remove(key);
                    times.Remove(key);
                }
            }

            return null;
        }

        internal void Save() {
            lock (LOCK) {
                // Handle the case where the disk is full or can't be written to
                // IOException: Disk full. Path /storage/emulated/0/Android/data/[BUNDLE_ID]/cache/deltadna
                //   at System.IO.Directory.CreateDirectoriesInternal (System.String path)
                //   at System.IO.Directory.CreateDirectoriesInternal (System.String path)
                //   at DeltaDNA.EngageCache.Save ()
                //   at DeltaDNA.DDNAImpl.OnApplicationPause (System.Boolean pauseStatus)
                try {
                    CreateDirectory();
                } catch (Exception ex) {
                    Logger.LogError("Unable to create directory " + location + ": " + ex);
                    return;
                }

                // Handle the case where the disk is full or can't be written to
                // IOException: Disk full. Path /var/mobile/Containers/Data/Application/[HASH]/Library/Caches/deltadna/engagements/times
                //   at System.IO.FileStream.FlushBuffer ()
                //   at System.IO.StreamWriter.Dispose (System.Boolean disposing)
                //   at System.IO.TextWriter.Dispose ()
                //   at System.IO.File.WriteAllLines (System.String path, System.String[] contents)
                //   at DeltaDNA.EngageCache.Save ()
                //   at DeltaDNA.DDNAImpl.OnApplicationPause (System.Boolean pauseStatus)
                //
                // IOException: Disk full. Path /storage/emulated/0/Android/data/[BUNDLE_ID]/cache/deltadna/engagements/times
                //   at System.IO.FileStream.FlushBuffer ()
                //   at System.IO.FileStream.Dispose (System.Boolean disposing)
                //   at System.IO.Stream.Close ()
                //   at System.IO.StreamWriter.Dispose (System.Boolean disposing)
                //   at System.IO.TextWriter.Dispose ()
                //   at System.IO.File.WriteAllText (System.String path, System.String contents, System.Text.Encoding encoding)
                //   at DeltaDNA.EngageCache.Save ()
                //   at DeltaDNA.DDNAImpl.OnApplicationPause (System.Boolean pauseStatus)
                try {
                    foreach (var item in cache) {
                        File.WriteAllText(location + item.Key, item.Value);
                    }

                    File.WriteAllLines(
                        location + TIMES,
                        times.Select(e => e.Key + ' ' + e.Value.Ticks).ToArray());
                } catch (Exception ex) {
                    Logger.LogError("Unable to write cache: " + ex);
                }
            }
        }

        internal void Clear() {
            lock (LOCK) {
                cache.Clear();
                times.Clear();
                Save();
            }
        }

        private void CreateDirectory() {
            if (!Directory.Exists(location)) Directory.CreateDirectory(location);
        }

        private static string Key(string decisionPoint, string flavour) {
            return decisionPoint + '@' + flavour;
        }
    }
}
