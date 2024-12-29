using NUnit.Framework;
using System.Collections;
using System.Threading;
using UnityEngine.TestTools;

namespace MyGameDevTools.SceneLoading.Tests
{
    public class SceneManager_CancellationTests : SceneTestBase
    {
        [UnityTest]
        public IEnumerator Cancellation_DuringLoad([ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.SceneManagers))] ISceneManager manager, [ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.SceneParametersList))] SceneParameters sceneParameters)
        {
            CancellationTokenSource tokenSource = new();
            WaitTask<SceneResult> waitTask = new(manager.LoadAsync(sceneParameters, token: tokenSource.Token).AsTask());
            tokenSource.Cancel();
            yield return waitTask;
            Assert.True(waitTask.Task.IsCanceled);
            tokenSource.Dispose();
        }

        [UnityTest]
        public IEnumerator Cancellation_DuringUnload([ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.SceneManagers))] ISceneManager manager, [ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.SceneParametersList))] SceneParameters sceneParameters)
        {
            CancellationTokenSource tokenSource = new();
            WaitTask<SceneResult> waitTask = new(manager.LoadAsync(sceneParameters).AsTask());
            yield return waitTask;

            waitTask = new(manager.UnloadAsync(sceneParameters, token: tokenSource.Token).AsTask());
            tokenSource.Cancel();
            yield return waitTask;
            Assert.True(waitTask.Task.IsCanceled);
            tokenSource.Dispose();
        }
    }
}