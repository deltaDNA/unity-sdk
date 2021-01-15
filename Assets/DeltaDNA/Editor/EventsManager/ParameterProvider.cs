using System;
using System.Collections.Generic;

namespace DeltaDNA
{
    internal class ParameterProvider : EventsManagerDataProvider<DDNAEventManagerEventParameter>
    {
        private const string PARAMETERS_ENDPOINT_URL = "https://api.deltadna.net/api/events/v1/event-parameters";

        private static readonly HashSet<string> LOCKED_PARAMETER_NAMES = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "userXP", "userLevel", "userScore", "platform", "sdkVersion"
        };

        private static readonly HashSet<string> SPECIAL_PARAMETER_NAMES = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "eventName", "eventParams", "eventTimestamp", "eventUUID", "sessionID", "userID"
        };

        /// <summary>
        /// These parameters cannot be deleted from an event
        /// </summary>
        public HashSet<int> LockedParameterIDs { get; private set; }

        /// <summary>
        /// These parameters are outside eventParams and so do not show up in the API
        /// </summary>
        public HashSet<int> SpecialParameterIDs { get; private set; }

        public ParameterProvider() : base(PARAMETERS_ENDPOINT_URL)
        {
            LockedParameterIDs = new HashSet<int>();
            SpecialParameterIDs = new HashSet<int>();

            OnResponseArrived += RefreshLockedParametersList;
        }

        private void RefreshLockedParametersList()
        {
            if (HasData)
            {
                LockedParameterIDs.Clear();
                SpecialParameterIDs.Clear();
                foreach (DDNAEventManagerEventParameter apiParameter in Data)
                {
                    if (LOCKED_PARAMETER_NAMES.Contains(apiParameter.name))
                    {
                        LockedParameterIDs.Add(apiParameter.id);
                    }

                    if (SPECIAL_PARAMETER_NAMES.Contains(apiParameter.name))
                    {
                        SpecialParameterIDs.Add(apiParameter.id);
                    }
                }
            }
        }
    }
}
