using System;

namespace LDY.Script {
    // angleStepDegrees: 회전 단위, maxAngleDegrees: 최대 회전, blendTime: 붙고 놓을 때 블렌드 시간, footSmoothTime: 발 위치 보간 시간
    [Serializable]
    public struct GripTuning {
        public float angleStepDegrees;
        public float maxAngleDegrees;
        public float blendTime;
        public float footSmoothTime;
    }
}
