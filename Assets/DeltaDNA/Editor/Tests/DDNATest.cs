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

#if !UNITY_4
using NUnit.Framework;
using UnityEngine;

namespace DeltaDNA {

    public class DDNATest : AssertionHelper {

        private DDNA uut;
        private Configuration config;

        [SetUp]
        public void SetUp() {
            config = new Configuration() {
                environmentKeyDev = "envKeyDev",
                environmentKeyLive = "envKeyLive",
                environmentKey = 0,
                collectUrl = "https://collectUrl",
                engageUrl = "https://engageUrl"
            };

            uut = DDNA.Instance;
            uut.Settings.BackgroundEventUpload = false;
            uut.Awake();
        }

        [TearDown]
        public void TearDown() {
            uut.StopSDK();
            uut.ClearPersistentData();
            uut.OnDestroy();
        }

        [Test]
        public void CrossGameUserIdClearedOnNewPlayer() {
            uut.StartSDK(config, "id1");
            PlayerPrefs.SetString(DDNA.PF_KEY_CROSS_GAME_USER_ID, "id");

            uut.StartSDK(config, "id2");

            Expect(PlayerPrefs.HasKey(DDNA.PF_KEY_CROSS_GAME_USER_ID), Is.False);
        }

        [Test]
        public void ClearPersistentDataClearsKeys() {
            PlayerPrefs.SetInt(DDNA.PF_KEY_USER_ID, 1);
            PlayerPrefs.SetInt(DDNA.PF_KEY_FIRST_SESSION, 2);
            PlayerPrefs.SetInt(DDNA.PF_KEY_LAST_SESSION, 3);
            PlayerPrefs.SetInt(DDNA.PF_KEY_CROSS_GAME_USER_ID, 4);
            PlayerPrefs.SetInt(DDNA.PF_KEY_ADVERTISING_ID, 5);
            PlayerPrefs.SetInt(DDNA.PF_KEY_FORGET_ME, 6);
            PlayerPrefs.SetInt(DDNA.PF_KEY_FORGOTTEN, 7);
            PlayerPrefs.SetInt(DDNA.PF_KEY_ACTIONS_SALT, 8);

            uut.ClearPersistentData();

            Expect(PlayerPrefs.HasKey(DDNA.PF_KEY_USER_ID), Is.False);
            Expect(PlayerPrefs.HasKey(DDNA.PF_KEY_FIRST_SESSION), Is.False);
            Expect(PlayerPrefs.HasKey(DDNA.PF_KEY_LAST_SESSION), Is.False);
            Expect(PlayerPrefs.HasKey(DDNA.PF_KEY_CROSS_GAME_USER_ID), Is.False);
            Expect(PlayerPrefs.HasKey(DDNA.PF_KEY_ADVERTISING_ID), Is.False);
            Expect(PlayerPrefs.HasKey(DDNA.PF_KEY_FORGET_ME), Is.False);
            Expect(PlayerPrefs.HasKey(DDNA.PF_KEY_FORGOTTEN), Is.False);
            Expect(PlayerPrefs.HasKey(DDNA.PF_KEY_ACTIONS_SALT), Is.False);
        }
    }
}
#endif
