using NUnit.Framework;

namespace MyGameDevTools.SceneLoading.Tests
{
    public class LoadingProgressTests
    {
        [Test]
        public void SetState_Test()
        {
            var progress = new LoadingProgress();

            bool completed = false;
            progress.LoadingCompleted += () => completed = true;

            progress.SetLoadingCompleted();
            Assert.True(completed);
        }

        [Test]
        public void Progress_Test()
        {
            var progress = new LoadingProgress();

            float reportedValue = 0;
            progress.Progressed += value => reportedValue = value;

            progress.Report(.5f);
            Assert.AreEqual(.5f, reportedValue);

            progress.Report(1);
            Assert.AreEqual(1, reportedValue);

            progress.Report(2);
            Assert.AreEqual(1, reportedValue);
        }
    }
}