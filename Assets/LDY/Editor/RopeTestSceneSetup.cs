using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace LDY.Script.Editor {
    public static class RopeTestSceneSetup {
        private const string ScenePath = "Assets/LDY/Scenes/SampleScene.unity";
        private const string SettingsPath = "Assets/LDY/Data/RopeSettings.asset";
        private const string RootName = "RopeTest";
        private const string GroundLayerName = "Ground";
        private const string ReferencePlatformName = "Square (1)";
        private const float RootX = 21f;
        private const float AnchorHeight = 2.5f;
        private const float PlatformY = -4.5776f;
        private const float PlatformOffsetX = 7f;
        private static readonly Vector2 PlatformScale = new(8f, 0.7576084f);
        private const float GrabRange = 3.5f;

        [MenuItem("LDY/Rope Test/Setup SampleScene")]
        public static void Setup() {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            if (GameObject.Find(RootName) != null) {
                Debug.LogWarning($"'{RootName}'가 이미 씬에 있습니다. 삭제 후 다시 실행하세요.");
                return;
            }

            var groundLayer = LayerMask.NameToLayer(GroundLayerName);
            if (groundLayer < 0) {
                Debug.LogError($"'{GroundLayerName}' 레이어가 없어 중단합니다.");
                return;
            }

            var settings = EnsureSettings();
            var root = CreateRope(settings);
            CreatePlatform(root.transform, "RopeTestPlatform_L", -PlatformOffsetX, groundLayer);
            CreatePlatform(root.transform, "RopeTestPlatform_R", PlatformOffsetX, groundLayer);

            ReportPlayer();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Selection.activeGameObject = root;
        }

        private static RopeSettings EnsureSettings() {
            var existing = AssetDatabase.LoadAssetAtPath<RopeSettings>(SettingsPath);
            if (existing != null)
                return existing;

            var asset = ScriptableObject.CreateInstance<RopeSettings>();
            AssetDatabase.CreateAsset(asset, SettingsPath);
            AssetDatabase.SaveAssets();
            return asset;
        }

        private static GameObject CreateRope(RopeSettings settings) {
            var root = new GameObject(RootName);
            root.transform.position = new Vector3(RootX, 0f, 0f);

            var anchor = new GameObject("Anchor");
            anchor.transform.SetParent(root.transform, false);
            anchor.transform.localPosition = new Vector3(0f, AnchorHeight, 0f);

            var line = root.AddComponent<LineRenderer>();
            line.sharedMaterial = AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");
            line.startWidth = 0.12f;
            line.endWidth = 0.12f;
            line.startColor = new Color(0.55f, 0.38f, 0.2f);
            line.endColor = new Color(0.55f, 0.38f, 0.2f);
            line.sortingOrder = 5;
            line.positionCount = 2;
            line.useWorldSpace = true;

            root.AddComponent<RopeLineRenderer>();
            var controller = root.AddComponent<RopeHangController>();

            var serialized = new SerializedObject(controller);
            serialized.FindProperty("anchor").objectReferenceValue = anchor.transform;
            serialized.FindProperty("settings").objectReferenceValue = settings;
            serialized.FindProperty("grabRange").floatValue = GrabRange;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return root;
        }

        private static void CreatePlatform(Transform parent, string objectName, float localX, int layer) {
            var go = new GameObject(objectName) { layer = layer };
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(localX, PlatformY, 0f);
            go.transform.localScale = new Vector3(PlatformScale.x, PlatformScale.y, 1f);

            var renderer = go.AddComponent<SpriteRenderer>();
            var reference = GameObject.Find(ReferencePlatformName);
            var referenceRenderer = reference != null ? reference.GetComponent<SpriteRenderer>() : null;
            renderer.sprite = referenceRenderer != null
                ? referenceRenderer.sprite
                : AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            if (referenceRenderer != null)
                renderer.sharedMaterial = referenceRenderer.sharedMaterial;

            go.AddComponent<BoxCollider2D>().size = Vector2.one;
        }

        private static void ReportPlayer() {
            try {
                var player = GameObject.FindWithTag("Player");
                if (player == null)
                    Debug.LogWarning("씬에 Player 태그 오브젝트가 없습니다. Assets/prefab/Player.prefab을 씬에 배치하세요.");
            }
            catch (UnityException) {
                Debug.LogWarning("Player 태그를 찾을 수 없습니다.");
            }
        }
    }
}
