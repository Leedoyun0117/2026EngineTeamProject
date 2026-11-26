using System;
using System.IO;
using LDY.Flow.Core;
using NUnit.Framework;

namespace LDY.Tests.Flow {
    public class JsonProgressStoreTests {
        private static readonly DateTime FixedNow = new DateTime(2026, 11, 24, 21, 5, 9);

        private string _dir;
        private string _path;

        [SetUp]
        public void SetUp() {
            _dir = Path.Combine(Path.GetTempPath(), "ldy-flow-tests-" + Guid.NewGuid().ToString("N"));
            _path = Path.Combine(_dir, JsonProgressStore.FileName);
        }

        [TearDown]
        public void TearDown() {
            if (Directory.Exists(_dir)) Directory.Delete(_dir, true);
        }

        private JsonProgressStore Store() => new JsonProgressStore(_path, () => FixedNow);

        [Test]
        public void MissingFileLoadsEmpty() {
            var progress = Store().Load();

            Assert.IsEmpty(progress.Records);
            Assert.IsFalse(File.Exists(_path));
        }

        [Test]
        public void SaveThenLoadRoundTrips() {
            var saved = new ProgressModel();
            saved.RecordClear("1-T", 61.25d);
            saved.RecordClear("1-1", 0.1d);
            saved.Restore("1-2", new StageRecord(true, null));
            saved.ForceUnlock("2-T");

            Store().Save(saved);
            var loaded = Store().Load();

            Assert.IsTrue(loaded.TryGetBest("1-T", out var t));
            Assert.AreEqual(61.25d, t);
            Assert.IsTrue(loaded.TryGetBest("1-1", out var one));
            Assert.AreEqual(0.1d, one);
            Assert.IsTrue(loaded.IsCleared("1-2"));
            Assert.IsFalse(loaded.TryGetBest("1-2", out _));
            Assert.IsTrue(loaded.IsForcedUnlocked("2-T"));
            Assert.IsFalse(File.Exists(_path + ".tmp"));
        }

        [Test]
        public void SavedFileCarriesVersion() {
            Store().Save(new ProgressModel());

            StringAssert.Contains($"\"version\": {ProgressJson.Version}", File.ReadAllText(_path));
        }

        [Test]
        public void CorruptFileIsBackedUpAndReset() {
            Directory.CreateDirectory(_dir);
            File.WriteAllText(_path, "{\"version\": 1, \"stages\": [ {\"id\": ");

            var store = Store();
            var progress = store.Load();

            Assert.IsEmpty(progress.Records);
            Assert.IsFalse(File.Exists(_path));
            Assert.AreEqual(_path + ".corrupt-20261124-210509", store.LastBackupPath);
            Assert.AreEqual("{\"version\": 1, \"stages\": [ {\"id\": ", File.ReadAllText(store.LastBackupPath));
        }

        [Test]
        public void WrongTypesCountAsCorrupt() {
            Directory.CreateDirectory(_dir);
            File.WriteAllText(_path, "{\"version\": 1, \"stages\": [{\"id\": 3, \"cleared\": true}]}");

            var store = Store();

            Assert.IsEmpty(store.Load().Records);
            Assert.IsNotNull(store.LastBackupPath);
        }

        [Test]
        public void UnknownVersionIsBackedUpAndReset() {
            Directory.CreateDirectory(_dir);
            File.WriteAllText(_path, "{\"version\": 99, \"stages\": []}");

            var store = Store();

            Assert.IsEmpty(store.Load().Records);
            Assert.IsTrue(File.Exists(store.LastBackupPath));
        }

        [Test]
        public void MissingVersionIsBackedUpAndReset() {
            Directory.CreateDirectory(_dir);
            File.WriteAllText(_path, "{\"stages\": []}");

            var store = Store();

            Assert.IsEmpty(store.Load().Records);
            Assert.IsTrue(File.Exists(store.LastBackupPath));
        }
    }
}
