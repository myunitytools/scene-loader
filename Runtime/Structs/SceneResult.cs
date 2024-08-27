using System.Text;
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
            _sceneArray = null;
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
            int sceneCount = (_sceneArray == null || _sceneArray.Length == 0) ? 1 : _sceneArray.Length;

            StringBuilder builder = new("{ ");
            if (sceneCount == 1)
            {
                builder.Append(_singleScene.name);
            }
            else
            {
                for (int i = 0; i < sceneCount; i++)
                {
                    if (i > 0)
                    {
                        builder.Append(",");
                    }
                    builder.Append(_sceneArray[i].name);
                }
            }

            builder.Append(" }");
            return builder.ToString();
        }
    }
}