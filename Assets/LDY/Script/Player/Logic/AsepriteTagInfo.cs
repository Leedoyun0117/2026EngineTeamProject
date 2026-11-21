using System.Collections.Generic;

namespace LDY.Script {
    public readonly struct AsepriteTag {
        public readonly string Name;
        public readonly int From;
        public readonly int To;

        public AsepriteTag(string name, int from, int to) {
            Name = name;
            From = from;
            To = to;
        }

        public int Length => To - From + 1;
    }

    public sealed class AsepriteTagInfo {
        public AsepriteTagInfo(int frameCount, IReadOnlyList<int> durationsMs, IReadOnlyList<AsepriteTag> tags) {
            FrameCount = frameCount;
            DurationsMs = durationsMs;
            Tags = tags;
        }

        public int FrameCount { get; }
        public IReadOnlyList<int> DurationsMs { get; }
        public IReadOnlyList<AsepriteTag> Tags { get; }
    }
}
