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

#if !UNITY_4
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace DeltaDNA {

    using JSONObject = Dictionary<string, object>;

    public class TransactionTest {

        [Test]
        public void CreateTransaction()
        {
            var productsReceived = new Product();
            var productsSpent = new Product();

            var transaction = new Transaction("shop", "weapon", productsReceived, productsSpent);

            CollectionAssert.AreEquivalent(new JSONObject() {
                { "eventName", "transaction" },
                { "eventParams", new JSONObject() {
                        { "transactionName", "shop" },
                        { "transactionType", "weapon" },
                        { "productsReceived", new JSONObject() {} },
                        { "productsSpent", new JSONObject() {} }
                    }
                }
            }, transaction.AsDictionary());
        }

        [Test]
        public void CreateTransactionWithOptionalValues()
        {
            var productsReceived = new Product();
            var productsSpent = new Product();

            var transaction = new Transaction("shop", "weapon", productsReceived, productsSpent);
            transaction.SetTransactionId("12345");
            transaction.SetServer("local");
            transaction.SetReceipt("123223----***5433");
            transaction.SetReceiptSignature("receiptSignature");
            transaction.SetTransactorId("abcde");
            transaction.SetProductId("5678-4332");

            CollectionAssert.AreEquivalent(new JSONObject() {
                { "eventName", "transaction" },
                { "eventParams", new JSONObject() {
                        { "transactionName", "shop" },
                        { "transactionType", "weapon" },
                        { "productsReceived", new JSONObject() {} },
                        { "productsSpent", new JSONObject() {} },
                        { "transactionID", "12345" },
                        { "transactionServer", "local" },
                        { "transactionReceipt", "123223----***5433" },
                        { "transactionReceiptSignature", "receiptSignature" },
                        { "transactorID", "abcde" },
                        { "productID", "5678-4332" }
                    }
                }
            }, transaction.AsDictionary());
        }

        [Test]
        public void WillThrowIfNullOrEmptyValues()
        {
            var productsReceived = new Product();
            var productsSpent = new Product();

            Assert.Throws<ArgumentException>(() => {
                new Transaction(null, "weapon", productsReceived, productsSpent);
            });

            Assert.Throws<ArgumentException>(() => {
                new Transaction("", "weapon", productsReceived, productsSpent);
            });

            Assert.Throws<ArgumentException>(() => {
                new Transaction("shop", null, productsReceived, productsSpent);
            });

            Assert.Throws<ArgumentException>(() => {
                new Transaction("shop", "", productsReceived, productsSpent);
            });

            Assert.Throws<ArgumentException>(() => {
                new Transaction("shop", "weapon", null, productsSpent);
            });

            Assert.Throws<ArgumentException>(() => {
                new Transaction("shop", "weapon", productsReceived, null);
            });
        }

        [Test]
        public void WillThrowIfNullOrEmptyOptionalValues() {
            Assert.Throws<ArgumentException>(() => {
                new Transaction("name", "type", new Product(), new Product()).SetReceiptSignature(null);
            });
            Assert.Throws<ArgumentException>(() => {
                new Transaction("name", "type", new Product(), new Product()).SetReceiptSignature("");
            });
        }
    }
}
#endif
