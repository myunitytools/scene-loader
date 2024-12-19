using System.Collections;
using System.Threading.Tasks;

namespace MyGameDevTools.SceneLoading
{
    public readonly struct WaitTask<T> : IEnumerator
    {
        public object Current => null;

        public readonly Task<T> Task;

        readonly bool _throwOnException;

        public WaitTask(Task<T> task, bool throwOnException = true)
        {
            Task = task;
            _throwOnException = throwOnException;
        }

        public bool MoveNext()
        {
            if (_throwOnException && Task.IsFaulted)
                throw Task.Exception;

            return !Task.IsCompleted && !Task.IsCanceled && !Task.IsFaulted;
        }

        public void Reset() { }
    }

    public readonly struct WaitValueTask<T> : IEnumerator
    {
        public object Current => null;

        public readonly ValueTask<T> ValueTask;

        public WaitValueTask(ValueTask<T> valueTask)
        {
            ValueTask = valueTask;
        }

        public bool MoveNext()
        {
            return !ValueTask.IsCompleted && !ValueTask.IsCanceled && !ValueTask.IsFaulted;
        }

        public void Reset() { }
    }
}