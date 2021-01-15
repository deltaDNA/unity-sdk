using System;
using System.Collections.Generic;

namespace DeltaDNA
{
// DDSDK-69 - https://jira.unity3d.com/browse/DDSDK-69
#pragma warning disable CS0649
    [Serializable]
    internal class DDNAEventManagerEventParameter
    {
        public int id;
        public string name;
        public string description;
        public int application;
        public string type;
        public string format;
        public bool required;
        public bool? calculatingMetricFirst;
        public bool? calculatingMetricLast;
        public bool? calculatingMetricCount;
        public bool? calculatingMetricMin;
        public bool? calculatingMetricMax;
        public bool? calculatingMetricSum;
    }
    
    [Serializable]
    internal class DDNAEventManagerEvent
    {
        public int application;
        public int environment;
        public int id;
        public string name;
        public string description;
        public string createdDate;
        public bool dev;
        public bool published;
        public bool active;
        public List<DDNAEventManagerEventParameter> parameters;
    }
#pragma warning restore CS0649
}