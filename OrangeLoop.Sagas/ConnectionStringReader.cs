namespace OrangeLoop.Sagas
{
    public abstract class ConnectionStringReader
    {
        protected abstract string ConnectionName { get; }
    }
}
