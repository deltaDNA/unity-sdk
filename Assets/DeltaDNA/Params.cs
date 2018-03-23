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

using System;
using System.Globalization;
using System.Collections.Generic;

namespace DeltaDNA {

    public class Params {

        private Dictionary<string, object> _params = new Dictionary<string, object>();
        
        public Params() {}
        
        public Params(Params p)
        {
            this._params = new Dictionary<string, object>(p._params);
        }

        public Params AddParam(string key, object value)
        {
            try {
                if (value is Params) {
                    _params[key] = ((Params) value).AsDictionary();
                }
                else if (value is DateTime) {
                    _params[key] = ((DateTime) value).ToString(Settings.EVENT_TIMESTAMP_FORMAT, CultureInfo.InvariantCulture);
                }
                else {
                    _params[key] = value;
                }
            } catch (ArgumentNullException ex) {
                throw new ArgumentNullException("Key can not be null.", ex);
            }

            return this;
        }

        public object GetParam(string key)
        {
            try {
                return _params.ContainsKey(key) ? _params[key] : null;
            } catch (ArgumentNullException ex) {
                throw new Exception("Key can not be null.", ex);
            } catch (KeyNotFoundException ex) {
                throw new Exception("Key "+key+ " not found.", ex);
            }
        }

        public Dictionary<string, object> AsDictionary()
        {
            return _params;
        }

    }

} // namespace DeltaDNA
