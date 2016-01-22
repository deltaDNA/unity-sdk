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

        internal static void SetLogLevel(Level logLevel)
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
