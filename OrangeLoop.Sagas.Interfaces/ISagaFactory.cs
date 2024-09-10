namespace OrangeLoop.Sagas.Interfaces
{
    public interface ISagaFactory
    {
        ISaga<T> Create<T>() where T : class;
    }
}
