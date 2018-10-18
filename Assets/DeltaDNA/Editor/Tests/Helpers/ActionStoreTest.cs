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
using NSubstitute;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace DeltaDNA {

    using JSONObject = Dictionary<string, object>;

    public class ActionStoreTest : AssertionHelper {

        private string dir;
        private ActionStore uut;

        [SetUp]
        public void SetUp() {
            dir = Settings.ACTIONS_STORAGE_PATH.Replace(
                "{persistent_path}", Application.persistentDataPath);
            uut = new ActionStore(dir);
        }

        [TearDown]
        public void TearDown() {
            if (Directory.Exists(dir)) Directory.Delete(dir, true);
        }

        [Test]
        public void SavingAndRetrievingAction() {
            var trigger = Substitute.For<EventTrigger>();
            trigger.GetCampaignId().Returns(1);
            var action = new JSONObject() {{ "a", 1 }};

            uut.Put(trigger, action);

            Expect(uut.Get(trigger), Is.EqualTo(action));
        }

        [Test]
        public void TamperedActionIsNotRetrievedAndDeleted() {
            var trigger = Substitute.For<EventTrigger>();
            trigger.GetCampaignId().Returns(1);
            var action = new JSONObject() { { "a", 1 } };

            uut.Put(trigger, action);
            var file = dir + 1;
            File.WriteAllText(
                file,
                File.ReadAllText(file).Replace("\"a\":1", "\"a\":2"));

            Expect(uut.Get(trigger), Is.Null);
            Expect(File.Exists(file), Is.False);
        }

        [Test]
        public void RemovingAction() {
            var trigger1 = Substitute.For<EventTrigger>();
            trigger1.GetCampaignId().Returns(1);
            var action1 = new JSONObject() { { "a", 1 } };
            var trigger2 = Substitute.For<EventTrigger>();
            trigger2.GetCampaignId().Returns(2);
            var action2 = new JSONObject() { { "b", 2 } };

            uut.Put(trigger1, action1);
            uut.Put(trigger2, action2);
            uut.Remove(trigger1);

            Expect(uut.Get(trigger1), Is.Null);
            Expect(uut.Get(trigger2), Is.EqualTo(action2));
        }

        [Test]
        public void ClearningActions() {
            var trigger1 = Substitute.For<EventTrigger>();
            trigger1.GetCampaignId().Returns(1);
            var action1 = new JSONObject() { { "a", 1 } };
            var trigger2 = Substitute.For<EventTrigger>();
            trigger2.GetCampaignId().Returns(2);
            var action2 = new JSONObject() { { "b", 2 } };

            uut.Put(trigger1, action1);
            uut.Put(trigger2, action2);
            uut.Clear();

            Expect(uut.Get(trigger1), Is.Null);
            Expect(uut.Get(trigger2), Is.Null);
            Expect(PlayerPrefs.HasKey(DDNA.PF_KEY_ACTIONS_SALT), Is.False);
        }

        [Test]
        public void PersistingActionAfterClearing() {
            var trigger = Substitute.For<EventTrigger>();
            trigger.GetCampaignId().Returns(1);
            var action = new JSONObject() { { "a", 1 } };

            uut.Clear();
            uut.Put(trigger, action);

            Expect(uut.Get(trigger), Is.EqualTo(action));
        }

        [Test]
        public void RetrievingActionAfterReinitialisation() {
            var trigger = Substitute.For<EventTrigger>();
            trigger.GetCampaignId().Returns(1);
            var action = new JSONObject() { { "a", 1 } };

            uut.Put(trigger, action);
            uut = new ActionStore(dir);

            Expect(uut.Get(trigger), Is.EqualTo(action));
        }
    }
}
#endif
