using System;
using System.Threading;
using System.Threading.Tasks;

namespace MyGameDevTools.SceneLoading
{
    public static class TaskExtensions
    {
        public static async Task<T> RunAndDisposeToken<T>(this Task<T> valueTask, CancellationTokenSource tokenSource)
        {
            try
            {
                return await valueTask;
            }
            finally
            {
                tokenSource.Dispose();
            }
        }

        public static async void Forget(this Task task, Action<Exception> onException = null)
        {
            try
            {
                await task;
            }
            catch (Exception exception)
            {
                if (onException == null)
                    throw exception;

                onException(exception);
            }
        }
    }

    public static class ValueTaskExtensions
    {
        public static async ValueTask<T> RunAndDisposeToken<T>(this ValueTask<T> valueTask, CancellationTokenSource tokenSource)
        {
            try
            {
                return await valueTask;
            }
            finally
            {
                tokenSource.Dispose();
            }
        }

        public static async void Forget<T>(this ValueTask<T> valueTask, Action<Exception> onException = null)
        {
            try
            {
                await valueTask;
            }
            catch (Exception exception)
            {
                if (onException == null)
                    throw exception;

                onException(exception);
            }
        }
    }
}