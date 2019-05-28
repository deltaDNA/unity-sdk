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


using Random = System.Random;
#if !UNITY_4
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DeltaDNA.MiniJSON;
using UnityEngine;

namespace DeltaDNA {

    using JSONObject = Dictionary<string, object>;

    public class EventTriggerTest : AssertionHelper {

        private DDNABase ddna;

        [SetUp]
        public void PreTest() {
            ddna = Substitute.For<DDNABase>(null);
        }

        [Test]
        public void AttributesAreExtractedAtConstruction() {
            var uut = new EventTrigger(
                ddna,
                0,
                "{\"eventName\":\"name\",\"response\":{\"a\":1}}".Json(),
                new ExecutionCountManager());

            Expect(uut.GetEventName(), Is.EqualTo("name"));
            Expect(uut.GetResponse(), Is.EqualTo("{\"a\":1}".Json()));
        }

        [Test]
        public void MissingAttributesAreReplacedWithSaneDefaults() {
            var uut = new EventTrigger(
                ddna,
                0,
                "{}".Json(),
                new ExecutionCountManager());

            Expect(uut.GetEventName(), Is.EqualTo(""));
            Expect(uut.GetResponse(), Is.EqualTo("{}".Json()));
        }

        [Test]
        public void ActionReturnsGameParametersByDefault() {
            var uut = new EventTrigger(
                ddna,
                0,
                "{\"response\":{\"image\":{}}}".Json(),
                new ExecutionCountManager());

            Expect(uut.GetAction(), Is.EqualTo("gameParameters"));
        }

        [Test]
        public void ActionReturnsImageMessageWhenPresent() {
            var uut = new EventTrigger(
                ddna,
                0,
                "{\"response\":{\"image\":{\"a\":1}}}".Json(),
                new ExecutionCountManager());

            Expect(uut.GetAction(), Is.EqualTo("imageMessage"));
        }

        [Test]
        public void TriggersAreOrderedBasedOnPriorities() {
            var top = new EventTrigger(ddna, 0, "{\"priority\":2}".Json(), new ExecutionCountManager());
            var middle = new EventTrigger(ddna, 0, "{\"priority\":1}".Json(), new ExecutionCountManager());
            var bottom = new EventTrigger(ddna, 0, "{\"priority\":0}".Json(), new ExecutionCountManager());
            var expected = new List<EventTrigger>() { top, middle, bottom };

            var actual = new List<EventTrigger>() { middle, bottom, top };
            actual.Sort();

            Expect(actual.SequenceEqual(expected));
        }

        [Test]
        public void TriggersAreOrderedBasedOnIndicesIfPrioritiesAreSame() {
            var top = new EventTrigger(ddna, 0, "{\"priority\":0}".Json(), new ExecutionCountManager());
            var middle = new EventTrigger(ddna, 1, "{\"priority\":0}".Json(), new ExecutionCountManager());
            var bottom = new EventTrigger(ddna, 2, "{\"priority\":0}".Json(), new ExecutionCountManager());
            var expected = new List<EventTrigger>() { top, middle, bottom };

            var actual = new List<EventTrigger>() { middle, bottom, top };
            actual.Sort();

            Expect(actual.SequenceEqual(expected));
        }

        [Test]
        public void IsEvaluatedIndefinitelyByDefault() {
            var uut = new EventTrigger(ddna, 0, "{\"eventName\":\"a\"}".Json(), new ExecutionCountManager());
            var evnt = new GameEvent("a");

            for (int i = 0; i < 10; i++) { Expect(uut.Evaluate(evnt)); }
        }

        [Test]
        public void IsEvaluatedUpToTheLimitNumberOfTimes() {
            var limit = 3;
            var uut = new EventTrigger(
                ddna,
                0,
                ("{\"eventName\":\"a\",\"limit\":" + limit + "}").Json(), new ExecutionCountManager());
            var evnt = new GameEvent("a");

            for (var i = 0; i <= limit; i++) {
                if (i < limit) {
                    Expect(uut.Evaluate(evnt));
                } else {
                    Expect(uut.Evaluate(evnt), Is.False);
                }
            }
        }

        [Test]
        public void SendsConversionEventWhenEvaluated(){
            var jsonObject = new JSONObject();
            var responseJson = new JSONObject();
            var eventParamJson = new JSONObject();
            var eventName = "a";
            var priority = 1L;
            var campaignId = 2L;
            var vaiantId = 3L;
            var campaignName = "CAMPAIGN NAME HERE";
            var variantName = "VARIANT NAME HERE";

            eventParamJson.Add("responseEngagementName", campaignName);
            eventParamJson.Add("responseVariantName", variantName);

            responseJson.Add("eventParams", eventParamJson);

            jsonObject.Add("eventName", eventName);
            jsonObject.Add("priority", priority);
            jsonObject.Add("campaignID", campaignId);
            jsonObject.Add("variantID", vaiantId);
            jsonObject.Add("response", responseJson);


            new EventTrigger(ddna,0,jsonObject, new ExecutionCountManager())
                .Evaluate(new GameEvent(eventName));

            ddna.Received().RecordEvent(Arg.Is<GameEvent>(e =>
                e.Name == "ddnaEventTriggeredAction" &&
                e.parameters.GetParam("ddnaEventTriggeredCampaignPriority") as long? == priority &&
                e.parameters.GetParam("ddnaEventTriggeredCampaignID") as long? == campaignId &&
                e.parameters.GetParam("ddnaEventTriggeredVariantID") as long? == vaiantId &&
                e.parameters.GetParam("ddnaEventTriggeredActionType") as string == "gameParameters" &&
                e.parameters.GetParam("ddnaEventTriggeredCampaignName") as string == campaignName &&
                e.parameters.GetParam("ddnaEventTriggeredVariantName") as string == variantName &&
                e.parameters.GetParam("ddnaEventTriggeredSessionCount") as int? == 1
            ));
        }

        [Test]
        public void SendsConversionEventWhenEvaluatedWithoutOptionalParameters(){
            var jsonObject = new JSONObject();
            var responseJson = new JSONObject();
            var eventParamJson = new JSONObject();
            var eventName = "a";
            var priority = 1L;
            var campaignId = 2L;
            var vaiantId = 3L;

            responseJson.Add("eventParams", eventParamJson);

            jsonObject.Add("eventName", eventName);
            jsonObject.Add("priority", priority);
            jsonObject.Add("campaignID", campaignId);
            jsonObject.Add("variantID", vaiantId);
            jsonObject.Add("response", responseJson);


            new EventTrigger(ddna,0,jsonObject, new ExecutionCountManager())
                .Evaluate(new GameEvent(eventName));

            ddna.Received().RecordEvent(Arg.Is<GameEvent>(e =>
                e.Name == "ddnaEventTriggeredAction" &&
                e.parameters.GetParam("ddnaEventTriggeredCampaignPriority") as long? == priority &&
                e.parameters.GetParam("ddnaEventTriggeredCampaignID") as long? == campaignId &&
                e.parameters.GetParam("ddnaEventTriggeredVariantID") as long? == vaiantId &&
                e.parameters.GetParam("ddnaEventTriggeredActionType") as string == "gameParameters" &&
                e.parameters.GetParam("ddnaEventTriggeredSessionCount") as int? == 1 &&
                ! e.parameters.AsDictionary().ContainsKey("ddnaEventTriggeredVariantName") &&
                ! e.parameters.AsDictionary().ContainsKey("ddnaEventTriggeredCampaignName")
            ));
        }

        [Test]
        public void DoesNotSendAConversionEventWhenEvaluationFails() {
            new EventTrigger(ddna, 0, "{\"eventName\":\"a\"}".Json(), new ExecutionCountManager())
                .Evaluate(new GameEvent("b"));

            ddna.DidNotReceive();
        }

        [Test]
        public void EvaluationFailsAgainstEventWithDifferentName() {
            Expect(new EventTrigger(ddna, 0, "{\"eventName\":\"a\"}".Json(), new ExecutionCountManager())
                .Evaluate(new GameEvent("b")),
                Is.False);
        }

        [Test]
        public void EmptyConditionEvaluatesSuccessfully() {
            Expect(Cond((Evnt())));
        }

        [Test]
        public void EvaluationFailsOnInvalidOperator() {
            Expect(Cond(Evnt(), true.B(), true.B(), "equalz to".O()),
                Is.False);
        }

        #if !UNITY_5_4 // Unhandled Exception: Mono.CSharp.InternalErrorException: VerifyArgumentsCompat didn't find any problem with rejected candidate MethodBuilder

        [Test]
        public void EvaluationOfLogicalOperators() {
            Expect(Cond(Evnt("a"), true.B(), true.B(), "and".O()));
            Expect(Cond(Evnt("a"), true.B(), false.B(), "and".O()),
                Is.False);

            Expect(Cond(Evnt("a", new object[] { "a", true }), "a".P(), true.B(), "and".O()));
            Expect(Cond(Evnt("a", new object[] { "a", true }), "a".P(), false.B(), "and".O()),
                Is.False);

            Expect(Cond(Evnt("a"), false.B(), true.B(), "or".O()));
            Expect(Cond(Evnt("a"), false.B(), false.B(), "or".O()),
                Is.False);

            Expect(Cond(Evnt("a", new object[] { "a", false }), "a".P(), true.B(), "or".O()));
            Expect(Cond(Evnt("a", new object[] { "a", false }), "a".P(), false.B(), "or".O()),
                Is.False);
        }

        #endif

        [Test]
        public void EvaluationOfLogicalOperatorsAgainstIncompatibleTypes() {
            Expect(Cond(Evnt("a", new object[] { "a", 1 }), "a".P(), 1.I(), "and".O()),
                Is.False);
            Expect(Cond(Evnt("a", new object[] { "a", 1F }), "a".P(), 1F.F(), "and".O()),
                Is.False);
            Expect(Cond(Evnt("a", new object[] { "a", "b" }), "a".P(), "b".S(), "and".O()),
                Is.False);
            Expect(Cond(Evnt("a", new object[] { "a", new DateTime() }), "a".P(), new DateTime().T(), "and".O()),
                Is.False);

            Expect(Cond(Evnt("a", new object[] { "a", 1 }), "a".P(), 1.I(), "or".O()),
                Is.False);
            Expect(Cond(Evnt("a", new object[] { "a", 1f }), "a".P(), 1F.F(), "or".O()),
                Is.False);
            Expect(Cond(Evnt("a", new object[] { "a", "b" }), "a".P(), "b".S(), "or".O()),
                Is.False);
            Expect(Cond(Evnt("a", new object[] { "a", new DateTime() }), "a".P(), new DateTime().T(), "or".O()),
                Is.False);
        }

        [Test]
        public void EvaluationOfEqualityComparisonOperators() {
            Expect(Cond(Evnt("a", new object[] { "a", true }), "a".P(), true.B(), "equal to".O()));
            Expect(Cond(Evnt("a", new object[] { "a", true }), "a".P(), false.B(), "equal to".O()),
                Is.False);

            Expect(Cond(Evnt("a", new object[] { "a", 5 }), "a".P(), 4.I(), "equal to".O()),
                Is.False);
            Expect(Cond(Evnt("a", new object[] { "a", 5 }), "a".P(), 5.I(), "equal to".O()));
            Expect(Cond(Evnt("a", new object[] { "a", 5 }), "a".P(), 6.I(), "equal to".O()),
                Is.False);

            Expect(Cond(Evnt("a", new object[] { "a", 5F }), "a".P(), 4F.F(), "equal to".O()),
                Is.False);
            Expect(Cond(Evnt("a", new object[] { "a", 5F }), "a".P(), 5F.F(), "equal to".O()));
            Expect(Cond(Evnt("a", new object[] { "a", 5F }), "a".P(), 6F.F(), "equal to".O()),
                Is.False);

            Expect(Cond(Evnt("a", new object[] { "a", "b" }), "a".P(), "b".S(), "equal to".O()));
            Expect(Cond(Evnt("a", new object[] { "a", "b" }), "a".P(), "c".S(), "equal to".O()),
                Is.False);

            Expect(Cond(Evnt("a", new object[] { "a", new DateTime(50000) }), "a".P(), new DateTime(40000).T(), "equal to".O()),
                Is.False);
            Expect(Cond(Evnt("a", new object[] { "a", new DateTime(50000) }), "a".P(), new DateTime(50000).T(), "equal to".O()));
            Expect(Cond(Evnt("a", new object[] { "a", new DateTime(50000) }), "a".P(), new DateTime(60000).T(), "equal to".O()),
                Is.False);

            Expect(Cond(Evnt("a", new object[] { "a", true }), "a".P(), true.B(), "not equal to".O()),
                Is.False);
            Expect(Cond(Evnt("a", new object[] { "a", true }), "a".P(), false.B(), "not equal to".O()));

            Expect(Cond(Evnt("a", new object[] { "a", 5 }), "a".P(), 4.I(), "not equal to".O()));
            Expect(Cond(Evnt("a", new object[] { "a", 5 }), "a".P(), 5.I(), "not equal to".O()),
                Is.False);
            Expect(Cond(Evnt("a", new object[] { "a", 5 }), "a".P(), 6.I(), "not equal to".O()));

            Expect(Cond(Evnt("a", new object[] { "a", 5F }), "a".P(), 4F.F(), "not equal to".O()));
            Expect(Cond(Evnt("a", new object[] { "a", 5F }), "a".P(), 5F.F(), "not equal to".O()),
                Is.False);
            Expect(Cond(Evnt("a", new object[] { "a", 5F }), "a".P(), 6F.F(), "not equal to".O()));

            Expect(Cond(Evnt("a", new object[] { "a", "b" }), "a".P(), "b".S(), "not equal to".O()),
                Is.False);
            Expect(Cond(Evnt("a", new object[] { "a", "b" }), "a".P(), "c".S(), "not equal to".O()));

            Expect(Cond(Evnt("a", new object[] { "a", new DateTime(50000) }), "a".P(), new DateTime(40000).T(), "not equal to".O()));
            Expect(Cond(Evnt("a", new object[] { "a", new DateTime(50000) }), "a".P(), new DateTime(50000).T(), "not equal to".O()),
                Is.False);
            Expect(Cond(Evnt("a", new object[] { "a", new DateTime(50000) }), "a".P(), new DateTime(60000).T(), "not equal to".O()));
        }

        [Test]
        public void EvaluationOfComparisonOperators() {
            Expect(Cond(Evnt("a", new object[] { "a", 5 }), "a".P(), 4.I(), "greater than".O()));
            Expect(Cond(Evnt("a", new object[] { "a", 5 }), "a".P(), 5.I(), "greater than".O()),
                Is.False);
            Expect(Cond(Evnt("a", new object[] { "a", 5 }), "a".P(), 6.I(), "greater than".O()),
                Is.False);

            Expect(Cond(Evnt("a", new object[] { "a", 5F }), "a".P(), 4F.F(), "greater than".O()));
            Expect(Cond(Evnt("a", new object[] { "a", 5F }), "a".P(), 5F.F(), "greater than".O()),
                Is.False);
            Expect(Cond(Evnt("a", new object[] { "a", 5F }), "a".P(), 6F.F(), "greater than".O()),
                Is.False);

            Expect(Cond(Evnt("a", new object[] { "a", new DateTime(50000) }), "a".P(), new DateTime(40000).T(), "greater than".O()));
            Expect(Cond(Evnt("a", new object[] { "a", new DateTime(50000) }), "a".P(), new DateTime(50000).T(), "greater than".O()),
                Is.False);
            Expect(Cond(Evnt("a", new object[] { "a", new DateTime(50000) }), "a".P(), new DateTime(60000).T(), "greater than".O()),
                Is.False);

            Expect(Cond(Evnt("a", new object[] { "a", 5 }), "a".P(), 4.I(), "greater than eq".O()));
            Expect(Cond(Evnt("a", new object[] { "a", 5 }), "a".P(), 5.I(), "greater than eq".O()));
            Expect(Cond(Evnt("a", new object[] { "a", 5 }), "a".P(), 6.I(), "greater than eq".O()),
                Is.False);

            Expect(Cond(Evnt("a", new object[] { "a", 5F }), "a".P(), 4F.F(), "greater than eq".O()));
            Expect(Cond(Evnt("a", new object[] { "a", 5F }), "a".P(), 5F.F(), "greater than eq".O()));
            Expect(Cond(Evnt("a", new object[] { "a", 5F }), "a".P(), 6F.F(), "greater than eq".O()),
                Is.False);

            Expect(Cond(Evnt("a", new object[] { "a", new DateTime(50000) }), "a".P(), new DateTime(40000).T(), "greater than eq".O()));
            Expect(Cond(Evnt("a", new object[] { "a", new DateTime(50000) }), "a".P(), new DateTime(50000).T(), "greater than eq".O()));
            Expect(Cond(Evnt("a", new object[] { "a", new DateTime(50000) }), "a".P(), new DateTime(60000).T(), "greater than eq".O()),
                Is.False);

            Expect(Cond(Evnt("a", new object[] { "a", 5 }), "a".P(), 4.I(), "less than".O()),
                Is.False);
            Expect(Cond(Evnt("a", new object[] { "a", 5 }), "a".P(), 5.I(), "less than".O()),
                Is.False);
            Expect(Cond(Evnt("a", new object[] { "a", 5 }), "a".P(), 6.I(), "less than".O()));

            Expect(Cond(Evnt("a", new object[] { "a", 5F }), "a".P(), 4F.F(), "less than".O()),
                Is.False);
            Expect(Cond(Evnt("a", new object[] { "a", 5F }), "a".P(), 5F.F(), "less than".O()),
                Is.False);
            Expect(Cond(Evnt("a", new object[] { "a", 5F }), "a".P(), 6F.F(), "less than".O()));

            Expect(Cond(Evnt("a", new object[] { "a", new DateTime(50000) }), "a".P(), new DateTime(40000).T(), "less than".O()),
                Is.False);
            Expect(Cond(Evnt("a", new object[] { "a", new DateTime(50000) }), "a".P(), new DateTime(50000).T(), "less than".O()),
                Is.False);
            Expect(Cond(Evnt("a", new object[] { "a", new DateTime(50000) }), "a".P(), new DateTime(60000).T(), "less than".O()));

            Expect(Cond(Evnt("a", new object[] { "a", 5 }), "a".P(), 4.I(), "less than eq".O()),
                Is.False);
            Expect(Cond(Evnt("a", new object[] { "a", 5 }), "a".P(), 5.I(), "less than eq".O()));
            Expect(Cond(Evnt("a", new object[] { "a", 5 }), "a".P(), 6.I(), "less than eq".O()));

            Expect(Cond(Evnt("a", new object[] { "a", 5f }), "a".P(), 4F.F(), "less than eq".O()),
                Is.False);
            Expect(Cond(Evnt("a", new object[] { "a", 5f }), "a".P(), 5F.F(), "less than eq".O()));
            Expect(Cond(Evnt("a", new object[] { "a", 5f }), "a".P(), 6F.F(), "less than eq".O()));

            Expect(Cond(Evnt("a", new object[] { "a", new DateTime(50000) }), "a".P(), new DateTime(40000).T(), "less than eq".O()),
                Is.False);
            Expect(Cond(Evnt("a", new object[] { "a", new DateTime(50000) }), "a".P(), new DateTime(50000).T(), "less than eq".O()));
            Expect(Cond(Evnt("a", new object[] { "a", new DateTime(50000) }), "a".P(), new DateTime(60000).T(), "less than eq".O()));
        }

        [Test]
        public void EvaluationOfComparisonOperatorsAgainstIncompatibleTypes() {
            Expect(Cond(Evnt("a", new object[] { "a", true }), "a".P(), true.B(), "greater than".O()),
                Is.False);
            Expect(Cond(Evnt("a", new object[] { "a", true }), "a".P(), true.B(), "greater than eq".O()),
                Is.False);
            Expect(Cond(Evnt("a", new object[] { "a", true }), "a".P(), true.B(), "less than".O()),
                Is.False);
            Expect(Cond(Evnt("a", new object[] { "a", true }), "a".P(), true.B(), "less than eq".O()),
                Is.False);

            Expect(Cond(Evnt("a", new object[] { "a", "b" }), "a".P(), "b".S(), "greater than".O()),
                Is.False);
            Expect(Cond(Evnt("a", new object[] { "a", "b" }), "a".P(), "b".S(), "greater than eq".O()),
                Is.False);
            Expect(Cond(Evnt("a", new object[] { "a", "b" }), "a".P(), "b".S(), "less than".O()),
                Is.False);
            Expect(Cond(Evnt("a", new object[] { "a", "b" }), "a".P(), "b".S(), "less than eq".O()),
                Is.False);
        }

        [Test]
        public void EvaluationOfStringComparisonOperators() {
            Expect(Cond(Evnt("a", new object[] { "a", "b" }), "a".P(), "b".S(), "equal to".O()));
            Expect(Cond(Evnt("a", new object[] { "a", "b" }), "a".P(), "B".S(), "equal to".O()),
                Is.False);

            Expect(Cond(Evnt("a", new object[] { "a", "b" }), "a".P(), "b".S(), "not equal to".O()),
                Is.False);
            Expect(Cond(Evnt("a", new object[] { "a", "b" }), "a".P(), "B".S(), "not equal to".O()));

            Expect(Cond(Evnt("a", new object[] { "a", "HeLlO wOrLd" }), "a".P(), "O w".S(), "contains".O()));
            Expect(Cond(Evnt("a", new object[] { "a", "HeLlO wOrLd" }), "a".P(), "o W".S(), "contains".O()),
                Is.False);

            Expect(Cond(Evnt("a", new object[] { "a", "HeLlO wOrLd" }), "a".P(), "O w".S(), "contains ic".O()));
            Expect(Cond(Evnt("a", new object[] { "a", "HeLlO wOrLd" }), "a".P(), "o W".S(), "contains ic".O()));
            Expect(Cond(Evnt("a", new object[] { "a", "HeLlO wOrLd" }), "a".P(), "oW".S(), "contains ic".O()),
                Is.False);

            Expect(Cond(Evnt("a", new object[] { "a", "HeLlO wOrLd" }), "a".P(), "HeLlO".S(), "starts with".O()));
            Expect(Cond(Evnt("a", new object[] { "a", "HeLlO wOrLd" }), "a".P(), "Hello".S(), "starts with".O()),
                Is.False);

            Expect(Cond(Evnt("a", new object[] { "a", "HeLlO wOrLd" }), "a".P(), "HeLlO".S(), "starts with ic".O()));
            Expect(Cond(Evnt("a", new object[] { "a", "HeLlO wOrLd" }), "a".P(), "hElLo".S(), "starts with ic".O()));
            Expect(Cond(Evnt("a", new object[] { "a", "HeLlO wOrLd" }), "a".P(), "wOrLd".S(), "starts with ic".O()),
                Is.False);

            Expect(Cond(Evnt("a", new object[] { "a", "HeLlO wOrLd" }), "a".P(), "wOrLd".S(), "ends with".O()));
            Expect(Cond(Evnt("a", new object[] { "a", "HeLlO wOrLd" }), "a".P(), "World".S(), "ends with".O()),
                Is.False);

            Expect(Cond(Evnt("a", new object[] { "a", "HeLlO wOrLd" }), "a".P(), "wOrLd".S(), "ends with ic".O()));
            Expect(Cond(Evnt("a", new object[] { "a", "HeLlO wOrLd" }), "a".P(), "WoRlD".S(), "ends with ic".O()));
            Expect(Cond(Evnt("a", new object[] { "a", "HeLlO wOrLd" }), "a".P(), "HeLlO".S(), "ends with ic".O()),
                Is.False);
        }

        [Test]
        public void EvaluationOfComplexExpressions() {
            Expect(Cond(
                Evnt(
                    "a",
                    new object[] { "a", 10, "b", 5, "c", "c", "d", true }),
                "c".P(), "c".S(), "equal to".O(), "a".P(), 15.I(), "less than".O(), "and".O(), "b".P(), 15.I(), "greater than eq".O(), "and".O(), "d".P(), true.B(), "equal to".O(), "or".O()));
        }

        [Test]
        public void EvaluationDisambiguatesBetweenStringsAndTimestamps() {
            Expect(Cond(
                Evnt(
                    "a",
                    new object[] { "a", "value", "b", DateTime.ParseExact("1970-01-01 00:00:00.000", Settings.EVENT_TIMESTAMP_FORMAT, CultureInfo.InvariantCulture) }),
                "a".P(), "value".S(), "not equal to".O(), "b".P(), new JSONObject() {{ "t", "1971-01-01T00:00:00.000+0000" }}, "less than".O(), "or".O()));
        }

        [Test]
        public void EvaluationFailsOnMissingParameters() {
            Expect(Cond(
                Evnt("a", new object[] { "a", 5 }),
                "b".P(), 5.I(), "equal to".O()),
                Is.False);
        }

        [Test]
        public void EvaluationFailsOnMismatchedParameterTypes() {
            Expect(Cond(
                Evnt("a",new object[] { "a", "b" }),
                "a".P(), 5.I(), "not equal to".O()),
                Is.False);
        }

        [Test]
        public void EvaluationFailsWhenExecutionsLessThanRequirements(){
            JSONObject limitations = getLimitations(
                new JSONObject{{"executionsRequiredCount", 2}});

            GameEvent evnt = Evnt(
                "a",
                new object[]{"a", 10, "b", 5, "c", "c", "d", true});

            EventTrigger trigger = getEventTrigger(
                limitations,
                evnt ,
                "c".P(), "c".S(), "equal to".O());


            Expect(trigger.Evaluate(evnt),Is.False);

        }

        [Test]
        public void EvaluationSucceedsWhenExecutionsEqualToRequirements(){
            JSONObject limitations = getLimitations(
                new JSONObject{{"executionsRequiredCount", 2}});

            GameEvent evnt = Evnt(
                "a",
                new object[]{"a", 10, "b", 5, "c", "c", "d", true});

            EventTrigger trigger = getEventTrigger(
                limitations,
                    evnt ,
                    "c".P(), "c".S(), "equal to".O());

            Expect(trigger.Evaluate(evnt),Is.False);
            Expect(trigger.Evaluate(evnt),Is.True);

        }

        [Test]
        public void EvaluationSucceedsWithMultipleExecutionConditions(){
            //Session 1
            JSONObject limitations = getLimitations(
                new JSONObject{{"executionsRequiredCount", 2}},
                new JSONObject{{"executionsRequiredCount", 4}});

            GameEvent evnt = Evnt(
                "a",
                new object[]{"a", 10, "b", 5, "c", "c", "d", true});

            EventTrigger trigger = getEventTrigger(
                limitations,
                evnt,
                "c".P(), "c".S(), "equal to".O());

            Expect(trigger.Evaluate(evnt),Is.False);
            Expect(trigger.Evaluate(evnt),Is.True);
            Expect(trigger.Evaluate(evnt),Is.False);
            Expect(trigger.Evaluate(evnt),Is.True);
        }

        [Test]
        public void EvaluationFailsWhenExecutionsLessThanRepeatCount(){
            JSONObject limitations = getLimitations(
                new JSONObject{{"executionsRepeat", 3}});

            GameEvent evnt = Evnt(
                "a",
                new object[]{"a", 10, "b", 5, "c", "c", "d", true});

            EventTrigger trigger = getEventTrigger(
                limitations,
                evnt ,
                "c".P(), "c".S(), "equal to".O());


            Expect(trigger.Evaluate(evnt),Is.False);
            Expect(trigger.Evaluate(evnt),Is.False);
        }

        [Test]
        public void EvaluationSucceedsOnRepeatedExecutions(){
            JSONObject limitations = getLimitations(
                new JSONObject{{"executionsRepeat", 2}});

            GameEvent evnt = Evnt(
                "a",
                new object[]{"a", 10, "b", 5, "c", "c", "d", true});

            EventTrigger trigger = getEventTrigger(
                limitations,
                evnt ,
                "c".P(), "c".S(), "equal to".O());


            Expect(trigger.Evaluate(evnt),Is.False);
            Expect(trigger.Evaluate(evnt),Is.True);
            Expect(trigger.Evaluate(evnt),Is.False);
            Expect(trigger.Evaluate(evnt), Is.True);
        }

        [Test]
        public void EvaluationSucceedsOnRepeatedExecutionsWithMultipleCriteria(){
            JSONObject limitations = getLimitations(
                new JSONObject{{"executionsRepeat", 4}},
                new JSONObject{{"executionsRepeat", 3}}
            );


            GameEvent evnt = Evnt(
                "a",
                new object[]{"a", 10, "b", 5, "c", "c", "d", true});

            EventTrigger trigger = getEventTrigger(
                limitations,
                evnt ,
                "c".P(), "c".S(), "equal to".O());

            // succeed on 3, 4, 6, 8, and 9
            Expect(trigger.Evaluate(evnt),Is.False, "Execution 1 should not trigger");
            Expect(trigger.Evaluate(evnt),Is.False, "Execution 2 should not trigger");
            Expect(trigger.Evaluate(evnt),Is.True, "Execution 3 should trigger");
            Expect(trigger.Evaluate(evnt), Is.True, "Execution 4 should trigger");
            Expect(trigger.Evaluate(evnt),Is.False, "Execution 5 should not trigger");
            Expect(trigger.Evaluate(evnt),Is.True, "Execution 6 should trigger");
            Expect(trigger.Evaluate(evnt),Is.False, "Execution 7 should not trigger");
            Expect(trigger.Evaluate(evnt), Is.True, "Execution 8 should trigger");
            Expect(trigger.Evaluate(evnt),Is.True, "Execution 9 should trigger");
            Expect(trigger.Evaluate(evnt),Is.False, "Execution 10 should not trigger");
        }

        [Test]
        public void EvaluationSucceedsOnRepeatedExecutionsAndRequiredExecutions(){
            JSONObject limitations = getLimitations(
                new JSONObject{{"executionsRepeat", 4}},
                new JSONObject{{"executionsRequiredCount", 3}},
                new JSONObject{{"executionsRequiredCount", 7}}
            );


            GameEvent evnt = Evnt(
                "a",
                new object[]{"a", 10, "b", 5, "c", "c", "d", true});

            EventTrigger trigger = getEventTrigger(
                limitations,
                evnt ,
                "c".P(), "c".S(), "equal to".O());

            // succeed on 3, 4, 7, and 8
            Expect(trigger.Evaluate(evnt),Is.False, "Execution 1 should not trigger");
            Expect(trigger.Evaluate(evnt),Is.False, "Execution 2 should not trigger");
            Expect(trigger.Evaluate(evnt),Is.True, "Execution 3 should trigger");
            Expect(trigger.Evaluate(evnt), Is.True, "Execution 4 should trigger");
            Expect(trigger.Evaluate(evnt),Is.False, "Execution 5 should not trigger");
            Expect(trigger.Evaluate(evnt),Is.False, "Execution 6 should not trigger");
            Expect(trigger.Evaluate(evnt),Is.True, "Execution 7 should trigger");
            Expect(trigger.Evaluate(evnt), Is.True, "Execution 8 should trigger");
            Expect(trigger.Evaluate(evnt),Is.False, "Execution 9 should not trigger");
            Expect(trigger.Evaluate(evnt),Is.False, "Execution 10 should not trigger");
        }

        [Test]
        public void EvaluationFailsOnRepeatedExecutionsAboveLimit(){
            JSONObject limitations = getLimitations(
                new JSONObject{{"executionsRepeat", 2},{"executionsLimit", 2}});

            GameEvent evnt = Evnt(
                "a",
                new object[]{"a", 10, "b", 5, "c", "c", "d", true});

            EventTrigger trigger = getEventTrigger(
                limitations,
                evnt ,
                "c".P(), "c".S(), "equal to".O());


            Expect(trigger.Evaluate(evnt),Is.False);
            Expect(trigger.Evaluate(evnt),Is.True);
            Expect(trigger.Evaluate(evnt),Is.False);
            Expect(trigger.Evaluate(evnt),Is.True);
            Expect(trigger.Evaluate(evnt),Is.False);
            Expect(trigger.Evaluate(evnt),Is.False);
        }

        [Test]
        public void EvaluationSucceedsOnMultipleExecutionsWithLimits(){
            JSONObject limitations = getLimitations(
                new JSONObject{{"executionsRepeat", 2},{"executionsLimit", 2}},
                new JSONObject{{"executionsRepeat", 3},{"executionsLimit", 3}},
                new JSONObject{{"executionsRepeat", 7},{"executionsLimit", 1}}
                );

            GameEvent evnt = Evnt(
                "a",
                new object[]{"a", 10, "b", 5, "c", "c", "d", true});

            EventTrigger trigger = getEventTrigger(
                limitations,
                evnt ,
                "c".P(), "c".S(), "equal to".O());


            Expect(trigger.Evaluate(evnt),Is.False, "Execution 1 should not trigger");
            Expect(trigger.Evaluate(evnt),Is.True, "Execution 2 should trigger");
            Expect(trigger.Evaluate(evnt),Is.True, "Execution 3 should trigger");
            Expect(trigger.Evaluate(evnt), Is.True, "Execution 4 should trigger");
            Expect(trigger.Evaluate(evnt),Is.False, "Execution 5 should not trigger");
            Expect(trigger.Evaluate(evnt),Is.True, "Execution 6 should trigger");
            Expect(trigger.Evaluate(evnt),Is.True, "Execution 7 should trigger");
            Expect(trigger.Evaluate(evnt), Is.False, "Execution 8 should not trigger");
            Expect(trigger.Evaluate(evnt),Is.True, "Execution 9 should trigger");
            Expect(trigger.Evaluate(evnt),Is.False, "Execution 10 should not trigger");
        }

        [Test]
        public void EvaluationSucceedsOnMultipleExecutionRequirementsAndRepeatsWithLimits(){
            JSONObject limitations = getLimitations(
                new JSONObject{{"executionsRepeat", 2},{"executionsLimit", 2}},
                new JSONObject{{"executionsRepeat", 3},{"executionsLimit", 1}},
                new JSONObject{{"executionsRepeat", 5}},
                new JSONObject{{"executionsRequiredCount", 7}},
                new JSONObject{{"executionsRequiredCount", 11}}
            );

            GameEvent evnt = Evnt(
                "a",
                new object[]{"a", 10, "b", 5, "c", "c", "d", true});

            EventTrigger trigger = getEventTrigger(
                limitations,
                evnt ,
                "c".P(), "c".S(), "equal to".O());


            Expect(trigger.Evaluate(evnt),Is.False, "Execution 1 should not trigger");
            Expect(trigger.Evaluate(evnt),Is.True, "Execution 2 should trigger");
            Expect(trigger.Evaluate(evnt),Is.True, "Execution 3 should trigger");
            Expect(trigger.Evaluate(evnt), Is.True, "Execution 4 should trigger");
            Expect(trigger.Evaluate(evnt),Is.True, "Execution 5 should trigger");
            Expect(trigger.Evaluate(evnt),Is.False, "Execution 6 should not trigger");
            Expect(trigger.Evaluate(evnt),Is.True, "Execution 7 should trigger");
            Expect(trigger.Evaluate(evnt), Is.False, "Execution 8 should not trigger");
            Expect(trigger.Evaluate(evnt),Is.False, "Execution 9 should not trigger");
            Expect(trigger.Evaluate(evnt),Is.True, "Execution 10 should trigger");
            Expect(trigger.Evaluate(evnt),Is.True, "Execution 11 should trigger");
            Expect(trigger.Evaluate(evnt),Is.False, "Execution 12 should not trigger");
        }

        private bool Cond(GameEvent evnt, params JSONObject[] values){
            return Cond( new JSONObject(), evnt,  values);
        }

        private bool Cond(JSONObject limitations, GameEvent evnt, params JSONObject[] values) {
             // convert to exact representation as from backend
             return getEventTrigger(limitations, evnt, values).Evaluate(evnt);
        }

        private JSONObject getLimitations(params JSONObject[] showConditions){
            return new JSONObject{
                {"showConditions", showConditions}
            };
        }

        private EventTrigger getEventTrigger(JSONObject limitations, GameEvent evnt, params JSONObject[] values){
            Random random = new Random();
            long campaignId = random.Next();
            long variantId = random.Next();

            return new EventTrigger(
                ddna,
                0,
                new JSONObject{
                    {"eventName", evnt.Name},
                    {"condition", values},
                    {"campaignLimitsConfig", limitations},
                    {"campaignID", campaignId},
                    {"variantID", variantId}
                }.Json().Json(),
                new ExecutionCountManager());
        }


        private static GameEvent Evnt(string name = "a", params object[] values) {
            var evnt = new GameEvent(name);
            for (int i = 0; i < values.Length; i+=2) {
                evnt.AddParam(values[i] as string, values[i+1]);
            }

            return evnt;
        }
    }
}
#endif
