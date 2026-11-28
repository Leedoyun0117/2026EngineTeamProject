using LDY.Hud.Core;
using NUnit.Framework;

namespace LDY.Tests {
    public class HudManifestTests {
        private const string Sample = @"{
  ""hud_taskbar"": { ""size"": [16, 22], ""border_LBRT"": [0, 0, 0, 3], ""imageType"": ""Sliced"" },
  ""hud_select_ylw"": { ""size"": [12, 12], ""frames"": 4, ""ms"": 120, ""border_LBRT"": [4, 4, 4, 4] },
  ""hud_icon_cut"": { ""size"": [16, 16] }
}";

        [Test]
        public void ReadsBorderInLbrtOrder() {
            var taskbar = HudManifest.Parse(Sample)["hud_taskbar"];

            Assert.AreEqual(0, taskbar.Left);
            Assert.AreEqual(0, taskbar.Bottom);
            Assert.AreEqual(0, taskbar.Right);
            Assert.AreEqual(3, taskbar.Top);
            Assert.IsTrue(taskbar.HasBorder);
            Assert.AreEqual("Sliced", taskbar.ImageType);
        }

        [Test]
        public void ReadsFramesAndPerFrameSize() {
            var select = HudManifest.Parse(Sample)["hud_select_ylw"];

            Assert.AreEqual(4, select.Frames);
            Assert.AreEqual(120, select.FrameMilliseconds);
            Assert.AreEqual(12, select.FrameWidth);
            Assert.AreEqual(12, select.Height);
            Assert.AreEqual(4, select.Left);
        }

        [Test]
        public void IconWithoutBorderIsSingleFrame() {
            var icon = HudManifest.Parse(Sample)["hud_icon_cut"];

            Assert.AreEqual(1, icon.Frames);
            Assert.IsFalse(icon.HasBorder);
            Assert.IsFalse(icon.IsTiled);
        }
    }
}
