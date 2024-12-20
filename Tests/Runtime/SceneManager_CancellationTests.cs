using NUnit.Framework;
using System.Collections;
using System.Threading;
using UnityEngine.TestTools;

namespace MyGameDevTools.SceneLoading.Tests
{
    public class SceneManager_CancellationTests : SceneTestBase
    {
        [UnityTest]
        public IEnumerator Cancellation_DuringLoadScene([ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.SceneManagers))] ISceneManager manager, [ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.SingleLoadSceneInfoList))] ILoadSceneInfo sceneInfo)
        {
            CancellationTokenSource tokenSource = new();
            WaitTask<SceneResult> waitTask = new(manager.LoadAsync(new SceneParameters(sceneInfo), token: tokenSource.Token).AsTask());
            tokenSource.Cancel();
            yield return waitTask;
            Assert.That(waitTask.Task.IsCompleted);
            tokenSource.Dispose();
        }

        [UnityTest]
        public IEnumerator Cancellation_DuringLoadScenes([ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.SceneManagers))] ISceneManager manager, [ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.MultipleLoadSceneInfoList))] ILoadSceneInfo[] sceneInfos)
        {
            CancellationTokenSource tokenSource = new();
            WaitTask<SceneResult> waitTask = new(manager.LoadAsync(new SceneParameters(sceneInfos), token: tokenSource.Token).AsTask());
            tokenSource.Cancel();
            yield return waitTask;
            Assert.True(waitTask.Task.IsCanceled);
            tokenSource.Dispose();
        }

        [UnityTest]
        public IEnumerator Cancellation_DuringUnloadScene([ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.SceneManagers))] ISceneManager manager, [ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.SingleLoadSceneInfoList))] ILoadSceneInfo sceneInfo)
        {
            CancellationTokenSource tokenSource = new();
            SceneParameters parameters = new SceneParameters(sceneInfo);
            WaitTask<SceneResult> waitTask = new(manager.LoadAsync(parameters).AsTask());
            yield return waitTask;

            waitTask = new(manager.UnloadAsync(parameters, token: tokenSource.Token).AsTask());
            tokenSource.Cancel();
            yield return waitTask;
            Assert.True(waitTask.Task.IsCanceled);
            tokenSource.Dispose();
        }

        [UnityTest]
        public IEnumerator Cancellation_DuringUnloadScenes([ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.SceneManagers))] ISceneManager manager, [ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.MultipleLoadSceneInfoList))] ILoadSceneInfo[] sceneInfos)
        {
            CancellationTokenSource tokenSource = new();
            SceneParameters parameters = new SceneParameters(sceneInfos);
            WaitTask<SceneResult> waitTask = new(manager.LoadAsync(parameters).AsTask());
            yield return waitTask;

            waitTask = new(manager.UnloadAsync(parameters, token: tokenSource.Token).AsTask());
            tokenSource.Cancel();
            yield return waitTask;
            Assert.True(waitTask.Task.IsCanceled);
            tokenSource.Dispose();
        }
    }
}