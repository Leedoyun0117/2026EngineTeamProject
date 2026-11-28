using System;
using LDY.Hud.Core;

namespace LDY.Hud {
    public sealed class HudPorts : IDisposable {
        public static readonly HudPorts Empty = new HudPorts(null, null, null, null, null);

        public HudPorts(IAbilitySource ability, IAbsorbTargetSource absorb, IControlTargetSource control,
            IInputLockSource inputLock, IStoppedObjectsSource stopped) {
            Ability = ability;
            Absorb = absorb;
            Control = control;
            InputLock = inputLock;
            Stopped = stopped;
        }

        public IAbilitySource Ability { get; }
        public IAbsorbTargetSource Absorb { get; }
        public IControlTargetSource Control { get; }
        public IInputLockSource InputLock { get; }
        public IStoppedObjectsSource Stopped { get; }

        public void Refresh() {
            Refresh(Ability);
            Refresh(Absorb);
            Refresh(Control);
            Refresh(InputLock);
            Refresh(Stopped);
        }

        public void Dispose() {
            Dispose(Ability);
            Dispose(Absorb);
            Dispose(Control);
            Dispose(InputLock);
            Dispose(Stopped);
        }

        private static void Refresh(object port) {
            if (port is IHudRefreshable refreshable)
                refreshable.Refresh();
        }

        private static void Dispose(object port) {
            if (port is IDisposable disposable)
                disposable.Dispose();
        }
    }
}
