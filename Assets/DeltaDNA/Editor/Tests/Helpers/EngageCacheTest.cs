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

#if !UNITY_4_5 && !UNITY_4_6 && !UNITY_4_7
using NUnit.Framework;
using System.IO;
using UnityEngine;

namespace DeltaDNA {
    public class EngageCacheTest : AssertionHelper {

        private Settings settings;

        private EngageCache uut;

        [SetUp]
        public void SetUp() {
            settings = new Settings();
            uut = new EngageCache(settings);
        }

        [TearDown]
        public void TearDown() {
            Directory.Delete(
                Application.temporaryCachePath + "/deltadna/engagements/",
                true);
        }

        [Test]
        public void InsertsAndRetrievesEngagement() {
            uut.Put("dp", "flavour", "data");

            Expect(uut.Get("dp", "flavour"), Is.EqualTo("data"));
        }

        [Test]
        public void ReturnsNullWhenEngagementNotFound() {
            Expect(uut.Get("dp", "flavour"), Is.Null);
        }

        [Test]
        public void ReturnsNullWhenExpiryIsZero() {
            uut.Put("dp", "flavour", "data");
            settings.EngageCacheExpirySeconds = 0;

            Expect(uut.Get("dp", "flavour"), Is.Null);
        }

        [Test]
        public void ReturnsNullWhenCachedEngagementExpires() {
            uut.Put("dp", "flavour", "data");
            settings.EngageCacheExpirySeconds = 1;

            System.Threading.Thread.Sleep(1000);

            Expect(uut.Get("dp", "flavour"), Is.Null);
        }

        [Test]
        public void SavePersistsCache() {
            uut.Put("dp1", "flavour", "data1");
            uut.Put("dp2", "flavour", "data2");
            uut.Save();

            uut = new EngageCache(settings);
            Expect(uut.Get("dp1", "flavour"), Is.EqualTo("data1"));
            Expect(uut.Get("dp2", "flavour"), Is.EqualTo("data2"));
        }

        [Test]
        public void ClearsCache() {
            uut.Put("dp", "flavour", "data");
            uut.Clear();

            Expect(uut.Get("dp", "flavour"), Is.Null);
        }
    }
}
#endif
