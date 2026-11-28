using LDY.Background.Core;
using UnityEngine;

namespace LDY.Background {
    public sealed class NoteSpawner : MonoBehaviour, IBackgroundDimmable {
        [SerializeField] private SpriteRenderer[] pool = new SpriteRenderer[0];
        [SerializeField] private Color[] colors = { Color.white };
        [SerializeField] private Vector2 spawnMin;
        [SerializeField] private Vector2 spawnMax;
        [SerializeField] private float riseDistance = 1f;
        [SerializeField, Min(0.1f)] private float lifetime = 2f;
        [SerializeField, Min(0.05f)] private float interval = 0.45f;
        [SerializeField] private int seed = 7;
        [SerializeField] private BackgroundTime time;

        private Note[] _notes;
        private System.Random _random;
        private float _untilSpawn;
        private float _dim;

        private struct Note {
            public Vector3 Start;
            public Color Color;
            public float Age;
        }

        public void Configure(SpriteRenderer[] renderers, Color[] palette, Vector2 min, Vector2 max, float rise) {
            pool = renderers;
            colors = palette;
            spawnMin = min;
            spawnMax = max;
            riseDistance = rise;
            foreach (var renderer in pool) renderer.gameObject.SetActive(false);
        }

        public void ApplyDim(float dim) => _dim = dim;

        private void Awake() {
            _random = new System.Random(seed);
            _notes = new Note[pool.Length];
            foreach (var renderer in pool)
                if (renderer != null)
                    renderer.gameObject.SetActive(false);
        }

        private void Update() {
            var dt = time.Delta;
            if (dt <= 0f) return;
            _untilSpawn -= dt;
            if (_untilSpawn <= 0f) {
                _untilSpawn += interval;
                Spawn();
            }

            for (var i = 0; i < pool.Length; i++) Tick(i, dt);
        }

        private void Spawn() {
            for (var i = 0; i < pool.Length; i++) {
                if (pool[i] == null || pool[i].gameObject.activeSelf) continue;
                var x = Mathf.Lerp(spawnMin.x, spawnMax.x, (float)_random.NextDouble());
                var y = Mathf.Lerp(spawnMin.y, spawnMax.y, (float)_random.NextDouble());
                _notes[i] = new Note {
                    Start = new Vector3(x, y, 0f),
                    Color = colors.Length > 0 ? colors[_random.Next(colors.Length)] : Color.white
                };
                pool[i].gameObject.SetActive(true);
                Tick(i, 0f);
                return;
            }
        }

        private void Tick(int index, float dt) {
            var renderer = pool[index];
            if (renderer == null || !renderer.gameObject.activeSelf) return;
            _notes[index].Age += dt;
            var note = _notes[index];
            if (note.Age >= lifetime) {
                renderer.gameObject.SetActive(false);
                return;
            }

            var pose = NoteMotion.Evaluate(note.Age, lifetime, riseDistance);
            renderer.transform.localPosition = note.Start + Vector3.up * pose.Rise;
            var color = DimColor.Of(note.Color, _dim);
            color.a *= pose.Alpha;
            renderer.color = color;
        }
    }
}
