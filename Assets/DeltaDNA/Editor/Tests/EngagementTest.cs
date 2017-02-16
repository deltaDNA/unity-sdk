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
using System;
using System.Collections.Generic;

namespace DeltaDNA {

    public class EngagementTest {

        [Test]
        public void CreatingAnEngagementWithANullDecisionPointThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => { new Engagement(null); });
        }

        [Test]
        public void CreatingAnEngagementWithAnEmptyDecisionPointThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => { new Engagement(""); });
        }

        [Test]
        public void CreateAValidEngagement()
        {
            var engagement = new Engagement("testDecisionPoint");

            Assert.AreEqual("testDecisionPoint", engagement.DecisionPoint);
            Assert.AreEqual("engagement", engagement.Flavour);
            CollectionAssert.AreEquivalent(new Dictionary<string, object>() {
                    { "decisionPoint", "testDecisionPoint" },
                    { "flavour", "engagement" },
                    { "parameters", new Dictionary<string, object>() }
                }, engagement.AsDictionary());
        }

        [Test]
        public void CreateAValidEngagementWithExtraParameters()
        {
            var engagement = new Engagement("testDecisionPoint");
            engagement.AddParam("A", 1);
            engagement.AddParam("B", "two");
            engagement.AddParam("C", true);

            Assert.AreEqual("testDecisionPoint", engagement.DecisionPoint);
            Assert.AreEqual("engagement", engagement.Flavour);
            CollectionAssert.AreEquivalent(new Dictionary<string, object>() {
                    { "decisionPoint", "testDecisionPoint" },
                    { "flavour", "engagement" },
                    { "parameters", new Dictionary<string, object>() {
                            { "A", 1 },
                            { "B", "two" },
                            { "C", true } 
                        } 
                    }
                }, engagement.AsDictionary());
        }

        [Test]
        public void InvalidRawJSON()
        {
            var engagement = new Engagement("testDecisionPoint");

            engagement.Raw = "Not valid JSON";

            Assert.AreEqual("Not valid JSON", engagement.Raw);
            Assert.IsNotNull(engagement.JSON);
            CollectionAssert.IsEmpty(engagement.JSON);
        }

        [Test]
        public void ValidRawJSON()
        {
            var engagement = new Engagement("testDecisionPoint");

            engagement.Raw = "{\"x\": 1,\"y\": \"Hello\",\"z\": [{\"1\": \"a\"}]}";

            Assert.IsNotNull(engagement.JSON);
            CollectionAssert.IsNotEmpty(engagement.JSON);
            CollectionAssert.AreEquivalent(new Dictionary<string, object>() {
                { "x", 1 },
                { "y", "Hello" },
                { "z", new List<object>() {
                        new Dictionary<string, object>() {
                            { "1", "a" }
                        }
                    }
                }
            }, engagement.JSON);
        }
    }
}
#endif
