using OrangeLoop.Sagas.Interfaces;
using System;
using System.Threading.Tasks;

namespace OrangeLoop.Sagas
{
    public abstract class BaseSaga<K, T>
    {
        protected ISagaConfiguration<K> Configuration { get; private set; }

        public BaseSaga(ISagaConfiguration<K> configuration)
        {
            Configuration = configuration;
        }

        protected void Configure(Action<ISagaConfiguration<K>> config)
        {
            config.Invoke(Configuration);
        }

        protected async Task<T> Run(T context, Func<K, T, Task<T>> invoker)
        {
            var step = Configuration.Steps.First;

            try
            {
                while (step != null)
                {
                    context = await invoker.Invoke(step.Value.ExecuteMethod, context).ConfigureAwait(false);
                    step = step.Next;
                }
            }
            catch (Exception)
            {
                while (step != null)
                {
                    // What do we do if the rollback itself fails??
                    context = invoker.Invoke(step.Value.RollbackMethod, context).Result;
                    step = step.Previous;
                }

                throw;
            }

            return context;
        }

        public abstract Task<T> Run(T context);
    }
}
