using JJB.Script.Player;
using UnityEditor.Searcher;
using UnityEngine;
using UnityEngine.InputSystem;

public class CopyPlayer_HTY : MonoBehaviour, ISkill_HTY
{
    [Header("CopySetting")]
    [SerializeField] private GameObject _playerPrefab;
    private bool _canCopy = true;
    private GameObject _currentCopyObj;
    private CopyState _currentState;

    private GameObject _playerObj;
    private GameObject _copyObj;
    private bool _isPlayerControll = true;

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
        _playerObj = transform.parent.gameObject;
        _skillState = SkillState.Copy;
        _skillSO = SkillManager_HTY.Instance._skillSoList._skillDict[_skillState];
    }

    public void UseSkill()
    {
        Vector2 pointer = SkillManager_HTY.Instance.GetPointerPos();

        _currentState = _copyObj == null ? CopyState.Copy : CopyState.Paste;

        if (_currentState == CopyState.Copy)//copy 모드
        {
            if (!CanUse()) return;

            _currentCopyObj = Instantiate(_playerPrefab, pointer, Quaternion.identity);
            _currentCopyObj.GetComponent<JJBPlayerMovement>().SetPlay(false);//이동 컴포 바꾸기
            _copyObj = _currentCopyObj;
            _canCopy = false;
            _currentState = CopyState.Paste;
        }
        else if( _currentState == CopyState.Paste) //paste모드
        {
            _isPlayerControll = !_isPlayerControll;

            if (_playerObj.TryGetComponent(out JJBPlayerMovement moveCompo))
            {
                moveCompo.Rb.simulated = _isPlayerControll;
                if (!_isPlayerControll) moveCompo.Move(0);
                moveCompo.SetPlay(_isPlayerControll);
            }

            if (_copyObj.TryGetComponent(out JJBPlayerMovement copyMoveCompo))
            {
                copyMoveCompo.Rb.simulated = !_isPlayerControll;
                if (_isPlayerControll) copyMoveCompo.Move(0);
                copyMoveCompo.SetPlay(!_isPlayerControll);
            }
        }
    }

    public void Enter()
    {
        _canCopy = true;
        _isPlayerControll = true;
        _currentState = CopyState.Copy;
        SkillManager_HTY.Instance._utilityCompo.OnPointer();
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

        if (_playerObj.TryGetComponent(out JJBPlayerMovement moveCompo))
        {
            moveCompo.Rb.simulated = true;
            moveCompo.SetPlay(true);
            _isPlayerControll = true;
        }

        _canCopy = true;
        SkillManager_HTY.Instance._utilityCompo.OffPointer();
        _currentState = CopyState.Copy;
    }
}
