using LDY.PlayerMerge;
using System.Linq;
using LDY.Flow;
using LDY.Flow.Core;
using UnityEditor;
using UnityEngine;

namespace LDY.FlowEditor {
    internal sealed class CreateStageSceneWindow : EditorWindow {
        private string[] _ids;
        private int _idIndex;
        private string _sceneName;
        private string _folder = FlowPaths.StageSceneFolder;
        private GameObject _player;
        private bool _link = true;

        public static void Open() {
            var window = GetWindow<CreateStageSceneWindow>(true, "Create Stage Scene");
            window.minSize = new Vector2(380f, 170f);
        }

        private void OnEnable() {
            _ids = StagePlan.Entries.Select(e => e.Id).ToArray();
            _sceneName = "Stage_" + _ids[_idIndex];
            _player = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPaths.Final);
        }

        private void OnGUI() {
            var previous = _idIndex;
            _idIndex = EditorGUILayout.Popup("Stage id", _idIndex, _ids);
            if (previous != _idIndex) _sceneName = "Stage_" + _ids[_idIndex];
            _sceneName = EditorGUILayout.TextField("씬 이름", _sceneName);
            _folder = EditorGUILayout.TextField("폴더", _folder);
            _player = (GameObject)EditorGUILayout.ObjectField("Player 프리팹", _player, typeof(GameObject), false);
            _link = EditorGUILayout.Toggle("카탈로그에 연결", _link);
            EditorGUILayout.HelpBox("같은 경로에 씬이 있으면 없는 부분만 채운다. 카탈로그 항목에 다른 씬이 이미 연결돼 있으면 바꾸지 않는다.",
                MessageType.None);

            bool clicked;
            using (new EditorGUI.DisabledScope(string.IsNullOrWhiteSpace(_sceneName) || !_folder.StartsWith("Assets/")))
                clicked = GUILayout.Button("만들기");
            if (!clicked) return;

            var request = new StageSceneRequest {
                SceneName = _sceneName.Trim(), StageId = _ids[_idIndex], Folder = _folder.TrimEnd('/'),
                PlayerPrefabPath = _player != null ? AssetDatabase.GetAssetPath(_player) : null, LinkCatalog = _link
            };
            Close();
            EditorApplication.delayCall += () => {
                var log = new FlowLog("[Merge] Create Stage Scene");
                StageSceneBuilder.Build(request, log);
                log.Print();
            };
        }
    }
}
