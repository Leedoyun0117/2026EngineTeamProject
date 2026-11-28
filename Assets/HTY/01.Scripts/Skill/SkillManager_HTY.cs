using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class SkillManager_HTY : MonoBehaviour
{
    public static SkillManager_HTY Instance {  get; private set; }
    public event Action OnSkillDropped;
    
    public List<ISkill_HTY> _skillList = new();
    public Dictionary<SkillSo_HTY, ISkill_HTY> _skillDict = new();
    private SkillSo_HTY _currentSkill;
    public SkillListSo_HTY _skillSoList;
    public Utility_HTY _utilityCompo;
    
    public SkillSo_HTY CurrentSkill => _currentSkill;
    
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        StartCoroutine(Init());
    }

    private IEnumerator Init()
    {
        yield return new WaitForSeconds(0.2f);

        _skillList.Clear();
        _skillList = GetComponentsInChildren<ISkill_HTY>().ToList();

        foreach (var v in _skillSoList._skillSoList)
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
    }

    private void Update()
    {
        if (_currentSkill != null)
            _skillDict[_currentSkill].UpdateState();

        if (_currentSkill == null) return;

        if (Keyboard.current.qKey.wasPressedThisFrame)
            SkillDrop();
    }

    public void ChangeSkillState(SkillSo_HTY changeSkill)
    {
        if(_currentSkill != null)
            _skillDict[_currentSkill].Exit();
        
        _currentSkill = changeSkill;

        if (_currentSkill != null)
            _skillDict[_currentSkill].Enter();
    }

    public void SkillDrop()
    {
        if (_currentSkill == null)
            return;

        SkillSo_HTY droppedSkill = _currentSkill;
        _skillDict[droppedSkill].Exit();
        
        if(droppedSkill.prefab != null)
            Instantiate(_currentSkill.prefab,transform.position,Quaternion.identity);

        _currentSkill = null;
        OnSkillDropped?.Invoke();
    }

    public Vector2 GetPointerPos() => _utilityCompo.Pointer_HTY.transform.position;
}
