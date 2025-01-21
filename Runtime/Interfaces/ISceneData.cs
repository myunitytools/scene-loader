using UnityEngine.SceneManagement;

namespace MyGameDevTools.SceneLoading
{
    /// <summary>
    /// Interface to link a load scene info with its async load/unload operation and the result scene.
    /// </summary>
    public interface ISceneData
    {
        /// <summary>
        /// A reference to an <see cref="IAsyncSceneOperation"/> object, that can point to either an addressable or non-addressable operation.
        /// This operation can be the load or unload operations, depending on the lifetime of the linked scene.
        /// </summary>
        IAsyncSceneOperation AsyncOperation { get; }
        /// <summary>
        /// The <see cref="ILoadSceneInfo"/> that originally requested to load the scene.
        /// </summary>
        ILoadSceneInfo LoadSceneInfo { get; }
        /// <summary>
        /// The active scene reference. Can be invalid if the scene has not yet loaded.
        /// </summary>
        Scene SceneReference { get; }

        /// <summary>
        /// Manually updates the scene reference with a given scene.
        /// Useful for linking scenes that have not been loaded through addressable operations (that can directly link the loaded scene).
        /// <br/>
        /// Should not be called outside of an <see cref="ISceneManager"/>.
        /// </summary>
        void SetSceneReferenceManually(Scene scene);

        /// <summary>
        /// Updates the <see cref="SceneReference"/> value based on the <see cref="AsyncOperation"/> result.
        /// Cannot be used for non-addressable contexts, since they are not able to directly link the loaded scene.
        /// <br/>
        /// Should not be called outside of an <see cref="ISceneManager"/>.
        /// </summary>
        void UpdateSceneReference();

        /// <summary>
        /// Returns whether this <see cref="ISceneData"/> can be matched by the given <paramref name="loadSceneInfo"/>.
        /// If the <paramref name="loadSceneInfo"/> is equal to the <see cref="ISceneData.LoadSceneInfo"/> or has a direct reference to the scene, it returns true.
        /// </summary>
        /// <param name="loadSceneInfo"><see cref="ILoadSceneInfo"/> to validate a match.</param>
        bool MatchesLoadSceneInfo(ILoadSceneInfo loadSceneInfo);

        /// <summary>
        /// Triggers the load async operation and updates the <see cref="AsyncOperation"/> reference.
        /// </summary>
        /// <returns>The load async operation.</returns>
        IAsyncSceneOperation LoadSceneAsync();

        /// <summary>
        /// Triggers the unload async operation and updates the <see cref="AsyncOperation"/> reference.
        /// </summary>
        /// <returns>The unload async operation.</returns>
        IAsyncSceneOperation UnloadSceneAsync();
    }
}
