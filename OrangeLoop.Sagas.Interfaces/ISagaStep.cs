namespace OrangeLoop.Sagas.Interfaces
{
    public interface ISagaStep<T> where T : class
    {
        Func<T, Func<T, Task>, Func<T, Exception, Task>, Task> Execute { get; }
        Func<T, Func<T, Task>, Func<T, Exception, Task>, Task> Rollback { get; }

        //ISagaStep<T> OnExecute(Func<T, Task<T>> func);
        ISagaStep<T> OnExecute(Func<T, Func<T, Task>, Func<T, Exception, Task>, Task> func);
        ISagaStep<T> OnRollback(Func<T, Func<T, Task>, Func<T, Exception, Task>, Task> func);
    }
}
