using JJB.Script.Player;
using LDY.TitlePlayer.Core;
using UnityEditor;
using UnityEngine;

namespace LDY.Script.Editor {
    // JJBPlayerMovement·JJBPlayerJump의 직렬화 값을 읽기만 한다. 대상 파일은 저장하지 않는다.
    public static class JJBMovementReader {
        public static bool TryRead(GameObject player, out JJBMovementValues values) {
            values = default;
            var movement = player.GetComponent<JJBPlayerMovement>();
            var jump = player.GetComponent<JJBPlayerJump>();
            if (movement == null || jump == null) return false;

            var move = new SerializedObject(movement);
            var j = new SerializedObject(jump);
            var size = j.FindProperty("groundCheckSize").vector2Value;
            var check = j.FindProperty("groundCheck").objectReferenceValue as Transform;
            values = new JJBMovementValues(
                move.FindProperty("moveSpeed").floatValue,
                j.FindProperty("jumpSpeed").floatValue,
                j.FindProperty("maxJumpHoldTime").floatValue,
                j.FindProperty("extraGravity").floatValue,
                j.FindProperty("gravityDelay").floatValue,
                size.x, size.y,
                check != null ? check.localPosition.y : 0f,
                j.FindProperty("groundLayer").intValue);
            return true;
        }
    }
}
