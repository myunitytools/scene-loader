using System;
using UnityEngine.SceneManagement;

namespace MyGameDevTools.SceneLoading
{
    /// <summary>
    /// Interface to standardize addressable and non-addressable scene operations.
    /// </summary>
    public interface IAsyncSceneOperation
    {
        /// <summary>
        /// Event fired when the async operation is done.
        /// </summary>
        event Action Completed;

        /// <summary>
        /// Progress of the load/unload operation from 0 to 1 (float).
        /// </summary>
        float Progress { get; }
        /// <summary>
        /// Whether the load/unload operation is done.
        /// </summary>
        bool IsDone { get; }
        /// <summary>
        /// Whether this operation can directly reference the loaded scene.
        /// True for addressable operations and false otherwise.
        /// </summary>
        bool HasDirectReferenceToScene { get; }

        /// <summary>
        /// Returns the scene reference from the internal async operation.
        /// Only works for addressable operations.
        /// </summary>
        Scene GetResult();
    }
}
