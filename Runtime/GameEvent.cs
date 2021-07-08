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

    /// <summary>
    /// Base class for GameEvent so call to AddParam can be chained.
    /// </summary>
    public class GameEvent<T> where T : GameEvent<T> {
    
        internal readonly Params parameters;

        public GameEvent(string name) 
        {
            if (String.IsNullOrEmpty(name)) {
                throw new ArgumentException("Name cannot be null or empty");
            }

            this.Name = name;
            this.parameters = new Params();
        }

        public string Name { get; private set; }

        /// <summary>
        /// Adds an event parameter to the event.
        /// </summary>
        public T AddParam(string key, object value)
        {
            this.parameters.AddParam(key, value);
            return (T) this;
        }

        public Dictionary<string, object> AsDictionary()
        {
            return new Dictionary<string, object>() {
                { "eventName", Name },
                { "eventParams", new Dictionary<string, object>(parameters.AsDictionary()) }
            };
        }
    }

    /// <summary>
    /// Creates a GameEvent for sending an event to Collect.  If you want to extend the behaviour
    /// use <see cref="GameEvent{T}"/> so chaining works correctly. <seealso cref="DeltaDNA.Transaction"/>
    /// </summary>
    public class GameEvent : GameEvent<GameEvent> {

        public GameEvent(string name) : base(name) {}
    }
} // namespace DeltaDNA
