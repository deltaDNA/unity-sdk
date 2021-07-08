//
// Copyright (c) 2017 deltaDNA Ltd. All rights reserved.
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

using System.Collections.Generic;
using System.Linq;
using System.Timers;
using UnityEditor;
using UnityEngine;

namespace DeltaDNA.Editor {

    [InitializeOnLoad]
    internal abstract class SdkChecker {

        internal enum Severity { WARNING, ERROR }

        private static HashSet<SdkChecker> checkers = new HashSet<SdkChecker>();

        static SdkChecker() {
            var timer = new Timer(1000);
            timer.Elapsed += (source, args) => {
                if (GetProblems().Count > 0) {
                    Debug.LogWarning(
                        "Detected possible issues with the DeltaDNA SDK configuration. Please run 'DeltaDNA -> Health Check SDK' for more details.");
                }
            };
            timer.AutoReset = false;
            timer.Start();
        }

        static void RegisterChecker(SdkChecker checker) {
            checkers.Add(checker);
        }

        internal static bool Run() {
            var problems = GetProblems();
            
            string message;
            bool result;
            if (problems.Count == 0) {
                message = "Everything looks good, no issues have been found.";
                result = false;
            } else {
                message = problems
                    .Select(e => e.First + " [" + e.Second.ToString() + "]")
                    .Aggregate(
                        "",
                        (acc, e) => string.IsNullOrEmpty(acc) ? e : acc + "\n\n" + e);
                message += "\n\nPlease consult the migration guide in our README for more details on updating from older versions of the SDK.";
                result = true;
            }

            EditorUtility.DisplayDialog(
                "SDK Health Check Report",
                message,
                "OK");

            return result;
        }

        private static List<DDNATuple<string, Severity>> GetProblems() {
            var problems = new List<DDNATuple<string, Severity>>();
            foreach (var checker in checkers) {
                checker.PerformCheck(problems);
            }
            return problems;
        }

        protected void Register() {
            checkers.Add(this);
        }

        protected abstract void PerformCheck(IList<DDNATuple<string, Severity>> problems);
    }

    internal class DDNATuple<T1, T2> {

        internal T1 First { get; private set; }
        internal T2 Second { get; private set; }

        internal DDNATuple(T1 first, T2 second) {
            First = first;
            Second = second;
        }
    }

    internal static class DDNATuple {

        internal static DDNATuple<T1, T2> New<T1, T2>(T1 first, T2 second) {
            return new DDNATuple<T1, T2>(first, second);
        }
    }
}
