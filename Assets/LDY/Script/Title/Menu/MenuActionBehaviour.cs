using UnityEngine;

namespace LDY.Script
{
    // 아이콘마다 인스펙터에서 갈아끼우려고 컴포넌트로 둠
    public abstract class MenuActionBehaviour : MonoBehaviour, IMenuAction
    {
        public abstract void Execute();
    }
}
