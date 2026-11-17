using UnityEngine;
using UnityEngine.InputSystem;

public class CopyPlayer_HTY : MonoBehaviour, ISkill_HTY
{
    [Header("CopySetting")]
    private GameObject _playerPrefab;
    private bool _canCopy = true;
    private GameObject _currentCopyObj;
    private CopyState _currentState;

    private GameObject _playerObj;
    private GameObject _copyObj;

    public enum CopyState
    {
        Copy,
        Paste
    }

    public SkillSo_HTY _skillSO { get; set; }
    public SkillState _skillState { get;set; }

    public bool CanUse() => _canCopy;

    public void Init()
    {
        _playerPrefab = GameManager_HTY.Instance._playerPrefab;
        _playerObj = GameManager_HTY.Instance._player;
        _skillState = SkillState.Copy;
        _skillSO = GameManager_HTY.Instance._skillList._skillDict[_skillState];
    }

    public void UseSkill()
    {
        

        Vector2 pointer = GameManager_HTY.Instance._utiltyCompo.Pointer_HTY.transform.position;

        _currentState = _copyObj == null ? CopyState.Copy : CopyState.Paste;

        if (_currentState == CopyState.Copy)//copy 모드
        {
            if (!CanUse()) return;

            _currentCopyObj = Instantiate(_playerPrefab, pointer, Quaternion.identity);
            _currentCopyObj.GetComponent<TestPlayerMove_HTY>().SetPlay(false);//이동 컴포 바꾸기
            _copyObj = _currentCopyObj;
            _canCopy = false;
            _currentState = CopyState.Paste;
        }
        else if( _currentState == CopyState.Paste) //paste모드
        {
            if (_playerObj.TryGetComponent(out TestPlayerMove_HTY moveCompo))
            {
                moveCompo.SetPlay(false);
                if (_copyObj.TryGetComponent(out TestPlayerMove_HTY moveCompoOther))
                {
                    moveCompoOther.SetPlay(true);
                }
            }
            GameObject someObj = _playerObj;
            _playerObj = _copyObj;
            _copyObj = someObj;
        }
    }

    public void Enter()
    {
        _canCopy = true;
        _currentState = CopyState.Copy;
        GameManager_HTY.Instance._utiltyCompo.OnPointer();
    }

    public void UpdateState()
    {
        if (Keyboard.current.yKey.wasPressedThisFrame)
        {
            UseSkill();
        }
    }

    public void Exit()
    {
        Destroy(_currentCopyObj);
        _canCopy = true;
        GameManager_HTY.Instance._utiltyCompo.OffPointer();
        _currentState = CopyState.Copy;
    }
}
