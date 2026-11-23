using System;
using LDY.PlayerMerge;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LDY.Script {
    internal sealed class ComponentSync {
        private readonly GameObject _base;
        private readonly GameObject _ours;
        private readonly MergeLog _log;

        public ComponentSync(GameObject baseRoot, GameObject oursRoot, MergeLog log) {
            _base = baseRoot;
            _ours = oursRoot;
            _log = log;
        }

        public void Run() {
            foreach (var source in _base.GetComponents<Component>()) {
                if (source == null || source is Transform)
                    continue;

                var type = source.GetType();
                var existing = _ours.GetComponent(type);
                switch (MergePolicy.ForComponent(true, existing != null)) {
                    case MergeAction.CopyBaseValues:
                        SerializedValueCopier.CopyValues(source, existing, type.Name, _log);
                        break;
                    case MergeAction.AddFromBase:
                        Add(source, type);
                        break;
                }
            }
        }

        private void Add(Component source, Type type) {
            var added = _ours.AddComponent(type);
            SerializedValueCopier.CopyValues(source, added, type.Name, _log);
            CopyInternalReferences(source, added);
            _log.Changed.Add($"+{type.Name} (지빈이 프리팹에서 추가)");
        }

        private void CopyInternalReferences(Component source, Component target) {
            var from = new SerializedObject(source);
            var to = new SerializedObject(target);
            var iterator = from.GetIterator();
            for (var enter = true; iterator.NextVisible(enter); enter = false) {
                if (iterator.propertyType != SerializedPropertyType.ObjectReference || iterator.name == "m_Script")
                    continue;

                to.FindProperty(iterator.propertyPath).objectReferenceValue = Map(iterator.objectReferenceValue);
            }

            to.ApplyModifiedPropertiesWithoutUndo();
        }

        private Object Map(Object value) {
            var node = value is Component component ? component.transform : (value as GameObject)?.transform;
            if (node == null || !node.IsChildOf(_base.transform))
                return value;

            var counterpart = HierarchyPaths.Find(_ours.transform, HierarchyPaths.Relative(_base.transform, node));
            if (counterpart == null)
                return null;

            if (value is GameObject)
                return counterpart.gameObject;

            return value is Transform ? counterpart : counterpart.GetComponent(value.GetType());
        }
    }
}
