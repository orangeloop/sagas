using OrangeLoop.Sagas.Interfaces;
using System;
using System.Threading.Tasks;

namespace OrangeLoop.Sagas
{
    public class SagaStep<T> : ISagaStep<T> where T : class
    {
        public Func<T, Func<T, Task>, Func<T, Exception, Task>, Task> Execute { get; private set; }
        public Func<T, Func<T, Task>, Func<T, Exception, Task>, Task> Rollback { get; private set; }

        public ISagaStep<T> OnExecute(Func<T, Func<T, Task>, Func<T, Exception, Task>, Task> func)
        {
            Execute = func;
            return this;
        }

        public ISagaStep<T> OnRollback(Func<T, Func<T, Task>, Func<T, Exception, Task>, Task> func)
        {
            Rollback = func;
            return this;
        }
    }
}
