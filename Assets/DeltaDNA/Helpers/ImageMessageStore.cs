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
        internal ImageMessageStore() : this(null) {}
        #endif


        internal virtual bool Has(string url) {
            return File.Exists(cache + GetName(url));
        }

        internal Texture2D Get(string url) {
            var texture = new Texture2D(2, 2, TextureFormat.ARGB32, false) {name = "ImageMessageStore"};
            return texture.LoadImage(File.ReadAllBytes(cache + GetName(url))) ? texture : null;
        }

        internal IEnumerator Get(string url, Action<Texture2D> onSuccess, Action<string> onError){
            if (Has(url)){
                var texture = Get(url);
                if (texture != null){
                    onSuccess(texture);
                    yield break;
                }
            }
            else{
                yield return Fetch(
                    url,
                    fileTempPath => {
                        var filePath = cache + GetName(url);
                        File.Move(fileTempPath, filePath);
                        var tex = new Texture2D(2, 2){name = "ImageMessageStore"};
                        onSuccess(tex.LoadImage(File.ReadAllBytes(filePath)) ? tex : null);
                    },
                    onError);
            }
        }

        internal IEnumerator Prefetch(Action onSuccess, Action<string> onError, params string[] urls) {
            if (urls == null || urls.Length == 0) {
                onSuccess();
                yield break;
            }

            if (IsFull()){
                Logger.LogInfo("Not attempting image pre-fetch - cache is already full");
                onSuccess();
                yield break;
            }
            var downloaded = 0;
            string error = null;
            var downloading = 0;
            var userMaxConcurrent = DDNA.Instance.Settings.MaxConcurrentImageCacheFetches;
            var maxConcurrent = userMaxConcurrent > 0 ? userMaxConcurrent : 5; 
            foreach (var url in urls) {
                var name = GetName(url);
                if (IsFull()){
                    Logger.LogWarning("Did not attempt to download image message - Image Message cache is full");
                    downloaded++;
                } else if (!File.Exists(cache + name)){
                    yield return new WaitUntil(() => downloading <= maxConcurrent);
                    downloading++;
                    parent.StartCoroutine(Fetch(
                        url,
                        t => {
                          
                            var filePath = cache + GetName(url);
                            File.Move(t,  filePath );
                            downloaded++;
                            downloading--;
                        },
                        e => { error = e;
                            downloading--;
                        }));
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
            if (Directory.Exists(cache)) Directory.Delete(cache, true);
        }

        private IEnumerator Fetch(string url, Action<string> onSuccess, Action<string> onError) {
            bool success = true;
            var filePathTmp = Path.ChangeExtension(cache + GetName(url), ".tmp");
            #if UNITY_2017_1_OR_NEWER
            using (var downloadHandler = new DownloadHandlerFile(filePathTmp))
            using (var www = new UnityWebRequest(url)) {
                downloadHandler.removeFileOnAbort = true;
                www.downloadHandler = downloadHandler;
                yield return www.SendWebRequest();
                if (www.isNetworkError || www.isHttpError) {
                    Logger.LogWarning("Failed to load resource " + url + " " + www.error);
                    onError(www.error);
                } else {
                    onSuccess(filePathTmp);
                }
            }
            #else
            WWW www = new WWW(url);
            yield return www;

            if (www.error == null){
                File.WriteAllBytes(filePathTmp, www.texture.EncodeToPNG());
                onSuccess(filePathTmp);
            } else {
                Logger.LogWarning("Failed to load resource " + url + " " + www.error);
                onError(www.error);
            }
            #endif
        }

        private static string GetName(string url) {
            return new Uri(url).Segments.Last();
        }

        private bool IsFull(){
            string[] cachedFiles =  Directory.GetFiles(cache);
            //Convert Limit to bytes
            long limit = DDNA.Instance.Settings.ImageCacheLimitMB * 1048576;
            long currentSize = 0;
            foreach (string name in cachedFiles)
            {
                FileInfo info = new FileInfo(name);
                currentSize += info.Length;
                if (currentSize >= limit) return true;
            }
            return false;
        }
    }
}
