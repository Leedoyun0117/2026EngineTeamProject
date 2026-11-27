using LDY.PlayerMerge;
using UnityEditor;
using UnityEngine;

namespace LDY.Script {
    // A(지빈, 루트 스케일 2)의 게임플레이 값을 루트 스케일 1인 최종본으로 옮긴다. 월드 크기는 그대로 둔다.
    internal sealed class JibinValuesStep {
        private const float FinalRootScale = 1f;

        private readonly GameObject _jibin;
        private readonly GameObject _final;
        private readonly MergeLog _log;
        private float _jibinScale;

        public JibinValuesStep(GameObject jibin, GameObject final, MergeLog log) {
            _jibin = jibin;
            _final = final;
            _log = log;
        }

        public bool Run() {
            var scale = _jibin.transform.localScale;
            if (!Mathf.Approximately(scale.x, scale.y)) {
                _log.Notes.Add($"중단: A 루트 스케일이 균일하지 않음 {scale}");
                return false;
            }

            _jibinScale = scale.y;
            SyncRoot();
            foreach (var name in PlayerParts.JibinChildren)
                PlaceChild(name);

            var map = new HierarchyMap(_jibin.transform, _final.transform);
            foreach (var typeName in PlayerParts.JibinComponents)
                CopyComponent(typeName, map, null);

            CopyComponent(PlayerParts.Collider, map, ConvertCollider);
            EnsureGimmick();
            return true;
        }

        private void SyncRoot() {
            var root = _final.transform;
            if (root.localScale != Vector3.one) {
                _log.Changed.Add($"루트 localScale: {root.localScale} -> {Vector3.one}");
                root.localScale = Vector3.one;
            }

            if (_final.tag != _jibin.tag) {
                _log.Changed.Add($"루트 tag: {_final.tag} -> {_jibin.tag}");
                _final.tag = _jibin.tag;
            }

            if (_final.layer != _jibin.layer) {
                _log.Changed.Add($"루트 layer: {_final.layer} -> {_jibin.layer}");
                _final.layer = _jibin.layer;
            }
        }

        private void PlaceChild(string name) {
            var source = _jibin.transform.Find(name);
            if (source == null) {
                _log.Notes.Add($"A에 {name} 자식이 없어 건너뜀");
                return;
            }

            var target = _final.transform.Find(name) ?? Create(source);
            var local = SourceLocal(source);
            var converted = RootScaleConversion.Convert(local, _jibinScale, FinalRootScale);
            if (!WorldPreserved($"{name}.localPosition", local, converted))
                return;

            var next = new Vector3(converted.X, converted.Y, source.localPosition.z);
            if (target.localPosition == next)
                return;

            _log.Changed.Add($"{name}.localPosition: {target.localPosition.ToString("0.####")} -> {next.ToString("0.####")} "
                             + $"(A 로컬 {local} × 루트 {_jibinScale})");
            target.localPosition = next;
        }

        private static Float2 SourceLocal(Transform source) {
            var local = source.localPosition.ToFloat2();
            return source.name == PlayerParts.GroundCheck ? new Float2(local.X, PlayerParts.JibinGroundCheckLocalY) : local;
        }

        private Transform Create(Transform source) {
            var created = new GameObject(source.name) { layer = source.gameObject.layer };
            created.transform.SetParent(_final.transform, false);
            _log.Changed.Add($"+{source.name} (A의 자식 오브젝트)");
            return created.transform;
        }

        private void CopyComponent(string typeName, HierarchyMap map, System.Action<SerializedObject> adjust) {
            var source = ComponentNames.Find(_jibin, typeName);
            if (source == null) {
                _log.Notes.Add($"A에 {typeName}가 없어 건너뜀");
                return;
            }

            var target = ComponentNames.Find(_final, typeName);
            if (target == null) {
                target = _final.AddComponent(source.GetType());
                _log.Changed.Add($"+{typeName} (A에서 추가)");
            }

            SerializedValueCopier.Copy(source, target, map, $"A→{typeName}", _log, adjust);
        }

        private void ConvertCollider(SerializedObject collider) {
            ConvertVector(collider.FindProperty("m_Offset"), "CapsuleCollider2D.m_Offset");
            ConvertVector(collider.FindProperty("m_Size"), "CapsuleCollider2D.m_Size");
        }

        private void ConvertVector(SerializedProperty property, string label) {
            var local = property.vector2Value.ToFloat2();
            var converted = RootScaleConversion.Convert(local, _jibinScale, FinalRootScale);
            if (WorldPreserved(label, local, converted))
                property.vector2Value = converted.ToVector2();
        }

        private bool WorldPreserved(string label, Float2 local, Float2 converted) {
            var before = RootScaleConversion.ToWorld(local, _jibinScale);
            var after = RootScaleConversion.ToWorld(converted, FinalRootScale);
            if (RootScaleConversion.SameWorld(before, after))
                return true;

            _log.Notes.Add($"{label}: 환산 후 월드 {after}가 A 월드 {before}와 {RootScaleConversion.WorldTolerance}u 이상 달라 적용하지 않음");
            return false;
        }

        private void EnsureGimmick() {
            if (ComponentNames.Find(_final, PlayerParts.Gimmick) != null)
                return;

            var type = ComponentNames.Find(_jibin, PlayerParts.Gimmick)?.GetType() ?? FindType(PlayerParts.Gimmick);
            if (type == null) {
                _log.Notes.Add($"{PlayerParts.Gimmick} 타입을 찾지 못해 추가하지 않음. 흡수(E)는 동작하지 않음");
                return;
            }

            _final.AddComponent(type);
            _log.Changed.Add($"+{PlayerParts.Gimmick} (JJBPlayerAbsorb의 짝, 프리팹에 없던 것을 LDY가 추가)");
        }

        private static System.Type FindType(string name) {
            foreach (var type in TypeCache.GetTypesDerivedFrom<MonoBehaviour>())
                if (type.Name == name)
                    return type;

            return null;
        }
    }
}
