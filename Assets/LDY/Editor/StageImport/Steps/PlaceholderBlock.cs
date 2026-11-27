using LDY.Flow;
using UnityEditor;
using UnityEngine;

namespace LDY.StageImportEditor {
    // 대응표에 프리팹이 없는 custom 블록 자리. 분홍 사각형 + 이름표(EditorOnly), 충돌은 툴의 col 값을 따른다.
    internal static class PlaceholderBlock {
        public const string NamePrefix = "Temp_";
        private static readonly Color Pink = new Color(1f, 0.4f, 0.75f, 0.85f);

        public static GameObject Create(string entryName, string col, Rect area, Transform parent, bool label) {
            var root = new GameObject(NamePrefix + entryName);
            root.transform.SetParent(parent, false);
            root.transform.position = new Vector3(area.center.x, area.center.y, 0f);

            var body = new GameObject("Block");
            body.transform.SetParent(root.transform, false);
            body.transform.localScale = new Vector3(area.width, area.height, 1f);
            var renderer = body.AddComponent<SpriteRenderer>();
            renderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(StageImportPaths.TempSprite);
            renderer.color = Pink;
            renderer.sortingOrder = 4;
            AddCollision(body, col);

            if (label) BlockLabel.Attach(root.transform, entryName, Vector3.zero);
            return root;
        }

        // solid: 막힘 / oneway: 위에서만 밟힘 / hazard: 닿으면 죽음 / none·그 밖: 충돌 없음
        private static void AddCollision(GameObject body, string col) {
            switch (col) {
                case "solid":
                    SetLayer(body, "Ground");
                    body.AddComponent<BoxCollider2D>();
                    break;
                case "oneway":
                    SetLayer(body, "Ground");
                    body.AddComponent<BoxCollider2D>().usedByEffector = true;
                    body.AddComponent<PlatformEffector2D>();
                    break;
                case "hazard":
                    SetLayer(body, "Spike");
                    body.AddComponent<BoxCollider2D>().isTrigger = true;
                    body.AddComponent<StageDeathZone>();
                    break;
            }
        }

        private static void SetLayer(GameObject go, string name) {
            var layer = LayerMask.NameToLayer(name);
            if (layer >= 0) go.layer = layer;
        }
    }

    // 씬에서 보이는 이름표. EditorOnly 태그라 빌드에서 빠진다.
    internal static class BlockLabel {
        private const string EditorOnlyTag = "EditorOnly";
        private static Font _font;

        public static GameObject Attach(Transform parent, string text, Vector3 localPosition) {
            var go = new GameObject("Label");
            go.tag = EditorOnlyTag;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition + new Vector3(0f, 0f, -0.1f);

            var mesh = go.AddComponent<TextMesh>();
            mesh.text = text;
            mesh.anchor = TextAnchor.MiddleCenter;
            mesh.alignment = TextAlignment.Center;
            mesh.characterSize = 0.06f;
            mesh.fontSize = 48;
            mesh.color = Color.white;
            var font = LabelFont();
            var renderer = go.GetComponent<MeshRenderer>();
            if (font != null) {
                mesh.font = font;
                renderer.sharedMaterial = font.material;
            }

            renderer.sortingOrder = 50;
            return go;
        }

        private static Font LabelFont() {
            if (_font != null) return _font;
            _font = AssetDatabase.LoadAssetAtPath<Font>(StageImportPaths.LabelFont);
            if (_font == null) _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return _font;
        }
    }
}
