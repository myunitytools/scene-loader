using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
        public IEnumerator LoadScenes([ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.SceneManagers))] ISceneManager manager, [ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.SceneParametersList))] SceneParameters sceneParameters)
        {
            int scenesToLoad = sceneParameters.Length;
            int setIndexActive = sceneParameters.GetIndexToActivate();
            var reportedScenes = new List<Scene>(scenesToLoad);
            manager.SceneLoaded += reportSceneLoaded;

            var progress = new SimpleProgress();
            var loadTask = manager.LoadAsync(sceneParameters, progress);

            Assert.AreEqual(0, progress.Value);

            yield return new WaitValueTask<SceneResult>(loadTask);

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
        public IEnumerator LoadScene_Progress([ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.SceneManagers))] ISceneManager manager, [ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.SceneParametersList))] SceneParameters sceneParameters)
        {
            var progress = new SimpleProgress();
            Assert.AreEqual(0, progress.Value);
            yield return new WaitValueTask<SceneResult>(manager.LoadAsync(sceneParameters, progress));
            Assert.AreEqual(1, progress.Value);
        }

        [UnityTest]
        public IEnumerator LoadScene_Stress([ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.SceneManagers))] ISceneManager manager, [ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.SceneParametersList))] SceneParameters sceneParameters)
        {
            ILoadSceneInfo[] sceneInfos = sceneParameters.GetLoadSceneInfos();
            int length = sceneInfos.Length;
            bool setActive = sceneParameters.GetIndexToActivate() == 1;

            var loadedScenes = new Scene[length];

            for (int i = 0; i < length; i++)
            {
                var loadTask = manager.LoadAsync(new SceneParameters(sceneInfos[i], setActive));
                yield return new WaitValueTask<SceneResult>(loadTask);
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
            var wait = new WaitTask<SceneResult>(manager.LoadAsync(sceneName).AsTask());
            Assert.Throws<AggregateException>(() => wait.MoveNext());
        }

        [UnityTest]
        public IEnumerator UnloadScenes([ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.SceneManagers))] ISceneManager manager, [ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.SceneParametersList))] SceneParameters sceneParameters)
        {
            var loadTask = manager.LoadAsync(sceneParameters).AsTask();
            yield return new WaitTask<SceneResult>(loadTask);
            var loadedSceneHandles = loadTask.Result.GetScenes().Select(s => s.handle).ToArray();

            int scenesToUnload = sceneParameters.Length;
            var reportedScenes = new List<Scene>(scenesToUnload);
            manager.SceneUnloaded += reportSceneUnloaded;

            var task = manager.UnloadAsync(sceneParameters).AsTask();
            yield return new WaitTask<SceneResult>(task);

            manager.SceneUnloaded -= reportSceneUnloaded;
            Scene[] unloadedScenes = task.Result;

            Assert.AreEqual(scenesToUnload, unloadedScenes.Length);
            Assert.AreEqual(scenesToUnload, reportedScenes.Count);
            Assert.AreEqual(0, manager.LoadedSceneCount);
            Assert.AreEqual(scenesToUnload, _scenesLoaded);
            Assert.AreEqual(scenesToUnload, _scenesUnloaded);

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
            var wait = new WaitTask<SceneResult>(manager.UnloadAsync(sceneName).AsTask());
            Assert.Throws<AggregateException>(() => wait.MoveNext());
        }

        [UnityTest]
        public IEnumerator Transition([ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.SceneManagers))] ISceneManager manager, [ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.SceneParametersList))] SceneParameters sceneParameters, [ValueSource(nameof(LoadingSceneInfos))] ILoadSceneInfo loadingScene)
        {
            yield return Transition_Template(manager, () => manager.TransitionAsync(sceneParameters, loadingScene).AsTask(), sceneParameters.Length, sceneParameters.GetIndexToActivate());
        }

        public IEnumerator Transition_Template(ISceneManager manager, Func<Task<SceneResult>> transitionTask, int sceneCount, int setIndexActive)
        {
            yield return LoadFirstScene(manager);

            var task = transitionTask();
            yield return new WaitTask<SceneResult>(task);

            Scene[] loadedScenes = task.Result;
            Assert.AreEqual(sceneCount, loadedScenes.Length);
            if (setIndexActive >= 0)
                Assert.AreEqual(loadedScenes[setIndexActive], manager.GetActiveScene());

            yield return new WaitUntil(() => manager.TotalSceneCount == sceneCount);
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
            var task = manager.LoadAsync(new SceneParameters(sceneInfo));

            yield return new WaitValueTask<SceneResult>(task);

            Scene scene = task.Result;

            task = manager.UnloadAsync(scene);

            yield return new WaitValueTask<SceneResult>(task);

            Assert.Zero(manager.LoadedSceneCount);
        }

        /// <summary>
        /// Required to test some transition scenarios.
        /// </summary>
        public static WaitValueTask<SceneResult> LoadFirstScene(ISceneManager sceneManager) => new(sceneManager.LoadAsync(SceneBuilder.SceneNames[1], true));

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