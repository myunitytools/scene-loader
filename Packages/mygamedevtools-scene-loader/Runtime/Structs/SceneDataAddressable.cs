#if ENABLE_ADDRESSABLES
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace MyGameDevTools.SceneLoading
{
    public struct SceneDataAddressable : ISceneData
    {
        public readonly ILoadSceneOperation LoadOperation => _loadSceneOperation;

        public readonly ILoadSceneInfo LoadSceneInfo => _loadSceneInfo;

        public readonly Scene LoadedScene => _loadedScene;

        readonly ILoadSceneInfo _loadSceneInfo;

        ILoadSceneOperation _loadSceneOperation;
        Scene _loadedScene;

        public SceneDataAddressable(ILoadSceneInfo loadSceneInfo)
        {
            _loadSceneInfo = loadSceneInfo;
            _loadSceneOperation = default;
            _loadedScene = default;
        }

        public void SetSceneReferenceManually(Scene scene)
        {
            Debug.LogWarning($"[{GetType().Name}] This type of scene data should not have its scene set manually. Instead, it is expected to set it by calling {nameof(ISceneData.UpdateSceneReference)}.");
            _loadedScene = scene;
        }

        public void UpdateSceneReference()
        {
            if (!LoadOperation.IsDone)
                throw new System.Exception($"[{GetType().Name}] Cannot update the scene reference before the scene has been loaded.");

            _loadedScene = LoadOperation.GetResult();
        }

        public ILoadSceneOperation LoadSceneAsync()
        {
            switch (_loadSceneInfo.Type)
            {
                case LoadSceneInfoType.AssetReference:
                case LoadSceneInfoType.Name:
                    _loadSceneOperation = new LoadSceneOperationAddressable(Addressables.LoadSceneAsync(_loadSceneInfo.Reference, LoadSceneMode.Additive));
                    return _loadSceneOperation;
                default:
                    Debug.LogWarning($"[{GetType().Name}] Unexpected {nameof(ILoadSceneInfo.Reference)} type: {_loadSceneInfo.Reference}");
                    return default;
            }
        }
    }
}
#endif