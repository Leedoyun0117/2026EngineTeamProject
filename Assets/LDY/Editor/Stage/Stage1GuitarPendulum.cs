using System;
using System.Linq;
using LDY.Stage;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LDY.StageEditor {
    // 기요틴 Variant의 아트를 기타 진자로 바꾼다. 팀원 스크립트가 회전시키는 루트 아래에 아트를 붙이고, 팀원 프리팹과 코드는 건드리지 않는다.
    internal sealed class GuitarPendulumBuilder {
        public const string ContainerName = "GuitarPendulum";

        private const string ArmName = "Square (7)";
        private const string LegacyBladeName = "GameObject";
        private const string KillHitboxTypeName = "HES.Script.Object.KillHitbox";
        private const float FallbackArmLength = 7f;
        private const float AnchorOverlapUnits = 0.1f;
        private const float GuitarOverlapUnits = 0.04f;

        private const int GuitarOrder = 1;
        private const int ChainOrder = 2;
        private const int AnchorOrder = 3;

        // 기타 캔버스(360x120)와 사슬 연결점. 왼쪽 위 원점 픽셀이며 스프라이트 피벗은 캔버스 아래 가운데다.
        private static readonly Vector2 GuitarCanvasPixels = new(360f, 120f);
        private static readonly Vector2 GuitarAttachPixel = new(179f, 0f);

        public void Build(GameObject root, AsepriteArt anchor, AsepriteArt chain, AsepriteArt guitar) {
            var rotor = root.transform;
            var arm = ObjectTools.FindDeep(rotor, ArmName);
            var legacyBlade = ObjectTools.FindDeep(rotor, LegacyBladeName);
            var length = ArmLength(arm);

            HideLegacy(arm, legacyBlade);
            RemovePrevious(rotor);

            var container = new GameObject(ContainerName) { layer = root.layer };
            container.transform.SetParent(rotor, false);
            var layer = legacyBlade != null ? legacyBlade.gameObject.layer : root.layer;

            AddChain(container.transform, chain.First, length, layer);
            AddGuitar(container.transform, guitar.First, length, layer);
            AddAnchor(container.transform, anchor.First, layer);
        }

        private static float ArmLength(Transform arm) {
            if (arm == null)
                return FallbackArmLength;

            var renderer = arm.GetComponent<SpriteRenderer>();
            var spriteHeight = renderer != null && renderer.sprite != null ? renderer.sprite.bounds.size.y : 1f;
            return Mathf.Abs(arm.localScale.y) * spriteHeight;
        }

        // 흰 사각형 팔과 옛 날의 렌더러/판정만 끄고, 오브젝트와 스크립트 참조는 유지한다.
        private static void HideLegacy(Transform arm, Transform legacyBlade) {
            Disable(arm != null ? arm.GetComponent<SpriteRenderer>() : null);
            if (legacyBlade == null)
                return;

            Disable(legacyBlade.GetComponent<SpriteRenderer>());
            foreach (var collider in legacyBlade.GetComponents<Collider2D>())
                collider.enabled = false;
        }

        private static void Disable(Renderer renderer) {
            if (renderer != null)
                renderer.enabled = false;
        }

        private static void RemovePrevious(Transform rotor) {
            var previous = rotor.Find(ContainerName);
            if (previous != null)
                Object.DestroyImmediate(previous.gameObject);
        }

        // 천장 고정 장식. 부모가 흔들려도 회전하지 않고, 아래쪽 끝이 루트 원점에 닿는다.
        private static void AddAnchor(Transform parent, Sprite sprite, int layer) {
            var go = NewChild(parent, "Anchor", layer);
            go.transform.localPosition = new Vector3(0f, sprite.pivot.y / sprite.pixelsPerUnit, 0f);
            Render(go, sprite, AnchorOrder);
            go.AddComponent<KeepWorldRotation>();
        }

        // 피벗이 아래 가운데인 사슬을 팔 끝에서 위로 세로 타일링한다.
        private static void AddChain(Transform parent, Sprite sprite, float armLength, int layer) {
            var go = NewChild(parent, "Chain", layer);
            go.transform.localPosition = new Vector3(0f, -armLength - GuitarOverlapUnits, 0f);
            var renderer = Render(go, sprite, ChainOrder);
            renderer.drawMode = SpriteDrawMode.Tiled;
            renderer.size = new Vector2(sprite.bounds.size.x, armLength + AnchorOverlapUnits + GuitarOverlapUnits);
        }

        private static void AddGuitar(Transform parent, Sprite sprite, float armLength, int layer) {
            var go = NewChild(parent, "Guitar", layer);
            var attach = AttachOffsetUnits(sprite);
            go.transform.localPosition = new Vector3(-attach.x, -armLength - attach.y, 0f);
            Render(go, sprite, GuitarOrder);
            AddKillHitbox(go, sprite);
        }

        // 기타 스프라이트 피벗 기준 사슬 연결점 위치
        private static Vector2 AttachOffsetUnits(Sprite sprite) {
            var pixels = new Vector2(GuitarAttachPixel.x - GuitarCanvasPixels.x * 0.5f,
                GuitarCanvasPixels.y - GuitarAttachPixel.y);
            return pixels / sprite.pixelsPerUnit;
        }

        private static void AddKillHitbox(GameObject go, Sprite sprite) {
            var polygon = ObjectFactory.AddComponent<PolygonCollider2D>(go);
            polygon.isTrigger = true;
            if (polygon.pathCount == 0) {
                Debug.LogWarning("[Stage1] 기타 스프라이트에서 폴리곤을 만들지 못해 외곽 사각형으로 대체했다");
                var half = (Vector2)sprite.bounds.extents;
                var center = (Vector2)sprite.bounds.center;
                polygon.pathCount = 1;
                polygon.SetPath(0, new[] {
                    center + new Vector2(-half.x, -half.y), center + new Vector2(half.x, -half.y),
                    center + new Vector2(half.x, half.y), center + new Vector2(-half.x, half.y)
                });
            }

            var hitboxType = AppDomain.CurrentDomain.GetAssemblies()
                .Select(assembly => assembly.GetType(KillHitboxTypeName)).FirstOrDefault(type => type != null);
            if (hitboxType == null)
                Debug.LogWarning($"[Stage1] {KillHitboxTypeName} 를 찾지 못해 사망 판정 스크립트를 붙이지 못했다");
            else
                go.AddComponent(hitboxType);
        }

        private static GameObject NewChild(Transform parent, string name, int layer) {
            var go = new GameObject(name) { layer = layer };
            go.transform.SetParent(parent, false);
            return go;
        }

        private static SpriteRenderer Render(GameObject go, Sprite sprite, int order) {
            var renderer = ObjectTools.SetSprite(go, sprite);
            renderer.sortingOrder = order;
            return renderer;
        }
    }
}
