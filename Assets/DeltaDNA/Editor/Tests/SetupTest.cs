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

#if !UNITY_4 && UNITY_5_6_OR_NEWER
using UnityEngine;
using UnityEditor;
using NUnit.Framework;

namespace DeltaDNA {

    [SetUpFixture]
    public class SetupTest {

        [OneTimeSetUp]
        public void Setup()
        {
            Debug.Log("Starting Tests.");
            Logger.SetLogLevel(Logger.Level.ERROR);
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            Debug.Log("Completed Tests.");

        }
    }

}
#endif
