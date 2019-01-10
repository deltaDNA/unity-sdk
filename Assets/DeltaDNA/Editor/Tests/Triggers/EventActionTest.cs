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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace DeltaDNA {

    public sealed class EventActionTest {

        private DDNABase ddna;
        private ActionStore store;

        [SetUp]
        public void PreTest() {
            ddna = Substitute.For<DDNABase>(null);
            store = Substitute.For<ActionStore>(Settings.ACTIONS_STORAGE_PATH
                .Replace("{persistent_path}", Application.persistentDataPath));
        }

        [Test]
        public void TriggersAreEvaluatedInOrder() {
            var e = new GameEvent("event");
            var t1 = Substitute.For<EventTrigger>();
            var t2 = Substitute.For<EventTrigger>();
            var t3 = Substitute.For<EventTrigger>();
            var settings = Substitute.For<Settings>();
            

            new EventAction(
                e,
                new ReadOnlyCollection<EventTrigger>(
                    new List<EventTrigger>() { t2, t3, t1 }),
                store, settings)
                .Run();

            Received.InOrder(() => {
                t2.Received().Evaluate(e);
                t3.Received().Evaluate(e);
                t1.Received().Evaluate(e);
            });
        }

        [Test]
        public void HandlersAreRunInOrderOfAddition() {
            var e = new GameEvent("event");
            var t = Substitute.For<EventTrigger>(ddna, 0, "{\"eventName\":\"name\"}".Json());
            var h1 = Substitute.For<EventActionHandler>();
            var h2 = Substitute.For<EventActionHandler>();
            var h3 = Substitute.For<EventActionHandler>();
            var settings = Substitute.For<Settings>();
            t.Evaluate(e).Returns(true);

            new EventAction(
                e,
                new ReadOnlyCollection<EventTrigger>(new List<EventTrigger>() { t }),
                store, settings)
                .Add(h1)
                .Add(h2)
                .Add(h3)
                .Run();

            Received.InOrder(() => {
                h1.Received().Handle(t, store);
                h2.Received().Handle(t, store);
                h3.Received().Handle(t, store);
            });
        }

        [Test]
        public void HandlersAreRunUntilOneHandlesTheAction() {
            var e = new GameEvent("event");
            var t = Substitute.For<EventTrigger>(ddna, 0, "{\"eventName\":\"name\"}".Json());
            var h1 = Substitute.For<EventActionHandler>();
            var h2 = Substitute.For<EventActionHandler>();
            var h3 = Substitute.For<EventActionHandler>();
            var settings = Substitute.For<Settings>();
            t.Evaluate(e).Returns(true);
            h1.Handle(t, store).Returns(false);
            h2.Handle(t, store).Returns(true);

            new EventAction(
                e,
                new ReadOnlyCollection<EventTrigger>(new List<EventTrigger>() { t }), store, settings)
                .Add(h1)
                .Add(h2)
                .Add(h3)
                .Run();

            h3.DidNotReceive().Handle(Arg.Any<EventTrigger>(), store);
        }

        [Test] public void EachActionIsHandledIfMultipleActionsForEventTriggerEnabled() {
            var e = new GameEvent("event");
            var t = Substitute.For<EventTrigger>(ddna, 0, "{\"eventName\":\"name\"}".Json());
            var t2 = Substitute.For<EventTrigger>(ddna, 0, "{\"eventName\":\"name\"}".Json());
            var h1 = Substitute.For<EventActionHandler>();
            var h2 = Substitute.For<EventActionHandler>();
            var h3 = Substitute.For<EventActionHandler>();
            var settings = Substitute.For<Settings>();
            settings.MultipleActionsForEventTriggerEnabled = true;
            t.Evaluate(e).Returns(true);
            t2.Evaluate(e).Returns(true);
            h1.Handle(t, store).ReturnsForAnyArgs(false);
            h2.Handle(t, store).ReturnsForAnyArgs(true);
            new EventAction(
                    e,
                    new ReadOnlyCollection<EventTrigger>(new List<EventTrigger>() { t, t2 }), store, settings)
                .Add(h1)
                .Add(h2)
                .Add(h3)
                .Run();

            h1.Received(2).Handle(Arg.Any<EventTrigger>(), store);
            h2.Received(2).Handle(Arg.Any<EventTrigger>(), store);
            h3.DidNotReceive().Handle(Arg.Any<EventTrigger>(), store);       
        }
        
        [Test] public void ImageMessagesAreHandledOnlyOnceIfIsHandledIfMultipleActionsForEventTriggerEnabled() {
            var e = new GameEvent("event");
            var t = Substitute.For<EventTrigger>(ddna, 0, "{\"eventName\":\"name\"}".Json());
            var t2 = Substitute.For<EventTrigger>(ddna, 0, "{\"eventName\":\"name\"}".Json());
            var h1 = Substitute.For<EventActionHandler>();
            var h2 = Substitute.For<EventActionHandler>();
            var h3 = Substitute.For<EventActionHandler>();
            var settings = Substitute.For<Settings>();
            settings.MultipleActionsForEventTriggerEnabled = true;
            t.GetAction().Returns("imageMessage");
            t2.GetAction().Returns("imageMessage");
            t.Evaluate(e).Returns(true);
            t2.Evaluate(e).Returns(true);
            h1.Handle(t, store).ReturnsForAnyArgs(false);
            h2.Handle(t, store).ReturnsForAnyArgs(true);
            new EventAction(
                    e,
                    new ReadOnlyCollection<EventTrigger>(new List<EventTrigger>() { t, t2 }), store, settings)
                .Add(h1)
                .Add(h2)
                .Add(h3)
                .Run();

            h1.Received(1).Handle(t, store);
            h2.Received(1).Handle(t, store);
            h3.DidNotReceive().Handle(Arg.Any<EventTrigger>(), store);       
            h1.DidNotReceive().Handle(t2, store);       
            h2.DidNotReceive().Handle(t2, store);       
        }
        
        [Test] public void ImageMessagesDoNotBlockSubsequentParameterActionsIfIsHandledIfMultipleActionsForEventTriggerEnabled() {
            var e = new GameEvent("event");
            var t = Substitute.For<EventTrigger>(ddna, 0, "{\"eventName\":\"name\"}".Json());
            var t2 = Substitute.For<EventTrigger>(ddna, 0, "{\"eventName\":\"name\"}".Json());
            var h1 = Substitute.For<EventActionHandler>();
            var h2 = Substitute.For<EventActionHandler>();
            var h3 = Substitute.For<EventActionHandler>();
            var settings = Substitute.For<Settings>();
            settings.MultipleActionsForEventTriggerEnabled = true;
            t.GetAction().Returns("imageMessage");
            t2.GetAction().Returns("notImageAction");
            t.Evaluate(e).Returns(true);
            t2.Evaluate(e).Returns(true);
            h1.Handle(t, store).ReturnsForAnyArgs(false);
            h2.Handle(t, store).ReturnsForAnyArgs(true);
            new EventAction(
                e,
                new ReadOnlyCollection<EventTrigger>(new List<EventTrigger>() { t }),
                store, settings)
                .Add(h1)
                .Add(h2)
                .Add(h3)
                .Run();

            h3.DidNotReceive().Handle(Arg.Any<EventTrigger>(), store);
        }
    }
}
#endif
