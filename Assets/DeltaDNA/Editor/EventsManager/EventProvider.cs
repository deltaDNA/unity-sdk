namespace DeltaDNA
{
    internal class EventProvider : EventsManagerDataProvider<DDNAEventManagerEvent>
    {
        private const string ENDPOINT_URL = "https://api.deltadna.net/api/events/v1/events";

        public EventProvider() : base(ENDPOINT_URL)
        {
        }

        public bool IsDirty(DDNAEventManagerEvent devEvent)
        {
            bool result = false;

            if (HasData)
            {
                DDNAEventManagerEvent liveEvent = null;

                foreach (DDNAEventManagerEvent apiEvent in Data)
                {
                    if (apiEvent.id != devEvent.id &&
                        apiEvent.name == devEvent.name &&
                        apiEvent.application == devEvent.application)
                    {
                        liveEvent = apiEvent;
                    }
                }

                if (devEvent != null &&
                    !devEvent.published)
                {
                    result = true;
                }
                else if (devEvent != null &&
                         liveEvent != null)
                {
                    result = AreDifferent(devEvent, liveEvent);
                }
                // NOTE: if the liveEvent is null, we can't make any assertions about whether or not it is dirty...
                // (i.e. the user's API token does not give access to the live environment for us to snoop on the changes)
            }

            return result;
        }

        private bool AreDifferent(DDNAEventManagerEvent devEvent, DDNAEventManagerEvent liveEvent)
        {
            bool result = false;

            if (devEvent.parameters.Count != liveEvent.parameters.Count)
            {
                // Different parameter counts, clearly a change.
                result = true;
            }
            else
            {
                // Same parameter counts, could still have removed one and added another

                foreach (DDNAEventManagerEventParameter devParameter in devEvent.parameters)
                {
                    bool existsInLive = false;

                    foreach (DDNAEventManagerEventParameter liveParameter in liveEvent.parameters)
                    {
                        if (liveParameter.id == devParameter.id)
                        {
                            existsInLive = true;
                            break;
                        }
                    }

                    if (!existsInLive)
                    {
                        result = true;
                        break;
                    }
                }
            }

            return result;
        }
    }
}
