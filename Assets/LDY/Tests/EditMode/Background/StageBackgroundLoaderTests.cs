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

        [Test]
        public void static_키가_있으면_그_파일() {
            var layout = LoadText("{\"canvas\": [640, 360], \"static\": \"base.png\", \"texts\": []}");
            Assert.That(layout.StaticFile, Is.EqualTo("Assets/Fake/base.png"));
        }

        [Test]
        public void static_키가_없으면_window_base_notext() {
            var layout = LoadText("{\"canvas\": [640, 360], \"texts\": []}");
            Assert.That(layout.StaticFile, Is.EqualTo("Assets/Fake/window_base_notext.png"));
        }

        [Test]
        public void reference는_text_positions_우선_없으면_window_with_text() {
            const string json = "{\"canvas\": [640, 360], \"texts\": []}";
            var both = StageBackgroundLoader.Load(json, "Assets/Fake", 40f, "t", _ => true);
            Assert.That(both.ReferenceFile, Is.EqualTo("Assets/Fake/reference_text_positions.png"));
            var fallback = StageBackgroundLoader.Load(json, "Assets/Fake", 40f, "t",
                f => f.EndsWith("reference_window_with_text.png"));
            Assert.That(fallback.ReferenceFile, Is.EqualTo("Assets/Fake/reference_window_with_text.png"));
            Assert.That(LoadText(json).ReferenceFile, Is.Null);
        }

        [Test]
        public void 위젯_frame_ms_없으면_120_있으면_그_값() {
            var layout = LoadText("{\"canvas\": [640, 360], \"texts\": [], \"widgets\": {" +
                                  "\"a\": {\"sheet\": \"a.png\", \"frames\": 2, \"center\": [0, 0]}," +
                                  "\"b\": {\"sheet\": \"b.png\", \"frames\": 2, \"center\": [0, 0], \"frame_ms\": 80}}}");
            Assert.That(layout.Sheets.Single(s => s.Id == "a").FrameMs, Is.EqualTo(120f));
            Assert.That(layout.Sheets.Single(s => s.Id == "b").FrameMs, Is.EqualTo(80f));
        }

        [Test]
        public void texts_누락은_키_이름이_들어간_에러() {
            var e = Assert.Throws<BackgroundDataException>(() => LoadText("{\"canvas\": [640, 360]}"));
            StringAssert.Contains("'texts'", e.Message);
        }

        [Test]
        public void static_객체_형식은_file과_위치() {
            var layout = LoadText("{\"canvas\": [640, 360], \"static\": {\"file\": \"bg.png\", \"x\": 3, \"y\": 4}, \"texts\": []}");
            Assert.That(layout.StaticFile, Is.EqualTo("Assets/Fake/bg.png"));
            Assert.That(layout.StaticX, Is.EqualTo(3f));
            Assert.That(layout.StaticY, Is.EqualTo(4f));
        }

        [Test]
        public void sheets_배치는_키_file_접미사_순으로_찾고_최상위_frames_frame_ms를_쓴다() {
            var layout = LoadText("{\"canvas\": [640, 360], \"frames\": 8, \"frame_ms\": 90, \"texts\": [], " +
                                  "\"widgets\": {\"progress\": {\"sheet\": \"p.png\"}, \"hdd_led\": {\"sheet\": \"l.png\"}, \"x\": {\"sheet\": \"f.png\"}}, " +
                                  "\"sheets\": {\"progress\": {\"x\": 442, \"y\": 341, \"w\": 149, \"h\": 9, \"frames\": 4}, " +
                                  "\"led\": {\"x\": 604, \"y\": 342, \"w\": 6, \"h\": 6}, \"other\": {\"file\": \"f.png\", \"x\": 0, \"y\": 0, \"w\": 2, \"h\": 2}}}");
            var progress = layout.Sheets.Single(s => s.Id == "progress");
            Assert.That(progress.Frames, Is.EqualTo(4));
            Assert.That(progress.FrameMs, Is.EqualTo(90f));
            Assert.That(progress.CenterX, Is.EqualTo(516.5f));
            Assert.That(progress.CenterY, Is.EqualTo(345.5f));
            var led = layout.Sheets.Single(s => s.Id == "hdd_led");
            Assert.That(led.Frames, Is.EqualTo(8));
            Assert.That(led.CenterX, Is.EqualTo(607f));
            Assert.That(layout.Sheets.Single(s => s.Id == "x").CenterX, Is.EqualTo(1f));
        }

        [Test]
        public void frames가_어디에도_없으면_에러() {
            var e = Assert.Throws<BackgroundDataException>(() => LoadText(
                "{\"canvas\": [640, 360], \"texts\": [], \"widgets\": {\"cd\": {\"sheet\": \"c.png\", \"center\": [0, 0]}}}"));
            StringAssert.Contains("'frames'", e.Message);
        }

        [Test]
        public void rect_없는_선택_바는_row_h와_x로_첫_행에_맞춘다() {
            var layout = LoadText("{\"canvas\": [640, 360], \"texts\": [" +
                                  "{\"id\": \"tree_0\", \"rect\": [43, 69, 128, 10], \"color\": \"#243046\"}," +
                                  "{\"id\": \"tree_1\", \"rect\": [49, 86, 128, 10]}, {\"id\": \"other\", \"rect\": [0, 0, 1, 1]}], " +
                                  "\"widgets\": {\"tree_selection_bar\": {\"row_h\": 17, \"x\": [16, 150], \"color\": \"#000080\", \"text_color\": \"#FFFFFF\"}}}");
            var bar = layout.SelectionBars.Single();
            Assert.That(bar.RowTextIds, Is.EqualTo(new[] { "tree_0", "tree_1" }));
            Assert.That(bar.Rect.X, Is.EqualTo(16f));
            Assert.That(bar.Rect.Width, Is.EqualTo(134f));
            Assert.That(bar.Rect.Y, Is.EqualTo(65f));
            Assert.That(bar.Rect.Height, Is.EqualTo(17f));
            Assert.That(bar.RowStep, Is.EqualTo(17f));
            Assert.That(bar.DefaultRow, Is.EqualTo(4));
            Assert.That(layout.Texts[0].SelectedColor.HasValue, Is.True);
            Assert.That(layout.Texts[2].SelectedColor.HasValue, Is.False);
        }

        [Test]
        public void 캐럿은_같은_줄_왼쪽_가장_가까운_텍스트에_붙는다() {
            var layout = LoadText("{\"canvas\": [640, 360], \"frames\": 8, \"texts\": [" +
                                  "{\"id\": \"address_label\", \"rect\": [96, 45, 48, 9]}, {\"id\": \"address_path\", \"rect\": [153, 45, 420, 10]}], " +
                                  "\"widgets\": {\"address_caret\": {\"sheet\": \"caret.png\"}}, " +
                                  "\"sheets\": {\"caret\": {\"x\": 214, \"y\": 46, \"w\": 1, \"h\": 9}}}");
            var caret = layout.Carets.Single();
            Assert.That(caret.SheetId, Is.EqualTo("address_caret"));
            Assert.That(caret.TextId, Is.EqualTo("address_path"));
            Assert.That(caret.GapPixels, Is.EqualTo(4f));
        }

        [Test]
        public void mask는_텍스트_스펙에_들어간다() {
            var layout = LoadText("{\"canvas\": [640, 360], \"texts\": [{\"id\": \"a\", \"rect\": [0, 0, 1, 1], \"mask\": true}]}");
            Assert.That(layout.Texts.Single().Mask, Is.True);
        }

        private static StageBackgroundLayout Load(StageBackgroundSource source) =>
            StageBackgroundLoader.Load(File.ReadAllText(source.JsonPath), source.FolderPath, source.PixelsPerUnit,
                source.JsonPath, File.Exists);

        private static StageBackgroundLayout LoadText(string json) =>
            StageBackgroundLoader.Load(json, "Assets/Fake", 40f, "test.json", _ => false);
    }
}
