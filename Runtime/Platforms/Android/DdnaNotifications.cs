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

#if UNITY_ANDROID && !UNITY_EDITOR

using UnityEngine;

namespace DeltaDNA.Android {

    internal class DDNANotifications {
    
        private AndroidJavaClass ddnaNotifications;
        
        public DDNANotifications() {
            ddnaNotifications = new AndroidJavaClass(Utils.DdnaNotificationsClassName);
        }

        public void MarkUnityLoaded() {
            ddnaNotifications.CallStatic("markUnityLoaded");
        }
        
        public void Register(AndroidJavaObject context, bool secondary) {
            ddnaNotifications.CallStatic("register", context, secondary);
        }
    }
}

#endif
