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

using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace DeltaDNA {

    public class Product<T> : Params where T : Product<T> {

        private List<Dictionary<string, object>> virtualCurrencies;
        private List<Dictionary<string, object>> items;

        public T SetRealCurrency(string type, int amount)
        {
            if (String.IsNullOrEmpty(type)) {
                throw new ArgumentException("Type cannot be null or empty");
            }

            var realCurrency = new Dictionary<string, object>() {
                { "realCurrencyType", type },
                { "realCurrencyAmount", amount }
            };

            AddParam("realCurrency", realCurrency);

            return (T) this;
        }

        public T AddVirtualCurrency(string name, string type, int amount)
        {
            if (String.IsNullOrEmpty(name)) {
                throw new ArgumentException("Name cannot be null or empty");
            }

            if (String.IsNullOrEmpty(type)) {
                throw new ArgumentException("Type cannot be null or empty");
            }

            var virtualCurrency = new Dictionary<string, object>() {
                { "virtualCurrency", new Dictionary<string, object>() {
                        { "virtualCurrencyName", name },
                        { "virtualCurrencyType", type },
                        { "virtualCurrencyAmount", amount }
                    }
                }
            };

            if (GetParam("virtualCurrencies") == null) {
                this.virtualCurrencies = new List<Dictionary<string, object>>();
                AddParam("virtualCurrencies", this.virtualCurrencies);
            }

            this.virtualCurrencies.Add(virtualCurrency);

            return (T) this;
        }

        public T AddItem(string name, string type, int amount)
        {
            if (String.IsNullOrEmpty(name)) {
                throw new ArgumentException("Name cannot be null or empty");
            }

            if (String.IsNullOrEmpty(type)) {
                throw new ArgumentException("Type cannot be null or empty");
            }

            var item = new Dictionary<string, object>() {
                { "item", new Dictionary<string, object>() {
                        { "itemName", name },
                        { "itemType", type },
                        { "itemAmount", amount }
                    }
                }
            };

            if (GetParam("items") == null) {
                this.items = new List<Dictionary<string, object>>();
                AddParam("items", this.items);
            }

            this.items.Add(item);

            return (T) this;
        }

        /// <summary>
        /// Converts a currency in a decimal format, such as '1.23' EUR, into an
        /// integer representation which can be used with the SetRealCurrency method.
        /// This method will also work for currencies which don't use a minor currency
        /// unit, for example such as the Japanese Yen (JPY).
        /// </summary>
        /// <param name="code">The ISO 4217 currency code</param>
        /// <param name="value">The currency value to convert</param>
        /// <returns>The converted integer value</returns>
        /// <seealso cref="SetRealCurrency(string, int)"/>
        public static int ConvertCurrency(string code, decimal value) {
            if (ISO4217.ContainsKey(code)) {
                return decimal.ToInt32(value * (decimal) Math.Pow(10, ISO4217[code]));
            } else {
                Debug.LogWarning("Failed to find currency for: " + code);
                return 0;
            }
        }

        private static readonly IDictionary<string, int> ISO4217;
        static Product() {
            var res = Resources.Load("iso_4217", typeof(TextAsset)) as TextAsset;
            var xml = XDocument.Parse(res.text);

            ISO4217 = xml
                .Descendants("CcyNtry")
                .Where(e => e.Element("Ccy") != null)
                .Aggregate(new Dictionary<string, int>(), (result, e) => {
                    var key = e.Element("Ccy").Value;
                    int value;
                    try {
                        value = int.Parse(e.Element("CcyMnrUnts").Value);
                    } catch (FormatException) {
                        value = 0;
                    }

                    result[key] = value;

                    return result;
                });
        }
    }

    /// <summary>
    /// Creates a Product, which helps build Transaction and Achievement events.
    /// </summary>
    public class Product : Product<Product> {}

} // namespace DeltaDNA
