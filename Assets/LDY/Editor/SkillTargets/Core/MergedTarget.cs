using System.Collections.Generic;

namespace LDY.SkillTargets.Core {
    public sealed class MergedTarget {
        public MergedTarget(string prefabPath, string child) {
            PrefabPath = prefabPath;
            Child = child;
        }

        public string PrefabPath { get; }
        public string Child { get; }
        public List<string> Labels { get; } = new List<string>();
        public List<string> AbilityIds { get; } = new List<string>();
        public string MyCan { get; set; }
        public List<string> MyCanConflicts { get; } = new List<string>();
        public List<ComponentRequirement> Components { get; } = new List<ComponentRequirement>();
        public bool NeedsRigidbody { get; set; }
        public BodyPolicy Body { get; set; }
        public string MaterialPath { get; set; }
        public int Layer { get; set; } = -1;
        public List<string> Warnings { get; } = new List<string>();

        public FolderOwner Owner => FolderOwners.Resolve(PrefabPath);
        public bool HasMyCanConflict => MyCanConflicts.Count > 1;
    }
}
