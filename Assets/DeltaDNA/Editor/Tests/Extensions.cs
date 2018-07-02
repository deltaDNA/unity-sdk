//
// Copyright (c) 2018 deltaDNA Ltd. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed, in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

#if !UNITY_4_5 && !UNITY_4_6 && !UNITY_4_7
using System;
using System.Collections.Generic;
using System.Globalization;

namespace DeltaDNA {

    using JSONObject = Dictionary<string, object>;

    public static class Extensions {

        public static JSONObject Json(this string value) {
            return MiniJSON.Json.Deserialize(value) as JSONObject;
        }

        public static string Json(this JSONObject value) {
            return MiniJSON.Json.Serialize(value);
        }

        public static JSONObject B(this bool value) {
            return new JSONObject() { { "b", value } };
        }

        public static JSONObject F(this float value) {
            return new JSONObject() { { "f", value } };
        }

        public static JSONObject I(this int value) {
            return new JSONObject() { { "i", value } };
        }

        public static JSONObject P(this string value) {
            return new JSONObject() { { "p", value } };
        }

        public static JSONObject O(this string value) {
            return new JSONObject() { { "o", value } };
        }

        public static JSONObject S(this string value) {
            return new JSONObject() { { "s", value } };
        }

        public static JSONObject T(this DateTime value) {
            return new JSONObject() { { "t", value.ToString("o", CultureInfo.InvariantCulture) } };
        }
    }
}
#endif
