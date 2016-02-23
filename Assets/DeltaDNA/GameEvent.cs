using System;
using System.Collections.Generic;

namespace DeltaDNA {

    public class GameEvent<T> where T : GameEvent<T> {
    
        private readonly Params parameters;

        public GameEvent(string name) 
        {
            if (String.IsNullOrEmpty(name)) {
                throw new ArgumentException("Name cannot be null or empty");
            }

            this.Name = name;
            this.parameters = new Params();
        }

        public string Name { get; private set; }

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

    public class GameEvent : GameEvent<GameEvent> {

        public GameEvent(string name) : base(name) {}
    }

} // namespace DeltaDNA
