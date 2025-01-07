using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
#if ENABLE_ADDRESSABLES
using UnityEngine.AddressableAssets;
#endif
using UnityEngine.SceneManagement;

namespace MyGameDevTools.SceneLoading.Api
{
    public static class AdvancedSceneManager
    {
        static ISceneManager Instance
        {
            get
            {
                if (_instance == null)
                    throw new NullReferenceException("[AdvancedSceneManager] The static scene manager instance is not available before the first scene is fully loaded. Try moving the call to `Start()`.");
                return _instance;
            }
        }

        static ISceneManager _instance;

        [RuntimeInitializeOnLoadMethod]
        internal static void Initialize()
        {
            _instance = new SceneLoading.AdvancedSceneManager(true);
        }

        #region ISceneManager
        /// <summary>
        /// Reports that the active scene has changed, passing the <b>previous</b> and <b>current</b> active scene as parameters.
        /// <br/>
        /// In some scenarios, the previous or the current scene might be invalid <i>(you can check it through <see cref="Scene.IsValid()"/>)</i>, but never both at the same time.
        /// <br/>
        /// This can occur when the first active scene is being set (there was no previous active scene) or when the last scene gets unloaded (leaving no other scene to be activated).
        /// </summary>
        public static event Action<Scene, Scene> ActiveSceneChanged
        {
            add => Instance.ActiveSceneChanged += value;
            remove => Instance.ActiveSceneChanged -= value;
        }
        /// <summary>
        /// Reports when a scene gets unloaded.
        /// </summary>
        public static event Action<Scene> SceneUnloaded
        {
            add => Instance.SceneUnloaded += value;
            remove => Instance.SceneUnloaded -= value;
        }
        /// <summary>
        /// Reports when a scene gets loaded.
        /// </summary>
        public static event Action<Scene> SceneLoaded
        {
            add => Instance.SceneLoaded += value;
            remove => Instance.SceneLoaded -= value;
        }

        /// <summary>
        /// The amount of scenes loaded.
        /// </summary>
        public static int LoadedSceneCount => Instance.LoadedSceneCount;
        /// <summary>
        /// The amount of scenes managed by the internal <see cref="ISceneManager"/>.
        /// This includes scenes that are being unloaded.
        /// </summary>
        public static int TotalSceneCount => Instance.TotalSceneCount;

        /// <summary>
        /// Sets the target <paramref name="scene"/> as the active scene.
        /// Internally calls <see cref="SceneManager.SetActiveScene(Scene)"/>.
        /// </summary>
        /// <param name="scene">Scene to be enabled as the active scene.</param>
        public static void SetActiveScene(Scene scene) => Instance.SetActiveScene(scene);

        /// <summary>
        /// Triggers a transition to a group of scenes.
        /// It will transition from the current active scene (<see cref="GetActiveScene()"/>)
        /// to the target scene or a group of scenes via a <see cref="SceneParameters"/> struct, with an optional <paramref name="intermediateSceneReference"/>.
        /// If the <paramref name="intermediateSceneReference"/> is not set, the transition will have no intermediate loading scene and will instead simply load the target scene directly.
        /// The complete transition flow is:
        /// <br/><br/>
        /// 1. Load the intermediate scene (if provided).<br/>
        /// 2. Unload the source scene (if any).<br/>
        /// 3. Load all target scenes.<br/>
        /// 4. Unload the intermediate scene (if provided).<br/>
        /// </summary>
        /// <param name="sceneParameters">
        /// A <see cref="SceneParameters"/> struct that may hold one or more scenes and the target active index.
        /// </param>
        /// <param name="intermediateSceneInfo">
        /// A reference to the scene that's going to be loaded as the transition intermediate (as a loading scene).
        /// If null, the transition will not have an intermediate loading scene.
        /// </param>
        /// <param name="token">Optional token to manually cancel the operation. Note that Unity Scene Manager operations cannot be manually canceled and will continue to run.</param>
        /// <returns>A <see cref="System.Threading.Tasks.ValueTask{TResult}"/> with all scenes loaded.</returns>
        public static ValueTask<SceneResult> TransitionAsync(SceneParameters sceneParameters, ILoadSceneInfo intermediateSceneReference = default, CancellationToken token = default) => Instance.TransitionAsync(sceneParameters, intermediateSceneReference, token);

        /// <summary>
        /// Loads the target scene or group of scenes provided via a <see cref="SceneParameters"/> struct.
        /// You may also provide the desired index to set as the active scene.
        /// Also, you can pass an <see cref="IProgress{T}"/> object to receive the average progress of all loading operations, from 0 to 1.
        /// </summary>
        /// <param name="sceneParameters">
        /// A <see cref="SceneParameters"/> struct that may hold one or more scenes and the target active index.
        /// </param>
        /// <param name="progress">
        /// Object to report the loading operations progress to, from 0 to 1.
        /// </param>
        /// <param name="token">
        /// Optional token to manually cancel the operation. Note that Unity Scene Manager operations cannot be manually canceled and will continue to run.
        /// </param>
        /// <returns>A <see cref="System.Threading.Tasks.ValueTask{TResult}"/> with all scenes loaded.</returns>
        public static ValueTask<SceneResult> LoadAsync(SceneParameters sceneParameters, IProgress<float> progress = null, CancellationToken token = default) => Instance.LoadAsync(sceneParameters, progress, token);

        /// <summary>
        /// Unloads the target scene or group of scenes provided via a <see cref="SceneParameters"/> struct.
        /// </summary>
        /// <param name="sceneParameters">
        /// A <see cref="SceneParameters"/> struct that may hold one or more scenes.
        /// </param>
        /// <param name="token">Optional token to manually cancel the operation. Note that Unity Scene Manager operations cannot be manually canceled and will continue to run.</param>
        /// <returns>
        /// A <see cref="System.Threading.Tasks.ValueTask{TResult}"/> with all the unloaded scenes.
        /// <br/>
        /// Note that in some cases, the returned scenes might no longer have a reference to its native representation, hich means its <see cref="Scene.handle"/> will not point anywhere and you won't be able to perform equal comparisons between scenes.
        /// </returns>
        public static ValueTask<SceneResult> UnloadAsync(SceneParameters sceneParameters, CancellationToken token = default) => Instance.UnloadAsync(sceneParameters, token);

        /// <summary>
        /// Gets the current active scene.
        /// This should point to the same scene you get via <see cref="SceneManager.GetActiveScene()"/>.
        /// </summary>
        /// <returns>The current active scene, or an invalid scene if none of the loaded scenes are enabled as the active scene.</returns>
        public static Scene GetActiveScene() => Instance.GetActiveScene();

        /// <summary>
        /// Gets the loaded scene at the <paramref name="index"/> of the loaded scenes list.
        /// </summary>
        /// <param name="index">Index of the target scene in the loaded scenes list.</param>
        /// <returns>The loaded scene at the <paramref name="index"/> of the loaded scenes list.</returns>
        public static Scene GetLoadedSceneAt(int index) => Instance.GetLoadedSceneAt(index);

        /// <summary>
        /// Gets the last loaded scene of this <see cref="ISceneManager"/>.
        /// </summary>
        /// <returns>The last loaded scene, or an invalid scene if there are no loaded scenes in this <see cref="ISceneManager"/>.</returns>
        public static Scene GetLastLoadedScene() => Instance.GetLastLoadedScene();

        /// <summary>
        /// Gets a loaded scene by its <paramref name="name"/>.
        /// </summary>
        /// <param name="name">Name of the loaded scene to be found.</param>
        /// <returns>A loaded scene with the given <paramref name="name"/>.</returns>
        public static Scene GetLoadedSceneByName(string name) => Instance.GetLoadedSceneByName(name);
        #endregion

        #region Extensions
        /// <summary>
        /// Loads the target scenes.
        /// You may also provide the desired index to set as the active scene.
        /// Also, you can pass an <see cref="IProgress{T}"/> object to receive the average progress of all loading operations, from 0 to 1.
        /// </summary>
        /// <param name="sceneNames">
        /// The target scenes' names.
        /// </param>
        /// <param name="setIndexActive">
        /// The index of the scene to be set active, or -1 if none.
        /// </param>
        /// <param name="progress">
        /// Object to report the loading operations progress to, from 0 to 1.
        /// </param>
        /// <param name="token">
        /// Optional token to manually cancel the operation. Note that Unity Scene Manager operations cannot be manually canceled and will continue to run.
        /// </param>
        /// <returns>A <see cref="System.Threading.Tasks.ValueTask{TResult}"/> with all scenes loaded.</returns>
        public static ValueTask<SceneResult> LoadAsync(string[] sceneNames, int setIndexActive = -1, IProgress<float> progress = null, CancellationToken token = default) => Instance.LoadAsync(sceneNames, setIndexActive, progress, token);

        /// <summary>
        /// Loads the target scenes.
        /// You may also provide the desired index to set as the active scene.
        /// Also, you can pass an <see cref="IProgress{T}"/> object to receive the average progress of all loading operations, from 0 to 1.
        /// </summary>
        /// <param name="buildIndices">
        /// The target scenes' build indexes.
        /// </param>
        /// <param name="setIndexActive">
        /// The index of the scene to be set active, or -1 if none.
        /// </param>
        /// <param name="progress">
        /// Object to report the loading operations progress to, from 0 to 1.
        /// </param>
        /// <param name="token">
        /// Optional token to manually cancel the operation. Note that Unity Scene Manager operations cannot be manually canceled and will continue to run.
        /// </param>
        /// <returns>A <see cref="System.Threading.Tasks.ValueTask{TResult}"/> with all scenes loaded.</returns>
        public static ValueTask<SceneResult> LoadAsync(int[] buildIndices, int setIndexActive = -1, IProgress<float> progress = null, CancellationToken token = default) => Instance.LoadAsync(buildIndices, setIndexActive, progress, token);

        /// <summary>
        /// Loads the target scene.
        /// You may also provide the desired index to set as the active scene.
        /// Also, you can pass an <see cref="IProgress{T}"/> object to receive the average progress of all loading operations, from 0 to 1.
        /// </summary>
        /// <param name="sceneName">
        /// The target scene's name.
        /// </param>
        /// <param name="setActive">
        /// If the scene should be activated after load.
        /// </param>
        /// <param name="progress">
        /// Object to report the loading operations progress to, from 0 to 1.
        /// </param>
        /// <param name="token">
        /// Optional token to manually cancel the operation. Note that Unity Scene Manager operations cannot be manually canceled and will continue to run.
        /// </param>
        /// <returns>A <see cref="System.Threading.Tasks.ValueTask{TResult}"/> with all scenes loaded.</returns>
        public static ValueTask<SceneResult> LoadAsync(string sceneName, bool setActive = false, IProgress<float> progress = null, CancellationToken token = default) => Instance.LoadAsync(sceneName, setActive, progress, token);

        /// <summary>
        /// Loads the target scene.
        /// You may also provide the desired index to set as the active scene.
        /// Also, you can pass an <see cref="IProgress{T}"/> object to receive the average progress of all loading operations, from 0 to 1.
        /// </summary>
        /// <param name="buildIndex">
        /// The target scene's build index.
        /// </param>
        /// <param name="setActive">
        /// If the scene should be activated after load.
        /// </param>
        /// <param name="progress">
        /// Object to report the loading operations progress to, from 0 to 1.
        /// </param>
        /// <param name="token">
        /// Optional token to manually cancel the operation. Note that Unity Scene Manager operations cannot be manually canceled and will continue to run.
        /// </param>
        /// <returns>A <see cref="System.Threading.Tasks.ValueTask{TResult}"/> with all scenes loaded.</returns>
        public static ValueTask<SceneResult> LoadAsync(int buildIndex, bool setActive = false, IProgress<float> progress = null, CancellationToken token = default) => Instance.LoadAsync(buildIndex, setActive, progress, token);

#if ENABLE_ADDRESSABLES
        /// <summary>
        /// Loads the target scenes.
        /// You may also provide the desired index to set as the active scene.
        /// Also, you can pass an <see cref="IProgress{T}"/> object to receive the average progress of all loading operations, from 0 to 1.
        /// </summary>
        /// <param name="assetReferences">
        /// The target scenes' <see cref="AssetReference">.
        /// </param>
        /// <param name="setIndexActive">
        /// The index of the scene to be set active, or -1 if none.
        /// </param>
        /// <param name="progress">
        /// Object to report the loading operations progress to, from 0 to 1.
        /// </param>
        /// <param name="token">
        /// Optional token to manually cancel the operation. Note that Unity Scene Manager operations cannot be manually canceled and will continue to run.
        /// </param>
        /// <returns>A <see cref="System.Threading.Tasks.ValueTask{TResult}"/> with all scenes loaded.</returns>
        public static ValueTask<SceneResult> LoadAddressableAsync(AssetReference[] assetReferences, int setIndexActive = -1, IProgress<float> progress = null, CancellationToken token = default) => Instance.LoadAddressableAsync(assetReferences, setIndexActive, progress, token);

        /// <summary>
        /// Loads the target scenes.
        /// You may also provide the desired index to set as the active scene.
        /// Also, you can pass an <see cref="IProgress{T}"/> object to receive the average progress of all loading operations, from 0 to 1.
        /// </summary>
        /// <param name="addresses">
        /// The target scenes' addressable address.
        /// </param>
        /// <param name="setIndexActive">
        /// The index of the scene to be set active, or -1 if none.
        /// </param>
        /// <param name="progress">
        /// Object to report the loading operations progress to, from 0 to 1.
        /// </param>
        /// <param name="token">
        /// Optional token to manually cancel the operation. Note that Unity Scene Manager operations cannot be manually canceled and will continue to run.
        /// </param>
        /// <returns>A <see cref="System.Threading.Tasks.ValueTask{TResult}"/> with all scenes loaded.</returns>
        public static ValueTask<SceneResult> LoadAddressableAsync(string[] addresses, int setIndexActive = -1, IProgress<float> progress = null, CancellationToken token = default) => Instance.LoadAddressableAsync(addresses, setIndexActive, progress, token);

        /// <summary>
        /// Loads the target scene.
        /// You may also provide the desired index to set as the active scene.
        /// Also, you can pass an <see cref="IProgress{T}"/> object to receive the average progress of all loading operations, from 0 to 1.
        /// </summary>
        /// <param name="assetReference">
        /// The target scene's <see cref="AssetReference">.
        /// </param>
        /// <param name="setActive">
        /// If the scene should be activated after load.
        /// </param>
        /// <param name="progress">
        /// Object to report the loading operations progress to, from 0 to 1.
        /// </param>
        /// <param name="token">
        /// Optional token to manually cancel the operation. Note that Unity Scene Manager operations cannot be manually canceled and will continue to run.
        /// </param>
        /// <returns>A <see cref="System.Threading.Tasks.ValueTask{TResult}"/> with all scenes loaded.</returns>
        public static ValueTask<SceneResult> LoadAddressableAsync(AssetReference assetReference, bool setActive = false, IProgress<float> progress = null, CancellationToken token = default) => Instance.LoadAddressableAsync(assetReference, setActive, progress, token);

        /// <summary>
        /// Loads the target scene.
        /// You may also provide the desired index to set as the active scene.
        /// Also, you can pass an <see cref="IProgress{T}"/> object to receive the average progress of all loading operations, from 0 to 1.
        /// </summary>
        /// <param name="address">
        /// The target scene's addressable address.
        /// </param>
        /// <param name="setActive">
        /// If the scene should be activated after load.
        /// </param>
        /// <param name="progress">
        /// Object to report the loading operations progress to, from 0 to 1.
        /// </param>
        /// <param name="token">
        /// Optional token to manually cancel the operation. Note that Unity Scene Manager operations cannot be manually canceled and will continue to run.
        /// </param>
        /// <returns>A <see cref="System.Threading.Tasks.ValueTask{TResult}"/> with all scenes loaded.</returns>
        public static ValueTask<SceneResult> LoadAddressableAsync(string address, bool setActive = false, IProgress<float> progress = null, CancellationToken token = default) => Instance.LoadAddressableAsync(address, setActive, progress, token);
#endif

        /// <summary>
        /// Triggers a transition to a group of scenes.
        /// It will transition from the current active scene (<see cref="GetActiveScene()"/>)
        /// to a group of scenes, with an optional <paramref name="loadingSceneName"/>.
        /// If the <paramref name="loadingSceneName"/> is not set, the transition will have no intermediate loading scene and will instead simply load the target scene directly.
        /// The complete transition flow is:
        /// <br/><br/>
        /// 1. Load the intermediate scene (if provided).<br/>
        /// 2. Unload the source scene (if any).<br/>
        /// 3. Load all target scenes.<br/>
        /// 4. Unload the intermediate scene (if provided).<br/>
        /// </summary>
        /// <param name="targetSceneNames">
        /// An array of scenes by their names to transition to.
        /// </param>
        /// <param name="loadingSceneName">
        /// A reference to the scene that's going to be loaded as the transition intermediate (as a loading scene).
        /// If null, the transition will not have an intermediate loading scene.
        /// </param>
        /// <param name="setIndexActive">
        /// The index of the scene to be activated as the active scene. It must be greater than or equal 0.
        /// </param>
        /// <param name="token">Optional token to manually cancel the operation. Note that Unity Scene Manager operations cannot be manually canceled and will continue to run.</param>
        /// <returns>A <see cref="System.Threading.Tasks.ValueTask{TResult}"/> with all scenes loaded.</returns>
        public static ValueTask<SceneResult> TransitionAsync(string[] targetSceneNames, string loadingSceneName = null, int setIndexActive = 0, CancellationToken token = default) => Instance.TransitionAsync(targetSceneNames, loadingSceneName, setIndexActive, token);

        /// <summary>
        /// Triggers a transition to a group of scenes.
        /// It will transition from the current active scene (<see cref="GetActiveScene()"/>)
        /// to a group of scenes, with an optional <paramref name="loadingBuildIndex"/>.
        /// If the <paramref name="loadingBuildIndex"/> is not set, the transition will have no intermediate loading scene and will instead simply load the target scene directly.
        /// The complete transition flow is:
        /// <br/><br/>
        /// 1. Load the intermediate scene (if provided).<br/>
        /// 2. Unload the source scene (if any).<br/>
        /// 3. Load all target scenes.<br/>
        /// 4. Unload the intermediate scene (if provided).<br/>
        /// </summary>
        /// <param name="targetBuildIndices">
        /// An array of scenes by their build index to transition to.
        /// </param>
        /// <param name="loadingBuildIndex">
        /// A reference to the scene that's going to be loaded as the transition intermediate (as a loading scene).
        /// If null, the transition will not have an intermediate loading scene.
        /// </param>
        /// <param name="setIndexActive">
        /// The index of the scene to be activated as the active scene. It must be greater than or equal 0.
        /// </param>
        /// <param name="token">Optional token to manually cancel the operation. Note that Unity Scene Manager operations cannot be manually canceled and will continue to run.</param>
        /// <returns>A <see cref="System.Threading.Tasks.ValueTask{TResult}"/> with all scenes loaded.</returns>
        public static ValueTask<SceneResult> TransitionAsync(int[] targetBuildIndices, int loadingBuildIndex = -1, int setIndexActive = 0, CancellationToken token = default) => Instance.TransitionAsync(targetBuildIndices, loadingBuildIndex, setIndexActive, token);

        /// <summary>
        /// Triggers a transition to the target scene.
        /// It will transition from the current active scene (<see cref="GetActiveScene()"/>)
        /// to the target scene, with an optional <paramref name="loadingSceneName"/>.
        /// If the <paramref name="loadingSceneName"/> is not set, the transition will have no intermediate loading scene and will instead simply load the target scene directly.
        /// The complete transition flow is:
        /// <br/><br/>
        /// 1. Load the intermediate scene (if provided).<br/>
        /// 2. Unload the source scene (if any).<br/>
        /// 3. Load all target scenes.<br/>
        /// 4. Unload the intermediate scene (if provided).<br/>
        /// </summary>
        /// <param name="targetSceneName">
        /// The target scene name to be transitioned to.
        /// </param>
        /// <param name="loadingSceneName">
        /// A reference to the scene that's going to be loaded as the transition intermediate (as a loading scene).
        /// If null, the transition will not have an intermediate loading scene.
        /// </param>
        /// <param name="token">Optional token to manually cancel the operation. Note that Unity Scene Manager operations cannot be manually canceled and will continue to run.</param>
        /// <returns>A <see cref="System.Threading.Tasks.ValueTask{TResult}"/> with all scenes loaded.</returns>
        public static ValueTask<SceneResult> TransitionAsync(string targetSceneName, string loadingSceneName = null, CancellationToken token = default) => Instance.TransitionAsync(targetSceneName, loadingSceneName, token);

        /// <summary>
        /// Triggers a transition to the target scene.
        /// It will transition from the current active scene (<see cref="GetActiveScene()"/>)
        /// to the target scene, with an optional <paramref name="loadingBuildIndex"/>.
        /// If the <paramref name="loadingSceneName"/> is not set, the transition will have no intermediate loading scene and will instead simply load the target scene directly.
        /// The complete transition flow is:
        /// <br/><br/>
        /// 1. Load the intermediate scene (if provided).<br/>
        /// 2. Unload the source scene (if any).<br/>
        /// 3. Load all target scenes.<br/>
        /// 4. Unload the intermediate scene (if provided).<br/>
        /// </summary>
        /// <param name="targetBuildIndex">
        /// The target scene build index to be transitioned to.
        /// </param>
        /// <param name="loadingBuildIndex">
        /// A reference to the scene that's going to be loaded as the transition intermediate (as a loading scene).
        /// If -1, the transition will not have an intermediate loading scene.
        /// </param>
        /// <param name="token">Optional token to manually cancel the operation. Note that Unity Scene Manager operations cannot be manually canceled and will continue to run.</param>
        /// <returns>A <see cref="System.Threading.Tasks.ValueTask{TResult}"/> with all scenes loaded.</returns>
        public static ValueTask<SceneResult> TransitionAsync(int targetBuildIndex, int loadingBuildIndex = -1, CancellationToken token = default) => Instance.TransitionAsync(targetBuildIndex, loadingBuildIndex, token);

#if ENABLE_ADDRESSABLES
        /// <summary>
        /// Triggers a transition to a group of scenes.
        /// It will transition from the current active scene (<see cref="GetActiveScene()"/>)
        /// to a group of scenes, with an optional <paramref name="loadingAssetReference"/>.
        /// If the <paramref name="loadingAssetReference"/> is not set, the transition will have no intermediate loading scene and will instead simply load the target scene directly.
        /// The complete transition flow is:
        /// <br/><br/>
        /// 1. Load the intermediate scene (if provided).<br/>
        /// 2. Unload the source scene (if any).<br/>
        /// 3. Load all target scenes.<br/>
        /// 4. Unload the intermediate scene (if provided).<br/>
        /// </summary>
        /// <param name="targetAssetReferences">
        /// An array of scenes by their <see cref="AssetReference"/> to transition to.
        /// </param>
        /// <param name="loadingAssetReference">
        /// A reference to the scene that's going to be loaded as the transition intermediate (as a loading scene).
        /// If null, the transition will not have an intermediate loading scene.
        /// </param>
        /// <param name="setIndexActive">
        /// The index of the scene to be activated as the active scene. It must be greater than or equal 0.
        /// </param>
        /// <param name="token">Optional token to manually cancel the operation. Note that Unity Scene Manager operations cannot be manually canceled and will continue to run.</param>
        /// <returns>A <see cref="System.Threading.Tasks.ValueTask{TResult}"/> with all scenes loaded.</returns>
        public static ValueTask<SceneResult> TransitionAddressableAsync(AssetReference[] targetAssetReferences, AssetReference loadingAssetReference = null, int setIndexActive = 0, CancellationToken token = default) => Instance.TransitionAddressableAsync(targetAssetReferences, loadingAssetReference, setIndexActive, token);

        /// <summary>
        /// Triggers a transition to a group of scenes.
        /// It will transition from the current active scene (<see cref="GetActiveScene()"/>)
        /// to a group of scenes, with an optional <paramref name="loadingAddress"/>.
        /// If the <paramref name="loadingAddress"/> is not set, the transition will have no intermediate loading scene and will instead simply load the target scene directly.
        /// The complete transition flow is:
        /// <br/><br/>
        /// 1. Load the intermediate scene (if provided).<br/>
        /// 2. Unload the source scene (if any).<br/>
        /// 3. Load all target scenes.<br/>
        /// 4. Unload the intermediate scene (if provided).<br/>
        /// </summary>
        /// <param name="targetAddresses">
        /// An array of scenes by their addressable addresses to transition to.
        /// </param>
        /// <param name="loadingAddress">
        /// A reference to the scene that's going to be loaded as the transition intermediate (as a loading scene).
        /// If null, the transition will not have an intermediate loading scene.
        /// </param>
        /// <param name="setIndexActive">
        /// The index of the scene to be activated as the active scene. It must be greater than or equal 0.
        /// </param>
        /// <param name="token">Optional token to manually cancel the operation. Note that Unity Scene Manager operations cannot be manually canceled and will continue to run.</param>
        /// <returns>A <see cref="System.Threading.Tasks.ValueTask{TResult}"/> with all scenes loaded.</returns>
        public static ValueTask<SceneResult> TransitionAddressableAsync(string[] targetAddresses, string loadingAddress = null, int setIndexActive = 0, CancellationToken token = default) => Instance.TransitionAddressableAsync(targetAddresses, loadingAddress, setIndexActive, token);

        /// <summary>
        /// Triggers a transition to the target scene.
        /// It will transition from the current active scene (<see cref="GetActiveScene()"/>)
        /// to the target scene, with an optional <paramref name="loadingAssetReference"/>.
        /// If the <paramref name="loadingAssetReference"/> is not set, the transition will have no intermediate loading scene and will instead simply load the target scene directly.
        /// The complete transition flow is:
        /// <br/><br/>
        /// 1. Load the intermediate scene (if provided).<br/>
        /// 2. Unload the source scene (if any).<br/>
        /// 3. Load all target scenes.<br/>
        /// 4. Unload the intermediate scene (if provided).<br/>
        /// </summary>
        /// <param name="targetAssetReference">
        /// The target scene <see cref="AssetReference"/> to be transitioned to.
        /// </param>
        /// <param name="loadingAssetReference">
        /// A reference to the scene that's going to be loaded as the transition intermediate (as a loading scene).
        /// If null, the transition will not have an intermediate loading scene.
        /// </param>
        /// <param name="token">Optional token to manually cancel the operation. Note that Unity Scene Manager operations cannot be manually canceled and will continue to run.</param>
        /// <returns>A <see cref="System.Threading.Tasks.ValueTask{TResult}"/> with all scenes loaded.</returns>
        public static ValueTask<SceneResult> TransitionAddressableAsync(AssetReference targetAssetReference, AssetReference loadingAssetReference = null, CancellationToken token = default) => Instance.TransitionAddressableAsync(targetAssetReference, loadingAssetReference, token);

        /// <summary>
        /// Triggers a transition to the target scene.
        /// It will transition from the current active scene (<see cref="GetActiveScene()"/>)
        /// to the target scene, with an optional <paramref name="loadingAddress"/>.
        /// If the <paramref name="loadingAddress"/> is not set, the transition will have no intermediate loading scene and will instead simply load the target scene directly.
        /// The complete transition flow is:
        /// <br/><br/>
        /// 1. Load the intermediate scene (if provided).<br/>
        /// 2. Unload the source scene (if any).<br/>
        /// 3. Load all target scenes.<br/>
        /// 4. Unload the intermediate scene (if provided).<br/>
        /// </summary>
        /// <param name="targetAddress">
        /// The target scene addressable address to be transitioned to.
        /// </param>
        /// <param name="loadingAddress">
        /// A reference to the scene that's going to be loaded as the transition intermediate (as a loading scene).
        /// If null, the transition will not have an intermediate loading scene.
        /// </param>
        /// <param name="token">Optional token to manually cancel the operation. Note that Unity Scene Manager operations cannot be manually canceled and will continue to run.</param>
        /// <returns>A <see cref="System.Threading.Tasks.ValueTask{TResult}"/> with all scenes loaded.</returns>
        public static ValueTask<SceneResult> TransitionAddressableAsync(string targetAddress, string loadingAddress = null, CancellationToken token = default) => Instance.TransitionAddressableAsync(targetAddress, loadingAddress, token);
#endif

        /// <summary>
        /// Unloads the target scene or group of scenes.
        /// </summary>
        /// <param name="sceneNames">
        /// An array of scenes by their names to be unloaded.
        /// </param>
        /// <param name="token">Optional token to manually cancel the operation. Note that Unity Scene Manager operations cannot be manually canceled and will continue to run.</param>
        /// <returns>
        /// A <see cref="System.Threading.Tasks.ValueTask{TResult}"/> with all the unloaded scenes.
        /// <br/>
        /// Note that in some cases, the returned scenes might no longer have a reference to its native representation, hich means its <see cref="Scene.handle"/> will not point anywhere and you won't be able to perform equal comparisons between scenes.
        /// </returns>
        public static ValueTask<SceneResult> UnloadAsync(string[] sceneNames, CancellationToken token = default) => Instance.UnloadAsync(sceneNames, token);

        /// <summary>
        /// Unloads the target scene or group of scenes.
        /// </summary>
        /// <param name="buildIndices">
        /// An array of scenes by their build index to be unloaded.
        /// </param>
        /// <param name="token">Optional token to manually cancel the operation. Note that Unity Scene Manager operations cannot be manually canceled and will continue to run.</param>
        /// <returns>
        /// A <see cref="System.Threading.Tasks.ValueTask{TResult}"/> with all the unloaded scenes.
        /// <br/>
        /// Note that in some cases, the returned scenes might no longer have a reference to its native representation, hich means its <see cref="Scene.handle"/> will not point anywhere and you won't be able to perform equal comparisons between scenes.
        /// </returns>
        public static ValueTask<SceneResult> UnloadAsync(int[] buildIndices, CancellationToken token = default) => Instance.UnloadAsync(buildIndices, token);

        /// <summary>
        /// Unloads the target scene or group of scenes.
        /// </summary>
        /// <param name="scenes">
        /// An array of scenes to be unloaded.
        /// </param>
        /// <param name="token">Optional token to manually cancel the operation. Note that Unity Scene Manager operations cannot be manually canceled and will continue to run.</param>
        /// <returns>
        /// A <see cref="System.Threading.Tasks.ValueTask{TResult}"/> with all the unloaded scenes.
        /// <br/>
        /// Note that in some cases, the returned scenes might no longer have a reference to its native representation, hich means its <see cref="Scene.handle"/> will not point anywhere and you won't be able to perform equal comparisons between scenes.
        /// </returns>
        public static ValueTask<SceneResult> UnloadAsync(Scene[] scenes, CancellationToken token = default) => Instance.UnloadAsync(scenes, token);

        /// <summary>
        /// Unloads the target scene or group of scenes.
        /// </summary>
        /// <param name="sceneName">
        /// The target scene's name to be unloaded.
        /// </param>
        /// <param name="token">Optional token to manually cancel the operation. Note that Unity Scene Manager operations cannot be manually canceled and will continue to run.</param>
        /// <returns>
        /// A <see cref="System.Threading.Tasks.ValueTask{TResult}"/> with all the unloaded scenes.
        /// <br/>
        /// Note that in some cases, the returned scenes might no longer have a reference to its native representation, hich means its <see cref="Scene.handle"/> will not point anywhere and you won't be able to perform equal comparisons between scenes.
        /// </returns>
        public static ValueTask<SceneResult> UnloadAsync(string sceneName, CancellationToken token = default) => Instance.UnloadAsync(sceneName, token);

        /// <summary>
        /// Unloads the target scene or group of scenes.
        /// </summary>
        /// <param name="buildIndex">
        /// The target scene's build index to be unloaded.
        /// </param>
        /// <param name="token">Optional token to manually cancel the operation. Note that Unity Scene Manager operations cannot be manually canceled and will continue to run.</param>
        /// <returns>
        /// A <see cref="System.Threading.Tasks.ValueTask{TResult}"/> with all the unloaded scenes.
        /// <br/>
        /// Note that in some cases, the returned scenes might no longer have a reference to its native representation, hich means its <see cref="Scene.handle"/> will not point anywhere and you won't be able to perform equal comparisons between scenes.
        /// </returns>
        public static ValueTask<SceneResult> UnloadAsync(int buildIndex, CancellationToken token = default) => Instance.UnloadAsync(buildIndex, token);

        /// <summary>
        /// Unloads the target scene or group of scenes.
        /// </summary>
        /// <param name="scene">
        /// The target scene to be unloaded.
        /// </param>
        /// <param name="token">Optional token to manually cancel the operation. Note that Unity Scene Manager operations cannot be manually canceled and will continue to run.</param>
        /// <returns>
        /// A <see cref="System.Threading.Tasks.ValueTask{TResult}"/> with all the unloaded scenes.
        /// <br/>
        /// Note that in some cases, the returned scenes might no longer have a reference to its native representation, hich means its <see cref="Scene.handle"/> will not point anywhere and you won't be able to perform equal comparisons between scenes.
        /// </returns>
        public static ValueTask<SceneResult> UnloadAsync(Scene scene, CancellationToken token = default) => Instance.UnloadAsync(scene, token);

#if ENABLE_ADDRESSABLES
        /// <summary>
        /// Unloads the target scene or group of scenes.
        /// </summary>
        /// <param name="assetReferences">
        /// An array of scenes to be unloaded.
        /// </param>
        /// <param name="token">Optional token to manually cancel the operation. Note that Unity Scene Manager operations cannot be manually canceled and will continue to run.</param>
        /// <returns>
        /// A <see cref="System.Threading.Tasks.ValueTask{TResult}"/> with all the unloaded scenes.
        /// <br/>
        /// Note that in some cases, the returned scenes might no longer have a reference to its native representation, hich means its <see cref="Scene.handle"/> will not point anywhere and you won't be able to perform equal comparisons between scenes.
        /// </returns>
        public static ValueTask<SceneResult> UnloadAddressableAsync(AssetReference[] assetReferences, CancellationToken token = default) => Instance.UnloadAddressableAsync(assetReferences, token);

        /// <summary>
        /// Unloads the target scene or group of scenes.
        /// </summary>
        /// <param name="addresses">
        /// An array of scenes to be unloaded.
        /// </param>
        /// <param name="token">Optional token to manually cancel the operation. Note that Unity Scene Manager operations cannot be manually canceled and will continue to run.</param>
        /// <returns>
        /// A <see cref="System.Threading.Tasks.ValueTask{TResult}"/> with all the unloaded scenes.
        /// <br/>
        /// Note that in some cases, the returned scenes might no longer have a reference to its native representation, hich means its <see cref="Scene.handle"/> will not point anywhere and you won't be able to perform equal comparisons between scenes.
        /// </returns>
        public static ValueTask<SceneResult> UnloadAddressableAsync(string[] addresses, CancellationToken token = default) => Instance.UnloadAddressableAsync(addresses, token);

        /// <summary>
        /// Unloads the target scene or group of scenes.
        /// </summary>
        /// <param name="assetReference">
        /// The target scene's <see cref="AssetReference"/> to be unloaded.
        /// </param>
        /// <param name="token">Optional token to manually cancel the operation. Note that Unity Scene Manager operations cannot be manually canceled and will continue to run.</param>
        /// <returns>
        /// A <see cref="System.Threading.Tasks.ValueTask{TResult}"/> with all the unloaded scenes.
        /// <br/>
        /// Note that in some cases, the returned scenes might no longer have a reference to its native representation, hich means its <see cref="Scene.handle"/> will not point anywhere and you won't be able to perform equal comparisons between scenes.
        /// </returns>
        public static ValueTask<SceneResult> UnloadAddressableAsync(AssetReference assetReference, CancellationToken token = default) => Instance.UnloadAddressableAsync(assetReference, token);

        /// <summary>
        /// Unloads the target scene or group of scenes.
        /// </summary>
        /// <param name="address">
        /// The target scene's addressable address to be unloaded.
        /// </param>
        /// <param name="token">Optional token to manually cancel the operation. Note that Unity Scene Manager operations cannot be manually canceled and will continue to run.</param>
        /// <returns>
        /// A <see cref="System.Threading.Tasks.ValueTask{TResult}"/> with all the unloaded scenes.
        /// <br/>
        /// Note that in some cases, the returned scenes might no longer have a reference to its native representation, hich means its <see cref="Scene.handle"/> will not point anywhere and you won't be able to perform equal comparisons between scenes.
        /// </returns>
        public static ValueTask<SceneResult> UnloadAddressableAsync(string address, CancellationToken token = default) => Instance.UnloadAddressableAsync(address, token);
#endif
        #endregion
    }
}
