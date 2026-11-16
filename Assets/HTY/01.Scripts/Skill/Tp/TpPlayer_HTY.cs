using UnityEngine;

public class TpPlayer_HTY : MonoBehaviour, ISkill_HTY
{
    [SerializeField] private Transform _player;

    public SkillSo_HTY _skillSO { get; set; }
    public SkillState _skillState { get; set; }

    public bool CanUse() => true;

    private void Start()
    {
        Init();
    }

    public void Init()
    {
        _skillState = SkillState.Tp;
        _skillSO = GameManager_HTY.Instance._skillList._skillDict[_skillState];
    }

    public void UseSkill()
    {
        if (CanUse())
        {
            //Collider2D[] coll = Physics2D.OverlapPointAll(Pointer().position);
            /*foreach (Collider2D coll2d in coll)
            {
                if (coll2d.gameObject.TryGetComponent<TpObj_HTY>(out _))
                {
                    //_player.position = Pointer().position;
                }
            }*/
        }
    }

    public void Enter()
    {
        throw new System.NotImplementedException();
    }

    public void Exit()
    {
        throw new System.NotImplementedException();
    }

    public void UpdateState()
    {
        throw new System.NotImplementedException();
    }
}
