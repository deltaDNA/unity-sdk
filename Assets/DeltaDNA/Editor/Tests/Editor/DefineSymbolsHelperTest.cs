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
using System;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;


namespace DeltaDNA.Editor {

    public class DefineSymbolsHelperTest : AssertionHelper {

        private const string A = "a";
        private const string B = "b";

        private string symbols;

        [SetUp]
        public void SetUp() {
            symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(
                EditorUserBuildSettings.selectedBuildTargetGroup);
        }

        [TearDown]
        public void TearDown() {
            PlayerSettings.SetScriptingDefineSymbolsForGroup(
                EditorUserBuildSettings.selectedBuildTargetGroup,
                symbols);
        }

        [Test]
        public void SymbolAdded() {
            DefineSymbolsHelper.Add(A);
           Expect(GetSymbols(), Contains(A));
        }

        [Test]
        public void SymbolsAdded() {
            DefineSymbolsHelper.Add(A);
            DefineSymbolsHelper.Add(B);

            Expect(GetSymbols(), Contains(A));
            Expect(GetSymbols(), Contains(B));
        }

        [Test]
        public void SymbolRemoved() {
            DefineSymbolsHelper.Add(A);
            DefineSymbolsHelper.Remove(A);

            Expect(GetSymbols(), !Contains(A));
        }

        [Test]
        public void SymbolsAddedAndSingleRemoved() {
            DefineSymbolsHelper.Add(A);
            DefineSymbolsHelper.Add(B);
            DefineSymbolsHelper.Remove(A);

            Expect(GetSymbols(), !Contains(A));
            Expect(GetSymbols(), Contains(B));
        }

        private static List<string> GetSymbols() {
            return PlayerSettings
                .GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup)
                .Split(';')
                .ToList();
        }
    }
}
#endif
