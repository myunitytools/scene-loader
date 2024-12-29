using System.Linq;
using UnityEngine.SceneManagement;

namespace MyGameDevTools.SceneLoading
{
    public readonly struct SceneResult
    {
        readonly Scene[] _sceneArray;
        readonly Scene _singleScene;

        public SceneResult(Scene[] sceneArray)
        {
            if (sceneArray == null || sceneArray.Length == 0)
                throw new System.ArgumentException("Cannot create a `SceneResult` struct out of a null or empty scene array.", nameof(sceneArray));

            _sceneArray = sceneArray;
            _singleScene = sceneArray[0];
        }

        public SceneResult(Scene scene)
        {
            _sceneArray = new[] { scene };
            _singleScene = scene;
        }

        public readonly Scene GetScene()
        {
            return _singleScene;
        }

        public readonly Scene[] GetScenes()
        {
            return _sceneArray;
        }

        public static implicit operator Scene(SceneResult sceneResult) => sceneResult.GetScene();
        public static implicit operator Scene[](SceneResult sceneResult) => sceneResult.GetScenes();

        public override string ToString()
        {
            if (!_singleScene.IsValid())
                return "Empty SceneResult";

            return $"{{ {string.Join(", ", _sceneArray.Select(scene => scene.name))} }}";
        }
    }
}