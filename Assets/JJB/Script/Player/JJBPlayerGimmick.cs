using JJB.Script.Player.Interface;
using UnityEngine;

namespace JJB.Script.Player
{
    public class JJBPlayerGimmick : MonoBehaviour
    {
        public IAbsorbable CurrentGimmick { get; private set; }

        public bool HasGimmick => CurrentGimmick != null;

        public bool TryAbsorb(IAbsorbable gimmick)
        {
            if (HasGimmick || gimmick == null)
                return false;

            CurrentGimmick = gimmick;
            return true;
        }

        public void ReleaseGimmick()
        {
            CurrentGimmick = null;
        }
    }
}