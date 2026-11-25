using System;
using System.IO;
using LDY.Flow;
using LDY.StageSelect;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

namespace LDY.FlowEditor {
    // 정적 틀(창·영역)만 씬에 굽고, 트리·아이템 같은 동적 요소는 각 뷰가 런타임에 만든다. 다시 돌리면 이름으로 찾아 갱신한다.
    internal static class StageSelectSceneBuilder {
        private const float WindowX = 6f;
        private const float WindowY = 4f;
        private const float WindowWidth = 628f;
        private const float WindowHeight = 330f;
        private const float BodyTop = 54f;
        private const float StatusHeight = 15f;
        private const float TreeWidth = 150f;
        private const float TabsHeight = FilterTabsView.Height;
        private const float TaskbarHeight = 22f;

        private const string SettingsType = "LDY.Script.StageSelectSettings, Assembly-CSharp";
        private const string VolumeType = "LDY.Script.MixerAudioVolume, Assembly-CSharp";

        private static readonly string[] EntryTypes = {
            "LDY.Script.PlayerInputLock, Assembly-CSharp",
            "LDY.Script.SceneEntryDirector, Assembly-CSharp"
        };

        public static void Build(FlowLog log) {
            if (!File.Exists(FlowPaths.Skin)) {
                log.Warn("UiSkin이 없다. LDY/Flow/Build Flow Assets 먼저 실행");
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var existed = File.Exists(FlowPaths.SelectScene);
            var scene = existed
                ? EditorSceneManager.OpenScene(FlowPaths.SelectScene, OpenSceneMode.Single)
                : EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            log.Info((existed ? "갱신 " : "만듦 ") + FlowPaths.SelectScene);

            // 씬을 연 뒤에 읽는다. 먼저 읽으면 OpenScene의 언로드로 파괴될 수 있다.
            var skin = AssetDatabase.LoadAssetAtPath<UiSkin>(FlowPaths.Skin);

            var ui = new UiFactory(skin);
            Camera(scene, skin);
            EventSystem(scene);
            Entry(scene, log);
            var canvas = Canvas(scene);
            var screen = Ensure<StageSelectScreen>(canvas.gameObject);
            var input = Ensure<StageSelectInput>(canvas.gameObject);

            Area(canvas, "Desktop", r => UiFactory.Stretch(r), parent => ui.Solid("Desktop", parent, skin.desktop));
            var window = Area(canvas, "Window", r => UiFactory.Place(r, WindowX, WindowY, WindowWidth, WindowHeight),
                parent => ui.Panel("Window", parent));
            var titleBar = Area(window, "TitleBar",
                r => UiFactory.TopStrip(r, WindowChrome.TitleBarInset, WindowChrome.TitleBarHeight,
                    WindowChrome.TitleBarInset, WindowChrome.TitleBarInset), null);
            var menuBar = Area(window, "MenuBar", r => UiFactory.TopStrip(r, 18f, 14f, 3f, 3f),
                parent => ui.NewText("MenuBar", parent, StageSelectText.MenuBar));
            RestyleBakedText(ui, menuBar, log);
            var address = Area(window, "AddressBar", r => UiFactory.TopStrip(r, 33f, 18f, 3f, 3f), null);
            var bodyHeight = WindowHeight - BodyTop - StatusHeight - 5f;
            var tree = Area(window, "Tree", r => UiFactory.Place(r, 3f, BodyTop, TreeWidth, bodyHeight),
                parent => ui.Sunken("Tree", parent));
            var rightX = 3f + TreeWidth + 3f;
            var rightWidth = WindowWidth - rightX - 3f;
            var tabs = Area(window, "Tabs", r => UiFactory.Place(r, rightX, BodyTop, rightWidth, TabsHeight), null);
            var grid = Area(window, "Grid",
                r => UiFactory.Place(r, rightX, BodyTop + TabsHeight, rightWidth, bodyHeight - TabsHeight),
                parent => ui.Sunken("Grid", parent));
            var status = Area(window, "StatusBar", r => UiFactory.BottomStrip(r, 3f, StatusHeight, 3f, 3f),
                parent => ui.Sunken("StatusBar", parent));
            var taskbar = Area(canvas, "Taskbar", r => UiFactory.BottomStrip(r, 0f, TaskbarHeight), null);
            var settings = Area(canvas, "Settings", r => UiFactory.Stretch(r), null);
            var dialog = Area(canvas, "Dialog", r => UiFactory.Stretch(r), null);
            settings.SetAsLastSibling();
            dialog.SetAsLastSibling();

            Wire(screen, input,
                Ensure<TitleBarView>(titleBar.gameObject), Ensure<AddressBarView>(address.gameObject),
                Ensure<TreeView>(tree.gameObject), Ensure<FilterTabsView>(tabs.gameObject),
                Ensure<FileGridView>(grid.gameObject), Ensure<StatusBarView>(status.gameObject),
                Ensure<TaskbarView>(taskbar.gameObject), Ensure<DialogView>(dialog.gameObject));
            WireSettings(screen, Ensure<SettingsDialogView>(settings.gameObject), SettingsService(scene, log));

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, FlowPaths.SelectScene);
            BuildSettingsSync.Run(AssetDatabase.LoadAssetAtPath<StageCatalog>(FlowPaths.Catalog), log);
        }

        // 씬에 구워 둔 글자는 Area가 다시 만들지 않으므로, 스킨이 바뀌어도 따라가도록 매번 스킨 값을 다시 입힌다.
        private static void RestyleBakedText(UiFactory ui, Component area, FlowLog log) {
            var text = area.GetComponent<TMP_Text>();
            if (text == null) return;
            var before = $"{(text.font != null ? text.font.name : "없음")}/{text.fontSize}";
            ui.Style(text);
            text.alignment = TextAlignmentOptions.Left;
            var after = $"{(text.font != null ? text.font.name : "없음")}/{text.fontSize}";
            if (before != after) log.Info($"{area.name} 글자 {before} -> {after}");
        }

        private static void WireSettings(StageSelectScreen screen, SettingsDialogView view, Component source) {
            var so = new SerializedObject(screen);
            so.FindProperty("settings").objectReferenceValue = view;
            so.FindProperty("settingsSource").objectReferenceValue = source;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // 타이틀과 같은 음량·화면 모드 구현. Assembly-CSharp 타입이라 이름으로 붙이고, 믹서는 타이틀과 같은 에셋을 건다.
        private static Component SettingsService(Scene scene, FlowLog log) {
            var type = Type.GetType(SettingsType);
            if (type == null) {
                log.Warn("타입 없음 " + SettingsType);
                return null;
            }

            var go = Root(scene, "SettingsService") ?? Create(scene, "SettingsService");
            var service = go.GetComponent(type);
            if (service == null) service = go.AddComponent(type);
            var volumeType = Type.GetType(VolumeType);
            var volume = volumeType != null ? go.GetComponent(volumeType) : null;
            var mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>(FlowPaths.AudioMixer);
            if (volume != null && mixer != null) {
                var so = new SerializedObject(volume);
                so.FindProperty("mixer").objectReferenceValue = mixer;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
            else {
                log.Warn("음량 어댑터 또는 믹서 없음 " + FlowPaths.AudioMixer);
            }

            return service;
        }

        private static void Wire(StageSelectScreen screen, StageSelectInput input, TitleBarView titleBar,
            AddressBarView address, TreeView tree, FilterTabsView tabs, FileGridView grid, StatusBarView status,
            TaskbarView taskbar, DialogView dialog) {
            var so = new SerializedObject(screen);
            so.FindProperty("titleBar").objectReferenceValue = titleBar;
            so.FindProperty("addressBar").objectReferenceValue = address;
            so.FindProperty("tree").objectReferenceValue = tree;
            so.FindProperty("tabs").objectReferenceValue = tabs;
            so.FindProperty("grid").objectReferenceValue = grid;
            so.FindProperty("statusBar").objectReferenceValue = status;
            so.FindProperty("taskbar").objectReferenceValue = taskbar;
            so.FindProperty("dialog").objectReferenceValue = dialog;
            so.FindProperty("input").objectReferenceValue = input;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // 이름으로 찾고 없으면 만든다. 배치는 매번 다시 맞춘다.
        private static RectTransform Area(Transform parent, string name, Action<RectTransform> layout,
            Func<Transform, Component> create) {
            var found = parent.Find(name);
            RectTransform rect;
            if (found != null) rect = (RectTransform)found;
            else if (create != null) rect = (RectTransform)create(parent).transform;
            else rect = UiFactory.Rect(name, parent);
            rect.name = name;
            layout(rect);
            return rect;
        }

        private static Transform Canvas(Scene scene) {
            var root = Root(scene, "StageSelectCanvas");
            if (root != null) return root.transform;
            var canvas = UiFactory.CreateCanvas("StageSelectCanvas", 0);
            SceneManager.MoveGameObjectToScene(canvas.gameObject, scene);
            return canvas.transform;
        }

        private static void Camera(Scene scene, UiSkin skin) {
            var go = Root(scene, "Main Camera") ?? Create(scene, "Main Camera");
            go.tag = "MainCamera";
            var camera = Ensure<Camera>(go);
            camera.orthographic = true;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = skin.desktop;
            Ensure<AudioListener>(go);
        }

        private static void EventSystem(Scene scene) {
            var go = Root(scene, "EventSystem") ?? Create(scene, "EventSystem");
            Ensure<EventSystem>(go);
            Ensure<InputSystemUIInputModule>(go);
        }

        // 타이틀 전환(CRT 오버레이)을 이어받아 정리하는 타이틀 쪽 컴포넌트. Assembly-CSharp라 이름으로 붙인다.
        private static void Entry(Scene scene, FlowLog log) {
            var go = Root(scene, "SceneEntry") ?? Create(scene, "SceneEntry");
            foreach (var typeName in EntryTypes) {
                var type = Type.GetType(typeName);
                if (type == null) {
                    log.Warn("타입 없음 " + typeName);
                    continue;
                }

                if (go.GetComponent(type) == null) go.AddComponent(type);
            }
        }

        private static GameObject Root(Scene scene, string name) {
            foreach (var root in scene.GetRootGameObjects())
                if (root.name == name)
                    return root;
            return null;
        }

        private static GameObject Create(Scene scene, string name) {
            var go = new GameObject(name);
            SceneManager.MoveGameObjectToScene(go, scene);
            return go;
        }

        private static T Ensure<T>(GameObject go) where T : Component {
            var component = go.GetComponent<T>();
            return component != null ? component : go.AddComponent<T>();
        }
    }
}
