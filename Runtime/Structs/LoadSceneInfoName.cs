using System;
using UnityEngine.SceneManagement;

namespace MyGameDevTools.SceneLoading
{
    /// <summary>
    /// Struct to manage scene operations with the scene's name. Implements <see cref="ILoadSceneInfo"/>.
    /// </summary>
    public readonly struct LoadSceneInfoName : ILoadSceneInfo
    {
        public readonly LoadSceneInfoType Type => LoadSceneInfoType.Name;

        public readonly object Reference => _sceneNameOrPath;

        readonly string _sceneNameOrPath;

        /// <summary>
        /// Creates a new <see cref="ILoadSceneInfo"/> based on the scene's name or path.
        /// The scene must be added to the Build Settings in order to be loaded.
        /// </summary>
        /// <param name="sceneNameOrPath">The scene's asset name or path, as displayed in the Build Settings window.</param>
        public LoadSceneInfoName(string sceneNameOrPath)
        {
            if (string.IsNullOrWhiteSpace(sceneNameOrPath))
                throw new ArgumentException("Cannot create a LoadSceneInfoName from an empty string.", nameof(sceneNameOrPath));
            _sceneNameOrPath = sceneNameOrPath;
        }

        public bool CanBeReferenceToScene(Scene scene) => scene.name == _sceneNameOrPath || scene.path == _sceneNameOrPath;

        public override string ToString()
        {
            return $"Scene with name/path '{_sceneNameOrPath}'";
        }

        public bool Equals(ILoadSceneInfo other)
        {
            return Type == other.Type && Reference.Equals(other.Reference);
        }
    }
}