using UnityEngine;

namespace MyGameDevTools.SceneLoading
{
    /// <summary>
    /// Component responsible for handling the loading progress report through its <see cref="Progress"/> property.
    /// Use <see cref="Progress"/> to add listener to scene loading progress events or to control loading screen transitions.
    /// <br/>
    /// This component is located by the <see cref="ISceneLoader"/> during a scene transition.
    /// </summary>
    [AddComponentMenu("Scene Loading/Loading Behavior")]
    public class LoadingBehavior : MonoBehaviour
    {
        public LoadingProgress Progress { get; private set; }

        [Tooltip("Should it wait for an animation or script to allow starting the transition?")]
        public bool waitForScriptedStart;
        [Tooltip("Should it wait for an animation or script to allow finishing the transition?")]
        public bool waitForScriptedEnd;

        void Awake()
        {
            Progress = new LoadingProgress();
            Progress.LoadingCompleted += OnLoadingCompleted;
        }

        void Start()
        {
            if (!waitForScriptedStart)
                Progress.StartTransition();
        }

        void OnLoadingCompleted()
        {
            if (!waitForScriptedEnd)
                Progress.EndTransition();
        }
    }
}