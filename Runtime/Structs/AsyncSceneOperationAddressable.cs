#if ENABLE_ADDRESSABLES
using System;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace MyGameDevTools.SceneLoading
{
    /// <summary>
    /// Implementation of <see cref="IAsyncSceneOperation"/> with the addressable <see cref="AsyncOperationHandle<SceneInstance>"/>.
    /// </summary>
    public class AsyncSceneOperationAddressable : IAsyncSceneOperation
    {
        public event Action Completed;

        public float Progress => _asyncOperationHandle.PercentComplete;

        public bool IsDone => _asyncOperationHandle.IsDone;

        public bool HasDirectReferenceToScene => true;

        public AsyncOperationHandle<SceneInstance> AsyncOperationHandle => _asyncOperationHandle;

        readonly AsyncOperationHandle<SceneInstance> _asyncOperationHandle;

        public AsyncSceneOperationAddressable(AsyncOperationHandle<SceneInstance> operationHandle)
        {
            if (!operationHandle.IsValid())
                throw new ArgumentException($"Cannot create a {nameof(AsyncSceneOperationAddressable)} from an invalid AsyncOperationHandle.", nameof(operationHandle));

            _asyncOperationHandle = operationHandle;
            _asyncOperationHandle.CompletedTypeless += OnAsyncOperationCompleted;
        }

        public Scene GetResult()
        {
            if (_asyncOperationHandle.Status == AsyncOperationStatus.Failed)
            {
                throw _asyncOperationHandle.OperationException;
            }

            return _asyncOperationHandle.Result.Scene;
        }

        void OnAsyncOperationCompleted(AsyncOperationHandle asyncOperation)
        {
            _asyncOperationHandle.CompletedTypeless -= OnAsyncOperationCompleted;
            Completed?.Invoke();
        }
    }
}
#endif