namespace OrangeLoop.Sagas.Console
{
    public class LiveFreeConnectionStringFactory : AppSettingsConnectionStringFactory
    {
        protected override string ConnectionName => "LiveFree";
    }
}
