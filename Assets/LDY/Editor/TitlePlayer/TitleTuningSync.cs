using LDY.TitlePlayer.Core;
using UnityEditor;
using UnityEngine;

namespace LDY.Script.Editor {
    // 지빈 Player.prefab 값을 읽어 TitlePlayerTuning에 복사한다. 지빈 파일은 열기만 하고 저장하지 않는다.
    public static class TitleTuningSync {
        private const string JibinPlayer = "Assets/JJB/Prefab/Player.prefab";

        [MenuItem("LDY/Player/Sync Title Tuning From JJB")]
        private static void Run() {
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(JibinPlayer);
            if (source == null || !JJBMovementReader.TryRead(source, out var values)) {
                Debug.LogWarning("[TitlePlayer] 읽을 수 없음: " + JibinPlayer);
                return;
            }

            var tuning = TitlePlayerAssembler.LoadOrCreateTuning(out _);
            var before = tuning.Describe();
            tuning.Apply(TuningMapping.FromJJB(values));
            EditorUtility.SetDirty(tuning);
            AssetDatabase.SaveAssets();
            Debug.Log($"[TitlePlayer] {JibinPlayer} -> {TitlePlayerAssembler.TuningPath}\n  전: {before}\n  후: {tuning.Describe()}");
        }
    }
}
