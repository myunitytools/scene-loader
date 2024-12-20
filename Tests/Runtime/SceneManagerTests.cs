using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace MyGameDevTools.SceneLoading.Tests
{
    public partial class SceneManagerTests : SceneTestBase
    {
        public static readonly ILoadSceneInfo[] LoadingSceneInfos = new ILoadSceneInfo[]
        {
            null,
            new LoadSceneInfoName(SceneBuilder.SceneNames[3]),
            new LoadSceneInfoName(SceneBuilder.SceneNames[0]),
        };

        static readonly bool[] _setActiveParameterValues = new bool[] { true, false };

        static readonly int[] _setIndexActiveParameterValues = new int[] { -1, 1 };

        int _scenesActivated;
        int _scenesUnloaded;
        int _scenesLoaded;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            SceneTestEnvironment.ValidateSceneEnvironment();

            ISceneManager[] sceneManagers = SceneTestEnvironment.SceneManagers;
            for (int i = 0; i < sceneManagers.Length; i++)
            {
                var manager = sceneManagers[i];
                manager.ActiveSceneChanged += ReportSceneActivation;
                manager.SceneUnloaded += ReportSceneUnloaded;
                manager.SceneLoaded += ReportSceneLoaded;
            }
        }

        [OneTimeTearDown]
        public void OneTimeTeardown()
        {
            ISceneManager[] sceneManagers = SceneTestEnvironment.SceneManagers;
            for (int i = 0; i < sceneManagers.Length; i++)
            {
                var manager = sceneManagers[i];
                manager.ActiveSceneChanged -= ReportSceneActivation;
                manager.SceneUnloaded -= ReportSceneUnloaded;
                manager.SceneLoaded -= ReportSceneLoaded;
            }
        }

        [SetUp]
        public void SetUp()
        {
            _scenesActivated = 0;
            _scenesUnloaded = 0;
            _scenesLoaded = 0;
        }

        [UnityTest]
        public IEnumerator Constructor_AddLoadedScenes()
        {
            yield return SceneManager.LoadSceneAsync(SceneBuilder.SceneNames[1], LoadSceneMode.Additive);

            ISceneManager sceneManager = new AdvancedSceneManager(true);

            Assert.AreEqual(2, sceneManager.LoadedSceneCount);
            Assert.AreEqual(sceneManager.TotalSceneCount, sceneManager.LoadedSceneCount);
        }

        [UnityTest]
        public IEnumerator Constructor_InitializationScenes()
        {
            yield return SceneManager.LoadSceneAsync(SceneBuilder.SceneNames[1], LoadSceneMode.Additive);
            Scene loadedScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);

            ISceneManager sceneManager = new AdvancedSceneManager(new Scene[] { loadedScene });

            Assert.AreEqual(1, sceneManager.LoadedSceneCount);
            Assert.AreEqual(sceneManager.TotalSceneCount, sceneManager.LoadedSceneCount);
        }

        [UnityTest]
        public IEnumerator InitializationScene_Unload()
        {
            yield return SceneManager.LoadSceneAsync(SceneBuilder.SceneNames[1], LoadSceneMode.Additive);
            Scene loadedScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);

            ISceneManager sceneManager = new AdvancedSceneManager(new Scene[] { loadedScene });

            WaitTask<SceneResult> waitTask = default;
            Assert.DoesNotThrow(() => waitTask = new(sceneManager.UnloadAsync(new SceneParameters(new LoadSceneInfoScene(loadedScene))).AsTask()));

            yield return waitTask;
        }

        [UnityTest]
        public IEnumerator SetActive_NotThroughManager([ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.SceneManagers))] ISceneManager manager)
        {
            Scene loadedScene = default;
            SceneManager.sceneLoaded += assignLoadedScene;

            yield return SceneManager.LoadSceneAsync(SceneBuilder.SceneNames[1], LoadSceneMode.Additive);
            yield return new WaitUntil(() => loadedScene.IsValid());

            Assert.Throws<InvalidOperationException>(() => manager.SetActiveScene(loadedScene));

            yield return SceneManager.UnloadSceneAsync(loadedScene);

            void assignLoadedScene(Scene scene, LoadSceneMode loadSceneMode)
            {
                SceneManager.sceneLoaded -= assignLoadedScene;
                loadedScene = scene;
            }
        }

        [Test]
        public void GetActiveScene_Empty([ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.SceneManagers))] ISceneManager manager)
        {
            Assert.False(manager.GetActiveScene().IsValid());
        }

        [UnityTest]
        public IEnumerator GetActiveScene_Valid([ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.SceneManagers))] ISceneManager manager)
        {
            var loadTask = manager.LoadAsync(new SceneParameters(new LoadSceneInfoName(SceneBuilder.SceneNames[1]), true)).AsTask();

            yield return new WaitTask<SceneResult>(loadTask);

            Scene loadedScene = loadTask.Result;
            var managerActiveScene = manager.GetActiveScene();

            Assert.True(loadedScene.IsValid());
            Assert.True(managerActiveScene.IsValid());
            Assert.AreEqual(loadedScene, managerActiveScene);
        }

        [Test]
        public void GetLoadedSceneByName_Invalid([ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.SceneManagers))] ISceneManager manager)
        {
            Assert.Throws<ArgumentException>(() => manager.GetLoadedSceneByName("not-a-real-scene"));
        }

        [UnityTest]
        public IEnumerator GetLoadedSceneByName_Valid([ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.SceneManagers))] ISceneManager manager)
        {
            yield return new WaitTask<SceneResult>(manager.LoadAsync(new SceneParameters(new LoadSceneInfoName(SceneBuilder.SceneNames[1]))).AsTask());

            Assert.True(manager.GetLoadedSceneByName(SceneBuilder.SceneNames[1]).IsValid());
        }

        [Test]
        public void EmptyState([ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.SceneManagers))] ISceneManager manager)
        {
            Assert.False(manager.GetLastLoadedScene().IsValid());
            Assert.False(manager.GetActiveScene().IsValid());
        }

        [Test]
        public void GetLoadedSceneAt_IndexError([ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.SceneManagers))] ISceneManager manager)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => manager.GetLoadedSceneAt(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => manager.GetLoadedSceneAt(1));
        }

        [UnityTest]
        public IEnumerator LoadScene([ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.SceneManagers))] ISceneManager manager, [ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.SingleLoadSceneInfoList))] ILoadSceneInfo sceneInfo, [ValueSource(nameof(_setActiveParameterValues))] bool setActive)
        {
            var loadTask = manager.LoadAsync(new SceneParameters(sceneInfo, setActive)).AsTask();

            Scene eventScene = default;
            manager.SceneLoaded += setEventScene;

            yield return new WaitTask<SceneResult>(loadTask);

            manager.SceneLoaded -= setEventScene;
            Scene loadedScene = loadTask.Result;

            Assert.AreEqual(1, manager.LoadedSceneCount);
            Assert.That(setActive ? loadedScene == manager.GetActiveScene() : loadedScene != manager.GetActiveScene());
            Assert.AreEqual(loadedScene, manager.GetLastLoadedScene());
            Assert.AreEqual(loadedScene, manager.GetLoadedSceneByName(loadedScene.name));
            Assert.AreEqual(loadedScene, eventScene);
            Assert.AreEqual(1, _scenesLoaded);
            Assert.AreEqual(0, _scenesUnloaded);
            Assert.AreEqual(setActive ? 1 : 0, _scenesActivated);

            void setEventScene(Scene scene) => eventScene = scene;
        }

        [UnityTest]
        public IEnumerator LoadScenes([ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.SceneManagers))] ISceneManager manager, [ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.MultipleLoadSceneInfoList))] ILoadSceneInfo[] sceneInfos, [ValueSource(nameof(_setIndexActiveParameterValues))] int setIndexActive)
        {
            int scenesToLoad = sceneInfos.Length;
            var reportedScenes = new List<Scene>(scenesToLoad);
            manager.SceneLoaded += reportSceneLoaded;

            var progress = new SimpleProgress();
            var loadTask = manager.LoadAsync(new SceneParameters(sceneInfos, setIndexActive), progress).AsTask();

            Assert.AreEqual(0, progress.Value);

            yield return new WaitTask<SceneResult>(loadTask);

            manager.SceneLoaded -= reportSceneLoaded;
            Scene[] loadedScenes = loadTask.Result;

            Assert.AreEqual(1, progress.Value);
            Assert.AreEqual(scenesToLoad, loadedScenes.Length);
            Assert.AreEqual(scenesToLoad, reportedScenes.Count);
            Assert.AreEqual(scenesToLoad, manager.LoadedSceneCount);
            if (setIndexActive >= 0)
                Assert.AreEqual(manager.GetActiveScene(), loadedScenes[setIndexActive]);
            Assert.AreEqual(scenesToLoad, _scenesLoaded);
            Assert.AreEqual(0, _scenesUnloaded);
            Assert.AreEqual(setIndexActive >= 0 ? 1 : 0, _scenesActivated);

            void reportSceneLoaded(Scene loadedScene) => reportedScenes.Add(loadedScene);
        }

        [UnityTest]
        public IEnumerator LoadScene_Progress([ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.SceneManagers))] ISceneManager manager, [ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.SingleLoadSceneInfoList))] ILoadSceneInfo sceneInfo, [ValueSource(nameof(_setActiveParameterValues))] bool setActive)
        {
            var progress = new SimpleProgress();
            Assert.AreEqual(0, progress.Value);
            yield return new WaitTask<SceneResult>(manager.LoadAsync(new SceneParameters(sceneInfo, setActive), progress).AsTask());
            Assert.AreEqual(1, progress.Value);
        }

        [UnityTest]
        public IEnumerator LoadScene_Multiple([ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.SceneManagers))] ISceneManager manager, [ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.MultipleLoadSceneInfoList))] ILoadSceneInfo[] sceneInfos, [ValueSource(nameof(_setActiveParameterValues))] bool setActive)
        {
            var length = sceneInfos.Length;
            var loadedScenes = new Scene[length];

            for (int i = 0; i < length; i++)
            {
                var loadTask = manager.LoadAsync(new SceneParameters(sceneInfos[i], setActive)).AsTask();
                yield return new WaitTask<SceneResult>(loadTask);
                loadedScenes[i] = loadTask.Result;
            }

            Assert.AreEqual(length, manager.LoadedSceneCount);
            Assert.AreEqual(loadedScenes[^1], manager.GetLastLoadedScene());

            for (int i = 0; i < length; i++)
                Assert.AreEqual(loadedScenes[i], manager.GetLoadedSceneAt(i));

            Assert.That(setActive ? loadedScenes[^1] == manager.GetActiveScene() : loadedScenes[^1] != manager.GetActiveScene());
            Assert.AreEqual(length, _scenesLoaded);
            Assert.AreEqual(0, _scenesUnloaded);
            Assert.AreEqual(setActive ? length : 0, _scenesActivated);
        }

        [Test]
        public void LoadScene_NotInBuild([ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.SceneManagers))] ISceneManager manager)
        {
            var sceneName = "not-a-real-scene";
            if (manager is SceneManager || manager is AdvancedSceneManager)
                LogAssert.Expect(LogType.Error, new Regex("'not-a-real-scene' couldn't be loaded"));
            var wait = new WaitTask<SceneResult>(manager.LoadAsync(new SceneParameters(new LoadSceneInfoName(sceneName), false)).AsTask());
            Assert.Throws<AggregateException>(() => wait.MoveNext());
        }

        [UnityTest]
        public IEnumerator UnloadScene([ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.SceneManagers))] ISceneManager manager, [ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.SingleLoadSceneInfoList))] ILoadSceneInfo sceneInfo, [ValueSource(nameof(_setActiveParameterValues))] bool setActive)
        {
            yield return new WaitTask<SceneResult>(manager.LoadAsync(new SceneParameters(sceneInfo, setActive)).AsTask());

            Scene eventScene = default;
            manager.SceneUnloaded += setEventScene;

            var workingScene = manager.GetLastLoadedScene();

            var task = manager.UnloadAsync(new SceneParameters(new LoadSceneInfoScene(workingScene))).AsTask();
            yield return new WaitTask<SceneResult>(task);

            manager.SceneUnloaded -= setEventScene;
            Scene unloadedScene = task.Result;

            Assert.AreEqual(workingScene, unloadedScene);
            Assert.AreEqual(workingScene, eventScene);
            Assert.IsFalse(workingScene.isLoaded);
            Assert.IsFalse(manager.GetActiveScene().IsValid());
            Assert.AreEqual(0, manager.LoadedSceneCount);
            Assert.AreEqual(1, _scenesLoaded);
            Assert.AreEqual(1, _scenesUnloaded);
            Assert.AreEqual(setActive ? 2 : 0, _scenesActivated, "Activated scenes did not match expectation");

            void setEventScene(Scene scene) => eventScene = scene;
        }

        [UnityTest]
        public IEnumerator UnloadScenes([ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.SceneManagers))] ISceneManager manager, [ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.MultipleLoadSceneInfoList))] ILoadSceneInfo[] sceneInfos, [ValueSource(nameof(_setIndexActiveParameterValues))] int setIndexActive)
        {
            var loadTask = manager.LoadAsync(new SceneParameters(sceneInfos, setIndexActive)).AsTask();
            yield return new WaitTask<SceneResult>(loadTask);
            var loadedSceneHandles = loadTask.Result.GetScenes().Select(s => s.handle).ToArray();

            int scenesToUnload = sceneInfos.Length;
            var reportedScenes = new List<Scene>(scenesToUnload);
            manager.SceneUnloaded += reportSceneUnloaded;

            var task = manager.UnloadAsync(new SceneParameters(sceneInfos)).AsTask();
            yield return new WaitTask<SceneResult>(task);

            manager.SceneUnloaded -= reportSceneUnloaded;
            Scene[] unloadedScenes = task.Result;

            Assert.AreEqual(scenesToUnload, unloadedScenes.Length);
            Assert.AreEqual(scenesToUnload, reportedScenes.Count);
            Assert.AreEqual(0, manager.LoadedSceneCount);
            Assert.AreEqual(scenesToUnload, _scenesLoaded);
            Assert.AreEqual(scenesToUnload, _scenesUnloaded);
            Assert.AreEqual(setIndexActive >= 0 ? 2 : 0, _scenesActivated, "Activated scenes did not match expectation");

            for (int i = 0; i < scenesToUnload; i++)
                Assert.True(hasReference(loadedSceneHandles[i], reportedScenes));

            void reportSceneUnloaded(Scene loadedScene) => reportedScenes.Add(loadedScene);

            bool hasReference(int handle, List<Scene> scenes)
            {
                foreach (var scene in scenes)
                    if (scene.handle == handle)
                    {
                        scenes.Remove(scene);
                        return true;
                    }
                return false;
            }
        }

        [Test]
        public void UnloadScene_NotLoaded([ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.SceneManagers))] ISceneManager manager)
        {
            var sceneName = "not-a-real-scene";
            if (manager is not AdvancedSceneManager)
                LogAssert.Expect(LogType.Warning, new Regex("Some of the scenes could not be found loaded"));
            var wait = new WaitTask<SceneResult>(manager.UnloadAsync(new SceneParameters(new LoadSceneInfoName(sceneName))).AsTask());
            Assert.Throws<AggregateException>(() => wait.MoveNext());
        }

        [UnityTest]
        public IEnumerator TransitionToScenes([ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.SceneManagers))] ISceneManager manager, [ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.MultipleLoadSceneInfoList))] ILoadSceneInfo[] targetScenes, [ValueSource(nameof(LoadingSceneInfos))] ILoadSceneInfo loadingScene)
        {
            yield return LoadFirstScene(manager);

            int sceneCount = targetScenes.Length;

            var task = manager.TransitionAsync(new SceneParameters(targetScenes, 0), loadingScene).AsTask();

            yield return new WaitTask<SceneResult>(task);

            Scene[] loadedScenes = task.Result;
            Assert.AreEqual(sceneCount, loadedScenes.Length);

            yield return new WaitUntil(() => manager.TotalSceneCount == sceneCount);
        }

        [UnityTest]
        public IEnumerator Transition([ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.SceneManagers))] ISceneManager manager, [ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.SingleLoadSceneInfoList))] ILoadSceneInfo targetScene, [ValueSource(nameof(LoadingSceneInfos))] ILoadSceneInfo loadingScene)
        {
            yield return LoadFirstScene(manager);

            var task = manager.TransitionAsync(new SceneParameters(targetScene, true), loadingScene).AsTask();

            yield return new WaitTask<SceneResult>(task);

            Scene loadedScene = task.Result;
            Assert.AreEqual(loadedScene, manager.GetActiveScene());

            yield return new WaitUntil(() => manager.TotalSceneCount == 1);
        }

        [UnityTest]
        public IEnumerator Transition_NoSourceScene([ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.SceneManagers))] ISceneManager manager, [ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.SingleLoadSceneInfoList))] ILoadSceneInfo targetScene, [ValueSource(nameof(LoadingSceneInfos))] ILoadSceneInfo loadingScene)
        {
            int expectedLoadedScenes = loadingScene == null ? 1 : 2;
            // If there's no loading scene, the scene manager will create a temporary scene
            // for the transition, and will unload it after the transition is complete.
            int expectedUnloadedScenes = 1;

            int unloadedScenesCount = 0;

            // The temporary scene unload does not go through the ISceneManager
            SceneManager.sceneUnloaded += sceneUnloaded;

            var task = manager.TransitionAsync(new SceneParameters(targetScene, true), loadingScene).AsTask();

            yield return new WaitTask<SceneResult>(task);

            Scene loadedScene = task.Result;

            SceneManager.sceneUnloaded -= sceneUnloaded;

            Assert.AreEqual(loadedScene, manager.GetActiveScene());
            Assert.AreEqual(expectedLoadedScenes, _scenesLoaded);
            Assert.AreEqual(expectedUnloadedScenes, unloadedScenesCount);

            yield return new WaitUntil(() => manager.TotalSceneCount == 1);

            void sceneUnloaded(Scene scene)
            {
                unloadedScenesCount++;
            }
        }

        [UnityTest]
        public IEnumerator LoadByInfo_UnloadByScene([ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.SceneManagers))] ISceneManager manager, [ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.SingleLoadSceneInfoList))] ILoadSceneInfo sceneInfo)
        {
            var task = manager.LoadAsync(new SceneParameters(sceneInfo)).AsTask();

            yield return new WaitTask<SceneResult>(task);

            Scene scene = task.Result;

            task = manager.UnloadAsync(new SceneParameters(new LoadSceneInfoScene(scene))).AsTask();

            yield return new WaitTask<SceneResult>(task);

            Assert.Zero(manager.LoadedSceneCount);
        }

        /// <summary>
        /// Required to test transition some scenarios.
        /// </summary>
        public static WaitTask<SceneResult> LoadFirstScene(ISceneManager sceneManager)
        {
            var task = sceneManager.LoadAsync(new SceneParameters(new LoadSceneInfoName(SceneBuilder.SceneNames[1]), true)).AsTask();
            return new WaitTask<SceneResult>(task);
        }

        void ReportSceneActivation(Scene previousScene, Scene newScene)
        {
            _scenesActivated++;
        }

        void ReportSceneUnloaded(Scene unloadedScene)
        {
            _scenesUnloaded++;
        }

        void ReportSceneLoaded(Scene loadedScene)
        {
            _scenesLoaded++;
        }
    }

    public class SimpleProgress : IProgress<float>
    {
        public float Value;

        public void Report(float value) => Value = value;
    }
}