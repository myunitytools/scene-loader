#if ENABLE_UNITASK
#endif
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace MyGameDevTools.SceneLoading.Tests
{
    enum SceneLoaderType
    {
        Async,
#if ENABLE_UNITASK
        UniTask,
#endif
        Coroutine,
    }

    public class SceneLoaderTests : SceneTestBase
    {
        // Here we don't need to test multiple load scene infos, since that is already done
        // in the scene manager tests.
        public static readonly ILoadSceneInfo[] LoadingSceneInfos = new ILoadSceneInfo[]
        {
            null,
            new LoadSceneInfoName(SceneBuilder.SceneNames[3]),
            new LoadSceneInfoName(SceneBuilder.SceneNames[0]),
        };

        static readonly ISceneLoader[] _sceneLoaders = new ISceneLoader[]
        {
            new SceneLoaderAsync(SceneTestEnvironment.SceneManagers[0]),
#if ENABLE_UNITASK
            new SceneLoaderUniTask(SceneTestEnvironment.SceneManagers[0]),
#endif
            new SceneLoaderCoroutine(SceneTestEnvironment.SceneManagers[0]),
        };

        [UnityTest]
        public IEnumerator TransitionToScenes([ValueSource(nameof(_sceneLoaders))] ISceneLoader sceneLoader, [ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.MultipleLoadSceneInfoList))] ILoadSceneInfo[] targetScenes, [ValueSource(nameof(LoadingSceneInfos))] ILoadSceneInfo loadingScene)
        {
            yield return LoadFirstScene(sceneLoader);

            int sceneCount = targetScenes.Length;
            if (loadingScene != null)
                sceneCount++;

            var loadedScenes = new List<Scene>(sceneCount);

            sceneLoader.Manager.SceneLoaded += sceneLoaded;
            sceneLoader.TransitionToScenes(targetScenes, 0, loadingScene);

            var watch = new Stopwatch();
            watch.Start();
            yield return new WaitUntil(() => loadedScenes.Count == sceneCount || watch.ElapsedMilliseconds > SceneTestEnvironment.DefaultTimeout);
            watch.Stop();

            sceneLoader.Manager.SceneLoaded -= sceneLoaded;

            Assert.AreEqual(sceneCount, loadedScenes.Count);

            yield return new WaitUntil(() => sceneLoader.Manager.TotalSceneCount == sceneCount);

            void sceneLoaded(Scene scene)
            {
                loadedScenes.Add(scene);
            }
        }

        [UnityTest]
        public IEnumerator TransitionToScenesFromScenes([ValueSource(nameof(_sceneLoaders))] ISceneLoader sceneLoader, [ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.MultipleLoadSceneInfoList))] ILoadSceneInfo[] targetScenes, [ValueSource(nameof(LoadingSceneInfos))] ILoadSceneInfo loadingScene)
        {
            int sceneCount = targetScenes.Length;

            var loadedScenes = new List<Scene>(sceneCount);

            sceneLoader.Manager.SceneLoaded += sceneLoaded;
            sceneLoader.LoadScenes(targetScenes);

            var watch = new Stopwatch();
            watch.Start();
            yield return new WaitUntil(() => loadedScenes.Count == sceneCount || watch.ElapsedMilliseconds > SceneTestEnvironment.DefaultTimeout);
            watch.Stop();

            sceneCount += loadingScene == null ? 0 : 1;

            var unloadedScenes = new List<Scene>(sceneCount);
            sceneLoader.Manager.SceneUnloaded += sceneUnloaded;

            loadedScenes.Clear();
            sceneLoader.TransitionToScenesFromScenes(targetScenes, targetScenes, 0, loadingScene);

            watch.Restart();
            yield return new WaitUntil(() => (loadedScenes.Count == sceneCount && unloadedScenes.Count == sceneCount) || watch.ElapsedMilliseconds > SceneTestEnvironment.DefaultTimeout);
            watch.Stop();

            sceneLoader.Manager.SceneLoaded -= sceneLoaded;
            sceneLoader.Manager.SceneUnloaded -= sceneUnloaded;

            yield return new WaitUntil(() => sceneLoader.Manager.TotalSceneCount == targetScenes.Length);

            Assert.AreEqual(sceneCount, loadedScenes.Count);
            Assert.AreEqual(sceneCount, unloadedScenes.Count);

            void sceneLoaded(Scene scene)
            {
                loadedScenes.Add(scene);
            }

            void sceneUnloaded(Scene scene)
            {
                unloadedScenes.Add(scene);
            }
        }

        [UnityTest]
        public IEnumerator TransitionToScenesFromAll([ValueSource(nameof(_sceneLoaders))] ISceneLoader sceneLoader, [ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.MultipleLoadSceneInfoList))] ILoadSceneInfo[] targetScenes, [ValueSource(nameof(LoadingSceneInfos))] ILoadSceneInfo loadingScene)
        {
            int sceneCount = targetScenes.Length;

            var loadedScenes = new List<Scene>(sceneCount);

            sceneLoader.Manager.SceneLoaded += sceneLoaded;
            sceneLoader.LoadScenes(targetScenes);

            var watch = new Stopwatch();
            watch.Start();
            yield return new WaitUntil(() => loadedScenes.Count == sceneCount || watch.ElapsedMilliseconds > SceneTestEnvironment.DefaultTimeout);
            watch.Stop();

            sceneCount += loadingScene == null ? 0 : 1;

            var unloadedScenes = new List<Scene>(sceneCount);
            sceneLoader.Manager.SceneUnloaded += sceneUnloaded;

            loadedScenes.Clear();
            sceneLoader.TransitionToScenesFromAll(targetScenes, 0, loadingScene);

            watch.Restart();
            yield return new WaitUntil(() => (loadedScenes.Count == sceneCount && unloadedScenes.Count == sceneCount) || watch.ElapsedMilliseconds > SceneTestEnvironment.DefaultTimeout);
            watch.Stop();

            sceneLoader.Manager.SceneLoaded -= sceneLoaded;
            sceneLoader.Manager.SceneUnloaded -= sceneUnloaded;

            yield return new WaitUntil(() => sceneLoader.Manager.TotalSceneCount == targetScenes.Length);

            Assert.AreEqual(sceneCount, loadedScenes.Count);
            Assert.AreEqual(sceneCount, unloadedScenes.Count);

            void sceneLoaded(Scene scene)
            {
                loadedScenes.Add(scene);
            }

            void sceneUnloaded(Scene scene)
            {
                unloadedScenes.Add(scene);
            }
        }

        [UnityTest]
        public IEnumerator Transition([ValueSource(nameof(_sceneLoaders))] ISceneLoader sceneLoader, [ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.SingleLoadSceneInfoList))] ILoadSceneInfo targetScene, [ValueSource(nameof(LoadingSceneInfos))] ILoadSceneInfo loadingScene)
        {
            yield return LoadFirstScene(sceneLoader);

            List<Scene> loadedScenes = new();
            int expectedScenes = loadingScene == null ? 1 : 2;
            sceneLoader.Manager.SceneLoaded += sceneLoaded;

            sceneLoader.TransitionToScene(targetScene, loadingScene);

            var watch = new Stopwatch();
            watch.Start();
            yield return new WaitUntil(() => loadedScenes.Count == expectedScenes || watch.ElapsedMilliseconds > SceneTestEnvironment.DefaultTimeout);
            watch.Stop();

            sceneLoader.Manager.SceneLoaded -= sceneLoaded;

            Assert.AreEqual(loadedScenes[expectedScenes - 1], sceneLoader.Manager.GetActiveScene());

            yield return new WaitUntil(() => sceneLoader.Manager.TotalSceneCount == 1);

            void sceneLoaded(Scene scene)
            {
                loadedScenes.Add(scene);
            }
        }

        [UnityTest]
        public IEnumerator Transition_NoSourceScene([ValueSource(nameof(_sceneLoaders))] ISceneLoader sceneLoader, [ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.SingleLoadSceneInfoList))] ILoadSceneInfo targetScene, [ValueSource(nameof(LoadingSceneInfos))] ILoadSceneInfo loadingScene)
        {
            List<Scene> loadedScenes = new();
            List<Scene> unloadedScenes = new();
            int expectedLoadedScenes = loadingScene == null ? 1 : 2;
            // If there's no loading scene, the scene loader will create a temporary scene
            // for the transition, and will unload it after the transition is complete.
            int expectedUnloadedScenes = 1;

            sceneLoader.Manager.SceneLoaded += sceneLoaded;
            // The temporary scene unload does not go through the ISceneManager
            SceneManager.sceneUnloaded += sceneUnloaded;

            sceneLoader.TransitionToScene(targetScene, loadingScene);

            var watch = new Stopwatch();
            watch.Start();
            yield return new WaitUntil(() => (loadedScenes.Count == expectedLoadedScenes && unloadedScenes.Count == expectedUnloadedScenes) || watch.ElapsedMilliseconds > SceneTestEnvironment.DefaultTimeout);
            watch.Stop();

            sceneLoader.Manager.SceneLoaded -= sceneLoaded;
            SceneManager.sceneUnloaded -= sceneUnloaded;

            Assert.AreEqual(loadedScenes[expectedLoadedScenes - 1], sceneLoader.Manager.GetActiveScene());
            Assert.AreEqual(expectedUnloadedScenes, unloadedScenes.Count);

            yield return new WaitUntil(() => sceneLoader.Manager.TotalSceneCount == 1);

            void sceneLoaded(Scene scene)
            {
                loadedScenes.Add(scene);
            }

            void sceneUnloaded(Scene scene)
            {
                unloadedScenes.Add(scene);
            }
        }

        /// <summary>
        /// Required to test transition scenarios, otherwise the initial (test) scene would be unloaded and stop the tests.
        /// </summary>
        public static IEnumerator LoadFirstScene(ISceneLoader sceneLoader)
        {
            sceneLoader.Manager.SceneLoaded += sceneLoaded;
            sceneLoader.LoadScene(new LoadSceneInfoName(SceneBuilder.SceneNames[1]), true);
            Scene loadedScene = default;

            var watch = new Stopwatch();
            watch.Start();
            yield return new WaitUntil(() => loadedScene.IsValid() && loadedScene.isLoaded || watch.ElapsedMilliseconds > SceneTestEnvironment.DefaultTimeout);
            watch.Stop();

            sceneLoader.Manager.SceneLoaded -= sceneLoaded;

            Assert.AreEqual(loadedScene.name, SceneBuilder.SceneNames[1]);
            Assert.AreEqual(loadedScene, SceneManager.GetActiveScene());

            void sceneLoaded(Scene scene)
            {
                loadedScene = scene;
            }
        }
    }
}