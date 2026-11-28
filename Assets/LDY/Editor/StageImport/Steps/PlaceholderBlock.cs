using LDY.Flow;
using LDY.StageImport;
using UnityEditor;
using UnityEngine;

namespace LDY.StageImportEditor {
    // 대응표에 프리팹이 없는 custom 블록 자리. 반투명 분홍 단색 사각형, 충돌은 툴의 col 값을 따른다. 이름표는 BlockLabel이 묶음마다 따로 붙인다.
    internal static class PlaceholderBlock {
        public const string NamePrefix = "Temp_";
        private static readonly Color Pink = new Color(1f, 0.4f, 0.75f, 0.5f);

        public static GameObject Create(string entryName, string col, Rect area, Transform parent) {
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

    // 이름표. EditorOnly 태그(빌드 제외) + Scene 뷰 기즈모(StageLabel)로만 그린다. Game 뷰에 렌더되는 글자 컴포넌트는 쓰지 않는다.
    internal static class BlockLabel {
        private const string EditorOnlyTag = "EditorOnly";

        public static GameObject Attach(Transform parent, string text, Vector3 worldPosition) {
            var go = new GameObject("Label");
            go.tag = EditorOnlyTag;
            go.transform.SetParent(parent, false);
            go.transform.position = worldPosition;
            go.AddComponent<StageLabel>().SetText(text);
            return go;
        }
    }
}
