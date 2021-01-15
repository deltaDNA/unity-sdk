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

    using JSONObject = System.Collections.Generic.Dictionary<string, object>;

    public class Engagement<T> where T : Engagement<T> {

        private readonly Params parameters;
        private string response = null;

        /// <summary>
        /// Creates a new Engagement with a decision point.
        /// </summary>
        /// <param name="decisionPoint">The Decision Point.</param>
        public Engagement(string decisionPoint) : this(decisionPoint, new Params()) {}
        
        internal Engagement(string decisionPoint, Params parameters) {
            if (String.IsNullOrEmpty(decisionPoint)) {
                throw new ArgumentException("decisionPoint cannot be null or empty");
            }
            
            DecisionPoint = decisionPoint;
            Flavour = "engagement";
            this.parameters = parameters;
        }
        
        /// <summary>
        /// Gets the decision point.
        /// </summary>
        /// <value>The decision point for this Engagement.</value>
        public string DecisionPoint { get; private set; }

        /// <summary>
        /// Gets the flavour.
        /// </summary>
        /// <value>The flavour of this Engagement.</value>
        public string Flavour { get; internal set; }

        /// <summary>
        /// Add a parameter to the engage request.
        /// </summary>
        public T AddParam(string key, object value)
        {
            this.parameters.AddParam(key, value);
            return (T) this;
        }

        /// <summary>
        /// The Engagement as a JSON dictionary.
        /// </summary>
        /// <returns>The dictionary.</returns>
        public JSONObject AsDictionary()
        {
            return new JSONObject() {
                { "decisionPoint", DecisionPoint },
                { "flavour", Flavour },
                { "parameters", new JSONObject(parameters.AsDictionary()) }
            };
        }

        /// <summary>
        /// The Raw response from the Engage service.  A successful engagement will be a json string, else the error that occurred.
        /// </summary>
        /// <value>The raw response from Engage.</value>
        public string Raw { 
            get { return this.response; } 
            set {
                JSONObject json = null;
                if (!String.IsNullOrEmpty(value)) {
                    try {
                        json = DeltaDNA.MiniJSON.Json.Deserialize(value) as JSONObject;
                    } catch (Exception) {
                        // not valid json
                    }
                }
                this.response = value;
                this.JSON = json ?? new JSONObject();
            } 
        }

        /// <summary>
        /// The HTTP status code from the engage service.
        /// </summary>
        /// <value>The status code.</value>
        public int StatusCode { get; set; }

        /// <summary>
        /// Records any error that may have occurred.
        /// </summary>
        /// <value>The error.</value>
        public string Error { get; set; }

        /// <summary>
        /// The JSON response for the Engagement.  Will be empty if the engagement was unsuccessful, 
        /// or marked with 'isCachedResponse=true' key if the cache was used.  
        /// </summary>
        /// <value>The response from Engage as JSON.</value>
        public JSONObject JSON { get; internal set; }

        internal string GetDecisionPointAndFlavour() {
            return DecisionPoint + '@' + Flavour;
        }

        public override string ToString ()
        {
            return string.Format ("[Engagement: DecisionPoint={0}, Flavour={1}, Raw={2}, StatusCode={3}, Error={4}, JSON={5}]", 
                DecisionPoint, Flavour, Raw, StatusCode, Error, JSON);
        }
    }

    /// <summary>
    /// Creates an Engagement to make a request to engage.  If you want to extend the behaviour
    /// use <see cref="Engagement{T}"/> so chaining works correctly.
    /// </summary>
    public class Engagement : Engagement<Engagement> 
    {
        public Engagement(string decisionPoint) : base(decisionPoint) {}
        internal Engagement(string decisionPoint, Params parameters) : base(decisionPoint, parameters) {}
    };
} // namespace DeltaDNA
