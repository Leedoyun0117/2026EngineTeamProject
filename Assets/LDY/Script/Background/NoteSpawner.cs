using LDY.Background.Core;
using UnityEngine;

namespace LDY.Background {
    public sealed class NoteSpawner : MonoBehaviour {
        [SerializeField] private Sprite sprite;
        [SerializeField] private Color[] colors = { Color.white };
        [SerializeField] private Vector2 spawnMin;
        [SerializeField] private Vector2 spawnMax;
        [SerializeField] private float riseDistance = 1f;
        [SerializeField] private int sortingOrder;
        [SerializeField, Min(0.1f)] private float lifetime = 2f;
        [SerializeField, Min(0.05f)] private float interval = 0.45f;
        [SerializeField, Min(1)] private int poolSize = 8;
        [SerializeField] private int seed = 7;
        [SerializeField] private BackgroundTime time;

        private Note[] _pool;
        private System.Random _random;
        private float _untilSpawn;

        private sealed class Note {
            public SpriteRenderer Renderer;
            public Vector3 Start;
            public Color Color;
            public float Age;
        }

        public void Configure(Sprite noteSprite, Color[] palette, Vector2 min, Vector2 max, float rise, int order) {
            sprite = noteSprite;
            colors = palette;
            spawnMin = min;
            spawnMax = max;
            riseDistance = rise;
            sortingOrder = order;
        }

        private void Awake() {
            _random = new System.Random(seed);
            _pool = new Note[poolSize];
            for (var i = 0; i < poolSize; i++) _pool[i] = CreateNote(i);
        }

        private Note CreateNote(int index) {
            var go = new GameObject("Note_" + index);
            go.transform.SetParent(transform, false);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = sortingOrder;
            go.SetActive(false);
            return new Note { Renderer = renderer };
        }

        private void Update() {
            var dt = time.Delta;
            if (dt <= 0f) return;
            _untilSpawn -= dt;
            if (_untilSpawn <= 0f) {
                _untilSpawn += interval;
                Spawn();
            }

            foreach (var note in _pool) Tick(note, dt);
        }

        private void Spawn() {
            foreach (var note in _pool) {
                if (note.Renderer.gameObject.activeSelf) continue;
                var x = Mathf.Lerp(spawnMin.x, spawnMax.x, (float)_random.NextDouble());
                var y = Mathf.Lerp(spawnMin.y, spawnMax.y, (float)_random.NextDouble());
                note.Start = new Vector3(x, y, 0f);
                note.Color = colors.Length > 0 ? colors[_random.Next(colors.Length)] : Color.white;
                note.Age = 0f;
                note.Renderer.gameObject.SetActive(true);
                Tick(note, 0f);
                return;
            }
        }

        private void Tick(Note note, float dt) {
            if (!note.Renderer.gameObject.activeSelf) return;
            note.Age += dt;
            var pose = NoteMotion.Evaluate(note.Age, lifetime, riseDistance);
            if (note.Age >= lifetime) {
                note.Renderer.gameObject.SetActive(false);
                return;
            }

            note.Renderer.transform.localPosition = note.Start + Vector3.up * pose.Rise;
            var color = note.Color;
            color.a *= pose.Alpha;
            note.Renderer.color = color;
        }
    }
}
