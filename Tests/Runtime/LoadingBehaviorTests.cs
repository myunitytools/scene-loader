using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace MyGameDevTools.SceneLoading.Tests
{
    public class LoadingBehaviorTests
    {
        [OneTimeTearDown]
        public void Teardown()
        {
#if UNITY_2023_2_OR_NEWER
            var behaviors = Object.FindObjectsByType<LoadingBehavior>(FindObjectsSortMode.None);
#else
            var behaviors = Object.FindObjectsOfType<LoadingBehavior>();
#endif
            foreach (var b in behaviors)
                Object.DestroyImmediate(b.gameObject);
        }

        [UnityTest]
        public IEnumerator AutomaticTriggers()
        {
            var behavior = new GameObject().AddComponent<LoadingBehavior>();
            var progress = behavior.Progress;

            yield return null;

            progress.Report(1);
            progress.SetLoadingCompleted();

            Assert.True(progress.TransitionOutTask.Task.Result);
        }

        [UnityTest]
        public IEnumerator ManualTriggers()
        {
            var behavior = new GameObject().AddComponent<LoadingBehavior>();

            behavior.waitForScriptedStart = true;
            behavior.waitForScriptedEnd = true;

            bool completed = false;

            var progress = behavior.Progress;
            progress.LoadingCompleted += () => completed = true;
            yield return null;

            progress.StartTransition();
            Assert.True(progress.TransitionInTask.Task.Result);

            progress.Report(1);
            progress.SetLoadingCompleted();

            yield return null;
            Assert.True(completed);

            progress.EndTransition();
            Assert.True(progress.TransitionOutTask.Task.Result);
        }
    }
}