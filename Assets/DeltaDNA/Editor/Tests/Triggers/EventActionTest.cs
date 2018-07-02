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

#if !UNITY_4_5 && !UNITY_4_6 && !UNITY_4_7
using NSubstitute;
using NUnit.Framework;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DeltaDNA {

    public sealed class EventActionTest {

        private DDNABase ddna;

        [SetUp]
        public void PreTest() {
            ddna = Substitute.For<DDNABase>(null);
        }

        [Test]
        public void TriggersAreEvaluatedInOrder() {
            var e = new GameEvent("event");
            var t1 = Substitute.For<EventTrigger>();
            var t2 = Substitute.For<EventTrigger>();
            var t3 = Substitute.For<EventTrigger>();

            new EventAction(
                e,
                new ReadOnlyCollection<EventTrigger>(
                    new List<EventTrigger>() { t2, t3, t1 }))
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
            t.Evaluate(e).Returns(true);

            new EventAction(
                e,
                new ReadOnlyCollection<EventTrigger>(new List<EventTrigger>() { t }))
                .Add(h1)
                .Add(h2)
                .Add(h3)
                .Run();

            Received.InOrder(() => {
                h1.Received().Handle(t);
                h2.Received().Handle(t);
                h3.Received().Handle(t);
            });
        }

        [Test]
        public void HandlersAreRunUntilOneHandlesTheAction() {
            var e = new GameEvent("event");
            var t = Substitute.For<EventTrigger>(ddna, 0, "{\"eventName\":\"name\"}".Json());
            var h1 = Substitute.For<EventActionHandler>();
            var h2 = Substitute.For<EventActionHandler>();
            var h3 = Substitute.For<EventActionHandler>();
            t.Evaluate(e).Returns(true);
            h1.Handle(t).Returns(false);
            h2.Handle(t).Returns(true);

            new EventAction(
                e,
                new ReadOnlyCollection<EventTrigger>(new List<EventTrigger>() { t }))
                .Add(h1)
                .Add(h2)
                .Add(h3)
                .Run();

            h3.DidNotReceive().Handle(Arg.Any<EventTrigger>());
        }
    }
}
#endif
