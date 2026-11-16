using System.Collections.Generic;
using UnityEngine;

public enum SkillState
{
    None,
    ReSize,
    Copy,
    Paste,
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
    
    public Dictionary<SkillState, SkillSo_HTY> _skillDict = new();

    private void OnValidate()
    {
        if (_skillSoList.Count != _stateList.Count) return;

        _skillDict.Clear();

        for (int i =0;i < _skillSoList.Count;i++)
        {
            SkillState skillState = _stateList[i];
            SkillSo_HTY skillSo = _skillSoList[i];
            _skillDict.Add(skillState,skillSo);
        }
    }
}
