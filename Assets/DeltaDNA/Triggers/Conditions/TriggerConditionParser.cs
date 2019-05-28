using System.Collections.Generic;
using System.Linq;

namespace DeltaDNA{
    
    using JSONObject = Dictionary<string, object>;
    
    public class TriggerConditionParser{
        
        private readonly JSONObject campaignLimitsConfig;
        private readonly long variantId;

       public  TriggerConditionParser(JSONObject campaignLimitsConfig, long variantId){
            this.campaignLimitsConfig = campaignLimitsConfig;
            this.variantId = variantId;
        }
        
    
        public List<TriggerCondition> parseConditions(ExecutionCountManager executionCountManager) {
            List<TriggerCondition> limitations = new List<TriggerCondition>();

            if (campaignLimitsConfig.ContainsKey("showConditions")){
                JSONObject[] showConditions = (campaignLimitsConfig["showConditions"] as List<object>).Select(e => e as JSONObject).ToArray();
                foreach (var showCondition in showConditions){
                    TriggerCondition limitation = parseCondition(showCondition, executionCountManager);
                    if (limitation != null){
                        limitations.Add(limitation);
                    }
                }
            }

            return limitations;
        }
        
        public TriggerCondition parseCondition(JSONObject showCondition, ExecutionCountManager executionCountManager){
            if (showCondition.ContainsKey("executionsRequiredCount")){
                long executionsRequired = showCondition.GetOrDefault("executionsRequiredCount", 0L);
                return new ExecutionCountTriggerCondition(executionsRequired, executionCountManager, variantId);
            }
            
            if (showCondition.ContainsKey("executionsRepeat")){
                long executionsRepeat = showCondition.GetOrDefault("executionsRepeat", 1L);
                var limit = new ExecutionRepeatTriggerCondition(executionsRepeat, executionCountManager, variantId);

                if (showCondition.ContainsKey("executionsLimit")){
                    long executionsLimit = showCondition.GetOrDefault("executionsLimit", -1L);
                    limit.setExecutionLimit(executionsLimit);
                }
                
                return limit;
            }
            
            return null;
        }
        

    }
}