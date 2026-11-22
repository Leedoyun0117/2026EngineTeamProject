using UnityEngine;
using UnityEngine.InputSystem;

public class TpPlayer_HTY : MonoBehaviour, ISkill_HTY
{
    public SkillSo_HTY _skillSO { get; set; }
    public SkillState _skillState { get; set; }

    public bool CanUse() => true;

    public void Init()
    {
        _skillState = SkillState.Tp;
        _skillSO = GameManager_HTY.Instance._skillList._skillDict[_skillState];
    }

    public void UseSkill()
    {
        Vector2 pointer = GameManager_HTY.Instance.GetPointerPos();
        if (CanUse())
        {
            Collider2D[] coll = Physics2D.OverlapPointAll(pointer);
            foreach (Collider2D coll2d in coll)
            {
                if (coll2d.gameObject.TryGetComponent<TpObj_HTY>(out _))
                {
                    GameManager_HTY.Instance._player.transform.position = pointer;
                }
            }
        }
    }

    public void Enter()
    {
        GameManager_HTY.Instance._utiltyCompo.OnPointer();
    }

    public void Exit()
    {
        GameManager_HTY.Instance._utiltyCompo.OffPointer();
    }

    public void UpdateState()
    {
        if (Keyboard.current.yKey.wasPressedThisFrame)
        {
            UseSkill();
        }
    }
}
