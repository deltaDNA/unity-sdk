//
// Copyright (c) 2020 deltaDNA Ltd. All rights reserved.
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

using System.IO;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;

namespace DeltaDNA.Editor
{
    public class iOSConfiguration
    {
        private const string CONFIG_IOS = "Assets/DeltaDNA/Resources/ddna_configuration_ios.xml";
        private static readonly XmlSerializer _serialiser = new XmlSerializer(typeof(iOSConfiguration), new XmlRootAttribute("configuration"));

        [XmlIgnore]
        public bool Dirty { get; set; }

        [XmlElement("enable_rich_push_notifications")]
        public bool enableRichPushNotifications;
        [XmlElement("push_notification_service_extension_identifier")]
        public string pushNotificationServiceExtensionIdentifier;

        public iOSConfiguration()
        {
            enableRichPushNotifications = false;
            pushNotificationServiceExtensionIdentifier = Application.identifier + ".NotificationService";
        }

        internal static iOSConfiguration Load()
        {
            if (File.Exists(CONFIG_IOS))
            {
                using (var stringReader = new StringReader(File.ReadAllText(CONFIG_IOS)))
                {
                    using (var xmlReader = XmlReader.Create(stringReader))
                    {
                        return _serialiser.Deserialize(xmlReader) as iOSConfiguration;
                    }
                }
            }
            else
            {
                return new iOSConfiguration();
            }
        }

        internal void Save()
        {
            using (var stringWriter = new StringWriter())
            {
                using (XmlWriter xmlWriter = XmlWriter.Create(
                        stringWriter, new XmlWriterSettings() { Indent = true }))
                {
                    _serialiser.Serialize(xmlWriter, this);
                    File.WriteAllText(CONFIG_IOS, stringWriter.ToString());
                }
            }
        }
    }
}
