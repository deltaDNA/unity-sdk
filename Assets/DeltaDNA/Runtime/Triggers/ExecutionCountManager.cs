namespace DeltaDNA{
    public class ExecutionCountManager : SimpleDataStore<long, long>{
        
        public ExecutionCountManager(): base("eventTrigger", "counts") {
            
        }
        
        protected override long parseKey(string key){
            return long.Parse(key);
        }

        protected override long parseValue(string value){
            return parseKey(value);
        }

        protected override string createLine(long key, long value){
            return "" + key + getKeyValueSeparator() + value;
        }
        
        public long GetExecutionCount(long variantId){
            return GetOrDefault(variantId, 0L);
        }

        public void incrementExecutionCount(long variantId){
            Put(variantId, GetOrDefault(variantId, 0)+1);
        }

    }
        
}