using System;
using System.Threading.Tasks;
using UnityEngine;

namespace MyGameDevTools.SceneLoading
{
    /// <summary>
    /// Responsible for reporting the scene loading progress.
    /// </summary>
    public class LoadingProgress : IProgress<float>
    {
        /// <summary>
        /// Reports when the scene loading progress increases. Values range from 0 to 1.
        /// </summary>
        public event Action<float> Progressed;
        public event Action LoadingCompleted;

        public readonly TaskCompletionSource<bool> TransitionInTask;
        public readonly TaskCompletionSource<bool> TransitionOutTask;

        public LoadingProgress()
        {
            TransitionInTask = new TaskCompletionSource<bool>();
            TransitionOutTask = new TaskCompletionSource<bool>();
        }

        public void StartTransition()
        {
            TransitionInTask.SetResult(true);
        }

        public void EndTransition()
        {
            TransitionOutTask.SetResult(true);
        }

        public void SetLoadingCompleted()
        {
            LoadingCompleted?.Invoke();
        }

        /// <summary>
        /// <see cref="IProgress{T}"/> implementation. Reports the scene loading progress value, ranging from 0 to 1.
        /// </summary>
        /// <param name="value">Scene loading progress value, ranging from 0 to 1.</param>
        public void Report(float value)
        {
            Progressed?.Invoke(Mathf.Clamp01(value));
        }
    }
}