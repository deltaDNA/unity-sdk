using System;
using System.Globalization;
using System.Collections.Generic;

namespace DeltaDNA {

    public class Params {

        private Dictionary<string, object> _params = new Dictionary<string, object>();

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
                throw new Exception("Key can not be null.", ex);
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
