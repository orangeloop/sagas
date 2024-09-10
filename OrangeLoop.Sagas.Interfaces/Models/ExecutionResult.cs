namespace OrangeLoop.Sagas.Interfaces.Models
{
    public class ExecutionResult
    {
        private readonly List<Exception> _exceptions = new List<Exception>();

        public static ExecutionResult CreateSuccess()
            => new ExecutionResult
            {
                Success = true
            };

        public static ExecutionResult CreateFailure()
            => new ExecutionResult
            {
                Success = false,
            };

        public static ExecutionResult CreateEmpty()
            => new ExecutionResult
            {
                Success = false,
                Error = null
            };

        public bool Success { get; set; }
        public AggregateException? Error { get; set; }
        public virtual ExecutionResult AppendException(Exception ex)
        {
            _exceptions.Add(ex);
            Error = new AggregateException(_exceptions);
            return this;
        }

        public ExecutionResult SetSuccess()
        {
            Success = true;
            return this;
        }

        public virtual ExecutionResult SetFailure()
        {
            Success = false;
            return this;
        }
    }

    public class ExecutionResult<T> : ExecutionResult where T : class
    {

        public static ExecutionResult<T> CreateSuccess(T result)
            => new ExecutionResult<T>
            {
                Success = true,
                Value = result
            };

        public static ExecutionResult<T> CreateFailure(T result)
            => new ExecutionResult<T>
            {
                Success = false,
                Value = result,
            };

        public static new ExecutionResult<T> CreateEmpty()
            => new ExecutionResult<T>
            {
                Success = false,
                Value = null,
                Error = null
            };

        public static new ExecutionResult<T> CreateFailure()
            => new ExecutionResult<T>
            { 
                Success = false, 
                Value = null 
            };

        public T? Value { get; set; }

        public override ExecutionResult<T> AppendException(Exception ex)
        {
            base.AppendException(ex);
            return this;
        }

        public override ExecutionResult<T> SetFailure()
        {
            base.SetFailure();
            return this;
        }

        public ExecutionResult<T> SetFailure(T result)
        {
            base.SetFailure();
            Value = result;
            return this;
        }

        public ExecutionResult<T> SetSuccess(T result)
        {
            base.SetSuccess();
            this.Value = result;
            return this;
        }
    }
}
