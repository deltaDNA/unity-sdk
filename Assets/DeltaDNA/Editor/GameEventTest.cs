//
// Copyright (c) 2016 deltaDNA Ltd. All rights reserved.
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
using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using System.Collections.Generic;

namespace DeltaDNA {

    public class GameEventTest {

        [Test]
        public void CreateWithoutParameters()
        {
            var gameEvent = new GameEvent("myEvent");

            Assert.IsNotNull(gameEvent);
            CollectionAssert.AreEquivalent(new Dictionary<string, object>() {
                { "eventName", "myEvent" },
                { "eventParams", new Dictionary<string, object>() {}}
            }, gameEvent.AsDictionary());
        }

        [Test]
        public void CreateWithParameters()
        {
            var gameEvent = new GameEvent("myEvent");
            gameEvent.AddParam("level", 5);
            gameEvent.AddParam("ending", "Kaboom!");

            CollectionAssert.AreEquivalent(new Dictionary<string, object>() {
                { "eventName", "myEvent" },
                { "eventParams", new Dictionary<string, object>() {
                        { "level", 5 },
                        { "ending", "Kaboom!" }
                    }
                }
            }, gameEvent.AsDictionary());
        }

        [Test]
        public void CreateWithNestedParameters()
        {
            var gameEvent = new GameEvent("myEvent");
            gameEvent.AddParam("level1", new Dictionary<string, object>() {
                { "level2", new Dictionary<string, object>() {
                        { "yo!", "greeting" }
                    }
                }
            });

            CollectionAssert.AreEquivalent(new Dictionary<string, object>() {
                { "eventName", "myEvent" },
                { "eventParams", new Dictionary<string, object>() {
                        { "level1", new Dictionary<string, object>() {
                                { "level2", new Dictionary<string, object>() {
                                        { "yo!", "greeting" }
                                    }
                                }
                            }
                        }
                    }
                }
            }, gameEvent.AsDictionary());
        }

        [Test]
        public void DoesThrowArgumentExceptionIfKeyIsNull()
        {
            Assert.Throws<System.ArgumentNullException>(() => {
                var gameEvent = new GameEvent("myEvent");
                gameEvent.AddParam(null, null);
            });
        }
    }
}
#endif

