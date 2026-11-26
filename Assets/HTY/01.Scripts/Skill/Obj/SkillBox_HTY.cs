using JJB.Script.Player;
using JJB.Script.Player.Interface;
using System;

public class SkillBox_HTY : JJBAbsorbable
{
    public SkillSo_HTY _myskill;

    public override bool Absorb(JJBPlayerAbsorb target)
    {
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
