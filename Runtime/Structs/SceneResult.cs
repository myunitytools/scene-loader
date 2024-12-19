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
            if (!_singleScene.IsValid())
                return "Empty SceneResult";

            int sceneCount = (_sceneArray == null || _sceneArray.Length == 0) ? 1 : _sceneArray.Length;

            StringBuilder builder = new("{ ");
            if (sceneCount == 1)
            {
                builder.Append(_singleScene.name);
                builder.Append(" }");
                return builder.ToString();
            }

            for (int i = 0; i < sceneCount; i++)
            {
                if (i > 0)
                {
                    builder.Append(", ");
                }
                builder.Append(_sceneArray[i].name);
            }

            builder.Append(" }");
            return builder.ToString();
        }
    }

    public readonly struct SceneParameter
    {
        readonly ILoadSceneInfo[] _loadSceneInfoArray;
        readonly ILoadSceneInfo _singleLoadSceneInfo;
        readonly int _setIndexActive;

        public SceneParameter(ILoadSceneInfo[] loadSceneInfos, int setIndexActive = -1)
        {
            _loadSceneInfoArray = loadSceneInfos;
            _singleLoadSceneInfo = loadSceneInfos[0];
            _setIndexActive = setIndexActive;
        }

        public SceneParameter(ILoadSceneInfo loadSceneInfo, bool setActive = false)
        {
            _loadSceneInfoArray = null;
            _singleLoadSceneInfo = loadSceneInfo;
            _setIndexActive = setActive ? 0 : -1;
        }

        public readonly ILoadSceneInfo GetLoadSceneInfo()
        {
            return _singleLoadSceneInfo;
        }

        public readonly ILoadSceneInfo[] GetLoadSceneInfos()
        {
            return _loadSceneInfoArray;
        }

        public readonly bool ShouldSetActive()
        {
            return _setIndexActive == 0;
        }

        public readonly int GetIndexActive()
        {
            return _setIndexActive;
        }
    }
}