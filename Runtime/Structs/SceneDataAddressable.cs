#if ENABLE_ADDRESSABLES
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace MyGameDevTools.SceneLoading
{
    /// <summary>
    /// Struct to manage the link between addressable scene operations, its <see cref="ILoadSceneInfo"/> and resulting loaded scene.
    /// </summary>
    public struct SceneDataAddressable : ISceneData
    {
        public readonly IAsyncSceneOperation AsyncOperation => _asyncSceneOperation;

        public readonly ILoadSceneInfo LoadSceneInfo => _loadSceneInfo;

        public readonly Scene SceneReference => _sceneReference;

        readonly ILoadSceneInfo _loadSceneInfo;

        AsyncSceneOperationAddressable _asyncSceneOperation;
        Scene _sceneReference;

        /// <summary>
        /// Creates a new <see cref="SceneDataAddressable"/> with the provided <see cref="ILoadSceneInfo"/>.
        /// Only supports an <see cref="ILoadSceneInfo"/> with the types <see cref="LoadSceneInfoType.AssetReference" /> or <see cref="LoadSceneInfoType.Address"/>, since those are addressable types.
        /// </summary>
        public SceneDataAddressable(ILoadSceneInfo loadSceneInfo)
        {
            if (loadSceneInfo.Type != LoadSceneInfoType.AssetReference && loadSceneInfo.Type != LoadSceneInfoType.Address)
            {
                throw new ArgumentException($"Cannot create a {nameof(SceneDataAddressable)} with an {nameof(ILoadSceneInfo)} of type '{loadSceneInfo.Type}'. It only supports the {nameof(LoadSceneInfoType.AssetReference)} and {nameof(LoadSceneInfoType.Address)}");
            }

            _loadSceneInfo = loadSceneInfo;
            _asyncSceneOperation = default;
            _sceneReference = default;
        }

        public void SetSceneReferenceManually(Scene scene)
        {
            Debug.LogWarning($"[{GetType().Name}] This type of scene data should not have its scene set manually. Instead, it is expected to set it by calling {nameof(ISceneData.UpdateSceneReference)}.");
            _sceneReference = scene;
        }

        public void UpdateSceneReference()
        {
            if (!AsyncOperation.IsDone)
                throw new Exception($"[{GetType().Name}] Cannot update the scene reference before the scene has been loaded.");

            _sceneReference = AsyncOperation.GetResult();
        }

        public bool MatchesLoadSceneInfo(ILoadSceneInfo loadSceneInfo)
        {
            return loadSceneInfo.Type switch
            {
                LoadSceneInfoType.AssetReference or LoadSceneInfoType.Address => loadSceneInfo.Equals(_loadSceneInfo),
                _ => loadSceneInfo.CanBeReferenceToScene(_sceneReference),
            };
        }

        public IAsyncSceneOperation LoadSceneAsync()
        {
            switch (_loadSceneInfo.Type)
            {
                case LoadSceneInfoType.AssetReference:
                case LoadSceneInfoType.Address:
                    _asyncSceneOperation = new AsyncSceneOperationAddressable(Addressables.LoadSceneAsync(_loadSceneInfo.Reference, LoadSceneMode.Additive));
                    return _asyncSceneOperation;
                default:
                    Debug.LogWarning($"[{GetType().Name}] Unexpected {nameof(ILoadSceneInfo.Reference)} type: {_loadSceneInfo.Reference}");
                    return default;
            }
        }

        public IAsyncSceneOperation UnloadSceneAsync()
        {
            _asyncSceneOperation = new AsyncSceneOperationAddressable(Addressables.UnloadSceneAsync(_asyncSceneOperation.AsyncOperationHandle));
            return _asyncSceneOperation;
        }
    }
}
#endif