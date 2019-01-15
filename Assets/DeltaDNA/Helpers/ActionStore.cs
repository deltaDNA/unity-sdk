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
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace DeltaDNA {

    using JSONObject = Dictionary<string, object>;

    internal class ActionStore {

        private static object LOCK = new object();

        private readonly string location;
        private byte[] salt = new byte[32];

        internal ActionStore(string location) {
            this.location = location;

            InitialiseSalt();

            lock (LOCK) {
                if (!Directory.Exists(location)) Directory.CreateDirectory(location);
            }
        }

        virtual internal JSONObject Get(EventTrigger trigger) {
            string contents = null;

            var file = location + trigger.GetCampaignId();
            lock (LOCK) {
                if (File.Exists(file)) {
                    contents = File.ReadAllText(file);
                } else {
                    return null;
                }
            }

            try {
                var json = MiniJSON.Json.Deserialize(contents) as JSONObject;
                var action = json["contents"] as JSONObject;

                var persistedHash = Convert.FromBase64String(json["hash"] as string);
                var hash = GeneratedSaltedHash(
                    Encoding.UTF8.GetBytes(MiniJSON.Json.Serialize(action)),
                    salt);
                if (!(persistedHash.Length == hash.Length
                    && !persistedHash.Where((t, i) => t != hash[i]).Any())) {
                    Logger.LogWarning("Mismatched hash for action " + contents);
                    File.Delete(file);
                    return null;
                }

                return action;
            } catch (Exception e) {
                Logger.LogWarning(string.Format(
                    "Unable to deserialise action {0} due to {1}",
                    contents,
                    e.Message));
                return null;
            }
        }

        virtual internal void Put(EventTrigger trigger, JSONObject action) {
            InitialiseSalt();

            string contents = null;
            try {
                var json = new JSONObject() {
                    { "contents", action },
                    { "hash", Convert.ToBase64String(GeneratedSaltedHash(
                        Encoding.UTF8.GetBytes(MiniJSON.Json.Serialize(action)),
                        salt)) }
                };

                contents = MiniJSON.Json.Serialize(json);
            } catch (Exception e) {
                Logger.LogWarning(string.Format(
                    "Unable to serialise action {0} due to {1}",
                    action,
                    e.Message));
                return;
            }

            lock (LOCK) {
                File.WriteAllText(location + trigger.GetCampaignId(), contents);
            }
        }

        virtual internal void Remove(EventTrigger trigger) {
            lock (LOCK) {
                var file = location + trigger.GetCampaignId();
                if (File.Exists(file)) File.Delete(file);
            }
        }

        virtual internal void Clear() {
            lock (LOCK) {
                foreach (var file in new DirectoryInfo(location).GetFiles()) {
                    file.Delete();
                }
            }

            salt = new byte[32];
            PlayerPrefs.DeleteKey(DDNA.PF_KEY_ACTIONS_SALT);
        }

        private void InitialiseSalt() {
            if (!PlayerPrefs.HasKey(DDNA.PF_KEY_ACTIONS_SALT)) {
                new RNGCryptoServiceProvider().GetNonZeroBytes(salt);
                PlayerPrefs.SetString(
                    DDNA.PF_KEY_ACTIONS_SALT,
                    Convert.ToBase64String(salt));
            } else if (salt.All(e => e == 0)) {
                salt = Convert.FromBase64String(
                    PlayerPrefs.GetString(DDNA.PF_KEY_ACTIONS_SALT));
            }
        }

        private static byte[] GeneratedSaltedHash(byte[] text, byte[] salt) {
            byte[] merged = new byte[text.Length + salt.Length];

            for (int i = 0; i < text.Length; i++) {
                merged[i] = text[i];
            }
            for (int i = 0; i < salt.Length; i++) {
                merged[text.Length + i] = salt[i];
            }

            return new SHA256Managed().ComputeHash(merged);
        }
    }
}
