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
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
#if UNITY_2017_1_OR_NEWER
using UnityEngine.Networking;
#endif

namespace DeltaDNA {

    internal class ImageMessageStore {

        private readonly string cache = Application.temporaryCachePath + "/deltadna/image_messages/";

        private readonly MonoBehaviour parent;

        internal ImageMessageStore(MonoBehaviour parent) {
            this.parent = parent;

            if (!Directory.CreateDirectory(cache).Exists) {
                Logger.LogWarning("Failed to create image message cache at " + cache);
            } else {
                Logger.LogInfo("Created image message cache at " + cache);
            }
        }

        #if UNITY_EDITOR
        internal ImageMessageStore() {
            parent = null;
        }
        #endif

        internal virtual bool Has(string url) {
            return Directory
                .GetFiles(cache)
                .Where(e => GetName(e).Equals(GetName(url)))
                .Count() > 0;
        }

        internal Texture2D Get(string url) {
            return Directory
                .GetFiles(cache)
                .Where(e => GetName(e).Equals(GetName(url)))
                .Select(e => {
                    var texture = new Texture2D(2, 2);
                    return texture.LoadImage(File.ReadAllBytes(e)) ? texture : null;
                })
                .FirstOrDefault();
        }

        internal IEnumerator Get(string url, Action<Texture2D> onSuccess, Action<string> onError) {
            var texture = Get(url);
            if (texture != null) {
                onSuccess(texture);
                yield break;
            } else {
                yield return Fetch(
                        url,
                        t => {
                            File.WriteAllBytes(cache + GetName(url), t.EncodeToPNG());
                            onSuccess(t);
                        },
                        onError);
            }
        }

        internal IEnumerator Prefetch(Action onSuccess, Action<string> onError, params string[] urls) {
            if (urls == null || urls.Length == 0) {
                onSuccess();
                yield break;
            }
            
            var downloaded = 0;
            string error = null;
            foreach (var url in urls) {
                var name = GetName(url);
                if (!File.Exists(cache + name)) {
                    parent.StartCoroutine(Fetch(
                        url,
                        t => {
                            File.WriteAllBytes(cache + name, t.EncodeToPNG());
                            downloaded++;
                        },
                        e => { error = e; }));
                } else {
                    downloaded++;
                }
            }
                
            while (downloaded < urls.Length) {
                if (error != null) {
                    onError(error);
                    yield break;
                } else {
                    yield return null;
                }
            }
            onSuccess();
        }

        internal void Clear() {
            Directory.Delete(cache);
        }

        private IEnumerator Fetch(string url, Action<Texture2D> onSuccess, Action<string> onError) {
            #if UNITY_2017_1_OR_NEWER
            using (var www = UnityWebRequestTexture.GetTexture(url)) {
                #if UNITY_2017_2_OR_NEWER
                yield return www.SendWebRequest();
                #else
                yield return www.Send();
                #endif

                if (www.isNetworkError || www.isHttpError) {
                    Logger.LogWarning("Failed to load resource " + url + " " + www.error);
                    onError(www.error);
                } else {
                    onSuccess(DownloadHandlerTexture.GetContent(www));
                }
            }
            #else
            WWW www = new WWW(url);
            yield return www;

            if (www.error == null) {
                onSuccess(www.texture);
            } else {
                Logger.LogWarning("Failed to load resource " + url + " " + www.error);
                onError(www.error);
            }
            #endif
        }

        private static string GetName(string url) {
            return new Uri(url).Segments.Last();
        }
    }
}
