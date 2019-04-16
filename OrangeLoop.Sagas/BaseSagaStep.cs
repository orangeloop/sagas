using OrangeLoop.Sagas.Interfaces;

namespace OrangeLoop.Sagas
{
    public abstract class BaseSagaStep<K> : ISagaStep<K>
    {
        public BaseSagaStep() { }

        public BaseSagaStep(K executeMethod, K rollbackMethod)
        {
            ExecuteMethod = executeMethod;
            RollbackMethod = rollbackMethod;
        }

        public K ExecuteMethod { get; protected set; }
        public K RollbackMethod { get; protected set; }
    }
}
