using System.IO;
using System.Linq;
using LDY.TitlePlayer;
using UnityEditor;
using UnityEngine;

namespace LDY.Script.Editor {
    // 타이틀 전용 플레이어 컴포넌트를 붙이고 연결한다. 변환 메뉴, TitleSceneSetup, AltTestSceneSetup이 같이 쓴다.
    public static class TitlePlayerAssembler {
        public const string TuningPath = "Assets/LDY/Data/Player/TitlePlayerTuning.asset";
        private const string GroundCheckName = "GroundCheck";

        public static TitlePlayerTuning LoadOrCreateTuning(out bool created) {
            var tuning = AssetDatabase.LoadAssetAtPath<TitlePlayerTuning>(TuningPath);
            created = tuning == null;
            if (!created) return tuning;
            Directory.CreateDirectory(Path.GetDirectoryName(TuningPath));
            AssetDatabase.Refresh();
            tuning = ScriptableObject.CreateInstance<TitlePlayerTuning>();
            AssetDatabase.CreateAsset(tuning, TuningPath);
            return tuning;
        }

        public static void Install(GameObject player, TitlePlayerTuning tuning) {
            Ensure<TitlePlayerInput>(player);
            var motor = Ensure<TitlePlayerMotor>(player);
            var facing = Ensure<TitlePlayerFacing>(player);
            Ensure<TitlePlayerControl>(player);
            Ensure<TitlePlayerAnimator>(player);

            var so = new SerializedObject(motor);
            so.FindProperty("tuning").objectReferenceValue = tuning;
            so.FindProperty("groundCheck").objectReferenceValue = GroundCheck(player.transform, tuning);
            so.ApplyModifiedPropertiesWithoutUndo();

            var facingSo = new SerializedObject(facing);
            facingSo.FindProperty("sprite").objectReferenceValue = player.GetComponentInChildren<SpriteRenderer>(true);
            facingSo.ApplyModifiedPropertiesWithoutUndo();
        }

        public static string Components(GameObject go) =>
            string.Join(", ", go.GetComponents<Component>().Where(c => c != null).Select(c => c.GetType().Name)) +
            " | 자식: " + string.Join(", ", go.transform.Cast<Transform>().Select(t => t.name));

        private static Transform GroundCheck(Transform root, TitlePlayerTuning tuning) {
            var existing = root.Find(GroundCheckName);
            if (existing != null) return existing;
            var check = new GameObject(GroundCheckName).transform;
            check.SetParent(root, false);
            check.localPosition = new Vector3(0f, tuning.groundCheckOffsetY, 0f);
            return check;
        }

        private static T Ensure<T>(GameObject go) where T : Component =>
            go.TryGetComponent<T>(out var existing) ? existing : go.AddComponent<T>();
    }
}
