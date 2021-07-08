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
                CreateDirectory();

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
                CreateDirectory();

                foreach (var item in cache) {
                    File.WriteAllText(location + item.Key, item.Value);
                }

                File.WriteAllLines(
                    location + TIMES,
                    times.Select(e => e.Key + ' ' + e.Value.Ticks).ToArray());
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
