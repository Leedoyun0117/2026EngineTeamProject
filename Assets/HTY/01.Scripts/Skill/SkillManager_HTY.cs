using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HTY._01.Scripts.Skill
{
    public class SkillManager_HTY : MonoBehaviour
    {
        public static SkillManager_HTY Instance { get; private set; }
        public event Action OnSkillDropped;
        
        public List<ISkill_HTY> _skillList = new();
        public Dictionary<SkillSo_HTY, ISkill_HTY> _skillDict = new();
        public Dictionary<SkillState, SkillSo_HTY> _stateToSoDict = new();
        private SkillSo_HTY _currentSkill;
        public SkillListSo_HTY _skillSoList;
        public Utility_HTY _utilityCompo;

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
            SetStateDict();
            StartCoroutine(Init());
        }

        public void SetStateDict()
        {
            _stateToSoDict.Clear();
            if (_skillSoList != null && _skillSoList._stateList != null && _skillSoList._skillSoList != null)
            {
                int count = Mathf.Min(_skillSoList._stateList.Count, _skillSoList._skillSoList.Count);
                for (int i = 0; i < count; i++)
                {
                    var state = _skillSoList._stateList[i];
                    var so = _skillSoList._skillSoList[i];
                    if (!_stateToSoDict.ContainsKey(state))
                    {
                        _stateToSoDict.Add(state, so);
                    }
                }
            }
        }

        private IEnumerator Init()
        {
            yield return new WaitForSeconds(0.2f);

            _skillList.Clear();
            _skillList = GetComponentsInChildren<ISkill_HTY>().ToList();

            foreach (var s in _skillList)
            {
                s.Init();
            }

            _skillDict.Clear();
            foreach (var v in _skillSoList._skillSoList)
            {
                foreach (var s in _skillList)
                {
                    if (v == s._skillSO)
                    {
                        _skillDict[v] = s;
                        break;
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

            if (_skillDict.TryGetValue(droppedSkill, out ISkill_HTY skill))
                skill.Exit();

            if (droppedSkill.prefab != null)
            {
                Instantiate(droppedSkill.prefab, transform.position, Quaternion.identity);
            }

            _currentSkill = null;

            OnSkillDropped?.Invoke();
        }

        public Vector2 GetPointerPos() => _utilityCompo.Pointer_HTY.transform.position;
    }
}