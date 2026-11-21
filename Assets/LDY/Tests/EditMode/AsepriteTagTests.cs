using System.Collections.Generic;
using System.IO;
using System.Text;
using LDY.Script;
using NUnit.Framework;

namespace LDY.Tests {
    public class AsepriteTagTests {
        private static readonly string[] Expected = { "idle", "walk" };

        private static byte[] Build(int[] durations, params (string name, int from, int to)[] tags) {
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream, Encoding.UTF8);
            var header = new byte[128];
            header[4] = 0xE0;
            header[5] = 0xA5;
            header[6] = (byte)durations.Length;
            writer.Write(header);
            for (var frame = 0; frame < durations.Length; frame++) {
                var chunk = frame == 0 && tags.Length > 0 ? TagsChunk(tags) : new byte[0];
                writer.Write(16 + chunk.Length);
                writer.Write((ushort)0xF1FA);
                writer.Write((ushort)0);
                writer.Write((ushort)durations[frame]);
                writer.Write(new byte[2]);
                writer.Write(chunk.Length > 0 ? 1 : 0);
                writer.Write(chunk);
            }

            return stream.ToArray();
        }

        private static byte[] TagsChunk(IEnumerable<(string name, int from, int to)> tags) {
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream, Encoding.UTF8);
            var list = new List<(string name, int from, int to)>(tags);
            var body = new MemoryStream();
            var bodyWriter = new BinaryWriter(body, Encoding.UTF8);
            foreach (var tag in list) {
                bodyWriter.Write((ushort)tag.from);
                bodyWriter.Write((ushort)tag.to);
                bodyWriter.Write(new byte[13]);
                var name = Encoding.UTF8.GetBytes(tag.name);
                bodyWriter.Write((ushort)name.Length);
                bodyWriter.Write(name);
            }

            writer.Write(16 + (int)body.Length);
            writer.Write((ushort)0x2018);
            writer.Write((ushort)list.Count);
            writer.Write(new byte[8]);
            writer.Write(body.ToArray());
            return stream.ToArray();
        }

        private static AsepriteTagInfo Parse(byte[] data) {
            Assert.IsTrue(AsepriteTagParser.TryParse(data, out var info, out var error), error);
            return info;
        }

        [Test]
        public void ParsesFrameCountDurationsAndTags() {
            var info = Parse(Build(new[] { 240, 110, 90 }, ("idle", 0, 1), ("walk", 2, 2)));
            Assert.AreEqual(3, info.FrameCount);
            Assert.AreEqual(110, info.DurationsMs[1]);
            Assert.AreEqual(2, info.Tags.Count);
            Assert.AreEqual("walk", info.Tags[1].Name);
            Assert.AreEqual(2, info.Tags[1].From);
            Assert.AreEqual(2, info.Tags[1].To);
            Assert.AreEqual(2, info.Tags[0].Length);
        }

        [Test]
        public void RejectsNonAsepriteData() {
            Assert.IsFalse(AsepriteTagParser.TryParse(new byte[200], out _, out var error));
            Assert.IsNotEmpty(error);
        }

        [Test]
        public void ValidTagsPass() {
            var info = Parse(Build(new[] { 1, 1, 1 }, ("idle", 0, 1), ("walk", 2, 2)));
            Assert.IsEmpty(TagRangeValidator.Validate(info, new[] { true, true, true }, Expected));
        }

        [Test]
        public void MissingSpriteIsReportedWithTagName() {
            var info = Parse(Build(new[] { 1, 1, 1 }, ("idle", 0, 1), ("walk", 2, 2)));
            var errors = TagRangeValidator.Validate(info, new[] { true, false, true }, Expected);
            Assert.IsTrue(errors.Exists(e => e.Contains("idle") && e.Contains("Frame_1")));
        }

        [Test]
        public void SpriteCountMismatchIsReported() {
            var info = Parse(Build(new[] { 1, 1, 1 }, ("idle", 0, 1), ("walk", 2, 2)));
            var errors = TagRangeValidator.Validate(info, new[] { true, true }, Expected);
            Assert.IsTrue(errors.Exists(e => e.Contains("스프라이트")));
        }

        [Test]
        public void TagBeyondFrameCountIsReported() {
            var info = Parse(Build(new[] { 1, 1 }, ("idle", 0, 1), ("walk", 1, 5)));
            var errors = TagRangeValidator.Validate(info, new[] { true, true }, Expected);
            Assert.IsTrue(errors.Exists(e => e.Contains("walk") && e.Contains("밖")));
        }

        [Test]
        public void MissingAndUnknownTagsAreReported() {
            var info = Parse(Build(new[] { 1, 1 }, ("idle", 0, 0), ("jump", 1, 1)));
            var errors = TagRangeValidator.Validate(info, new[] { true, true }, Expected);
            Assert.IsTrue(errors.Exists(e => e.Contains("walk") && e.Contains("없음")));
            Assert.IsTrue(errors.Exists(e => e.Contains("jump") && e.Contains("기대")));
        }

        [Test]
        public void OverlappingTagsAreReported() {
            var info = Parse(Build(new[] { 1, 1, 1 }, ("idle", 0, 1), ("walk", 1, 2)));
            var errors = TagRangeValidator.Validate(info, new[] { true, true, true }, Expected);
            Assert.IsTrue(errors.Exists(e => e.Contains("겹침")));
        }

        [Test]
        public void DuplicateTagNamesAreReported() {
            var info = Parse(Build(new[] { 1, 1 }, ("idle", 0, 0), ("idle", 1, 1)));
            var errors = TagRangeValidator.Validate(info, new[] { true, true }, Expected);
            Assert.IsTrue(errors.Exists(e => e.Contains("idle") && e.Contains("2번")));
        }
    }
}
