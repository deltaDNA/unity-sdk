namespace DeltaDNA{
    internal abstract class ExecutionCountBasedTriggerCondition : TriggerCondition{
        protected readonly ExecutionCountManager executionCountManager;
        protected readonly long variantId;
        
        protected ExecutionCountBasedTriggerCondition(ExecutionCountManager executionCountManager, long variantId) {
            this.executionCountManager = executionCountManager;
            this.variantId = variantId;
        }

        protected long getCurrentExecutionCount(){
            return executionCountManager.GetExecutionCount(variantId);
        }

        public abstract bool CanExecute();
    }
}