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
using UnityEditor;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace DeltaDNA {

    using JSONObject = System.Collections.Generic.Dictionary<string, object>;

    public class ProductTest {

        private Product product;

        [SetUp]
        public void SetUp()
        {
            product = new Product();
        }

        [Test]
        public void CanAddItems()
        {
            product.AddItem("grow", "potion", 2);
            product.AddItem("shrink", "potion", 1);

            CollectionAssert.AreEquivalent(new JSONObject() {
                { "items", new List<object>() {
                        new JSONObject() {
                            { "item", new JSONObject() {
                                    { "itemName", "grow" },
                                    { "itemType", "potion" },
                                    { "itemAmount", 2 }
                                }
                            }
                        },
                        new JSONObject() {
                            { "item", new JSONObject() {
                                    { "itemName", "shrink" },
                                    { "itemType", "potion" },
                                    { "itemAmount", 1 }
                                }
                            }
                        }
                    }
                }
            }, product.AsDictionary());
        }

        [Test]
        public void CanAddVirtualCurrencies()
        {
            product.AddVirtualCurrency("VIP Points", "GRIND", 50);
            product.AddVirtualCurrency("Gold Coins", "In-Game", 100);

            CollectionAssert.AreEquivalent(new JSONObject() {
                { "virtualCurrencies", new List<object>() {
                        new JSONObject() {
                            { "virtualCurrency", new JSONObject() {
                                    { "virtualCurrencyName", "VIP Points" },
                                    { "virtualCurrencyType", "GRIND" },
                                    { "virtualCurrencyAmount", 50 }
                                }
                            }
                        },
                        new JSONObject() {
                            { "virtualCurrency", new JSONObject() {
                                    { "virtualCurrencyName", "Gold Coins" },
                                    { "virtualCurrencyType", "In-Game" },
                                    { "virtualCurrencyAmount", 100 }
                                }
                            }
                        }
                    }
                }
            }, product.AsDictionary());
        }

        [Test]
        public void CanSetARealCurrency()
        {
            product.SetRealCurrency("USD", 15);

            CollectionAssert.AreEquivalent(new JSONObject() {
                { "realCurrency", new Dictionary<string, object>() {
                        { "realCurrencyType", "USD" },
                        { "realCurrencyAmount", 15 }
                    }
                }
            }, product.AsDictionary());
        }

        [Test]
        public void WillThrowIfNullOrEmptyValues()
        {
            Assert.Throws<ArgumentException>(() => {
                product.AddItem(null, "potion", 2);
            });

            Assert.Throws<ArgumentException>(() => {
                product.AddItem("", "potion", 2);
            });

            Assert.Throws<ArgumentException>(() => {
                product.AddItem("grow", null, 2);
            });

            Assert.Throws<ArgumentException>(() => {
                product.AddItem("grow", "", 2);
            });

            Assert.Throws<ArgumentException>(() => {
                product.AddVirtualCurrency(null, "GRIND", 50);
            });

            Assert.Throws<ArgumentException>(() => {
                product.AddVirtualCurrency("", "GRIND", 50);
            });

            Assert.Throws<ArgumentException>(() => {
                product.AddVirtualCurrency("VIP Points", null, 50);
            });

            Assert.Throws<ArgumentException>(() => {
                product.AddVirtualCurrency("VIP Points", "", 50);
            });

            Assert.Throws<ArgumentException>(() => {
                product.SetRealCurrency(null, 15);
            });

            Assert.Throws<ArgumentException>(() => {
                product.SetRealCurrency("", 15);
            });
        }
    }
}
