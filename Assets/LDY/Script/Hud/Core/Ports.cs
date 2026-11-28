using System;
using System.Collections.Generic;

namespace LDY.Hud.Core {
    public interface IAbilitySource {
        AbilitySlotState Slot { get; }
    }

    public interface IAbsorbTargetSource {
        bool HasTarget { get; }
        HudRect TargetBounds { get; }
        bool IsHolding { get; }
        event Action AbsorbPressed;
    }

    public interface IControlTargetSource {
        bool HasClone { get; }
        bool CloneControlled { get; }
        HudRect IdleBounds { get; }
    }

    public interface IInputLockSource {
        bool IsLocked { get; }
        HudRect PlayerBounds { get; }
    }

    public interface IStoppedObjectsSource {
        IReadOnlyList<HudRect> Stopped { get; }
    }
}
