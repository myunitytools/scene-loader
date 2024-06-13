using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MyGameDevTools.SceneLoading
{
    public readonly struct SceneLoaderAsync : ISceneLoaderAsync
    {
        public ISceneManager Manager => _manager;

        readonly ISceneManager _manager;
        readonly CancellationTokenSource _lifetimeTokenSource;

        public SceneLoaderAsync(ISceneManager manager)
        {
            _manager = manager ?? throw new ArgumentNullException("Cannot create a scene loader with a null Scene Manager");
            _lifetimeTokenSource = new CancellationTokenSource();
        }

        public void Dispose()
        {
            _lifetimeTokenSource.Cancel();
            _lifetimeTokenSource.Dispose();
            _manager.Dispose();
        }

        public void TransitionToScenes(ILoadSceneInfo[] targetScenes, int setIndexActive, ILoadSceneInfo intermediateSceneInfo = null, Scene externalOriginScene = default)
        {
            TransitionToScenesAsync(targetScenes, setIndexActive, intermediateSceneInfo, externalOriginScene).AsTask().Forget(HandleFireAndForgetException);
        }

        public void TransitionToScene(ILoadSceneInfo targetSceneInfo, ILoadSceneInfo intermediateSceneInfo = default, Scene externalOriginScene = default)
        {
            TransitionToSceneAsync(targetSceneInfo, intermediateSceneInfo, externalOriginScene).AsTask().Forget(HandleFireAndForgetException);
        }

        public void UnloadScenes(ILoadSceneInfo[] sceneInfos)
        {
            UnloadScenesAsync(sceneInfos).AsTask().Forget(HandleFireAndForgetException);
        }

        public void UnloadScene(ILoadSceneInfo sceneInfo)
        {
            UnloadSceneAsync(sceneInfo).AsTask().Forget(HandleFireAndForgetException);
        }

        public void LoadScenes(ILoadSceneInfo[] sceneInfos, int setIndexActive = -1)
        {
            LoadScenesAsync(sceneInfos, setIndexActive, null).AsTask().Forget(HandleFireAndForgetException);
        }

        public void LoadScene(ILoadSceneInfo sceneInfo, bool setActive = false)
        {
            LoadSceneAsync(sceneInfo, setActive, null).AsTask().Forget(HandleFireAndForgetException);
        }

        public ValueTask<Scene[]> TransitionToScenesAsync(ILoadSceneInfo[] targetScenes, int setIndexActive, ILoadSceneInfo intermediateSceneReference = null, Scene externalOriginScene = default)
        {
            return intermediateSceneReference == null
                ? TransitionDirectlyAsync(targetScenes, setIndexActive, externalOriginScene, _lifetimeTokenSource.Token)
                : TransitionWithIntermediateAsync(targetScenes, setIndexActive, intermediateSceneReference, externalOriginScene, _lifetimeTokenSource.Token);
        }

        public async ValueTask<Scene> TransitionToSceneAsync(ILoadSceneInfo targetSceneInfo, ILoadSceneInfo intermediateSceneInfo = default, Scene externalOriginScene = default)
        {
            var result = await TransitionToScenesAsync(new ILoadSceneInfo[] { targetSceneInfo }, 0, intermediateSceneInfo, externalOriginScene);
            return result == null || result.Length == 0 ? default : result[0];
        }

        public ValueTask<Scene[]> LoadScenesAsync(ILoadSceneInfo[] sceneReferences, int setIndexActive = -1, IProgress<float> progress = null)
        {
            return _manager.LoadScenesAsync(sceneReferences, setIndexActive, progress, _lifetimeTokenSource.Token);
        }

        public ValueTask<Scene> LoadSceneAsync(ILoadSceneInfo sceneInfo, bool setActive = false, IProgress<float> progress = null)
        {
            return _manager.LoadSceneAsync(sceneInfo, setActive, progress, _lifetimeTokenSource.Token);
        }

        public ValueTask<Scene[]> UnloadScenesAsync(ILoadSceneInfo[] sceneReferences)
        {
            return _manager.UnloadScenesAsync(sceneReferences, _lifetimeTokenSource.Token);
        }

        public ValueTask<Scene> UnloadSceneAsync(ILoadSceneInfo sceneInfo)
        {
            return _manager.UnloadSceneAsync(sceneInfo, _lifetimeTokenSource.Token);
        }

        async ValueTask<Scene[]> TransitionDirectlyAsync(ILoadSceneInfo[] targetScenes, int setIndexActive, Scene externalOriginScene, CancellationToken token)
        {
            bool fromExternalOrigin = externalOriginScene.IsValid();
            Scene sourceScene = fromExternalOrigin ? externalOriginScene : _manager.GetActiveScene();
            if (sourceScene.IsValid())
            {
                await UnloadCurrentScene(sourceScene, fromExternalOrigin, token);
            }
            return await _manager.LoadScenesAsync(targetScenes, setIndexActive, token: token);
        }

        async ValueTask<Scene[]> TransitionWithIntermediateAsync(ILoadSceneInfo[] targetScenes, int setIndexActive, ILoadSceneInfo intermediateSceneInfo, Scene externalOriginScene, CancellationToken token)
        {
            bool fromExternalOrigin = externalOriginScene.IsValid();

            Scene loadingScene;
            try
            {
                loadingScene = await _manager.LoadSceneAsync(intermediateSceneInfo, token: token);
            }
            catch
            {
                throw;
            }

            intermediateSceneInfo = new LoadSceneInfoScene(loadingScene);

            Scene sourceScene = fromExternalOrigin ? externalOriginScene : _manager.GetActiveScene();

#if UNITY_2023_2_OR_NEWER
            LoadingBehavior loadingBehavior = UnityEngine.Object.FindObjectsByType<LoadingBehavior>(FindObjectsSortMode.None).FirstOrDefault(l => l.gameObject.scene == loadingScene);
#else
            LoadingBehavior loadingBehavior = UnityEngine.Object.FindObjectsOfType<LoadingBehavior>().FirstOrDefault(l => l.gameObject.scene == loadingScene);
#endif
            return loadingBehavior
                ? await TransitionWithIntermediateLoadingAsync(targetScenes, setIndexActive, intermediateSceneInfo, loadingBehavior, sourceScene, fromExternalOrigin, token)
                : await TransitionWithIntermediateNoLoadingAsync(targetScenes, setIndexActive, intermediateSceneInfo, sourceScene, fromExternalOrigin, token);
        }

        async ValueTask<Scene[]> TransitionWithIntermediateLoadingAsync(ILoadSceneInfo[] targetScenes, int setIndexActive, ILoadSceneInfo intermediateSceneInfo, LoadingBehavior loadingBehavior, Scene sourceScene, bool fromExternalOrigin, CancellationToken token)
        {
            LoadingProgress progress = loadingBehavior.Progress;
            while (progress.State != LoadingState.Loading && !token.IsCancellationRequested)
                await Task.Yield();

            token.ThrowIfCancellationRequested();

            if (!fromExternalOrigin)
                sourceScene = _manager.GetActiveScene();

            if (sourceScene.IsValid())
            {
                await UnloadCurrentScene(sourceScene, fromExternalOrigin, token);
            }

            Scene[] loadedScenes = await _manager.LoadScenesAsync(targetScenes, setIndexActive, progress, token);
            progress.SetState(LoadingState.TargetSceneLoaded);

            while (progress.State != LoadingState.TransitionComplete && !token.IsCancellationRequested)
                await Task.Yield();

            token.ThrowIfCancellationRequested();

            _manager.UnloadSceneAsync(intermediateSceneInfo, token).Forget(HandleFireAndForgetException);
            return loadedScenes;
        }

        async ValueTask<Scene[]> TransitionWithIntermediateNoLoadingAsync(ILoadSceneInfo[] targetScenes, int setIndexActive, ILoadSceneInfo intermediateSceneInfo, Scene sourceScene, bool fromExternalOrigin, CancellationToken token)
        {
            if (sourceScene.IsValid())
            {
                await UnloadCurrentScene(sourceScene, fromExternalOrigin, token);
            }
            Scene[] loadedScenes = await _manager.LoadScenesAsync(targetScenes, setIndexActive, token: token);
            _manager.UnloadSceneAsync(intermediateSceneInfo, token).Forget(HandleFireAndForgetException);
            return loadedScenes;
        }

        async ValueTask UnloadCurrentScene(Scene currentScene, bool fromExternalOrigin, CancellationToken token)
        {
            if (!currentScene.IsValid())
                return;

            if (fromExternalOrigin)
            {
                AsyncOperation operation = SceneManager.UnloadSceneAsync(currentScene);
                while (operation != null && !operation.isDone)
                    await Task.Yield();

                token.ThrowIfCancellationRequested();
            }
            else
                await _manager.UnloadSceneAsync(new LoadSceneInfoScene(currentScene), token);
        }

        void HandleFireAndForgetException(Exception exception)
        {
            Debug.LogWarningFormat("[{0}] An exception was caught during a fire and forget task. Usually this can be caused due to internal task cancellation on exiting playmode. If that's not the case, investigate for issues on the async scene operations. Exception:\n{1}", GetType().Name, exception);
        }

        public override string ToString()
        {
            return $"Scene Loader [Async] with {_manager.GetType().Name}";
        }
    }
}