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

    public class Product : Product<Product> {}

} // namespace DeltaDNA
