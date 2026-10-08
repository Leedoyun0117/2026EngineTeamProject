using UnityEngine;

namespace LDY.Script
{
    // 인스펙터에서 아이콘마다 갈아끼울 수 있도록 IMenuAction을 컴포넌트로 노출한다.
    public abstract class MenuActionBehaviour : MonoBehaviour, IMenuAction
    {
        public abstract void Execute();
    }
}
