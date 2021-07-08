﻿//
// Copyright (c) 2018 deltaDNA Ltd. All rights reserved.
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

    /// <summary>
    /// Helps with creating the different types of action available from the Engage
    /// service.  It makes the request to Engage and notifies on a callback when the
    /// requst completes.
    /// </summary>
    public class EngageFactory {

        private readonly DDNABase ddna;

        internal EngageFactory(DDNABase ddna) {
            this.ddna = ddna;
        }

        /// <summary>
        /// Requests game parameters at a <code>decisionPoint</code>.
        /// </summary>
        /// <param name="decisionPoint">the decision point</param>
        /// <param name="callback">the callback to handler the result</param>
        /// <exception cref="ArgumentException">if the <code>decisionPoint</code> is null or empty</exception>
        public void RequestGameParameters(string decisionPoint, Action<JSONObject> callback) {
            RequestGameParameters(decisionPoint, null, callback);
        }
        
        /// <summary>
        /// Requests game parameters at a <code>decisionPoint</code>.
        /// </summary>
        /// <param name="decisionPoint">the decision point</param>
        /// <param name="parameters">an optional set of real-time parameters</param>
        /// <param name="callback">the callback to handle the result</param>
        /// <exception cref="ArgumentException">if the <code>decisionPoint</code> is null or empty</exception>
        public void RequestGameParameters(string decisionPoint, Params parameters, Action<JSONObject> callback) {

            Engagement engagement = BuildEngagement(decisionPoint, parameters);

            ddna.RequestEngagement(engagement, 
                (response) =>
                {
                    callback(response.JSON != null && response.JSON.ContainsKey("parameters") ? response.JSON["parameters"] as JSONObject : new JSONObject());
                }, (exception) =>
                {
                    callback(new JSONObject());
                });
        }
        
        /// <summary>
        /// Requests an image message at a <code>decisionPoint</code>.
        /// </summary>
        /// <param name="decisionPoint">the decision point</param>
        /// <param name="callback">the callback to handle the result</param>
        public void RequestImageMessage(string decisionPoint, Action<ImageMessage> callback) {
            RequestImageMessage(decisionPoint, null, callback);
        }
        
        /// <summary>
        /// Requests an image message at a <code>decisionPoint</code>.
        /// </summary>
        /// <param name="decisionPoint">the decision point</param>
        /// <param name="parameters">an optional set of real-time parameters</param>
        /// <param name="callback">the callback to handle the result</param>
        public void RequestImageMessage(string decisionPoint, Params parameters, Action<ImageMessage> callback) {

            Engagement engagement = BuildEngagement(decisionPoint, parameters);
            
            ddna.RequestEngagement(engagement, 
                (response) =>
                {
                    callback(ImageMessage.Create(response));
                }, (exception) =>
                {
                    callback(null);
                });
        }

        protected static Engagement BuildEngagement(string decisionPoint, Params parameters) {
            
            if (parameters != null) {

                Params parametersCopy;
                try {
                    parametersCopy = new Params(parameters);
                } catch (Exception) {
                    parametersCopy = new Params();
                }
                return new Engagement(decisionPoint, parametersCopy);
            } else {
                return new Engagement(decisionPoint);
            }
        }
    }
}
