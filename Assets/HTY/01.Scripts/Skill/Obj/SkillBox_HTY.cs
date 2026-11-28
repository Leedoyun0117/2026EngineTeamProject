using JJB.Script.Player;
using JJB.Script.Player.Interface;
using System;
using UnityEngine;

public class SkillBox_HTY : JJBAbsorbable
{
    public SkillSo_HTY _myskill;

    public override bool Absorb(JJBPlayerAbsorb target)
    {
        Debug.Log("사용됨");
        try
        {
            SkillManager_HTY.Instance.ChangeSkillState(_myskill);
            Destroy(gameObject);
        }
        catch(NullReferenceException)
        {
            return false;
        }
        return true;
    }
}
