using System.Collections.Generic;
using System.Linq;
using LDY.SkillTargets.Core;
using UnityEditor;
using UnityEngine;

namespace LDY.SkillTargetsEditor {
    // 규칙 표를 처음 만들 때 넣는 값. 기획서 능력별 대상 + HTY 코드(origin/HTY a2a7a98) 조사 결과.
    internal static class SkillTargetSeed {
        private const string MainObj = "Assets/Main/02.Prefabs/Obj/";
        private const string MainSkill = "Assets/Main/02.Prefabs/SkillBlock/";
        private const string Ldy1 = "Assets/LDY/Prefabs/Stage1/";
        private const string Hes = "Assets/HES/prefab/";
        private const string Jjb = "Assets/JJB/Prefab/";

        private const string NotDrivenByRigidbody = "Rigidbody가 아니라 DOTween/transform으로 움직여서 정지 스킬을 써도 안 멈춘다. 탐색·선택만 된다.";
        private const string NotFound = "기획 대상이지만 프리팹을 못 찾음. 정해지면 prefabs에 넣는다.";

        public static List<SkillTargetRules.Ability> Abilities() => new List<SkillTargetRules.Ability> {
            Ability("stop", "CanStop", true, true,
                "StopObjSkill_HTY: 대상의 Rigidbody2D 속도를 0으로, Kinematic으로 바꾼다. Rigidbody2D 없으면 아무 일도 없다."),
            Ability("drag", "CanDrag", false, true,
                "DragSkill_HTY: MyCan을 보지 않고 반경 2 안의 DragObj를 찾는다. DragObj._rb는 Awake에서도 채운다.",
                Component("DragObj", ("_rb", HtyContract.SelfReference))),
            Ability("zoom", "CanResize", true, false,
                "ReScaleObjSkill_HTY: MyCan=CanResize로 찾고 ReScaleObject_HTY.ChangeScale 호출.",
                Component("ReScaleObject_HTY", ("_smallScale", "0.5"), ("_largeScale", "2"))),
            Ability("alpha", "CanOpacity", true, false,
                "OpacitySkill_HTY: SpriteRenderer 알파 0.5/1. OpacityObj_HTY가 알파<0.9면 콜라이더를 트리거로.",
                Component("OpacityObj_HTY")),
            Ability("cut", "CanCut", true, false, "CuttingSkill_HTY: 선택 대상을 Destroy.")
        };

        public static List<SkillTargetRules.Target> Targets() => new List<SkillTargetRules.Target> {
            Target("움직이는 발판 ↔/↕", new[] { "stop" }, "", BodyPolicy.Skip, NotDrivenByRigidbody,
                MainObj + "Stage1_MovingPlatformH.prefab", MainObj + "Stage1_MovingPlatformV.prefab"),
            Target("상승 발판", new[] { "stop" }, "", BodyPolicy.Kinematic, NotFound),
            Target("360도 회전 블록/발판", new[] { "stop" }, "", BodyPolicy.Skip,
                NotDrivenByRigidbody + " LDY Stage1_RotateBlock은 JJB RotateBlock 중첩이라 원본을 따라간다.",
                MainObj + "RotateBlock.prefab", Jjb + "RotateBlock.prefab", Ldy1 + "Stage1_RotatingBlock.prefab"),
            Target("기요틴(진자 포함)", new[] { "stop" }, "GameObject", BodyPolicy.Skip,
                NotDrivenByRigidbody + " 루트에는 콜라이더가 없어 KillHitbox 자식(GameObject)에 붙인다.",
                MainObj + "Guillotine.prefab", Hes + "Guillotine.prefab"),
            Target("기요틴(LDY 기타 진자)", new[] { "stop" }, "Guitar", BodyPolicy.Skip, NotDrivenByRigidbody,
                Ldy1 + "Stage1_Guillotine.prefab"),
            Target("톱니", new[] { "stop" }, "", BodyPolicy.Skip, "Rigidbody2D·회전 스크립트 없음. 무엇이 돌리는지 확인 필요.",
                Ldy1 + "Stage1_Gear.prefab"),
            Target("상자 (정지)", new[] { "stop" }, "", BodyPolicy.Dynamic, "",
                MainObj + "Box.prefab", MainObj + "BoxStage2.prefab", Ldy1 + "Stage1_BoxCdCase.prefab"),
            Target("떨어지는 오브젝트", new[] { "stop" }, "", BodyPolicy.Dynamic, "",
                MainObj + "FallingBlock.prefab", Hes + "FallingBlock.prefab"),
            Target("엘리베이터", new[] { "stop" }, "platform", BodyPolicy.Skip,
                NotDrivenByRigidbody + " 콜라이더가 있는 platform 자식에 붙인다.",
                Hes + "Elevator.prefab"),
            Target("포탄을 막는 벽(신호 블록, 1-5)", new[] { "stop" }, "", BodyPolicy.Kinematic, "",
                Ldy1 + "Stage1_SignalBlock.prefab"),
            Target("상자 전체 (드래그)", new[] { "drag" }, "", BodyPolicy.Dynamic, "",
                MainObj + "Box.prefab", MainObj + "BoxStage2.prefab", Ldy1 + "Stage1_BoxCdCase.prefab"),
            Target("상자 (확대·축소)", new[] { "zoom" }, "", BodyPolicy.Dynamic, "",
                MainObj + "Box.prefab", MainObj + "BoxStage2.prefab", Ldy1 + "Stage1_BoxCdCase.prefab"),
            Target("피스톤(2-2)", new[] { "zoom" }, "", BodyPolicy.Kinematic, NotFound),
            Target("투명화 블록", new[] { "alpha" }, "", BodyPolicy.Skip, "",
                MainSkill + "OpacityBlock.prefab", Ldy1 + "Stage1_GhostBlock.prefab"),
            Target("상자를 받치는 블록(2-2, 2-3)", new[] { "alpha" }, "", BodyPolicy.Skip, NotFound),
            Target("벽(2-5)", new[] { "alpha" }, "", BodyPolicy.Skip, NotFound + " CustomBlockMap '벽'도 프리팹 비어 있음."),
            Target("자를 수 있는 블록", new[] { "cut" }, "", BodyPolicy.Skip, "",
                MainSkill + "CuttingBlock.prefab", Ldy1 + "Stage1_CutBlock.prefab")
        };

        private static SkillTargetRules.Ability Ability(string id, string myCan, bool selectorUsesMyCan, bool needsRigidbody,
            string note, params SkillTargetRules.ComponentSpec[] components) => new SkillTargetRules.Ability {
            id = id, myCan = myCan, selectorUsesMyCan = selectorUsesMyCan, needsRigidbody = needsRigidbody,
            components = components.ToList(), note = note
        };

        private static SkillTargetRules.ComponentSpec Component(string type, params (string field, string value)[] fields) =>
            new SkillTargetRules.ComponentSpec {
                typeName = type,
                fields = fields.Select(f => new SkillTargetRules.FieldValue { field = f.field, value = f.value }).ToList()
            };

        private static SkillTargetRules.Target Target(string label, string[] abilities, string child, BodyPolicy body,
            string note, params string[] prefabPaths) => new SkillTargetRules.Target {
            label = label, abilities = abilities.ToList(), child = child, rigidbodyIfMissing = body, note = note,
            prefabs = prefabPaths.Select(AssetDatabase.LoadAssetAtPath<GameObject>).Where(p => p != null).ToList()
        };
    }
}
