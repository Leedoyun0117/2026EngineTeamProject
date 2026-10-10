using UnityEngine;

[CreateAssetMenu(fileName = "SkillSo_HTY", menuName = "So_HTY/SkillSo_HTY")]
public class SkillSo_HTY : ScriptableObject
{
    public string skillName;
    [TextArea] public string des;
    public float coolTime;

}

