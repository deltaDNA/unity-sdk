namespace DeltaDNA{
    internal class ExecutionCountTriggerCondition : ExecutionCountBasedTriggerCondition {

        private readonly long executionsRequired;

        public ExecutionCountTriggerCondition(long executionsRequired, ExecutionCountManager executionCountManager, long variantId) : base(executionCountManager,  variantId){
            this.executionsRequired = executionsRequired;
        }

        public override bool CanExecute() {
            return executionsRequired == getCurrentExecutionCount();
        }
    }
}