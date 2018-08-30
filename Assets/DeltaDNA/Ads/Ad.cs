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

using System;

namespace DeltaDNA
{

    using JSONObject = System.Collections.Generic.Dictionary<string, object>;

    public abstract class Ad
    {
        protected Engagement engagement;

        protected Ad(Engagement engagement)
        {
            this.engagement = engagement;
        }

        public abstract bool IsReady();
        public abstract void Show();
        
        public string DecisionPoint {
            get { return engagement != null ? engagement.DecisionPoint : null; }
        }
        
        public Engagement Engagement {
            get { return engagement; }
        }

        public JSONObject EngageParams {
            get {
                return (engagement != null && engagement.JSON != null)
                    ? engagement.JSON["parameters"] as JSONObject
                    : null;
            }
        }

        public DateTime? LastShown {
            get { return DecisionPoint != null ? SmartAds.Instance.GetLastShown(DecisionPoint) : null; }
        }
        
        public long AdShowWaitSecs {
            get { return EngageParams.GetOrDefault("ddnaAdShowWaitSecs", 0L); }
        }
        
        public long SessionCount {
            get { return DecisionPoint != null ? SmartAds.Instance.GetSessionCount(DecisionPoint) : 0; }
        }
        
        public long SessionLimit {
            get { return EngageParams.GetOrDefault("ddnaAdSessionCount", 0L); }
        }
        
        public long DailyCount {
            get { return DecisionPoint != null ? SmartAds.Instance.GetDailyCount(DecisionPoint) : 0; }
        }
        
        public long DailyLimit {
            get { return EngageParams.GetOrDefault("ddnaAdDailyCount", 0L); }
        }
    }
}
