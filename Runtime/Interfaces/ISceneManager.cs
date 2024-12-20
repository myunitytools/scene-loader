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
        ValueTask<SceneResult> TransitionAsync(SceneParameters sceneParameters, ILoadSceneInfo intermediateSceneReference = default, CancellationToken token = default);

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
        ValueTask<SceneResult> LoadAsync(SceneParameters sceneParameters, IProgress<float> progress = null, CancellationToken token = default);

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
        ValueTask<SceneResult> UnloadAsync(SceneParameters sceneParameters, CancellationToken token = default);

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