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
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace DeltaDNA {

    using JSONObject = Dictionary<string, object>;

    internal class EventTrigger : IComparable<EventTrigger>{

        private readonly DDNABase ddna;
        private readonly int index;

        private readonly string eventName;
        private readonly JSONObject response;

        private readonly long priority;
        private readonly long limit;
        private readonly JSONObject[] condition;

        private readonly long campaignId;
        private readonly long variantId;


        private readonly string campaignName;
        private readonly string variantName;

        private readonly List<TriggerCondition> campaignTriggerConditions;
        private readonly ExecutionCountManager executionCountManager;

        private int runs;

        internal EventTrigger(DDNABase ddna, int index, JSONObject json, ExecutionCountManager executionCountManager){
            this.ddna = ddna;
            this.index = index;
            this.executionCountManager = executionCountManager;

            eventName = json.ContainsKey("eventName")
                ? json["eventName"] as string
                : "";
            response = json.ContainsKey("response")
                ? json["response"] as JSONObject
                : new JSONObject();

            priority = json.GetOrDefault("priority", 0L);
            limit = json.GetOrDefault("limit", -1L);
            condition = (json.ContainsKey("condition")
                    ? json["condition"] as List<object>
                    : new List<object>(0))
                .Select(e => e as JSONObject)
                .ToArray();

            campaignId = json.GetOrDefault("campaignID", -1L);
            variantId = json.GetOrDefault("variantID", -1L);
            var eventParams = response.GetOrDefault("eventParams", new JSONObject());

            campaignName = eventParams.GetOrDefault<string, string>("responseEngagementName", null);
            variantName = eventParams.GetOrDefault<string, string>("responseVariantName", null);

            JSONObject campaignLimitsConfig = json.GetOrDefault("campaignExecutionConfig", new JSONObject());
            TriggerConditionParser parser = new TriggerConditionParser(campaignLimitsConfig, variantId);
            this.campaignTriggerConditions = parser.parseConditions(executionCountManager);

        }

        internal string GetEventName(){
            return eventName;
        }

        internal virtual string GetAction(){
            if (response.ContainsKey("image")){
                var image = response["image"] as JSONObject;
                if (image.Count > 0){
                    return "imageMessage";
                }
            }

            return "gameParameters";
        }

        internal virtual JSONObject GetResponse(){
            return response;
        }

        internal virtual long GetCampaignId(){
            return campaignId;
        }

        internal virtual bool Evaluate(GameEvent evnt){
            if (evnt.Name != eventName) return false;

            var parameters = evnt.parameters.AsDictionary();
            var stack = new Stack<object>();
            foreach (var token in condition){
                if (token.ContainsKey("o")){
                    string op = (string) token["o"];
                    op = op.ToLower();
                    object right = stack.Pop();
                    object left = stack.Pop();

                    try{
                        if (right is bool){
                            if (left is bool){
                                stack.Push(BOOLS[op]((bool) left, (bool) right));
                            }
                            else{
                                Logger.LogWarning(
                                    left + " and " + right + " have mismatched types");
                                return false;
                            }
                        }
                        else if (right is long){
                            if (left is int){
                                stack.Push(LONGS[op]((int) left, (long) right));
                            }
                            else if (left is long){
                                stack.Push(LONGS[op]((long) left, (long) right));
                            }
                            else{
                                Logger.LogWarning(
                                    left + " and " + right + " have mismatched types");
                                return false;
                            }
                        }
                        else if (right is double){
                            if (left is float){
                                stack.Push(DOUBLES[op]((float) left, (double) right));
                            }
                            else if (left is double){
                                stack.Push(DOUBLES[op]((double) left, (double) right));
                            }
                            else{
                                Logger.LogWarning(
                                    left + " and " + right + " have mismatched types");
                                return false;
                            }
                        }
                        else if (right is string){
                            if (left is string){
                                stack.Push(STRINGS[op]((string) left, (string) right));
                            }
                            else{
                                Logger.LogWarning(
                                    left + " and " + right + " have mismatched types");
                                return false;
                            }
                        }
                        else if (right is DateTime){
                            if (left is string){
                                stack.Push(DATES[op](
                                    DateTime.ParseExact(
                                        (string) left,
                                        Settings.EVENT_TIMESTAMP_FORMAT,
                                        System.Globalization.CultureInfo.InvariantCulture),
                                    (DateTime) right));
                            }
                            else{
                                Logger.LogWarning(
                                    left + " and " + right + " have mismatched types");
                                return false;
                            }
                        }
                        else{
                            Logger.LogWarning("Unexpected type for " + right);
                            return false;
                        }
                    }
                    catch (KeyNotFoundException){
                        Logger.LogWarning(string.Format(
                            "Failed to find operation {0} for {1} and {2}",
                            op,
                            left,
                            right));
                        return false;
                    }
                    catch (FormatException){
                        Logger.LogWarning("Failed converting parameter " + left + " to DateTime");
                        return false;
                    }
                }
                else if (token.ContainsKey("p")){
                    var param = (string) token["p"];
                    if (parameters.ContainsKey(param)){
                        stack.Push(parameters[param]);
                    }
                    else{
                        Logger.LogWarning("Failed to find " + param + " in event params");
                        return false;
                    }
                }
                else if (token.ContainsKey("b")){
                    stack.Push((bool) token["b"]);
                }
                else if (token.ContainsKey("i")){
                    // ints are double precision in JSON
                    stack.Push((long) token["i"]);
                }
                else if (token.ContainsKey("f")){
                    var value = token["f"];
                    // serialiser inserts a whole double as a long
                    if (value is long){
                        stack.Push((double) (long) token["f"]);
                    }
                    else{
                        // floats are double precision in JSON
                        stack.Push((double) token["f"]);
                    }
                }
                else if (token.ContainsKey("s")){
                    stack.Push((string) token["s"]);
                }
                else if (token.ContainsKey("t")){
                    try{
                        stack.Push(DateTime.Parse((string) token["t"], null));
                    }
                    catch (FormatException){
                        Logger.LogWarning("Failed converting " + token["t"] + " to DateTime");
                        return false;
                    }
                }
                else{
                    stack.Push(token);
                }
            }


            
            var result = stack.Count == 0 || (stack.Pop() as bool? ?? false);
            if (result){
            
                // Default to true if no conditions exist
                bool triggerConditionsReached = campaignTriggerConditions.Count == 0;

                // Only one condition needs to be true to flip conditions to true
                this.executionCountManager.incrementExecutionCount(this.variantId);
                foreach (TriggerCondition campaignTriggerCondition in campaignTriggerConditions){
                    if (campaignTriggerCondition.CanExecute()){
                        triggerConditionsReached = true;
                    }
                }

                // If none reached return false
                if (!triggerConditionsReached){
                    return false;
                }
                if (limit != -1 && runs >= limit) return false;
                
                runs++;
                var eventTriggeredActionEvent = new GameEvent("ddnaEventTriggeredAction")
                    .AddParam("ddnaEventTriggeredCampaignID", campaignId)
                    .AddParam("ddnaEventTriggeredCampaignPriority", priority)
                    .AddParam("ddnaEventTriggeredVariantID", variantId)
                    .AddParam("ddnaEventTriggeredActionType", GetAction())
                    .AddParam("ddnaEventTriggeredSessionCount", runs);

                if (campaignName != null){
                    eventTriggeredActionEvent.AddParam("ddnaEventTriggeredCampaignName", campaignName);
                }

                if (variantName != null){
                    eventTriggeredActionEvent.AddParam("ddnaEventTriggeredVariantName", variantName);
                }

                ddna.RecordEvent(eventTriggeredActionEvent);

            }

            return result;
        }

        public int CompareTo(EventTrigger other){
            var primary = priority.CompareTo(other.priority) * -1;
            if (primary == 0){
                return index.CompareTo(other.index);
            }
            else{
                return primary;
            }
        }

#if UNITY_EDITOR
        internal EventTrigger() : this(null, 0, new JSONObject(), null){ }
#endif

        private static readonly Dictionary<string, Func<bool, bool, bool>> BOOLS =
            new Dictionary<string, Func<bool, bool, bool>>(){
                {"and", delegate(bool left, bool right){ return left && right; }},
                {"or", delegate(bool left, bool right){ return left || right; }},
                {"equal to", delegate(bool left, bool right){ return left == right; }},
                {"not equal to", delegate(bool left, bool right){ return left != right; }}
            };

        private static readonly Dictionary<string, Func<long, long, bool>> LONGS =
            new Dictionary<string, Func<long, long, bool>>(){
                {"equal to", delegate(long left, long right){ return left == right; }},
                {"not equal to", delegate(long left, long right){ return left != right; }},
                {"greater than", delegate(long left, long right){ return left > right; }},
                {"greater than eq", delegate(long left, long right){ return left >= right; }},
                {"less than", delegate(long left, long right){ return left < right; }},
                {"less than eq", delegate(long left, long right){ return left <= right; }},
            };

        private static readonly Dictionary<string, Func<double, double, bool>> DOUBLES =
            new Dictionary<string, Func<double, double, bool>>(){
                {"equal to", delegate(double left, double right){ return left == right; }},
                {"not equal to", delegate(double left, double right){ return left != right; }},
                {"greater than", delegate(double left, double right){ return left > right; }},
                {"greater than eq", delegate(double left, double right){ return left >= right; }},
                {"less than", delegate(double left, double right){ return left < right; }},
                {"less than eq", delegate(double left, double right){ return left <= right; }}
            };

        private static readonly Dictionary<string, Func<string, string, bool>> STRINGS =
            new Dictionary<string, Func<string, string, bool>>(){
                {"equal to", delegate(string left, string right){ return string.Equals(left, right); }},{
                    "equal to ic",
                    delegate(string left, string right){
                        return string.Equals(left, right, StringComparison.OrdinalIgnoreCase);
                    }
                },
                {"not equal to", delegate(string left, string right){ return !string.Equals(left, right); }},{
                    "not equal to ic",
                    delegate(string left, string right){
                        return !string.Equals(left, right, StringComparison.OrdinalIgnoreCase);
                    }
                },
                {"contains", delegate(string left, string right){ return left.IndexOf(right) >= 0; }},{
                    "contains ic",
                    delegate(string left, string right){
                        return left.IndexOf(right, StringComparison.OrdinalIgnoreCase) >= 0;
                    }
                },
                {"starts with", delegate(string left, string right){ return left.StartsWith(right); }},{
                    "starts with ic",
                    delegate(string left, string right){
                        return left.StartsWith(right, StringComparison.OrdinalIgnoreCase);
                    }
                },
                {"ends with", delegate(string left, string right){ return left.EndsWith(right); }},{
                    "ends with ic",
                    delegate(string left, string right){
                        return left.EndsWith(right, StringComparison.OrdinalIgnoreCase);
                    }
                }
            };

        private static readonly Dictionary<string, Func<DateTime, DateTime, bool>> DATES =
            new Dictionary<string, Func<DateTime, DateTime, bool>>(){
                {"equal to", delegate(DateTime left, DateTime right){ return left == right; }},
                {"not equal to", delegate(DateTime left, DateTime right){ return left != right; }},
                {"greater than", delegate(DateTime left, DateTime right){ return left > right; }},
                {"greater than eq", delegate(DateTime left, DateTime right){ return left >= right; }},
                {"less than", delegate(DateTime left, DateTime right){ return left < right; }},
                {"less than eq", delegate(DateTime left, DateTime right){ return left <= right; }}
            };
    }
}
