using LDY.PlayerMerge;
using UnityEditor;
using UnityEngine;

namespace LDY.Script {
    // B(태영)의 스킬 묶음(루트 컴포넌트 + Skills·Light 자식)을 직렬화 값 그대로 최종본에 붙인다. 태영 코드는 건드리지 않는다.
    internal sealed class HtyBundleStep {
        private readonly GameObject _hty;
        private readonly GameObject _final;
        private readonly GameObject _copyPrefab;
        private readonly MergeLog _log;

        public HtyBundleStep(GameObject hty, GameObject final, GameObject copyPrefab, MergeLog log) {
            _hty = hty;
            _final = final;
            _copyPrefab = copyPrefab;
            _log = log;
        }

        public void Run() {
            foreach (var name in PlayerParts.HtyChildren) {
                var source = _hty.transform.Find(name);
                if (source == null)
                    _log.Notes.Add($"B에 {name} 자식이 없어 건너뜀");
                else
                    EnsureStructure(source, _final.transform);
            }

            var map = new HierarchyMap(_hty.transform, _final.transform);
            foreach (var typeName in PlayerParts.HtyRootComponents)
                CopyRootComponent(typeName, map);

            foreach (var name in PlayerParts.HtyChildren) {
                var source = _hty.transform.Find(name);
                if (source != null)
                    CopySubtree(source, map);
            }
        }

        private void EnsureStructure(Transform source, Transform parent) {
            var target = parent.Find(source.name);
            if (target == null) {
                target = new GameObject(source.name).transform;
                target.SetParent(parent, false);
                _log.Changed.Add($"+{HierarchyPaths.Relative(_final.transform, target)} (B의 자식 오브젝트)");
            }

            SyncNode(source, target);
            foreach (var component in source.GetComponents<Component>())
                if (component != null && component is not Transform)
                    EnsureCount(target.gameObject, component.GetType(), source.GetComponents(component.GetType()).Length);

            foreach (Transform child in source)
                EnsureStructure(child, target);
        }

        private void SyncNode(Transform source, Transform target) {
            var label = HierarchyPaths.Relative(_final.transform, target);
            var go = target.gameObject;
            if (go.layer != source.gameObject.layer || go.tag != source.tag || go.activeSelf != source.gameObject.activeSelf
                || target.localPosition != source.localPosition || target.localRotation != source.localRotation
                || target.localScale != source.localScale)
                _log.Changed.Add($"{label}: layer/tag/active/transform을 B 값으로 맞춤 (위치 {source.localPosition.ToString("0.###")})");

            go.layer = source.gameObject.layer;
            go.tag = source.tag;
            go.SetActive(source.gameObject.activeSelf);
            target.localPosition = source.localPosition;
            target.localRotation = source.localRotation;
            target.localScale = source.localScale;
        }

        private void EnsureCount(GameObject go, System.Type type, int count) {
            var have = 0;
            foreach (var existing in go.GetComponents(type))
                if (existing.GetType() == type)
                    have++;

            for (; have < count; have++) {
                go.AddComponent(type);
                _log.Changed.Add($"+{HierarchyPaths.Relative(_final.transform, go.transform)}.{type.Name} (B에서 추가)");
            }
        }

        private void CopyRootComponent(string typeName, HierarchyMap map) {
            var source = ComponentNames.Find(_hty, typeName);
            if (source == null) {
                _log.Notes.Add($"B에 {typeName}가 없어 건너뜀");
                return;
            }

            var target = ComponentNames.Find(_final, typeName);
            if (target == null) {
                target = _final.AddComponent(source.GetType());
                _log.Changed.Add($"+{typeName} (B에서 추가)");
            }

            SerializedValueCopier.Copy(source, target, map, $"B→{typeName}", _log);
        }

        private void CopySubtree(Transform source, HierarchyMap map) {
            foreach (var component in source.GetComponents<Component>()) {
                if (component == null || component is Transform)
                    continue;

                var target = (Component)map.Resolve(component);
                if (target == null) {
                    _log.Notes.Add($"{source.name}.{component.GetType().Name}: 최종본에서 짝을 찾지 못함");
                    continue;
                }

                var label = $"B→{HierarchyPaths.Relative(_hty.transform, source)}.{component.GetType().Name}";
                SerializedValueCopier.Copy(component, target, map, label, _log, AdjustFor(component));
            }

            foreach (Transform child in source)
                CopySubtree(child, map);
        }

        private System.Action<SerializedObject> AdjustFor(Component component) {
            if (_copyPrefab == null || component.GetType().Name != PlayerParts.CopySkill)
                return null;

            return so => CopySkillLink.Point(so, _copyPrefab, _log);
        }
    }
}
