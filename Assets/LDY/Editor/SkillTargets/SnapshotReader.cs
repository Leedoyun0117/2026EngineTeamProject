using System.Collections.Generic;
using System.Linq;
using LDY.SkillTargets.Core;
using UnityEditor;
using UnityEngine;

namespace LDY.SkillTargetsEditor {
    internal static class SnapshotReader {
        public static TargetSnapshot Read(GameObject go, MergedTarget target) {
            if (go == null) return TargetSnapshot.NotFound();

            var renderer = go.GetComponent<SpriteRenderer>();
            var snapshot = new TargetSnapshot {
                HasCollider = go.GetComponent<Collider2D>() != null,
                HasSpriteRenderer = renderer != null,
                HasRigidbody = go.GetComponent<Rigidbody2D>() != null,
                MaterialPath = renderer != null && renderer.sharedMaterial != null
                    ? AssetDatabase.GetAssetPath(renderer.sharedMaterial)
                    : null,
                Layer = go.layer
            };

            foreach (var (typeName, fields) in WatchedFields(target)) {
                var type = HtyTypes.Find(typeName);
                var component = type != null ? go.GetComponent(type) : null;
                if (component != null) snapshot.Components[typeName] = ReadFields(component, fields, go);
            }

            return snapshot;
        }

        private static IEnumerable<(string, IEnumerable<string>)> WatchedFields(MergedTarget target) {
            foreach (var component in target.Components) yield return (component.TypeName, component.Fields.Keys);
            yield return (HtyContract.ObjectType, new[] { HtyContract.MyCanField });
        }

        private static Dictionary<string, string> ReadFields(Component component, IEnumerable<string> fields, GameObject owner) {
            var serialized = new SerializedObject(component);
            return fields
                .Select(f => (f, serialized.FindProperty(f)))
                .Where(p => p.Item2 != null)
                .ToDictionary(p => p.f, p => SerializedValue.Read(p.Item2, owner));
        }
    }
}
