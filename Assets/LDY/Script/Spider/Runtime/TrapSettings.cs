using UnityEngine;

namespace LDY.Spider {
    [CreateAssetMenu(menuName = "LDY/Stage2/Trap Settings", fileName = "TrapSettings")]
    public sealed class TrapSettings : ScriptableObject {
        [SerializeField] [Min(0.1f)] private float restMass = 1000f;
        [SerializeField] [Min(0f)] private float gravityScale = 3f;
        [SerializeField] [Min(0f)] private float hazardPadding = 0.05f;

        public float RestMass => restMass;
        public float GravityScale => gravityScale;
        public float HazardPadding => hazardPadding;
    }
}
