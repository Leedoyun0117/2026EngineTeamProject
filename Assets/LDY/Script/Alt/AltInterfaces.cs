using System;
using UnityEngine;

namespace LDY.Script
{
    public enum AltPhase
    {
        Normal,
        Alt,
        Return
    }

    // 시작 화면, 설정창 등 외부 UI가 읽는 읽기 전용 상태.
    public interface IAltStatus
    {
        AltPhase Phase { get; }
        event Action<AltPhase> PhaseChanged;
        Vector2 CursorWorldPosition { get; }
    }

    public interface IAltInput
    {
        bool Pressed { get; }
        bool Held { get; }
    }

    public interface IPointerSource
    {
        Vector2 WorldPosition { get; }
    }

    public interface IPlayerBody
    {
        Vector2 Position { get; set; }
        Vector2 Velocity { get; set; }
        bool PhysicsEnabled { get; set; }
        void SyncTransforms();
    }

    public interface IFacing
    {
        FacingState Capture();
        void Restore(FacingState state);
    }

    public interface IPlayerControl
    {
        void SetSuspended(bool suspended);
    }

    public interface IAfterimage : IDisposable
    {
        void SetFade(float visibility);
    }

    public interface IAfterimageSpawner
    {
        IAfterimage Spawn();
    }

    public interface ITimeFreezer
    {
        void Freeze();
        void Release();
    }

    public interface ICameraLock
    {
        void Freeze();
        void Release();
    }
}
