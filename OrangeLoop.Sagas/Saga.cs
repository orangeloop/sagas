using System.Threading.Tasks;

namespace OrangeLoop.Sagas
{
    public abstract class Saga<T> : BaseSaga<StepMethod<T>, T>
    {
        public Saga() : base(new SagaConfiguration<T>()) { }

        public override Task<T> Run(T context)
        {
            return base.Run(context, (func, ctx) => {
                return func.Invoke(ctx);
            });
        }
    }
}
