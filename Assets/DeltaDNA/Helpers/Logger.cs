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
#if UNITY_IOS
using System.Runtime.InteropServices;
#endif

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
        
         #if UNITY_IOS
        [DllImport("__Internal")]
        private static extern void _logToConsole(string message);
        #endif

        public const string PREFIX = "[DDSDK] ";

        static Level sLogLevel = Level.INFO;

        public static void SetLogLevel(Level logLevel)
        {
            sLogLevel = logLevel;
        }
        
        internal static Level LogLevel { get { return sLogLevel;  }}

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
            switch (level)
            {
                case Level.ERROR:
                    Debug.LogError(PREFIX + "[ERROR] " + msg);
                    break;
                case Level.WARNING:
                    Debug.LogWarning(PREFIX + "[WARNING] " + msg);
                    break;
                case Level.INFO:
                    Debug.Log(PREFIX + "[INFO] " + msg);
                    break;
                default:
                    Debug.Log(PREFIX + "[DEBUG] " + msg);
                    break;
            }
        }
        
        internal static void HandleLog(string logString, string stackTrace, LogType type) {
            #if UNITY_IOS
            if (Application.platform == RuntimePlatform.IPhonePlayer) {
                // Pump Unity logging to iOS NSLog
                if (logString.StartsWith(PREFIX)) {
                    _logToConsole(logString);
                }
            }
            #endif
        }

    }
}
