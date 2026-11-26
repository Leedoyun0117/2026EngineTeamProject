using System;

namespace LDY.Background.Core {
    public sealed class VisualizerModel {
        private readonly int _seed;
        private readonly int _segments;

        public VisualizerModel(int seed, int bars, int segments) {
            if (bars < 1) throw new ArgumentException("bars는 1 이상이어야 한다");
            if (segments < 1) throw new ArgumentException("segments는 1 이상이어야 한다");
            _seed = seed;
            Bars = bars;
            _segments = segments;
        }

        public int Bars { get; }

        public int Height(long step, int bar) {
            var wave = 0.5 + 0.5 * Math.Sin(step * 0.35 + bar * 0.9);
            var noise = Hash01(step, bar);
            var level = 0.15 + 0.85 * (0.55 * wave + 0.45 * noise);
            return Math.Max(0, Math.Min(_segments, (int)Math.Round(level * _segments)));
        }

        private double Hash01(long step, int bar) {
            unchecked {
                var h = (uint)_seed * 0x9E3779B1u;
                h ^= (uint)step * 0x85EBCA77u;
                h ^= (uint)bar * 0xC2B2AE3Du;
                h ^= h >> 15;
                h *= 0x2C1B3C6Du;
                h ^= h >> 12;
                h *= 0x297A2D39u;
                h ^= h >> 15;
                return h / (double)uint.MaxValue;
            }
        }
    }
}
