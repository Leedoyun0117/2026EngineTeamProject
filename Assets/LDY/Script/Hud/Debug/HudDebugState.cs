using System;
using System.Collections.Generic;
using LDY.Hud.Core;

namespace LDY.Hud {
    // HudDebugFeeder가 HUD에 강제로 넣는 가짜 상태. 모든 포트를 한곳에서 흉내 낸다.
    public sealed class HudDebugState : IAbilitySource, IAbsorbTargetSource, IControlTargetSource, IInputLockSource,
        IStoppedObjectsSource {
        public readonly List<HudRect> StoppedTargets = new List<HudRect>();

        public AbilitySlotState Slot { get; set; } = AbilitySlotState.Empty;

        public bool HasTarget { get; set; }
        public HudRect TargetBounds { get; set; }
        public bool IsHolding { get; set; }
        public event Action AbsorbPressed;

        public bool HasClone { get; set; }
        public bool CloneControlled { get; set; }
        public HudRect IdleBounds { get; set; }

        public bool IsLocked { get; set; }
        public HudRect PlayerBounds { get; set; }

        public IReadOnlyList<HudRect> Stopped => StoppedTargets;

        public void PressAbsorb() => AbsorbPressed?.Invoke();

        public HudPorts ToPorts() => new HudPorts(this, this, this, this, this);
    }
}
