//
// Copyright (c) 2017 deltaDNA Ltd. All rights reserved.
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

namespace DeltaDNA {
    /// <summary>
    /// The default platforms Unity builds for mapped into our analytics.  The
    /// SDK will pick a default for the platform, but the Platform can be 
    /// explicitly set before calling StartSDK.  For example, if building for
    /// Amazon store, manually set the platform else it will report as Android.
    /// 
    /// If you add / change the enum, remember to update the allowable values
    /// for you game's Platform key.
    /// </summary>
    public enum Platform {
        ANDROID,
        AMAZON,
        IOS,
        IOS_TV,
        WINDOWS,
        BLACKBERRY,
        FACEBOOK,
        PC_CLIENT,
        MAC_CLIENT,
        WEB,
        PSVITA,
        PS4,
        PS3,
        XBOXONE,
        XBOX360,
        WIIU,
        SWITCH,
        UNKNOWN
    }
}
