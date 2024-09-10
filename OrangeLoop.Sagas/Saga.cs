using OrangeLoop.Sagas.Interfaces;
using OrangeLoop.Sagas.Interfaces.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

namespace OrangeLoop.Sagas
{
    public class Saga<T> : ISaga<T> where T : class
    {
        protected LinkedList<ISagaStep<T>> Steps { get; private set; } = new LinkedList<ISagaStep<T>>();

        public virtual ISaga<T> AddStep(Action<ISagaStep<T>> action)
        {
            var step = new SagaStep<T>();
            action.Invoke(step);
            Steps.AddLast(step);
            return this;
        }

        public virtual async Task<ExecutionResult<T>> Run(T context)
        {
            var step = Steps.First;
            var result = ExecutionResult<T>.CreateEmpty();

            try
            {
                while (step != null)
                {
                    await step.Value.Execute(context, model =>
                    {
                        context = model;
                        step = step.Next;
                        result.SetSuccess(context);
                        return Task.CompletedTask;
                    },
                    async (model, error) =>
                    {
                        result = await HandleRollback(step, model, error);
                        step = null;
                    });
                }
            }
            catch (Exception ex)
            {
                return await HandleRollback(step, context, ex);
            }

            return result;
        }

        private static async Task<ExecutionResult<T>> HandleRollback(LinkedListNode<ISagaStep<T>> step, T context, Exception exception)
        {
            var result = ExecutionResult<T>
                .CreateEmpty()
                .AppendException(exception);

            while (step != null)
            {
                // If rollback results in an unhandled exception, include in the result
                // and continue on to the next rollback.
                try
                {
                    await step.Value.Rollback(context, model =>
                    {
                        context = model;
                        return Task.CompletedTask;
                    },
                    (model, error) =>
                    {
                        context = model;
                        result.AppendException(error);
                        return Task.CompletedTask;
                    });
                }
                catch (Exception inner)
                {
                    result.AppendException(inner);
                }

                result.SetFailure(context);
                step = step.Previous;
            }

            return result;
        }
    }
}
