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

    public class Engagement<T> where T : Engagement<T> {

        private readonly Params parameters;

        public Engagement(string decisionPoint)
        {
            if (String.IsNullOrEmpty(decisionPoint)) {
                throw new ArgumentException("decisionPoint cannot be null or empty");
            }

            this.DecisionPoint = decisionPoint;
            this.Flavour = "engagement";
            this.parameters = new Params();
        }

        public string DecisionPoint { get; private set; }

        public string Flavour { get; private set; }

        /// <summary>
        /// Add a parameter to the engage request.
        /// </summary>
        public T AddParam(string key, object value)
        {
            this.parameters.AddParam(key, value);
            return (T) this;
        }

        public Dictionary<string, object> AsDictionary()
        {
            return new Dictionary<string, object>() {
                { "decisionPoint", DecisionPoint },
                { "flavour", Flavour },
                { "parameters", new Dictionary<string, object>(parameters.AsDictionary()) }
            };
        }
    }

    /// <summary>
    /// Creates an Engagement to make a request to engage.  If you want to extend the behaviour
    /// use <see cref="Engagement{T}"/> so chaining works correctly.
    /// </summary>
    public class Engagement : Engagement<Engagement> 
    {
        public Engagement(string decisionPoint) : base(decisionPoint) {}
    };

} // namespace DeltaDNA
