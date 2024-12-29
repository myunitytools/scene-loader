namespace MyGameDevTools.SceneLoading
{
    public readonly struct SceneParameters
    {
        public readonly int Length => _loadSceneInfoArray.Length;

        readonly ILoadSceneInfo[] _loadSceneInfoArray;
        readonly ILoadSceneInfo _singleLoadSceneInfo;
        readonly int _setIndexActive;

        public SceneParameters(ILoadSceneInfo[] loadSceneInfos, int setIndexActive = -1)
        {
            _loadSceneInfoArray = loadSceneInfos;
            _singleLoadSceneInfo = loadSceneInfos[0];
            _setIndexActive = setIndexActive;
        }

        public SceneParameters(ILoadSceneInfo loadSceneInfo, bool setActive = false)
        {
            _loadSceneInfoArray = new ILoadSceneInfo[] { loadSceneInfo };
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
            return _setIndexActive >= 0;
        }

        public readonly int GetIndexToActivate()
        {
            return _setIndexActive;
        }
    }
}