using System;
using System.IO;
using JJB.Script.Player;
using LDY.TitlePlayer;
using LDY.TitlePlayer.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LDY.Script.Editor {
    // AltPlayer.prefab(GUID 유지, 열어서 갱신)과 LDY_TestScene의 AltTestPlayer를 JJB 없는 타이틀 플레이어로 바꾼다.
    public static class TitlePlayerConverter {
        private const string AltPlayerPrefab = "Assets/LDY/Prefab/AltPlayer.prefab";
        private const string TestScene = "Assets/LDY/Scenes/LDY_TestScene.unity";
        private const string TestPlayer = "AltTestPlayer";
        private const string ProxyChild = "JJBFacingProxy";

        // RequireComponent 의존 순서대로 지운다(요구하는 쪽 먼저).
        private static readonly Type[] RemoveOrder = {
            typeof(JJBControlGate), typeof(JJBPlayerMotionSource), typeof(JJBPlayer),
            typeof(JJBPlayerJump), typeof(JJBPlayerMovement), typeof(JJBPlayerInput)
        };

        [MenuItem("LDY/Player/Convert Title Players")]
        private static void Run() {
            ConvertPrefab();
            ConvertTestScene();
            AssetDatabase.SaveAssets();
        }

        public static void Convert(GameObject player, string label) {
            Debug.Log($"[TitlePlayer] {label} 전: {TitlePlayerAssembler.Components(player)}");
            var tuning = TitlePlayerAssembler.LoadOrCreateTuning(out var created);
            if (created) SeedTuning(tuning, player, label);

            foreach (var type in RemoveOrder)
                if (player.TryGetComponent(type, out var component))
                    Object.DestroyImmediate(component, true);
            var proxy = player.transform.Find(ProxyChild);
            if (proxy != null) Object.DestroyImmediate(proxy.gameObject, true);

            TitlePlayerAssembler.Install(player, tuning);
            Debug.Log($"[TitlePlayer] {label} 후: {TitlePlayerAssembler.Components(player)}");
        }

        // 튜닝 에셋을 처음 만들 때는 지금 타이틀 플레이어 값으로 채워 체감을 그대로 둔다.
        private static void SeedTuning(TitlePlayerTuning tuning, GameObject player, string label) {
            if (!JJBMovementReader.TryRead(player, out var values)) return;
            tuning.Apply(TuningMapping.FromJJB(values));
            EditorUtility.SetDirty(tuning);
            Debug.Log($"[TitlePlayer] 튜닝 생성 ({label} 값): {tuning.Describe()}");
        }

        private static void ConvertPrefab() {
            var root = PrefabUtility.LoadPrefabContents(AltPlayerPrefab);
            try {
                Convert(root, AltPlayerPrefab);
                PrefabUtility.SaveAsPrefabAsset(root, AltPlayerPrefab);
            }
            finally {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        // 테스트 씬을 열어 바꾸고 저장한 뒤, 원래 열려 있던 씬 구성으로 되돌린다.
        private static void ConvertTestScene() {
            if (!File.Exists(TestScene) || !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var setup = EditorSceneManager.GetSceneManagerSetup();
            try {
                var scene = EditorSceneManager.OpenScene(TestScene, OpenSceneMode.Single);
                var player = Array.Find(scene.GetRootGameObjects(), go => go.name == TestPlayer);
                if (player == null) {
                    Debug.LogWarning($"[TitlePlayer] {TestScene}에 {TestPlayer}가 없다");
                    return;
                }

                Convert(player, TestScene + "/" + TestPlayer);
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }
            finally {
                if (setup.Length > 0) EditorSceneManager.RestoreSceneManagerSetup(setup);
            }
        }
    }
}
