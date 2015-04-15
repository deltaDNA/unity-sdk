#if UNITY_5
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTest
{
    public class PlatformRunnerSettings : ProjectSettingsBase
    {
        public string resultsPath;
        public bool sendResultsOverNetwork = true;
        public int port = 0;
    }
}

#endif
