namespace DeltaDNA{
    internal class ExecutionRepeatTriggerCondition : ExecutionCountBasedTriggerCondition{
        
        private readonly long executionsRepeatInterval;
        private long executionLimit = -1;

        public ExecutionRepeatTriggerCondition(long executionsRepeatInterval, ExecutionCountManager executionCountManager, long variantId) : base(executionCountManager,  variantId){
            this.executionsRepeatInterval = executionsRepeatInterval;
        }

        /**
         * Sets an execution limit which is optional.
         * If none is set then the limit will be -1 (or no limit)
         */
        public void setExecutionLimit(long limit){
            executionLimit = limit>0? limit*executionsRepeatInterval : limit;
        }
        
        
        public override bool CanExecute(){
            long currentExecutions = getCurrentExecutionCount();
            if (executionLimit >= 0 && currentExecutions > executionLimit){
                return false;
            }
            return currentExecutions != 0 && currentExecutions % executionsRepeatInterval == 0 ;
        }
        
    }
}