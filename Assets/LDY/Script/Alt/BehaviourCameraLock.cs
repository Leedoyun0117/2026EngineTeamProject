using System;
using UnityEngine;

namespace LDY.Script
{
    // 카메라 추적 스크립트를 Alt 동안 비활성화한다. 추적 스크립트는 인스펙터에 등록.
    [Serializable]
    public class BehaviourCameraLock : ICameraLock
    {
        [SerializeField] private Behaviour[] followBehaviours;

        private bool[] _wasEnabled;

        public void Freeze()
        {
            if (followBehaviours == null || _wasEnabled != null)
                return;

            _wasEnabled = new bool[followBehaviours.Length];
            for (int i = 0; i < followBehaviours.Length; i++)
            {
                Behaviour b = followBehaviours[i];
                if (b == null)
                    continue;

                _wasEnabled[i] = b.enabled;
                b.enabled = false;
            }
        }

        public void Release()
        {
            if (_wasEnabled == null)
                return;

            for (int i = 0; i < followBehaviours.Length; i++)
            {
                if (followBehaviours[i] != null)
                    followBehaviours[i].enabled = _wasEnabled[i];
            }

            _wasEnabled = null;
        }
    }
}
