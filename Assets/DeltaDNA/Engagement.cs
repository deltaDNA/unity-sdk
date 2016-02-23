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

    public class Engagement : Engagement<Engagement> {
        public Engagement(string decisionPoint) : base(decisionPoint) {}
    };

} // namespace DeltaDNA
