using UnityEngine;

namespace LDY.Script.Editor
{
    // 도트 에셋이 없는 보조 스프라이트(LED 점멸용 원)를 만든다. 좌표는 (0~1, 위쪽이 +v).
    internal static class TempShapes
    {
        private static readonly Color Clear = new Color(1f, 1f, 1f, 0f);

        public static Color Circle(float u, float v)
        {
            float distance = Mathf.Sqrt((u - 0.5f) * (u - 0.5f) + (v - 0.5f) * (v - 0.5f));
            return distance <= 0.5f ? Color.white : Clear;
        }
    }
}
