using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#if ENABLE_ADDRESSABLES
using UnityEngine.AddressableAssets;
#endif
using UnityEngine.SceneManagement;

namespace MyGameDevTools.SceneLoading
{
    public static class SceneManagerExtensions
    {
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
        public static ValueTask<SceneResult> LoadAsync(this ISceneManager sceneManager, string[] sceneNames, int setIndexActive = -1, IProgress<float> progress = null, CancellationToken token = default)
        {
            ILoadSceneInfo[] sceneInfos = sceneNames.Select(name => (ILoadSceneInfo)new LoadSceneInfoName(name)).ToArray();
            SceneParameters sceneParams = new(sceneInfos, setIndexActive);
            return sceneManager.LoadAsync(sceneParams, progress, token);
        }

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
        public static ValueTask<SceneResult> LoadAsync(this ISceneManager sceneManager, int[] buildIndices, int setIndexActive = -1, IProgress<float> progress = null, CancellationToken token = default)
        {
            ILoadSceneInfo[] sceneInfos = buildIndices.Select(index => (ILoadSceneInfo)new LoadSceneInfoIndex(index)).ToArray();
            SceneParameters sceneParams = new(sceneInfos, setIndexActive);
            return sceneManager.LoadAsync(sceneParams, progress, token);
        }

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
        public static ValueTask<SceneResult> LoadAsync(this ISceneManager sceneManager, string sceneName, bool setActive = false, IProgress<float> progress = null, CancellationToken token = default)
        {
            SceneParameters sceneParams = new(new LoadSceneInfoName(sceneName), setActive);
            return sceneManager.LoadAsync(sceneParams, progress, token);
        }

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
        public static ValueTask<SceneResult> LoadAsync(this ISceneManager sceneManager, int buildIndex, bool setActive = false, IProgress<float> progress = null, CancellationToken token = default)
        {
            SceneParameters sceneParams = new(new LoadSceneInfoIndex(buildIndex), setActive);
            return sceneManager.LoadAsync(sceneParams, progress, token);
        }

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
        public static ValueTask<SceneResult> LoadAddressableAsync(this ISceneManager sceneManager, AssetReference[] assetReferences, int setIndexActive = -1, IProgress<float> progress = null, CancellationToken token = default)
        {
            ILoadSceneInfo[] sceneInfos = assetReferences.Select(asset => (ILoadSceneInfo)new LoadSceneInfoAssetReference(asset)).ToArray();
            SceneParameters sceneParams = new(sceneInfos, setIndexActive);
            return sceneManager.LoadAsync(sceneParams, progress, token);
        }

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
        public static ValueTask<SceneResult> LoadAddressableAsync(this ISceneManager sceneManager, string[] addresses, int setIndexActive = -1, IProgress<float> progress = null, CancellationToken token = default)
        {
            ILoadSceneInfo[] sceneInfos = addresses.Select(address => (ILoadSceneInfo)new LoadSceneInfoAddress(address)).ToArray();
            SceneParameters sceneParams = new(sceneInfos, setIndexActive);
            return sceneManager.LoadAsync(sceneParams, progress, token);
        }

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
        public static ValueTask<SceneResult> LoadAddressableAsync(this ISceneManager sceneManager, AssetReference assetReference, bool setActive = false, IProgress<float> progress = null, CancellationToken token = default)
        {
            SceneParameters sceneParams = new(new LoadSceneInfoAssetReference(assetReference), setActive);
            return sceneManager.LoadAsync(sceneParams, progress, token);
        }

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
        public static ValueTask<SceneResult> LoadAddressableAsync(this ISceneManager sceneManager, string address, bool setActive = false, IProgress<float> progress = null, CancellationToken token = default)
        {
            SceneParameters sceneParams = new(new LoadSceneInfoAddress(address), setActive);
            return sceneManager.LoadAsync(sceneParams, progress, token);
        }
#endif

        /// <summary>
        /// Triggers a transition to a group of scenes.
        /// It will transition from the current active scene (<see cref="ISceneManager.GetActiveScene()"/>)
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
        /// <param name="setIndexActive">
        /// The index of the scene to be activated as the active scene.
        /// </param>
        /// <param name="loadingSceneName">
        /// A reference to the scene that's going to be loaded as the transition intermediate (as a loading scene).
        /// If null, the transition will not have an intermediate loading scene.
        /// </param>
        /// <param name="token">Optional token to manually cancel the operation. Note that Unity Scene Manager operations cannot be manually canceled and will continue to run.</param>
        /// <returns>A <see cref="System.Threading.Tasks.ValueTask{TResult}"/> with all scenes loaded.</returns>
        public static ValueTask<SceneResult> TransitionAsync(this ISceneManager sceneManager, string[] targetSceneNames, int setIndexActive = -1, string loadingSceneName = null, CancellationToken token = default)
        {
            SceneParameters targetParams = new(targetSceneNames.Select(name => (ILoadSceneInfo)new LoadSceneInfoName(name)).ToArray(), setIndexActive);
            ILoadSceneInfo loadingSceneInfo = string.IsNullOrWhiteSpace(loadingSceneName) ? null : new LoadSceneInfoName(loadingSceneName);
            return sceneManager.TransitionAsync(targetParams, loadingSceneInfo, token);
        }

        /// <summary>
        /// Triggers a transition to a group of scenes.
        /// It will transition from the current active scene (<see cref="ISceneManager.GetActiveScene()"/>)
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
        /// <param name="setIndexActive">
        /// The index of the scene to be activated as the active scene.
        /// </param>
        /// <param name="loadingBuildIndex">
        /// A reference to the scene that's going to be loaded as the transition intermediate (as a loading scene).
        /// If null, the transition will not have an intermediate loading scene.
        /// </param>
        /// <param name="token">Optional token to manually cancel the operation. Note that Unity Scene Manager operations cannot be manually canceled and will continue to run.</param>
        /// <returns>A <see cref="System.Threading.Tasks.ValueTask{TResult}"/> with all scenes loaded.</returns>
        public static ValueTask<SceneResult> TransitionAsync(this ISceneManager sceneManager, int[] targetBuildIndices, int setIndexActive = -1, int? loadingBuildIndex = null, CancellationToken token = default)
        {
            SceneParameters targetParams = new(targetBuildIndices.Select(index => (ILoadSceneInfo)new LoadSceneInfoIndex(index)).ToArray(), setIndexActive);
            ILoadSceneInfo loadingSceneInfo = loadingBuildIndex.HasValue ? new LoadSceneInfoIndex(loadingBuildIndex.Value) : null;
            return sceneManager.TransitionAsync(targetParams, loadingSceneInfo, token);
        }

        /// <summary>
        /// Triggers a transition to the target scene.
        /// It will transition from the current active scene (<see cref="ISceneManager.GetActiveScene()"/>)
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
        /// <param name="setIndexActive">
        /// The index of the scene to be activated as the active scene.
        /// </param>
        /// <param name="loadingSceneName">
        /// A reference to the scene that's going to be loaded as the transition intermediate (as a loading scene).
        /// If null, the transition will not have an intermediate loading scene.
        /// </param>
        /// <param name="token">Optional token to manually cancel the operation. Note that Unity Scene Manager operations cannot be manually canceled and will continue to run.</param>
        /// <returns>A <see cref="System.Threading.Tasks.ValueTask{TResult}"/> with all scenes loaded.</returns>
        public static ValueTask<SceneResult> TransitionAsync(this ISceneManager sceneManager, string targetSceneName, bool setActive = false, string loadingSceneName = null, CancellationToken token = default)
        {
            SceneParameters targetParams = new(new LoadSceneInfoName(targetSceneName), setActive);
            ILoadSceneInfo loadingSceneInfo = string.IsNullOrWhiteSpace(loadingSceneName) ? null : new LoadSceneInfoName(loadingSceneName);
            return sceneManager.TransitionAsync(targetParams, loadingSceneInfo, token);
        }

        /// <summary>
        /// Triggers a transition to the target scene.
        /// It will transition from the current active scene (<see cref="ISceneManager.GetActiveScene()"/>)
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
        /// <param name="setIndexActive">
        /// The index of the scene to be activated as the active scene.
        /// </param>
        /// <param name="loadingBuildIndex">
        /// A reference to the scene that's going to be loaded as the transition intermediate (as a loading scene).
        /// If null, the transition will not have an intermediate loading scene.
        /// </param>
        /// <param name="token">Optional token to manually cancel the operation. Note that Unity Scene Manager operations cannot be manually canceled and will continue to run.</param>
        /// <returns>A <see cref="System.Threading.Tasks.ValueTask{TResult}"/> with all scenes loaded.</returns>
        public static ValueTask<SceneResult> TransitionAsync(this ISceneManager sceneManager, int targetBuildIndex, bool setActive = false, int? loadingBuildIndex = null, CancellationToken token = default)
        {
            SceneParameters targetParams = new(new LoadSceneInfoIndex(targetBuildIndex), setActive);
            ILoadSceneInfo loadingSceneInfo = loadingBuildIndex.HasValue ? new LoadSceneInfoIndex(loadingBuildIndex.Value) : null;
            return sceneManager.TransitionAsync(targetParams, loadingSceneInfo, token);
        }

#if ENABLE_ADDRESSABLES
        /// <summary>
        /// Triggers a transition to a group of scenes.
        /// It will transition from the current active scene (<see cref="ISceneManager.GetActiveScene()"/>)
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
        /// <param name="setIndexActive">
        /// The index of the scene to be activated as the active scene.
        /// </param>
        /// <param name="loadingAssetReference">
        /// A reference to the scene that's going to be loaded as the transition intermediate (as a loading scene).
        /// If null, the transition will not have an intermediate loading scene.
        /// </param>
        /// <param name="token">Optional token to manually cancel the operation. Note that Unity Scene Manager operations cannot be manually canceled and will continue to run.</param>
        /// <returns>A <see cref="System.Threading.Tasks.ValueTask{TResult}"/> with all scenes loaded.</returns>
        public static ValueTask<SceneResult> TransitionAddressableAsync(this ISceneManager sceneManager, AssetReference[] targetAssetReferences, int setIndexActive = -1, AssetReference loadingAssetReference = null, CancellationToken token = default)
        {
            SceneParameters targetParams = new(targetAssetReferences.Select(asset => (ILoadSceneInfo)new LoadSceneInfoAssetReference(asset)).ToArray(), setIndexActive);
            ILoadSceneInfo loadingSceneInfo = loadingAssetReference != null ? new LoadSceneInfoAssetReference(loadingAssetReference) : null;
            return sceneManager.TransitionAsync(targetParams, loadingSceneInfo, token);
        }

        /// <summary>
        /// Triggers a transition to a group of scenes.
        /// It will transition from the current active scene (<see cref="ISceneManager.GetActiveScene()"/>)
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
        /// <param name="setIndexActive">
        /// The index of the scene to be activated as the active scene.
        /// </param>
        /// <param name="loadingAddress">
        /// A reference to the scene that's going to be loaded as the transition intermediate (as a loading scene).
        /// If null, the transition will not have an intermediate loading scene.
        /// </param>
        /// <param name="token">Optional token to manually cancel the operation. Note that Unity Scene Manager operations cannot be manually canceled and will continue to run.</param>
        /// <returns>A <see cref="System.Threading.Tasks.ValueTask{TResult}"/> with all scenes loaded.</returns>
        public static ValueTask<SceneResult> TransitionAddressableAsync(this ISceneManager sceneManager, string[] targetAddresses, int setIndexActive = -1, string loadingAddress = null, CancellationToken token = default)
        {
            SceneParameters targetParams = new(targetAddresses.Select(address => (ILoadSceneInfo)new LoadSceneInfoAddress(address)).ToArray(), setIndexActive);
            ILoadSceneInfo loadingSceneInfo = string.IsNullOrWhiteSpace(loadingAddress) ? null : new LoadSceneInfoAddress(loadingAddress);
            return sceneManager.TransitionAsync(targetParams, loadingSceneInfo, token);
        }

        /// <summary>
        /// Triggers a transition to the target scene.
        /// It will transition from the current active scene (<see cref="ISceneManager.GetActiveScene()"/>)
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
        /// <param name="setIndexActive">
        /// The index of the scene to be activated as the active scene.
        /// </param>
        /// <param name="loadingAssetReference">
        /// A reference to the scene that's going to be loaded as the transition intermediate (as a loading scene).
        /// If null, the transition will not have an intermediate loading scene.
        /// </param>
        /// <param name="token">Optional token to manually cancel the operation. Note that Unity Scene Manager operations cannot be manually canceled and will continue to run.</param>
        /// <returns>A <see cref="System.Threading.Tasks.ValueTask{TResult}"/> with all scenes loaded.</returns>
        public static ValueTask<SceneResult> TransitionAddressableAsync(this ISceneManager sceneManager, AssetReference targetAssetReference, bool setActive = false, AssetReference loadingAssetReference = null, CancellationToken token = default)
        {
            SceneParameters targetParams = new(new LoadSceneInfoAssetReference(targetAssetReference), setActive);
            ILoadSceneInfo loadingSceneInfo = loadingAssetReference != null ? new LoadSceneInfoAssetReference(loadingAssetReference) : null;
            return sceneManager.TransitionAsync(targetParams, loadingSceneInfo, token);
        }

        /// <summary>
        /// Triggers a transition to the target scene.
        /// It will transition from the current active scene (<see cref="ISceneManager.GetActiveScene()"/>)
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
        /// <param name="setIndexActive">
        /// The index of the scene to be activated as the active scene.
        /// </param>
        /// <param name="loadingAddress">
        /// A reference to the scene that's going to be loaded as the transition intermediate (as a loading scene).
        /// If null, the transition will not have an intermediate loading scene.
        /// </param>
        /// <param name="token">Optional token to manually cancel the operation. Note that Unity Scene Manager operations cannot be manually canceled and will continue to run.</param>
        /// <returns>A <see cref="System.Threading.Tasks.ValueTask{TResult}"/> with all scenes loaded.</returns>
        public static ValueTask<SceneResult> TransitionAddressableAsync(this ISceneManager sceneManager, string targetAddress, bool setActive = false, string loadingAddress = null, CancellationToken token = default)
        {
            SceneParameters targetParams = new(new LoadSceneInfoAddress(targetAddress), setActive);
            ILoadSceneInfo loadingSceneInfo = string.IsNullOrWhiteSpace(loadingAddress) ? null : new LoadSceneInfoAddress(loadingAddress);
            return sceneManager.TransitionAsync(targetParams, loadingSceneInfo, token);
        }
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
        public static ValueTask<SceneResult> UnloadAsync(this ISceneManager sceneManager, string[] sceneNames, CancellationToken token = default)
        {
            ILoadSceneInfo[] sceneInfos = sceneNames.Select(name => (ILoadSceneInfo)new LoadSceneInfoName(name)).ToArray();
            SceneParameters sceneParams = new(sceneInfos);
            return sceneManager.UnloadAsync(sceneParams, token);
        }

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
        public static ValueTask<SceneResult> UnloadAsync(this ISceneManager sceneManager, int[] buildIndices, CancellationToken token = default)
        {
            ILoadSceneInfo[] sceneInfos = buildIndices.Select(index => (ILoadSceneInfo)new LoadSceneInfoIndex(index)).ToArray();
            SceneParameters sceneParams = new(sceneInfos);
            return sceneManager.UnloadAsync(sceneParams, token);
        }

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
        public static ValueTask<SceneResult> UnloadAsync(this ISceneManager sceneManager, Scene[] scenes, CancellationToken token = default)
        {
            ILoadSceneInfo[] sceneInfos = scenes.Select(scene => (ILoadSceneInfo)new LoadSceneInfoScene(scene)).ToArray();
            SceneParameters sceneParams = new(sceneInfos);
            return sceneManager.UnloadAsync(sceneParams, token);
        }

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
        public static ValueTask<SceneResult> UnloadAsync(this ISceneManager sceneManager, string sceneName, CancellationToken token = default)
        {
            SceneParameters sceneParams = new(new LoadSceneInfoName(sceneName));
            return sceneManager.UnloadAsync(sceneParams, token);
        }

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
        public static ValueTask<SceneResult> UnloadAsync(this ISceneManager sceneManager, int buildIndex, CancellationToken token = default)
        {
            SceneParameters sceneParams = new(new LoadSceneInfoIndex(buildIndex));
            return sceneManager.UnloadAsync(sceneParams, token);
        }

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
        public static ValueTask<SceneResult> UnloadAsync(this ISceneManager sceneManager, Scene scene, CancellationToken token = default)
        {
            SceneParameters sceneParams = new(new LoadSceneInfoScene(scene));
            return sceneManager.UnloadAsync(sceneParams, token);
        }

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
        public static ValueTask<SceneResult> UnloadAddressableAsync(this ISceneManager sceneManager, AssetReference[] assetReferences, CancellationToken token = default)
        {
            ILoadSceneInfo[] sceneInfos = assetReferences.Select(asset => (ILoadSceneInfo)new LoadSceneInfoAssetReference(asset)).ToArray();
            SceneParameters sceneParams = new(sceneInfos);
            return sceneManager.UnloadAsync(sceneParams, token);
        }

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
        public static ValueTask<SceneResult> UnloadAddressableAsync(this ISceneManager sceneManager, string[] addresses, CancellationToken token = default)
        {
            ILoadSceneInfo[] sceneInfos = addresses.Select(address => (ILoadSceneInfo)new LoadSceneInfoAddress(address)).ToArray();
            SceneParameters sceneParams = new(sceneInfos);
            return sceneManager.UnloadAsync(sceneParams, token);
        }

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
        public static ValueTask<SceneResult> UnloadAddressableAsync(this ISceneManager sceneManager, AssetReference assetReference, CancellationToken token = default)
        {
            SceneParameters sceneParams = new(new LoadSceneInfoAssetReference(assetReference));
            return sceneManager.UnloadAsync(sceneParams, token);
        }

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
        public static ValueTask<SceneResult> UnloadAddressableAsync(this ISceneManager sceneManager, string address, CancellationToken token = default)
        {
            SceneParameters sceneParams = new(new LoadSceneInfoAddress(address));
            return sceneManager.UnloadAsync(sceneParams, token);
        }
#endif
    }
}