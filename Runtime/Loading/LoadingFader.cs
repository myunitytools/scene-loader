using System.Collections;
using UnityEngine;

namespace MyGameDevTools.SceneLoading
{
    [AddComponentMenu("Scene Loading/Loading Fader")]
    [RequireComponent(typeof(CanvasGroup))]
    public class LoadingFader : MonoBehaviour
    {
        public LoadingBehavior loadingBehavior;
        [Range(.05f, 5)]
        public float fadeTime = 1;

        [SerializeField]
        AnimationCurve _fadeOutCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
        [SerializeField]
        AnimationCurve _fadeInCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        LoadingProgress _loadingProgress;
        CanvasGroup _canvasGroup;

        void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _canvasGroup.alpha = 0;
        }

        void Start()
        {
            _loadingProgress = loadingBehavior.Progress;
            _loadingProgress.LoadingCompleted += FadeOut;
            FadeIn();
        }

        void FadeOut()
        {
            StartCoroutine(fadeOutRoutine());
            IEnumerator fadeOutRoutine()
            {
                yield return FadeRoutine(_fadeOutCurve);
                _loadingProgress.EndTransition();
            }
        }

        void FadeIn()
        {
            StartCoroutine(fadeInRoutine());
            IEnumerator fadeInRoutine()
            {
                yield return FadeRoutine(_fadeInCurve);
                _loadingProgress.StartTransition();
            }
        }

        IEnumerator FadeRoutine(AnimationCurve fadeCurve)
        {
            var time = 0f;
            while (time < fadeTime)
            {
                time += Time.deltaTime;
                _canvasGroup.alpha = fadeCurve.Evaluate(time / fadeTime);
                yield return null;
            }
        }
    }
}