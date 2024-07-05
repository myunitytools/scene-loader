#if ENABLE_UNITASK
using Cysharp.Threading.Tasks;
#endif
using NUnit.Framework;
using System;
using System.Collections;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace MyGameDevTools.SceneLoading.Tests
{
    public class SceneLoader_DisposeTests : SceneTestBase
    {
        // Note: These functions must create new scene managers to correctly test the dispose flow
        static readonly Func<ISceneLoader>[] _sceneLoaderCreateFuncs = new Func<ISceneLoader>[]
        {
            () => new SceneLoaderAsync(new AdvancedSceneManager()),
#if ENABLE_UNITASK
            () => new SceneLoaderUniTask(new AdvancedSceneManager()),
#endif
            () => new SceneLoaderCoroutine(new AdvancedSceneManager()),
        };

        [UnityTest]
        public IEnumerator Dispose_DuringTransitionToScene([ValueSource(nameof(_sceneLoaderCreateFuncs))] Func<ISceneLoader> loaderCreateFunc, [ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.SingleLoadSceneInfoList))] ILoadSceneInfo targetScene, [ValueSource(typeof(SceneLoaderTests), nameof(SceneLoaderTests.LoadingSceneInfos))] ILoadSceneInfo loadingScene)
        {
            ISceneLoader loader = loaderCreateFunc();
            yield return SceneLoaderTests.LoadFirstScene(loader);

            loader.TransitionToScene(targetScene, loadingScene);
            Assert.DoesNotThrow(loader.Dispose);
            yield return null;
        }

        [UnityTest]
        public IEnumerator Dispose_DuringTransitionToScenes([ValueSource(nameof(_sceneLoaderCreateFuncs))] Func<ISceneLoader> loaderCreateFunc, [ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.MultipleLoadSceneInfoList))] ILoadSceneInfo[] targetScenes, [ValueSource(typeof(SceneLoaderTests), nameof(SceneLoaderTests.LoadingSceneInfos))] ILoadSceneInfo loadingScene)
        {
            ISceneLoader loader = loaderCreateFunc();
            yield return SceneLoaderTests.LoadFirstScene(loader);

            loader.TransitionToScenes(targetScenes, 0, loadingScene);
            Assert.DoesNotThrow(loader.Dispose);
            yield return null;
        }

#if ENABLE_UNITASK
        [UnityTest]
        public IEnumerator Dispose_DuringTransitionToSceneAsync([ValueSource(nameof(_sceneLoaderCreateFuncs))] Func<ISceneLoader> loaderCreateFunc, [ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.SingleLoadSceneInfoList))] ILoadSceneInfo targetScene, [ValueSource(typeof(SceneLoaderTests), nameof(SceneLoaderTests.LoadingSceneInfos))] ILoadSceneInfo loadingScene) => UniTask.ToCoroutine(async () =>
        {
            ISceneLoader loader = loaderCreateFunc();
            await SceneLoaderTests.LoadFirstScene(loader);

            SceneLoaderType type = GetSceneLoaderType(loader);

            UniTask<Scene> task = default;
            task = type switch
            {
                SceneLoaderType.Async => ((ISceneLoaderAsync)loader).TransitionToSceneAsync(targetScene, loadingScene).AsTask().AsUniTask(),
                SceneLoaderType.UniTask => ((ISceneLoaderUniTask)loader).TransitionToSceneAsync(targetScene, loadingScene),
                SceneLoaderType.Coroutine => ((ISceneLoaderCoroutine)loader).TransitionToSceneAsync(targetScene, loadingScene).Task.AsUniTask(),
                _ => throw new NotImplementedException($"Type {type} was not implemented"),
            };
            loader.Dispose();

            bool canceled = false;
            try
            {
                await task;
            }
            catch (OperationCanceledException)
            {
                canceled = true;
            }
            Assert.True(canceled);
        });

        [UnityTest]
        public IEnumerator Dispose_DuringTransitionToScenesAsync([ValueSource(nameof(_sceneLoaderCreateFuncs))] Func<ISceneLoader> loaderCreateFunc, [ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.MultipleLoadSceneInfoList))] ILoadSceneInfo[] targetScenes, [ValueSource(typeof(SceneLoaderTests), nameof(SceneLoaderTests.LoadingSceneInfos))] ILoadSceneInfo loadingScene) => UniTask.ToCoroutine(async () =>
        {
            ISceneLoader loader = loaderCreateFunc();
            await SceneLoaderTests.LoadFirstScene(loader);

            SceneLoaderType type = GetSceneLoaderType(loader);

            UniTask<Scene[]> task = default;
            task = type switch
            {
                SceneLoaderType.Async => ((ISceneLoaderAsync)loader).TransitionToScenesAsync(targetScenes, 0, loadingScene).AsTask().AsUniTask(),
                SceneLoaderType.UniTask => ((ISceneLoaderUniTask)loader).TransitionToScenesAsync(targetScenes, 0, loadingScene),
                SceneLoaderType.Coroutine => ((ISceneLoaderCoroutine)loader).TransitionToScenesAsync(targetScenes, 0, loadingScene).Task.AsUniTask(),
                _ => throw new NotImplementedException($"Type {type} was not implemented"),
            };
            loader.Dispose();

            bool canceled = false;
            try
            {
                await task;
            }
            catch (OperationCanceledException)
            {
                canceled = true;
            }
            Assert.True(canceled);
        });
#endif

        SceneLoaderType GetSceneLoaderType(ISceneLoader loader)
        {
            if (loader is SceneLoaderAsync)
                return SceneLoaderType.Async;
#if ENABLE_UNITASK
            else if (loader is SceneLoaderUniTask)
                return SceneLoaderType.UniTask;
#endif
            else if (loader is SceneLoaderCoroutine)
                return SceneLoaderType.Coroutine;

            throw new Exception("Unexpected ISceneLoader type");
        }
    }
}