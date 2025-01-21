using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MyGameDevTools.SceneLoading
{
    /// <summary>
    /// Struct to manage the link between non-addressable scene operations, its <see cref="ILoadSceneInfo"/> and resulting loaded scene.
    /// </summary>
    public struct SceneDataStandard : ISceneData
    {
        public readonly IAsyncSceneOperation AsyncOperation => _asyncSceneOperation;

        public readonly ILoadSceneInfo LoadSceneInfo => _loadSceneInfo;

        public readonly Scene SceneReference => _sceneReference;

        readonly ILoadSceneInfo _loadSceneInfo;

        IAsyncSceneOperation _asyncSceneOperation;
        Scene _sceneReference;

        /// <summary>
        /// Creates a new <see cref="SceneDataStandard"/> with the provided <see cref="ILoadSceneInfo"/>.
        /// Does not support an <see cref="ILoadSceneInfo"/> with the types <see cref="LoadSceneInfoType.AssetReference" /> or <see cref="LoadSceneInfoType.Address"/>, as those are addressable types.
        /// </summary>
        public SceneDataStandard(ILoadSceneInfo loadSceneInfo)
        {
            if (loadSceneInfo.Type == LoadSceneInfoType.AssetReference || loadSceneInfo.Type == LoadSceneInfoType.Address)
            {
                throw new ArgumentException($"Cannot create a {nameof(SceneDataStandard)} with an {nameof(ILoadSceneInfo)} of type '{loadSceneInfo.Type}'. It only supports the {nameof(LoadSceneInfoType.Name)}, {nameof(LoadSceneInfoType.BuildIndex)} and {nameof(LoadSceneInfoType.SceneHandle)}");
            }

            _loadSceneInfo = loadSceneInfo;
            _asyncSceneOperation = default;
            _sceneReference = default;
        }
        /// <summary>
        /// Creates a new <see cref="SceneDataStandard"/> with an already loaded <see cref="Scene"/>.
        /// This will create an <see cref="ISceneData"/> without a load <see cref="IAsyncSceneOperation"/>,
        /// and with an <see cref="LoadSceneInfoScene"/> as its <see cref="ILoadSceneInfo"/>.
        /// </summary>
        public SceneDataStandard(Scene loadedScene)
        {
            if (!loadedScene.IsValid() || !loadedScene.isLoaded)
            {
                throw new ArgumentException($"Cannot create a {nameof(SceneDataStandard)} with an invalid or not loaded scene: {loadedScene.name} ({loadedScene.handle})");
            }

            _loadSceneInfo = new LoadSceneInfoScene(loadedScene);
            _sceneReference = loadedScene;
            _asyncSceneOperation = default;
        }

        public void SetSceneReferenceManually(Scene scene)
        {
            if (!AsyncOperation.IsDone)
                throw new Exception($"[{nameof(SceneDataStandard)}] Cannot update the scene reference before the scene has been loaded.");

            _sceneReference = scene;
        }

        public void UpdateSceneReference()
        {
            Debug.LogWarning($"[{nameof(SceneDataStandard)}] This type of scene data should not have its scene set automatically. Instead, it is expected to set it by calling {nameof(ISceneData.SetSceneReferenceManually)}.");
        }

        public bool MatchesLoadSceneInfo(ILoadSceneInfo loadSceneInfo)
        {
            return loadSceneInfo.CanBeReferenceToScene(_sceneReference);
        }

        public IAsyncSceneOperation LoadSceneAsync()
        {
            switch (_loadSceneInfo.Type)
            {
                case LoadSceneInfoType.BuildIndex:
                    _asyncSceneOperation = new AsyncSceneOperationStandard(SceneManager.LoadSceneAsync((int)_loadSceneInfo.Reference, LoadSceneMode.Additive));
                    break;
                case LoadSceneInfoType.Name:
                    _asyncSceneOperation = new AsyncSceneOperationStandard(SceneManager.LoadSceneAsync((string)_loadSceneInfo.Reference, LoadSceneMode.Additive));
                    break;
                default:
                    Debug.LogWarning($"[{nameof(SceneDataStandard)}] Unexpected {nameof(ILoadSceneInfo.Reference)} type: {_loadSceneInfo.Reference}");
                    return default;
            }
            return _asyncSceneOperation;
        }

        public IAsyncSceneOperation UnloadSceneAsync()
        {
            _asyncSceneOperation = new AsyncSceneOperationStandard(SceneManager.UnloadSceneAsync(_sceneReference));
            return _asyncSceneOperation;
        }
    }
}