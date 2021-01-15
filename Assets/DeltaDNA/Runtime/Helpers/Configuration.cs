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
using UnityEngine;

namespace DeltaDNA
{
    public class Configuration : ScriptableObject
    {
        public const string RUNTIME_RSRC_PATH = "ddna_configuration";
        public const string RESOURCES_CONTAINER = "Assets";
        public const string RESOURCES_DIRECTORY = "Resources";
        public const string ASSET_DIRECTORY = RESOURCES_CONTAINER + "/" + RESOURCES_DIRECTORY;
        public const string FULL_ASSET_PATH = ASSET_DIRECTORY + "/" + RUNTIME_RSRC_PATH + ".asset";

        public string environmentKeyDev = "";
        public string environmentKeyLive = "";
        public int environmentKey = 0;
        public string collectUrl = "";
        public string engageUrl = "";
        public string hashSecret = "";
        public string clientVersion = "";
        public bool useApplicationVersion = false;

        public static Configuration GetAssetInstance()
        {
            // Try and load the asset, else create something to use.
            Configuration cfg = Resources.Load<Configuration>(RUNTIME_RSRC_PATH);

            if(cfg == null)
            {
                cfg = ScriptableObject.CreateInstance<Configuration>();
            }
            return cfg;
        }
    }
}