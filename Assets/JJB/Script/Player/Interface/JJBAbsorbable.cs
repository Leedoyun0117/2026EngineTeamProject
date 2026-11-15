using UnityEngine;

namespace JJB.Script.Player.Interface
{
    public abstract class JJBAbsorbable : MonoBehaviour, IAbsorbable
    {
        public abstract bool Absorb(JJBPlayerAbsorb target);
    }
}