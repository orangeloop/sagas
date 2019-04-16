namespace OrangeLoop.Sagas.Interfaces
{
    public interface ISagaStep<K>
    {
        K ExecuteMethod { get; }
        K RollbackMethod { get; }
    }
}
