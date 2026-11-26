using System.Collections.Generic;
using LDY.TitlePlayer.Core;
using NUnit.Framework;

namespace LDY.Tests {
    public sealed class InputActionLookupTests {
        private sealed class FakeAction { }

        [Test]
        public void Missing_ReportsOnlyAbsentRequiredInOrder() {
            var missing = InputActionNames.Missing(new[] { "Move", "Absorb" }, InputActionNames.Required);
            Assert.That(missing, Is.EqualTo(new[] { "Jump", "Interact" }));
        }

        [Test]
        public void Missing_AllPresent_Empty() {
            Assert.That(InputActionNames.Missing(new[] { "Interact", "Jump", "Move", "Left" }, InputActionNames.Required),
                Is.Empty);
        }

        [Test]
        public void Missing_NullAvailable_EverythingMissing() {
            Assert.That(InputActionNames.Missing(null, InputActionNames.Required), Is.EqualTo(InputActionNames.Required));
        }

        [Test]
        public void MissingMessage_MatchesAgreedText() {
            Assert.That(InputActionNames.MissingMessage("JJBInput", "Jump"),
                Is.EqualTo("JJBInput에 'Jump' 액션 없음 – 타이틀 입력 비활성"));
        }

        [Test]
        public void Lookup_Missing_ReturnsNullAndReportsOnce_FindsOnce() {
            var finds = 0;
            var reports = new List<string>();
            var lookup = new OptionalLookup<FakeAction>("Jump", _ => {
                finds++;
                return null;
            }, reports.Add);

            Assert.That(lookup.Get(), Is.Null);
            Assert.That(lookup.Get(), Is.Null);
            Assert.That(lookup.Get(), Is.Null);
            Assert.That(finds, Is.EqualTo(1));
            Assert.That(reports, Is.EqualTo(new[] { "Jump" }));
        }

        [Test]
        public void Lookup_Found_CachedWithoutReport() {
            var action = new FakeAction();
            var finds = 0;
            var reported = false;
            var lookup = new OptionalLookup<FakeAction>("Move", _ => {
                finds++;
                return action;
            }, _ => reported = true);

            Assert.That(lookup.Get(), Is.SameAs(action));
            Assert.That(lookup.Get(), Is.SameAs(action));
            Assert.That(finds, Is.EqualTo(1));
            Assert.That(reported, Is.False);
        }

        [Test]
        public void MissingReport_SameKeyOnlyOnce() {
            var report = new MissingReport();
            Assert.That(report.TryMark("JJBInput/Jump"), Is.True);
            Assert.That(report.TryMark("JJBInput/Jump"), Is.False);
            Assert.That(report.TryMark("JJBInput/Move"), Is.True);
        }
    }
}
