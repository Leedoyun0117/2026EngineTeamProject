using System;
using System.Collections.Generic;
using LDY.SkillTargets.Core;
using UnityEngine;

namespace LDY.SkillTargetsEditor {
    // 스킬 대상 규칙 표. 능력별 요구사항(abilities)과 기획 대상(targets)을 사람이 고쳐도 된다.
    [CreateAssetMenu(menuName = "LDY/Skill Targets/Rules", fileName = "SkillTargetRules")]
    internal sealed class SkillTargetRules : ScriptableObject {
        [Serializable]
        public sealed class FieldValue {
            public string field;

            [Tooltip("숫자·true/false·enum 이름. @self 는 같은 오브젝트의 컴포넌트로 연결")]
            public string value;
        }

        [Serializable]
        public sealed class ComponentSpec {
            public string typeName;
            public List<FieldValue> fields = new List<FieldValue>();
        }

        [Serializable]
        public sealed class Ability {
            public string id;

            [Tooltip("Object_HTY._myCan 에 넣을 CanIf 이름. 비우면 Object_HTY를 붙이지 않는다")]
            public string myCan;

            [Tooltip("TargetSelector_HTY가 이 MyCan으로 대상을 찾는가. 끄면 다른 능력과 겹칠 때 MyCan 후보에서 빠진다")]
            public bool selectorUsesMyCan = true;

            public bool needsRigidbody;
            public List<ComponentSpec> components = new List<ComponentSpec>();
            [TextArea(1, 4)] public string note;
        }

        [Serializable]
        public sealed class Target {
            [Tooltip("기획서 명칭")] public string label;
            public List<string> abilities = new List<string>();
            public List<GameObject> prefabs = new List<GameObject>();

            [Tooltip("프리팹 이름 패턴(* ?). 비우면 쓰지 않는다")]
            public string namePattern = "";

            [Tooltip("컴포넌트를 붙일 자식 이름. 비우면 루트. Collider2D·SpriteRenderer가 있는 오브젝트여야 탐색된다")]
            public string child;

            [Tooltip("Rigidbody2D가 필요한데 없을 때. Skip이면 추가하지 않고 보고만 한다")]
            public BodyPolicy rigidbodyIfMissing = BodyPolicy.Kinematic;

            [Tooltip("비우면 SpriteRenderer 머티리얼을 건드리지 않는다")]
            public Material material = null;

            [Tooltip("-1이면 레이어를 건드리지 않는다")]
            public int layer = -1;

            [TextArea(1, 4)] public string note;
        }

        [SerializeField] private List<Ability> abilities = new List<Ability>();
        [SerializeField] private List<Target> targets = new List<Target>();

        public IReadOnlyList<Ability> Abilities => abilities;
        public IReadOnlyList<Target> Targets => targets;

        public void Seed(List<Ability> seedAbilities, List<Target> seedTargets) {
            abilities = seedAbilities;
            targets = seedTargets;
        }
    }
}
