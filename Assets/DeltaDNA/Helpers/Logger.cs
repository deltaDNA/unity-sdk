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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DeltaDNA
{
    public static class Logger
    {
        public enum Level
        {
            DEBUG = 0,
            INFO = 1,
            WARNING = 2,
            ERROR = 3
        };

        static Level sLogLevel = Level.WARNING;

        public static void SetLogLevel(Level logLevel)
        {
            sLogLevel = logLevel;
        }

        internal static void LogDebug(string msg)
        {
            if (sLogLevel <= Level.DEBUG)
            {
                Log(msg, Level.DEBUG);
            }
        }

        internal static void LogInfo(string msg)
        {
            if (sLogLevel <= Level.INFO)
            {
                Log(msg, Level.INFO);
            }
        }

        internal static void LogWarning(string msg)
        {
            if (sLogLevel <= Level.WARNING)
            {
                Log(msg, Level.WARNING);
            }
        }

        internal static void LogError(string msg)
        {
            if (sLogLevel <= Level.ERROR)
            {
                Log(msg, Level.ERROR);
            }
        }

        private static void Log(string msg, Level level)
        {
            string prefix = "[DDSDK] ";

            switch (level)
            {
                case Level.ERROR:
                    Debug.LogError(prefix + msg);
                    break;
                case Level.WARNING:
                    Debug.LogWarning(prefix + msg);
                    break;
                default:
                    Debug.Log(prefix + msg);
                    break;
            }
        }

    }
}
