using UnityEngine;

namespace JJB.Script.Player
{
    public class GimmickSkillProvider : MonoBehaviour
    {
        [SerializeField] private SkillSo_HTY skillData;
        public SkillSo_HTY SkillData => skillData;
    }
}