using UnityEngine;

namespace LDY.Script {
    public interface IPlayerPoseSink {
        void Report(bool grabbing, RowMoveDirection rowMove);
    }

    [DisallowMultipleComponent]
    public class PlayerPoseRelay : MonoBehaviour, IPlayerPoseSink, IGrabStateSource, IRowMoveSource {
        public bool IsGrabbing { get; private set; }
        public RowMoveDirection RowMove { get; private set; }

        public void Report(bool grabbing, RowMoveDirection rowMove) {
            IsGrabbing = grabbing;
            RowMove = rowMove;
        }
    }
}
