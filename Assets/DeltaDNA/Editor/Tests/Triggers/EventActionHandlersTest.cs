//
// Copyright (c) 2018 deltaDNA Ltd. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed, in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

#if !UNITY_4
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DeltaDNA {

    using JSONObject = Dictionary<string, object>;

    public sealed class EventActionHandlersTest : AssertionHelper {

        private const string IMAGE_MESSAGE = "{\"image\":{" +
            "\"url\":\"url\"," +
            "\"height\":0," +
            "\"width\":0," +
            "\"spritemap\":{\"background\":{}}," +
            "\"layout\":{" +
                "\"landscape\":{}," +
                "\"portrait\":{}}" +
            "}}";

        private ActionStore store;

        [SetUp]
        public void SetUp() {
            store = Substitute.For<ActionStore>(Settings.ACTIONS_STORAGE_PATH
                .Replace("{persistent_path}", Application.persistentDataPath));
        }

        [Test]
        public void GameParametersHandlerOnlyHandlesGameParameterActions() {
            var cbk = Substitute.For<Action<JSONObject>>();
            var uut = new GameParametersHandler(cbk);
            var t1 = Substitute.For<EventTrigger>();
            t1.GetAction().Returns("imageMessage");

            Expect(uut.Type(), Is.EqualTo("gameParameters"));
            Expect(uut.Handle(t1, store), Is.False);
            cbk.DidNotReceive().Invoke(Arg.Any<JSONObject>());

            var t2 = Substitute.For<EventTrigger>();
            t2.GetAction().Returns("gameParameters");
            t2.GetResponse().Returns("{\"parameters\":{\"a\":1}}".Json());

            Expect(uut.Handle(t2, store));
            cbk.Received().Invoke(Arg.Is<JSONObject>(e => e.Json() == "{\"a\":1}"));

            var t3 = Substitute.For<EventTrigger>();
            t3.GetAction().Returns("gameParameters");
            t3.GetResponse().Returns("{}".Json());

            Expect(uut.Handle(t3, store));
            cbk.Received().Invoke(Arg.Is<JSONObject>(e => e.Json() == "{}"));
        }

        [Test]
        public void GameParametersHandlerUsesPersistentActionAndRemovesIt() {
            var cbk = Substitute.For<Action<JSONObject>>();
            var uut = new GameParametersHandler(cbk);
            var trigger = Substitute.For<EventTrigger>();
            trigger.GetAction().Returns("gameParameters");
            trigger.GetResponse().Returns("{\"parameters\":{\"a\":1}}".Json());
            var persisted = "{\"b\":2}".Json();
            store.Get(trigger).Returns(persisted);

            Expect(uut.Handle(trigger, store));
            store.Received().Remove(Arg.Is(trigger));
            cbk.Received().Invoke(Arg.Is<JSONObject>(e => e == persisted));
        }

        [Test]
        public void ImageMessageHandlerOnlyHandlesImageMessageActions() {
            var ddna = Substitute.For<DDNA>();
            var cbk = Substitute.For<Action<ImageMessage>>();
            var uut = new ImageMessageHandler(ddna, cbk);
            var t1 = Substitute.For<EventTrigger>();
            t1.GetAction().Returns("gameParameters");

            Expect(uut.Type(), Is.EqualTo("imageMessage"));
            Expect(uut.Handle(t1, store), Is.False);
            cbk.DidNotReceive().Invoke(Arg.Any<ImageMessage>());

            var t2 = Substitute.For<EventTrigger>();
            t2.GetAction().Returns("imageMessage");
            t2.GetResponse().Returns(IMAGE_MESSAGE.Json());
            var ims = Substitute.For<ImageMessageStore>();
            ddna.GetImageMessageStore().Returns(ims);
            ims.Has("url").Returns(true);

            Expect(uut.Handle(t2, store));
            cbk.Received().Invoke(Arg.Any<ImageMessage>());

            var t3 = Substitute.For<EventTrigger>();
            t3.GetAction().Returns("imageMessage");
            t3.GetResponse().Returns(IMAGE_MESSAGE.Json());
            ims.Has("url").Returns(false);

            Expect(uut.Handle(t3, store), Is.False);
            cbk.Received(1).Invoke(Arg.Any<ImageMessage>());
        }

        [Test]
        public void ImageMessageHandlerUsesPersistentActionAndRemovesIt() {
            var ddna = Substitute.For<DDNA>();
            var cbk = Substitute.For<Action<ImageMessage>>();
            var uut = new ImageMessageHandler(ddna, cbk);
            var trigger = Substitute.For<EventTrigger>();
            trigger.GetAction().Returns("imageMessage");
            trigger.GetResponse().Returns(IMAGE_MESSAGE.Json());
            var persisted = "{\"b\":2}".Json();
            store.Get(trigger).Returns(persisted);
            var ims = Substitute.For<ImageMessageStore>();
            ddna.GetImageMessageStore().Returns(ims);
            ims.Has("url").Returns(true);

            Expect(uut.Handle(trigger, store));
            store.Received().Remove(Arg.Is(trigger));
            cbk.Received().Invoke(Arg.Is<ImageMessage>(e => e.Parameters == persisted));
        }
    }
}
#endif
