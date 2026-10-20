using System.Collections;
using UnityEngine;

namespace LDY.Script
{
    public interface ITransitionAudio
    {
        void FanFade(float duration);
        void PowerOff();
        void PowerOn();
    }

    public class NullTransitionAudio : ITransitionAudio
    {
        public void FanFade(float duration) { }
        public void PowerOff() { }
        public void PowerOn() { }
    }

    public interface IGameNotifier
    {
        void Notify(string message);
    }

    public class LogGameNotifier : IGameNotifier
    {
        public void Notify(string message)
        {
            Debug.LogWarning($"[Notice] {message}");
        }
    }

    // Level 0은 켜짐, 1은 완전히 꺼짐. Fill은 켜졌을 때 화면이 차지하는 비율
    public interface ICrtPower
    {
        float Level { get; set; }
        Vector2 Fill { get; set; }
        IEnumerator AnimateTo(float target, float duration);
    }

    public interface ICameraRewind
    {
        IEnumerator Rewind(float duration);
    }

    public interface ISceneLoadHandle
    {
        bool Ready { get; }
        bool Failed { get; }
        bool IsDone { get; }
        void Activate();
    }

    public interface IGameSceneLoader
    {
        // 활성화 없이 불러만 둠. 이미 대기 중이면 같은 핸들 반환
        ISceneLoadHandle Begin();
    }
}
