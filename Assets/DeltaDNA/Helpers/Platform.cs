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
    /// If you use a different value, remember to update the allowable values
    /// for you game's Platform key.
    /// </summary>
    public sealed class Platform {

        public const string ANDROID = "ANDROID";
        public const string AMAZON = "AMAZON";
        public const string IOS = "IOS";
        public const string IOS_TV = "IOS_TV";
        public const string WINDOWS_MOBILE = "WINDOWS_MOBILE";
        public const string BLACKBERRY = "BLACKBERRY";
        public const string FACEBOOK = "FACEBOOK";
        public const string PC_CLIENT = "PC_CLIENT";
        public const string MAC_CLIENT = "MAC_CLIENT";
        public const string WEB = "WEB";
        public const string PSVITA = "PSVITA";
        public const string PS4 = "PS4";
        public const string PS3 = "PS3";
        public const string XBOXONE = "XBOXONE";
        public const string XBOX360 = "XBOX360";
        public const string WIIU = "WIIU";
        public const string SWITCH = "SWITCH";
        public const string UNKNOWN = "UNKNOWN";
    }
}
