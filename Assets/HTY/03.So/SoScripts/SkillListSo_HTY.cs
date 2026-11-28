using System.Collections.Generic;
using UnityEngine;

public enum SkillState
{
    None,
    Copy,
    Tp,
    Drag,
    TimeStop,
    Opacity,
    ReSizeObj,
    Luminosity,
    Cutting
}

[CreateAssetMenu(fileName = "SkillListSo_HTY", menuName = "So_HTY/SkillListSo_HTY")]
public class SkillListSo_HTY : ScriptableObject
{
    public List<SkillState> _stateList;
    public List<SkillSo_HTY> _skillSoList;
}