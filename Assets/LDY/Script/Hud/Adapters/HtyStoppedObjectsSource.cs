using System.Collections.Generic;
using HTY._01.Scripts.Skill;
using LDY.Hud.Core;
using UnityEngine;

namespace LDY.Hud.Adapters {
    // 태영 StopObjSkill_HTY가 멈춘 오브젝트. 공개 상태가 없어 비공개 필드를 읽는다.
    internal sealed class HtyStoppedObjectsSource : IStoppedObjectsSource, IHudRefreshable {
        private static readonly PrivateField<GameObject> StopObject =
            PrivateField<GameObject>.Find(typeof(StopObjSkill_HTY), "_stopObj");
        private static readonly PrivateField<bool> IsStop =
            PrivateField<bool>.Find(typeof(StopObjSkill_HTY), "_isStop");

        private readonly List<HudRect> _stopped = new List<HudRect>();
        private StopObjSkill_HTY _skill;

        public static HtyStoppedObjectsSource TryCreate() =>
            StopObject != null && IsStop != null ? new HtyStoppedObjectsSource() : null;

        public static string MissingReason => "StopObjSkill_HTY._stopObj/_isStop을 찾지 못함";

        public IReadOnlyList<HudRect> Stopped => _stopped;

        public void Refresh() {
            _stopped.Clear();
            var skill = Skill();
            if (skill == null || !IsStop.Read(skill))
                return;

            var target = StopObject.Read(skill);
            if (target != null)
                _stopped.Add(WorldBounds.Of(target));
        }

        private StopObjSkill_HTY Skill() {
            if (_skill != null)
                return _skill;

            var manager = SkillManager_HTY.Instance;
            _skill = manager != null ? manager.GetComponentInChildren<StopObjSkill_HTY>(true) : null;
            return _skill;
        }
    }
}
