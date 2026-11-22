using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SkillManager_HTY : MonoBehaviour
{
    public List<ISkill_HTY> _skillList = new();
    public Dictionary<SkillSo_HTY, ISkill_HTY> _skillDict = new();
    private SkillSo_HTY _currentSkill;

    private void Start()
    {
        StartCoroutine(Init());
    }

    private IEnumerator Init()
    {
        yield return new WaitForSeconds(0.2f);

        _skillList.Clear();
        _skillList = GetComponentsInChildren<ISkill_HTY>().ToList();
        Debug.Log(_skillList.Count);


        foreach (var v in GameManager_HTY.Instance._skillList._skillSoList)
        {
            foreach (var s in _skillList)
            {
                s.Init();

                if (v == s._skillSO)
                {
                    _skillDict[v] = s;
                }
            }
        }
        Debug.Log(_skillDict.Count);
    }


    private void Update()
    {
        if (_currentSkill != null)
            _skillDict[_currentSkill].UpdateState();
    }

    public void ChangeSkillState(SkillSo_HTY changeSkill)
    {
        if(_currentSkill != null)
            _skillDict[_currentSkill].Exit();
        
        _currentSkill = changeSkill;
        Debug.Log(changeSkill.ToString());

        if (_currentSkill != null)
            _skillDict[_currentSkill].Enter();
    }
}
