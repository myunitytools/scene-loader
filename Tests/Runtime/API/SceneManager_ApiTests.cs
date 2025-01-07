using MyGameDevTools.SceneLoading.Tests;
using NUnit.Framework;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace MyGameDevTools.SceneLoading.Api.Tests
{
    public class SceneManager_ApiTests : IPrebuildSetup, IPostBuildCleanup
    {
        int _scenesActivated;
        int _scenesUnloaded;
        int _scenesLoaded;

        public void Setup() => new SceneTestEnvironment().Setup();

        public void Cleanup() => new SceneTestEnvironment().Cleanup();

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            SceneTestEnvironment.ValidateSceneEnvironment();

            AdvancedSceneManager.ActiveSceneChanged += ReportSceneActivation;
            AdvancedSceneManager.SceneUnloaded += ReportSceneUnloaded;
            AdvancedSceneManager.SceneLoaded += ReportSceneLoaded;
        }

        [OneTimeTearDown]
        public void OneTimeTeardown()
        {
            AdvancedSceneManager.ActiveSceneChanged -= ReportSceneActivation;
            AdvancedSceneManager.SceneUnloaded -= ReportSceneUnloaded;
            AdvancedSceneManager.SceneLoaded -= ReportSceneLoaded;
        }

        [SetUp]
        public void TestSetup()
        {
            AdvancedSceneManager.SetActiveScene(AdvancedSceneManager.GetLoadedSceneAt(0));

            _scenesActivated = 0;
            _scenesUnloaded = 0;
            _scenesLoaded = 0;
        }

        [Test]
        public void InitialStateTest()
        {
            int loadedScenes = 0;
            Assert.DoesNotThrow(() => loadedScenes = AdvancedSceneManager.LoadedSceneCount);
            Assert.AreEqual(1, loadedScenes);
            Assert.AreEqual(1, AdvancedSceneManager.TotalSceneCount);

            Scene activeScene = SceneManager.GetActiveScene();
            Assert.AreEqual(activeScene, AdvancedSceneManager.GetActiveScene());
            Assert.AreEqual(activeScene, AdvancedSceneManager.GetLastLoadedScene());
            Assert.AreEqual(activeScene, AdvancedSceneManager.GetLoadedSceneAt(0));
            Assert.AreEqual(activeScene, AdvancedSceneManager.GetLoadedSceneByName(activeScene.name));
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
}
