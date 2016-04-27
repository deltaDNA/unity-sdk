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

using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using System;

namespace DeltaDNAAds {

    [SetUpFixture]
    public class SetupTest {

        [SetUp]
        public void Setup()
        {
            // Instantiating the SmartAds Singleton throws an exception 
            // in the test environment because it is calling DontDestroyOnLoad
            // Capturing it here ensures the individual tests that use the Singleton
            // work correctly.
            try {
                var smartads = DDNASmartAds.Instance;
                Debug.Log(smartads);
            } catch (Exception) {

            }
        }
    }

}
