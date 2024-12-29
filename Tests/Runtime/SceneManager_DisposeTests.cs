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
        public IEnumerator Dispose_DuringLoad([ValueSource(nameof(_sceneManagerCreateFuncs))] Func<ISceneManager> managerCreateFunc, [ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.SceneParametersList))] SceneParameters sceneParameters)
        {
            ISceneManager manager = managerCreateFunc();
            WaitTask<SceneResult> waitTask = new(manager.LoadAsync(sceneParameters).AsTask());
            manager.Dispose();
            yield return waitTask;
            Assert.True(waitTask.Task.IsCanceled);
        }

        [UnityTest]
        public IEnumerator Dipose_DuringUnload([ValueSource(nameof(_sceneManagerCreateFuncs))] Func<ISceneManager> managerCreateFunc, [ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.SceneParametersList))] SceneParameters sceneParameters)
        {
            ISceneManager manager = managerCreateFunc();
            WaitTask<SceneResult> waitTask = new(manager.LoadAsync(sceneParameters).AsTask());
            yield return waitTask;

            waitTask = new(manager.UnloadAsync(sceneParameters).AsTask());
            manager.Dispose();
            yield return waitTask;
            Assert.True(waitTask.Task.IsCanceled);
        }

        [UnityTest]
        public IEnumerator Dispose_DuringTransition([ValueSource(nameof(_sceneManagerCreateFuncs))] Func<ISceneManager> managerCreateFunc, [ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.TransitionSceneParametersList))] SceneParameters sceneParameters, [ValueSource(typeof(SceneManagerTests), nameof(SceneManagerTests.LoadingSceneInfos))] ILoadSceneInfo loadingScene)
        {
            async Awaitable Test()
            {
                ISceneManager manager = managerCreateFunc();
                await SceneManagerTests.LoadFirstScene(manager).ValueTask;

                var task = manager.TransitionAsync(sceneParameters, loadingScene);
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