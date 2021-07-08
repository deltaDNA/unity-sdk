//
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
using System.Collections.Generic;

namespace DeltaDNA {

    using JSONObject = Dictionary<string, object>;

    /// <summary>
    /// Handlers which can be registered on <see cref="EventAction"/>s for
    /// handling actions of different types.
    /// <para/>
    /// <see cref="GameParametersHandler"/> and <see cref="ImageMessageHandler"/>
    /// should be used.
    /// </summary>
    public abstract class EventActionHandler {

        internal abstract bool Handle(EventTrigger trigger, ActionStore store);
        internal abstract string Type();
    }

    /// <summary>
    /// <see cref="EventActionHandler"/> for handling game parameters, which
    /// will be returned as a <see cref="JSONObject"/>.
    /// </summary>
    public class GameParametersHandler : EventActionHandler {

        private readonly Action<JSONObject> callback;

        public GameParametersHandler(Action<JSONObject> callback) {
            this.callback = callback;
        }

        internal override bool Handle(EventTrigger trigger, ActionStore store) {
            if (trigger.GetAction() == Type()) {
                var response = trigger.GetResponse();
                var persistedParams = store.Get(trigger);

                if (persistedParams != null) {
                    store.Remove(trigger);
                    callback(persistedParams);
                } else if (response.ContainsKey("parameters")) {
                    callback((JSONObject) response["parameters"]);
                } else {
                    callback(new JSONObject());
                }

                return true;
            }

            return false;
        }

        internal override string Type() {
            return "gameParameters";
        }
    }

    /// <summary>
    /// <see cref="EventActionHandler"/> for handling <see cref="ImageMessage"/>s.
    /// </summary>
    public class ImageMessageHandler : EventActionHandler {

        private readonly DDNA ddna;
        private readonly Action<ImageMessage> callback;

        public ImageMessageHandler(DDNA ddna, Action<ImageMessage> callback) {
            this.ddna = ddna;
            this.callback = callback;
        }

        internal override bool Handle(EventTrigger trigger, ActionStore store) {
            if (trigger.GetAction() == Type()) {
                // copy the json to avoid modifying original
                var response = new JSONObject(trigger.GetResponse());
                var persistedParams = store.Get(trigger);

                if (persistedParams != null) {
                    response["parameters"] = persistedParams;
                }

                var image = ImageMessage.Create(
                    ddna,
                    new Engagement("dummy") { JSON = response },
                    null);

                if (image != null && image.IsReady()) {
                    if (persistedParams != null) {
                        store.Remove(trigger);
                    }

                    callback(image);
                    return true;
                }
            }

            return false;
        }

        internal override string Type() {
            return "imageMessage";
        }
    }
}
