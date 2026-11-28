using System.Collections.Generic;

namespace LDY.SkillTargets.Core {
    public enum BodyPolicy { Kinematic, Static, Dynamic, Skip }

    public sealed class ComponentRequirement {
        public ComponentRequirement(string typeName, IReadOnlyDictionary<string, string> fields = null) {
            TypeName = typeName;
            Fields = fields ?? new Dictionary<string, string>();
        }

        public string TypeName { get; }
        public IReadOnlyDictionary<string, string> Fields { get; }
    }

    public sealed class AbilityRequirement {
        public AbilityRequirement(string id, string myCan, bool selectorUsesMyCan, bool needsRigidbody,
            IReadOnlyList<ComponentRequirement> components = null) {
            Id = id;
            MyCan = myCan;
            SelectorUsesMyCan = selectorUsesMyCan;
            NeedsRigidbody = needsRigidbody;
            Components = components ?? new ComponentRequirement[0];
        }

        public string Id { get; }
        public string MyCan { get; }
        public bool SelectorUsesMyCan { get; }
        public bool NeedsRigidbody { get; }
        public IReadOnlyList<ComponentRequirement> Components { get; }
    }

    public sealed class TargetRequest {
        public TargetRequest(string prefabPath, string child, string label, IReadOnlyList<AbilityRequirement> abilities,
            BodyPolicy body = BodyPolicy.Kinematic, string materialPath = null, int layer = -1) {
            PrefabPath = prefabPath;
            Child = child ?? "";
            Label = label;
            Abilities = abilities;
            Body = body;
            MaterialPath = string.IsNullOrEmpty(materialPath) ? null : materialPath;
            Layer = layer;
        }

        public string PrefabPath { get; }
        public string Child { get; }
        public string Label { get; }
        public IReadOnlyList<AbilityRequirement> Abilities { get; }
        public BodyPolicy Body { get; }
        public string MaterialPath { get; }
        public int Layer { get; }
    }
}
