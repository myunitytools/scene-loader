using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MyGameDevTools.SceneLoading
{
    /// <summary>
    /// Implementation of <see cref="IAsyncSceneOperation"/> with the non-addressable <see cref="AsyncOperation"/>.
    /// </summary>
    public class AsyncSceneOperationStandard : IAsyncSceneOperation
    {
        public event Action Completed;

        public float Progress => _asyncOperation.progress;

        public bool IsDone => _asyncOperation.isDone;

        public bool HasDirectReferenceToScene => false;

        readonly AsyncOperation _asyncOperation;

        public AsyncSceneOperationStandard(AsyncOperation operation)
        {
            _asyncOperation = operation ?? throw new ArgumentException($"Cannot create a {nameof(AsyncSceneOperationStandard)} without a valid AsyncOperation instance.", nameof(operation));
            _asyncOperation.completed += OnAsyncOperationCompleted;
        }

        public Scene GetResult()
        {
            Debug.LogWarning($"{nameof(AsyncSceneOperationStandard)} cannot link directly to the loaded scene due to SceneManager API limitations.");
            return default;
        }

        void OnAsyncOperationCompleted(AsyncOperation asyncOperation)
        {
            _asyncOperation.completed -= OnAsyncOperationCompleted;
            Completed?.Invoke();
        }
    }
}