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
using System.Xml.Serialization;

namespace DeltaDNA {
    
    [Serializable, XmlRoot("configuration")]
    public sealed class Configuration {
        
        [XmlElement("environment_key_dev")]
        public string environmentKeyDev;
        [XmlElement("environment_key_live")]
        public string environmentKeyLive;
        [XmlElement("environment_key")]
        public int environmentKey;
        [XmlElement("collect_url")]
        public string collectUrl;
        [XmlElement("engage_url")]
        public string engageUrl;
        
        [XmlElement("hash_secret")]
        public string hashSecret;
        [XmlElement("client_version")]
        public string clientVersion;
        [XmlElement("use_application_version")]
        public bool useApplicationVersion;
        
        public Configuration() {
            environmentKeyDev = "";
            environmentKeyLive = "";
            environmentKey = 0;
            collectUrl = "";
            engageUrl = "";
            
            hashSecret = "";
            clientVersion = "";
            useApplicationVersion = true;
        }
    }
}
