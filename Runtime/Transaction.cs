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

namespace DeltaDNA {

    public class Transaction<T> : GameEvent<T> where T : Transaction<T> {

        public Transaction(string name, string type, Product productsReceived, Product productsSpent)
        :
            base("transaction")
        {
            if (String.IsNullOrEmpty(name)) {
                throw new ArgumentException("Name cannot be null or empty");
            }

            if (String.IsNullOrEmpty(type)) {
                throw new ArgumentException("Type cannot be null or empty");
            }

            if (productsReceived == null) {
                throw new ArgumentException("Products received cannot be null");
            }

            if (productsSpent == null) {
                throw new ArgumentException("Products spent cannot be null");
            }

            AddParam("transactionName", name);
            AddParam("transactionType", type);
            AddParam("productsReceived", productsReceived);
            AddParam("productsSpent", productsSpent);
        }

        public T SetTransactionId(string transactionId)
        {
            if (String.IsNullOrEmpty(transactionId)) {
                throw new ArgumentException("transactionId cannot be null or empty");
            }

            AddParam("transactionID", transactionId);
            return (T) this;
        }

        public T SetReceipt(string receipt)
        {
            if (String.IsNullOrEmpty(receipt)) {
                throw new ArgumentException("receipt cannot be null or empty");
            }

            AddParam("transactionReceipt", receipt);
            return (T) this;
        }

        public T SetReceiptSignature(string receiptSignature) {
            if (string.IsNullOrEmpty(receiptSignature)) {
                throw new ArgumentException("receipt signature cannot be null or empty");
            }

            AddParam("transactionReceiptSignature", receiptSignature);
            return (T) this;
        }

        public T SetServer(string server)
        {
            if (String.IsNullOrEmpty(server)) {
                throw new ArgumentException("server cannot be null or empty");
            }

            AddParam("transactionServer", server);
            return (T) this;
        }

        public T SetTransactorId(string transactorId)
        {
            if (String.IsNullOrEmpty(transactorId)) {
                throw new ArgumentException("transactorId cannot be null or empty");
            }

            AddParam("transactorID", transactorId);
            return (T) this;
        }

        public T SetProductId(string productId)
        {
            if (String.IsNullOrEmpty(productId)) {
                throw new ArgumentException("productId cannot be null or empty");
            }
            AddParam("productID", productId);
            return (T) this;
        }
    }

    /// <summary>
    /// Creates a Transaction event.
    /// </summary>
    public class Transaction : Transaction<Transaction> {

        public Transaction(string name, string type, Product productsReceived, Product productsSpent)
        : base(name, type, productsReceived, productsSpent) {}
    }

} // namespace DeltaDNA
