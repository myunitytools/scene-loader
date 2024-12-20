using NUnit.Framework;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace MyGameDevTools.SceneLoading.Tests
{
    public class SceneManager_DisposeTests : SceneTestBase
    {
        // Note: These functions must create new scene managers to correctly test the dispose flow
        static readonly Func<ISceneManager>[] _sceneManagerCreateFuncs = new Func<ISceneManager>[]
        {
            () => new AdvancedSceneManager(),
        };

        [Test]
        public void Dispose_Simple([ValueSource(nameof(_sceneManagerCreateFuncs))] Func<ISceneManager> managerCreateFunc)
        {
            ISceneManager manager = managerCreateFunc();
            Assert.DoesNotThrow(manager.Dispose);
        }

        [UnityTest]
        public IEnumerator Dispose_DuringLoadScene([ValueSource(nameof(_sceneManagerCreateFuncs))] Func<ISceneManager> managerCreateFunc, [ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.SingleLoadSceneInfoList))] ILoadSceneInfo sceneInfo)
        {
            ISceneManager manager = managerCreateFunc();
            WaitTask<SceneResult> waitTask = new(manager.LoadAsync(new SceneParameters(sceneInfo)).AsTask());
            manager.Dispose();
            yield return waitTask;
            Assert.That(waitTask.Task.IsCompleted);
        }

        [UnityTest]
        public IEnumerator Dispose_DuringLoadScenes([ValueSource(nameof(_sceneManagerCreateFuncs))] Func<ISceneManager> managerCreateFunc, [ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.MultipleLoadSceneInfoList))] ILoadSceneInfo[] sceneInfos)
        {
            ISceneManager manager = managerCreateFunc();
            WaitTask<SceneResult> waitTask = new(manager.LoadAsync(new SceneParameters(sceneInfos)).AsTask());
            manager.Dispose();
            yield return waitTask;
            Assert.True(waitTask.Task.IsCanceled);
        }

        [UnityTest]
        public IEnumerator Dipose_DuringUnloadScene([ValueSource(nameof(_sceneManagerCreateFuncs))] Func<ISceneManager> managerCreateFunc, [ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.SingleLoadSceneInfoList))] ILoadSceneInfo sceneInfo)
        {
            ISceneManager manager = managerCreateFunc();
            SceneParameters parameters = new SceneParameters(sceneInfo);
            WaitTask<SceneResult> waitTask = new(manager.LoadAsync(parameters).AsTask());
            yield return waitTask;

            waitTask = new(manager.UnloadAsync(parameters).AsTask());
            manager.Dispose();
            yield return waitTask;
            Assert.True(waitTask.Task.IsCanceled);
        }

        [UnityTest]
        public IEnumerator Dipose_DuringUnloadScenes([ValueSource(nameof(_sceneManagerCreateFuncs))] Func<ISceneManager> managerCreateFunc, [ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.MultipleLoadSceneInfoList))] ILoadSceneInfo[] sceneInfos)
        {
            ISceneManager manager = managerCreateFunc();
            SceneParameters parameters = new SceneParameters(sceneInfos);
            WaitTask<SceneResult> waitTask = new(manager.LoadAsync(parameters).AsTask());
            yield return waitTask;

            waitTask = new(manager.UnloadAsync(parameters).AsTask());
            manager.Dispose();
            yield return waitTask;
            Assert.True(waitTask.Task.IsCanceled);
        }

        [UnityTest]
        public IEnumerator Dispose_DuringTransitionToSceneAsync([ValueSource(nameof(_sceneManagerCreateFuncs))] Func<ISceneManager> managerCreateFunc, [ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.SingleLoadSceneInfoList))] ILoadSceneInfo targetScene, [ValueSource(typeof(SceneManagerTests), nameof(SceneManagerTests.LoadingSceneInfos))] ILoadSceneInfo loadingScene)
        {
            async Awaitable Test()
            {
                ISceneManager manager = managerCreateFunc();
                await SceneManagerTests.LoadFirstScene(manager).Task;

                var task = manager.TransitionAsync(new SceneParameters(targetScene), loadingScene);
                manager.Dispose();

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
            }
            return Test();
        }

        [UnityTest]
        public IEnumerator Dispose_DuringTransitionToScenesAsync([ValueSource(nameof(_sceneManagerCreateFuncs))] Func<ISceneManager> managerCreateFunc, [ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.MultipleLoadSceneInfoList))] ILoadSceneInfo[] targetScenes, [ValueSource(typeof(SceneManagerTests), nameof(SceneManagerTests.LoadingSceneInfos))] ILoadSceneInfo loadingScene)
        {
            async Awaitable Test()
            {
                ISceneManager manager = managerCreateFunc();
                await SceneManagerTests.LoadFirstScene(manager).Task;

                var task = manager.TransitionAsync(new SceneParameters(targetScenes, 0), loadingScene);
                manager.Dispose();

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
            }
            return Test();
        }
    }
}