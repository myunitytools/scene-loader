using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace MyGameDevTools.SceneLoading.Tests
{
    public class LoadingFaderTests
    {
        [OneTimeTearDown]
        public void Teardown()
        {
#if UNITY_2023_2_OR_NEWER
            Object.DestroyImmediate(Object.FindAnyObjectByType<LoadingBehavior>().gameObject);
            Object.DestroyImmediate(Object.FindAnyObjectByType<LoadingFader>().gameObject);
#else
            Object.DestroyImmediate(Object.FindObjectOfType<LoadingBehavior>().gameObject);
            Object.DestroyImmediate(Object.FindObjectOfType<LoadingFader>().gameObject);
#endif
        }

        [UnityTest]
        public IEnumerator FadeInOut()
        {
            var loadingBehavior = new GameObject().AddComponent<LoadingBehavior>();
            var progress = loadingBehavior.Progress;

            loadingBehavior.waitForScriptedStart = true;
            loadingBehavior.waitForScriptedEnd = true;

            var loadingFader = new GameObject("Fader", typeof(CanvasGroup)).AddComponent<LoadingFader>();
            var canvasGroup = loadingFader.GetComponent<CanvasGroup>();

            loadingFader.fadeTime = .2f;
            loadingFader.loadingBehavior = loadingBehavior;

            Assert.AreEqual(0, canvasGroup.alpha);
            yield return new WaitForSeconds(loadingFader.fadeTime * 2);

            Assert.True(progress.TransitionInTask.Task.IsCompletedSuccessfully);
            Assert.AreEqual(1, canvasGroup.alpha);

            progress.SetLoadingCompleted();

            yield return new WaitForSeconds(loadingFader.fadeTime * 2);

            Assert.True(progress.TransitionOutTask.Task.IsCompletedSuccessfully);
            Assert.AreEqual(0, canvasGroup.alpha);
        }
    }
}