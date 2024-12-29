using System.Collections;
using System.Threading.Tasks;

namespace MyGameDevTools.SceneLoading
{
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