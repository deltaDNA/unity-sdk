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
using System.Collections.Generic;

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
    }

    /// <summary>
    /// Creates a Product, which helps build Transaction and Achievement events.
    /// </summary>
    public class Product : Product<Product> {}

} // namespace DeltaDNA
