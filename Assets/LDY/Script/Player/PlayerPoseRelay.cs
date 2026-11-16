using UnityEngine;

namespace LDY.Script {
    public interface IPlayerPoseSink {
        void Report(bool grabbing, RowMoveDirection rowMove);
        void ReportRope(bool hanging, RowMoveDirection rowMove);
    }

    [DisallowMultipleComponent]
    public class PlayerPoseRelay : MonoBehaviour, IPlayerPoseSink, IGrabStateSource, IRowMoveSource, IRopeStateSource,
        IRopeAngleSink, IRopeAngleSource {
        public float RopeAngle { get; private set; }
        public bool IsGrabbing { get; private set; }
        public bool IsRopeHanging { get; private set; }
        public RowMoveDirection RowMove { get; private set; }

        public void Report(bool grabbing, RowMoveDirection rowMove) {
            IsGrabbing = grabbing;
            RowMove = rowMove;
        }

        public void ReportRopeAngle(float radians) {
            RopeAngle = radians;
        }

        public void ReportRope(bool hanging, RowMoveDirection rowMove) {
            IsRopeHanging = hanging;
            RowMove = rowMove;
        }
    }
}
