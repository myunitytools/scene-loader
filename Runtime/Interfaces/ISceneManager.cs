using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace MyGameDevTools.SceneLoading
{
    /// <summary>
    /// Interface to standardize scene management operations.
    /// The scene manager is responsible for scene loading operations, keeping track of its loaded scene stack and dispatching scene load events.
    /// </summary>
    public interface ISceneManager : IDisposable
    {
        /// <summary>
        /// Reports that the active scene has changed, passing the <b>previous</b> and <b>current</b> active scene as parameters.
        /// <br/>
        /// In some scenarios, the previous or the current scene might be invalid <i>(you can check it through <see cref="Scene.IsValid()"/>)</i>, but never both at the same time.
        /// <br/>
        /// This can occur when the first active scene is being set (there was no previous active scene) or when the last scene gets unloaded (leaving no other scene to be activated).
        /// </summary>
        event Action<Scene, Scene> ActiveSceneChanged;
        /// <summary>
        /// Reports when a scene gets unloaded.
        /// </summary>
        event Action<Scene> SceneUnloaded;
        /// <summary>
        /// Reports when a scene gets loaded.
        /// </summary>
        event Action<Scene> SceneLoaded;

        /// <summary>
        /// The amount of scenes loaded through this <see cref="ISceneManager"/>.
        /// To get the total amount of loaded scenes, check <see cref="SceneManager.sceneCount"/>.
        /// </summary>
        int LoadedSceneCount { get; }
        /// <summary>
        /// The amount of scenes managed by this <see cref="ISceneManager"/>.
        /// This includes scenes that are being unloaded.
        /// </summary>
        int TotalSceneCount { get; }

        /// <summary>
        /// Sets the target <paramref name="scene"/> as the active scene.
        /// Internally calls <see cref="SceneManager.SetActiveScene(Scene)"/>.
        /// </summary>
        /// <param name="scene">Scene to be enabled as the active scene.</param>
        void SetActiveScene(Scene scene);

        /// <summary>
        /// Triggers a transition to a group of scens.
        /// It will transition from the current active scene (<see cref="GetActiveScene()"/>)
        /// to a group of scenes (<paramref name="targetScenes"/>), with an optional intermediate loading scene (<paramref name="intermediateSceneInfo"/>).
        /// If the <paramref name="intermediateSceneInfo"/> is not set, the transition will have no intermediate loading scene and will instead simply load the target scene directly.
        /// The complete transition flow is:
        /// <br/><br/>
        /// 1. Load the intermediate scene (if provided).<br/>
        /// 2. Unload the source scene (if any).<br/>
        /// 3. Load all target scenes.<br/>
        /// 4. Unload the intermediate scene (if provided).<br/>
        /// </summary>
        /// <param name="targetScenes">
        /// A reference to all scenes that will be transitioned to.
        /// </param>
        /// <param name="setIndexActive">
        /// Index of the scene in the <paramref name="targetScenes"/> to be set as the active scene.
        /// </param>
        /// <param name="intermediateSceneInfo">
        /// A reference to the scene that's going to be loaded as the transition intermediate (as a loading scene).
        /// If null, the transition will not have an intermediate loading scene.
        /// </param>
        /// <param name="token">Optional token to manually cancel the operation. Note that Unity Scene Manager operations cannot be manually canceled and will continue to run.</param>
        /// <returns>A <see cref="System.Threading.Tasks.ValueTask{TResult}"/> with all scenes loaded.</returns>
        ValueTask<Scene[]> TransitionToScenesAsync(ILoadSceneInfo[] targetScenes, int setIndexActive, ILoadSceneInfo intermediateSceneReference = default, CancellationToken token = default);

        /// <summary>
        /// Triggers a scene transition.
        /// It will transition from the current active scene (<see cref="GetActiveScene()"/>)
        /// to the target scene (<paramref name="targetSceneInfo"/>), with an optional intermediate loading scene (<paramref name="intermediateSceneInfo"/>).
        /// If the <paramref name="intermediateSceneInfo"/> is not set, the transition will have no intermediate loading scene and will instead simply load the target scene directly.
        /// The complete transition flow is:
        /// <br/><br/>
        /// 1. Load the intermediate scene (if provided).<br/>
        /// 2. Unload the source scene (if any).<br/>
        /// 3. Load the target scene.<br/>
        /// 4. Unload the intermediate scene (if provided).<br/>
        /// </summary>
        /// <param name="targetSceneInfo">
        /// A reference to the scene that's going to be transitioned to.
        /// </param>
        /// <param name="intermediateSceneInfo">
        /// A reference to the scene that's going to be loaded as the transition intermediate (as a loading scene).
        /// If null, the transition will not have an intermediate loading scene.
        /// </param>
        /// <param name="token">Optional token to manually cancel the operation. Note that Unity Scene Manager operations cannot be manually canceled and will continue to run.</param>
        /// <returns>A <see cref="System.Threading.Tasks.ValueTask{TResult}"/> with the loaded scene as the result.</returns>
        ValueTask<Scene> TransitionToSceneAsync(ILoadSceneInfo targetSceneReference, ILoadSceneInfo intermediateSceneReference = default, CancellationToken token = default);

        /// <summary>
        /// Loads all scenes provided by the <paramref name="sceneInfos"/> array in parallel.
        /// You may also provide the desired index to set as the active scene through the <paramref name="setIndexActive"/> parameter.
        /// Also, you can pass an <see cref="IProgress{T}"/> object to receive the average progress of all loading operations, from 0 to 1.
        /// </summary>
        /// <param name="sceneInfos">References to all scenes to load.</param>
        /// <param name="setIndexActive">Index of the desired scene to set active, based on the <paramref name="sceneInfos"/> array.</param>
        /// <param name="progress">Object to report the loading operations progress to, from 0 to 1.</param>
        /// <param name="token">Optional token to manually cancel the operation. Note that Unity Scene Manager operations cannot be manually canceled and will continue to run.</param>
        /// <returns>A <see cref="System.Threading.Tasks.ValueTask{TResult}"/> with all scenes loaded.</returns>
        /// <exception cref="ArgumentException">When scene info group is null, empty or the setIndexName is bigger than the scene length.</exception>
        /// <exception cref="InvalidOperationException">When the provided scene info group fails to produce valid load scene operations.</exception>
        ValueTask<Scene[]> LoadScenesAsync(ILoadSceneInfo[] sceneInfos, int setIndexActive = -1, IProgress<float> progress = null, CancellationToken token = default);

        /// <summary>
        /// Loads a scene referenced by the <paramref name="sceneInfo"/>, optionally enabling it as the active scene.
        /// Also, you can pass an <see cref="IProgress{T}"/> object to receive the progress of the loading operation, from 0 to 1.
        /// </summary>
        /// <param name="sceneInfo">A reference to the scene that's going to be loaded.</param>
        /// <param name="setActive">Should the loaded scene be enabled as the active scene?</param>
        /// <param name="progress">Object to report the loading operation progress to, from 0 to 1.</param>
        /// <param name="token">Optional token to manually cancel the operation. Note that Unity Scene Manager operations cannot be manually canceled and will continue to run.</param>
        /// <returns>A <see cref="System.Threading.Tasks.ValueTask{TResult}"/> with the loaded scene as the result.</returns>
        /// <exception cref="ArgumentException">When scene info is null.</exception>
        /// <exception cref="InvalidOperationException">When the provided scene info fails to produce a valid load scene operation.</exception>
        ValueTask<Scene> LoadSceneAsync(ILoadSceneInfo sceneInfo, bool setActive = false, IProgress<float> progress = null, CancellationToken token = default);

        /// <summary>
        /// Unloads all scenes provided by the <paramref name="sceneInfos"/> array in parallel.
        /// </summary>
        /// <param name="sceneInfos">Reference to all scenes to unload.</param>
        /// <param name="token">Optional token to manually cancel the operation. Note that Unity Scene Manager operations cannot be manually canceled and will continue to run.</param>
        /// <returns>
        /// A <see cref="System.Threading.Tasks.ValueTask{TResult}"/> with all the unloaded scenes.
        /// <br/>
        /// Note that in some cases, the returned scenes might no longer have a reference to its native representation, hich means its <see cref="Scene.handle"/> will not point anywhere and you won't be able to perform equal comparisons between scenes.
        /// </returns>
        /// <exception cref="ArgumentException">When scene info group is null or empty.</exception>
        /// <exception cref="InvalidOperationException">When the provided scene info group fails to produce valid unload scene operations.</exception>
        ValueTask<Scene[]> UnloadScenesAsync(ILoadSceneInfo[] sceneInfos, CancellationToken token = default);

        /// <summary>
        /// Unloads a scene referenced by the <paramref name="sceneInfo"/>.
        /// </summary>
        /// <param name="sceneInfo">A reference to the scene that's going to be unloaded.</param>
        /// <param name="token">Optional token to manually cancel the operation. Note that Unity Scene Manager operations cannot be manually canceled and will continue to run.</param>
        /// <returns>
        /// A <see cref="System.Threading.Tasks.ValueTask{TResult}"/> with the unloaded scene as the result.
        /// <br/>
        /// Note that in some cases, the returned scene might no longer have a reference to its native representation, which means its <see cref="Scene.handle"/> will not point anywhere and you won't be able to perform equal comparisons between scenes.
        /// </returns>
        /// <exception cref="ArgumentException">When scene info is null.</exception>
        /// <exception cref="InvalidOperationException">When the provided scene info fails to produce a valid unload scene operation.</exception>
        ValueTask<Scene> UnloadSceneAsync(ILoadSceneInfo sceneInfo, CancellationToken token = default);

        /// <summary>
        /// Gets the current active scene in this <see cref="ISceneManager"/> instance.
        /// This should point to the same scene you get via <see cref="SceneManager.GetActiveScene()"/> if it was loaded through this <see cref="ISceneManager"/>.
        /// </summary>
        /// <returns>The current active scene, or an invalid scene if none of the loaded scenes are enabled as the active scene.</returns>
        Scene GetActiveScene();

        /// <summary>
        /// Gets the loaded scene at the <paramref name="index"/> of the loaded scenes list.
        /// </summary>
        /// <param name="index">Index of the target scene in the loaded scenes list.</param>
        /// <returns>The loaded scene at the <paramref name="index"/> of the loaded scenes list.</returns>
        Scene GetLoadedSceneAt(int index);

        /// <summary>
        /// Gets the last loaded scene of this <see cref="ISceneManager"/>.
        /// </summary>
        /// <returns>The last loaded scene, or an invalid scene if there are no loaded scenes in this <see cref="ISceneManager"/>.</returns>
        Scene GetLastLoadedScene();

        /// <summary>
        /// Gets a loaded scene by its <paramref name="name"/>.
        /// </summary>
        /// <param name="name">Name of the loaded scene to be found.</param>
        /// <returns>A loaded scene with the given <paramref name="name"/>.</returns>
        Scene GetLoadedSceneByName(string name);
    }
}