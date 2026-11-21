using System.Collections.Generic;
using LDY.Script;
using NUnit.Framework;

namespace LDY.Tests {
    public class PlayerAnimFallbackTests {
        private static Dictionary<PlayerAnimId, string> Clips(params PlayerAnimId[] ids) {
            var clips = new Dictionary<PlayerAnimId, string>();
            foreach (var id in ids)
                clips[id] = id.ToString();

            return clips;
        }

        [Test]
        public void ExistingIdIsFoundByKeyNotByPosition() {
            var clips = Clips(PlayerAnimId.Land, PlayerAnimId.Idle);
            Assert.IsTrue(PlayerAnimFallback.TryResolve(clips, PlayerAnimId.Idle, out var clip));
            Assert.AreEqual("Idle", clip);
        }

        [Test]
        public void MissingLandFallsBackToIdle() {
            Assert.IsTrue(PlayerAnimFallback.TryResolve(Clips(PlayerAnimId.Idle), PlayerAnimId.Land, out var clip));
            Assert.AreEqual("Idle", clip);
        }

        [Test]
        public void MissingPushFallsBackToWalk() {
            Assert.IsTrue(PlayerAnimFallback.TryResolve(Clips(PlayerAnimId.Walk), PlayerAnimId.Push, out var clip));
            Assert.AreEqual("Walk", clip);
        }

        [Test]
        public void PresentLandIsNotReplaced() {
            var clips = Clips(PlayerAnimId.Idle, PlayerAnimId.Land);
            PlayerAnimFallback.TryResolve(clips, PlayerAnimId.Land, out var clip);
            Assert.AreEqual("Land", clip);
        }

        [Test]
        public void MissingRopeFallsBackToGrab() {
            Assert.IsTrue(PlayerAnimFallback.TryResolve(Clips(PlayerAnimId.Grab), PlayerAnimId.RopeClimb, out var clip));
            Assert.AreEqual("Grab", clip);
        }

        [Test]
        public void NothingToFallBackToReturnsFalse() {
            Assert.IsFalse(PlayerAnimFallback.TryResolve(Clips(PlayerAnimId.Run), PlayerAnimId.Land, out _));
        }

        [Test]
        public void IdWithoutFallbackStopsImmediately() {
            Assert.IsFalse(PlayerAnimFallback.TryNext(PlayerAnimId.Jump, out _));
        }
    }
}
