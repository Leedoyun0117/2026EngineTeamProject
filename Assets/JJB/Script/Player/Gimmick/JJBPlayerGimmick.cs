using System;
using HTY._01.Scripts.Skill;
using JJB.Script.Player.Interface;
using UnityEngine;

namespace JJB.Script.Player
{
    public class JJBPlayerGimmick : MonoBehaviour
    {
        public SkillSo_HTY CurrentGimmick { get; private set; }

        public bool HasGimmick => CurrentGimmick != null;

        private void OnEnable()
        {
            SkillManager_HTY.Instance.OnSkillDropped += ReleaseGimmick;
        }

        private void OnDisable()
        {
            SkillManager_HTY.Instance.OnSkillDropped -= ReleaseGimmick;
        }

        public bool TryAbsorb(SkillSo_HTY skillData)
        {
            if (HasGimmick || skillData == null)
                return false;

            CurrentGimmick = skillData;
            return true;
        }

        public void ReleaseGimmick()
        {
            CurrentGimmick = null;
        }
    }
}