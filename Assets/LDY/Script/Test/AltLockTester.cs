using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LDY.Script
{
    // 테스트용: L 키로 컷씬 입력 잠금을 토글해 외부 잠금 시스템을 확인한다.
    [RequireComponent(typeof(PlayerInputLock))]
    public class AltLockTester : MonoBehaviour
    {
        private IInputLock _lock;
        private IDisposable _token;

        private void Awake()
        {
            _lock = GetComponent<PlayerInputLock>().Lock;
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null || !keyboard.lKey.wasPressedThisFrame)
                return;

            if (_token == null)
                _token = _lock.Acquire();
            else
            {
                _token.Dispose();
                _token = null;
            }
        }

        private void OnGUI()
        {
            GUI.Label(new Rect(10, 10, 420, 60),
                $"A/D 이동, Space 점프, Alt 홀드: Alt 모드\nL: 입력 잠금 토글 (잠금: {_lock.IsLocked})\ntimeScale: {Time.timeScale}");
        }
    }
}
