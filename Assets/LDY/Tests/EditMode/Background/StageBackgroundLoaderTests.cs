using System.Collections.Generic;
using System.IO;
using System.Linq;
using LDY.Background;
using LDY.Background.Core;
using NUnit.Framework;
using UnityEditor;

namespace LDY.Tests.Background {
    public class StageBackgroundLoaderTests {
        private static IEnumerable<string> SourcePaths() =>
            AssetDatabase.FindAssets("t:" + nameof(StageBackgroundSource)).Select(AssetDatabase.GUIDToAssetPath).OrderBy(p => p);

        [Test]
        public void 스테이지_소스는_3개() {
            Assert.That(SourcePaths().Count(), Is.EqualTo(3));
        }

        [TestCaseSource(nameof(SourcePaths))]
        public void 소스_json이_파싱된다(string sourcePath) {
            var source = AssetDatabase.LoadAssetAtPath<StageBackgroundSource>(sourcePath);
            if (!File.Exists(source.JsonPath)) Assert.Ignore("데이터 없음: " + source.JsonPath);

            var layout = Load(source);

            Assert.That(layout.Texts, Is.Not.Empty);
            Assert.That(layout.RequiredFiles.Where(f => !File.Exists(f)), Is.Empty);
        }

        [Test]
        public void Stage1_정규화_기본값() {
            var source = SourcePaths().Select(p => AssetDatabase.LoadAssetAtPath<StageBackgroundSource>(p))
                .First(s => s.FolderPath.EndsWith("Stage1BG"));
            var layout = Load(source);

            Assert.That(layout.StaticFile, Is.EqualTo(source.FolderPath + "/window_base_notext.png"));
            Assert.That(layout.ReferenceFile, Is.EqualTo(source.FolderPath + "/reference_window_with_text.png"));
            Assert.That(layout.Sheets.Single().FrameMs, Is.EqualTo(120f));
            Assert.That(layout.Sheets.Single().Frames, Is.EqualTo(12));
            Assert.That(layout.SelectionBars.Single().RowTextIds.Count, Is.EqualTo(8));
            Assert.That(layout.ProgressBars, Has.Count.EqualTo(1));
            Assert.That(layout.Visualizers, Has.Count.EqualTo(1));
            Assert.That(layout.Notes, Has.Count.EqualTo(1));
            Assert.That(layout.UnsupportedWidgets, Is.Empty);
        }

        [Test]
        public void canvas_누락은_키_이름이_들어간_에러() {
            var e = Assert.Throws<BackgroundDataException>(() => LoadText("{\"texts\": []}"));
            StringAssert.Contains("'canvas'", e.Message);
        }

        [Test]
        public void text_rect_누락은_경로가_들어간_에러() {
            var e = Assert.Throws<BackgroundDataException>(() =>
                LoadText("{\"canvas\": [640, 360], \"texts\": [{\"id\": \"a\"}]}"));
            StringAssert.Contains("texts[0]", e.Message);
            StringAssert.Contains("'rect'", e.Message);
        }

        [Test]
        public void 위젯_필수_키_누락은_위젯_경로가_들어간_에러() {
            var e = Assert.Throws<BackgroundDataException>(() =>
                LoadText("{\"canvas\": [640, 360], \"texts\": [], \"widgets\": {\"cd\": {\"sheet\": \"cd.png\", \"frames\": 4}}}"));
            StringAssert.Contains("widgets.cd", e.Message);
            StringAssert.Contains("'center'", e.Message);
        }

        [Test]
        public void 깨진_json은_데이터_에러() {
            Assert.Throws<BackgroundDataException>(() => LoadText("{\"canvas\": [640, 360"));
        }

        [Test]
        public void 모르는_위젯은_지원_안_함_목록에_들어감() {
            var layout = LoadText("{\"canvas\": [640, 360], \"texts\": [], \"widgets\": {\"mystery\": {}}}");
            Assert.That(layout.UnsupportedWidgets, Is.EqualTo(new[] { "mystery" }));
        }

        private static StageBackgroundLayout Load(StageBackgroundSource source) =>
            StageBackgroundLoader.Load(File.ReadAllText(source.JsonPath), source.FolderPath, source.PixelsPerUnit,
                source.JsonPath, File.Exists);

        private static StageBackgroundLayout LoadText(string json) =>
            StageBackgroundLoader.Load(json, "Assets/Fake", 40f, "test.json", _ => false);
    }
}
