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
}