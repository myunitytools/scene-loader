using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace MyGameDevTools.SceneLoading.Tests
{
    public partial class SceneManagerTests
    {
        int[] _buildIndexes = new[] { 0, 1, 2, 3 };

        [UnityTest]
        public IEnumerator Load_Extension_ByIndex([ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.SceneManagers))] ISceneManager manager)
        {
            var progress = new SimpleProgress();
            yield return Load_Template(manager, () => manager.LoadAsync(1, true, progress), progress, 1, 0);
        }

        [UnityTest]
        public IEnumerator Load_Extension_ByIndex_Multiple([ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.SceneManagers))] ISceneManager manager)
        {
            var progress = new SimpleProgress();
            yield return Load_Template(manager, () => manager.LoadAsync(_buildIndexes, 1, progress), progress, _buildIndexes.Length, 1);
        }

        [UnityTest]
        public IEnumerator Load_Extension_ByName([ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.SceneManagers))] ISceneManager manager)
        {
            var progress = new SimpleProgress();
            yield return Load_Template(manager, () => manager.LoadAsync(SceneBuilder.SceneNames[1], true, progress), progress, 1, 0);
        }

        [UnityTest]
        public IEnumerator Load_Extension_ByName_Multiple([ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.SceneManagers))] ISceneManager manager)
        {
            var progress = new SimpleProgress();
            yield return Load_Template(manager, () => manager.LoadAsync(SceneBuilder.SceneNames, 1, progress), progress, SceneBuilder.SceneNames.Length, 1);
        }

#if ENABLE_ADDRESSABLES
        [UnityTest]
        public IEnumerator Load_Extension_Addressable_ByAddress([ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.SceneManagers))] ISceneManager manager)
        {
            var progress = new SimpleProgress();
            yield return Load_Template(manager, () => manager.LoadAddressableAsync(SceneBuilder.SceneNames[1], true, progress), progress, 1, 0);
        }

        [UnityTest]
        public IEnumerator Load_Extension_Addressable_ByAddress_Multiple([ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.SceneManagers))] ISceneManager manager)
        {
            var progress = new SimpleProgress();
            yield return Load_Template(manager, () => manager.LoadAddressableAsync(SceneBuilder.SceneNames, 1, progress), progress, SceneBuilder.SceneNames.Length, 1);
        }

        [UnityTest]
        public IEnumerator Load_Extension_Addressable_ByAssetReference([ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.SceneManagers))] ISceneManager manager)
        {
            var progress = new SimpleProgress();
            yield return Load_Template(manager, () => manager.LoadAddressableAsync(_assetReferences[1], true, progress), progress, 1, 0);
        }

        [UnityTest]
        public IEnumerator Load_Extension_Addressable_ByAssetReference_Multiple([ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.SceneManagers))] ISceneManager manager)
        {
            var progress = new SimpleProgress();
            yield return Load_Template(manager, () => manager.LoadAddressableAsync(_assetReferences, 1, progress), progress, _assetReferences.Length, 1);
        }
#endif

        [UnityTest]
        public IEnumerator Transition_Extension_ByIndex([ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.SceneManagers))] ISceneManager manager)
        {
            yield return Transition_Template(manager, () => manager.TransitionAsync(1, 0).AsTask(), 1, 0);
        }

        [UnityTest]
        public IEnumerator Transition_Extension_ByIndex_Multiple([ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.SceneManagers))] ISceneManager manager)
        {
            yield return Transition_Template(manager, () => manager.TransitionAsync(_buildIndexes, 0).AsTask(), _buildIndexes.Length, 0);
        }

        [UnityTest]
        public IEnumerator Transition_Extension_ByName([ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.SceneManagers))] ISceneManager manager)
        {
            yield return Transition_Template(manager, () => manager.TransitionAsync(SceneBuilder.SceneNames[1], SceneBuilder.SceneNames[0]).AsTask(), 1, 0);
        }

        [UnityTest]
        public IEnumerator Transition_Extension_ByName_Multiple([ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.SceneManagers))] ISceneManager manager)
        {
            yield return Transition_Template(manager, () => manager.TransitionAsync(SceneBuilder.SceneNames, SceneBuilder.ScenePaths[0]).AsTask(), SceneBuilder.SceneNames.Length, 0);
        }

#if ENABLE_ADDRESSABLES
        [UnityTest]
        public IEnumerator Transition_Extension_Addressable_ByAddress([ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.SceneManagers))] ISceneManager manager)
        {
            yield return Transition_Template(manager, () => manager.TransitionAddressableAsync(SceneBuilder.SceneNames[1], SceneBuilder.SceneNames[0]).AsTask(), 1, 0);
        }

        [UnityTest]
        public IEnumerator Transition_Extension_Addressable_ByAddress_Multiple([ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.SceneManagers))] ISceneManager manager)
        {
            yield return Transition_Template(manager, () => manager.TransitionAddressableAsync(SceneBuilder.SceneNames, SceneBuilder.SceneNames[0]).AsTask(), SceneBuilder.SceneNames.Length, 0);
        }

        [UnityTest]
        public IEnumerator Transition_Extension_Addressable_ByAssetReference([ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.SceneManagers))] ISceneManager manager)
        {
            yield return Transition_Template(manager, () => manager.TransitionAddressableAsync(_assetReferences[1], _assetReferences[0]).AsTask(), 1, 0);
        }

        [UnityTest]
        public IEnumerator Transition_Extension_Addressable_ByAssetReference_Multiple([ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.SceneManagers))] ISceneManager manager)
        {
            yield return Transition_Template(manager, () => manager.TransitionAddressableAsync(_assetReferences, _assetReferences[0]).AsTask(), SceneBuilder.SceneNames.Length, 0);
        }
#endif

        [UnityTest]
        public IEnumerator Unload_Extension_ByIndex([ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.SceneManagers))] ISceneManager manager)
        {
            yield return Unload_Template(manager, () => manager.LoadAsync(1, true), () => manager.UnloadAsync(1), 1);
        }

        [UnityTest]
        public IEnumerator Unload_Extension_ByIndex_Multiple([ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.SceneManagers))] ISceneManager manager)
        {
            yield return Unload_Template(manager, () => manager.LoadAsync(_buildIndexes, 0), () => manager.UnloadAsync(_buildIndexes), _buildIndexes.Length);
        }

        [UnityTest]
        public IEnumerator Unload_Extension_ByName([ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.SceneManagers))] ISceneManager manager)
        {
            yield return Unload_Template(manager, () => manager.LoadAsync(SceneBuilder.SceneNames[1], true), () => manager.UnloadAsync(SceneBuilder.SceneNames[1]), 1);
        }

        [UnityTest]
        public IEnumerator Unload_Extension_ByName_Multiple([ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.SceneManagers))] ISceneManager manager)
        {
            yield return Unload_Template(manager, () => manager.LoadAsync(SceneBuilder.SceneNames, 0), () => manager.UnloadAsync(SceneBuilder.SceneNames), SceneBuilder.SceneNames.Length);
        }

#if ENABLE_ADDRESSABLES
        [UnityTest]
        public IEnumerator Unload_Extension_Addressable_ByAddress([ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.SceneManagers))] ISceneManager manager)
        {
            yield return Unload_Template(manager, () => manager.LoadAddressableAsync(SceneBuilder.SceneNames[1], true), () => manager.UnloadAddressableAsync(SceneBuilder.SceneNames[1]), 1);
        }

        [UnityTest]
        public IEnumerator Unload_Extension_Addressable_ByAddress_Multiple([ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.SceneManagers))] ISceneManager manager)
        {
            yield return Unload_Template(manager, () => manager.LoadAddressableAsync(SceneBuilder.SceneNames, 0), () => manager.UnloadAddressableAsync(SceneBuilder.SceneNames), SceneBuilder.SceneNames.Length);
        }

        [UnityTest]
        public IEnumerator Unload_Extension_Addressable_ByAssetReference([ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.SceneManagers))] ISceneManager manager)
        {
            yield return Unload_Template(manager, () => manager.LoadAddressableAsync(_assetReferences[1], true), () => manager.UnloadAddressableAsync(_assetReferences[1]), 1);
        }

        [UnityTest]
        public IEnumerator Unload_Extension_Addressable_ByAssetReference_Multiple([ValueSource(typeof(SceneTestEnvironment), nameof(SceneTestEnvironment.SceneManagers))] ISceneManager manager)
        {
            yield return Unload_Template(manager, () => manager.LoadAddressableAsync(_assetReferences, 0), () => manager.UnloadAddressableAsync(_assetReferences), _assetReferences.Length);
        }
#endif
    }
}
