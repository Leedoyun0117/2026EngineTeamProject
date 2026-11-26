using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace LDY.FlowEditor {
    // 일회용: 선택 화면에 걷는 캐릭터를 넣었던 시기의 씬 오브젝트를 걷어낸다(오브젝트 2개, 카메라 맞춤, 대화상자용 중첩 캔버스).
    // Build Stage Select Scene을 한 번 돌려 씬이 정리되면 스텁 스크립트 2개와 함께 이 파일도 삭제한다.
    internal static class StageSelectLegacyCleanup {
        private static readonly string[] RootNames = { "AltPlayer", "FileBoard" };
        private const string CameraFitType = "LDY.StageSelect.PixelCameraFit, LDY.StageSelect";
        private const string EntryDirectorType = "LDY.Script.SceneEntryDirector, Assembly-CSharp";

        public static void Run(Scene scene, Camera camera, IEnumerable<Component> modalAreas, FlowLog log) {
            ClearEntryLock(scene, log);
            foreach (var name in RootNames) DestroyRoot(scene, name, log);
            RemoveByTypeName(camera.gameObject, CameraFitType, log);
            foreach (var area in modalAreas) RemoveNestedCanvas(area.gameObject, log);
        }

        // SceneEntryDirector가 지울 오브젝트의 입력 잠금을 가리키고 있으면 비운다(원래대로 실행 시 FindFirst로 찾는다).
        private static void ClearEntryLock(Scene scene, FlowLog log) {
            var entry = Root(scene, "SceneEntry");
            var type = Type.GetType(EntryDirectorType);
            var director = entry != null && type != null ? entry.GetComponent(type) : null;
            if (director == null) return;

            var so = new SerializedObject(director);
            var property = so.FindProperty("playerLock");
            if (!(property?.objectReferenceValue is Component target)) return;
            if (Array.IndexOf(RootNames, target.transform.root.name) < 0) return;
            property.objectReferenceValue = null;
            so.ApplyModifiedPropertiesWithoutUndo();
            log.Info("SceneEntryDirector.playerLock 연결 해제");
        }

        private static void DestroyRoot(Scene scene, string name, FlowLog log) {
            var go = Root(scene, name);
            if (go == null) return;
            Object.DestroyImmediate(go);
            log.Info("삭제 " + name);
        }

        private static void RemoveByTypeName(GameObject go, string typeName, FlowLog log) {
            var type = Type.GetType(typeName);
            var component = type != null ? go.GetComponent(type) : null;
            if (component == null) return;
            Object.DestroyImmediate(component);
            log.Info($"삭제 {go.name}.{type.Name}");
        }

        // 루트 캔버스가 아닌 영역에 붙은 Canvas·GraphicRaycaster. 레이캐스터가 Canvas에 의존하므로 먼저 뗀다.
        private static void RemoveNestedCanvas(GameObject go, FlowLog log) {
            var canvas = go.GetComponent<Canvas>();
            if (canvas == null || go.transform.parent == null) return;
            var raycaster = go.GetComponent<GraphicRaycaster>();
            if (raycaster != null) Object.DestroyImmediate(raycaster);
            Object.DestroyImmediate(canvas);
            log.Info($"삭제 {go.name} 중첩 캔버스");
        }

        private static GameObject Root(Scene scene, string name) {
            foreach (var root in scene.GetRootGameObjects())
                if (root.name == name)
                    return root;
            return null;
        }
    }
}
