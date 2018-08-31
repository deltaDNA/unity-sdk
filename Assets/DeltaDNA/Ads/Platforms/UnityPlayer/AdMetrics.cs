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

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
#endif

namespace DeltaDNA.Ads.UnityPlayer {

    #if UNITY_EDITOR
    internal class AdMetrics {

        private const string LAST_SHOWN = "_last_shown";
        private const string SESSION_COUNT = "_session_count";
        private const string DAILY_COUNT = "_daily_count";

        private readonly HashSet<string> decisionPoints =
            new HashSet<string>();
        private readonly IDictionary<string, object> values =
            new Dictionary<string, object>();

        private bool newDay;

        internal DateTime? LastShown(string decisionPoint) {
            Validate(decisionPoint);

            return values.GetOrDefault(
                decisionPoint + LAST_SHOWN,
                (DateTime?) null);
        }

        internal long SessionCount(string decisionPoint) {
            Validate(decisionPoint);

            return values.GetOrDefault(
                decisionPoint + SESSION_COUNT,
                0L);
        }

        internal long DailyCount(string decisionPoint) {
            Validate(decisionPoint);

            return values.GetOrDefault(
                decisionPoint + DAILY_COUNT,
                0L);
        }

        internal void RecordAdShown(string decisionPoint, DateTime date) {
            Validate(decisionPoint);

            decisionPoints.Add(decisionPoint);

            var sessionCount = 1 + SessionCount(decisionPoint);
            var dailyCount = 1 + DailyCount(decisionPoint);

            var today = DateTime.UtcNow;
            if (StartedNewDay(LastShown(decisionPoint) ?? today, today)) {
                newDay = true;
            }

            values[decisionPoint + LAST_SHOWN] = date.ToUniversalTime();
            values[decisionPoint + SESSION_COUNT] = sessionCount;
            values[decisionPoint + DAILY_COUNT] = dailyCount;
        }

        internal void NewSession(DateTime date) {
            foreach (var decisionPoint in decisionPoints) {
                var resetDailyCount =
                    StartedNewDay(
                        LastShown(decisionPoint) ?? DateTime.UtcNow,
                        date.ToUniversalTime())
                    || newDay;

                values[decisionPoint + SESSION_COUNT] = 0;
                if (resetDailyCount) {
                    values[decisionPoint + DAILY_COUNT] = 0;
                }
            }

            newDay = false;
        }

        private static void Validate(string decisionPoint) {
            if (string.IsNullOrEmpty(decisionPoint))
                throw new Exception("Decision point cannot be null or empty");
        }

        private static bool StartedNewDay(DateTime last, DateTime current) {
            return current.Date.CompareTo(last.Date) > 0;
        }
    }
    #endif
}
