using JJB.Script.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace LDY.Script.Editor {
    public static class RopeTestSceneSetup {
        private const string ScenePath = "Assets/LDY/Scenes/SampleScene.unity";
        private const string SettingsPath = "Assets/LDY/Data/RopeSettings.asset";
        private const string RootName = "RopeTest";
        private const string GroundLayerName = "Ground";
        private const string InteractLayerName = "Object";
        private const string ReferencePlatformName = "Square (1)";
        private const float RootX = 21f;
        private const float AnchorHeight = 2.5f;
        private const float PlatformY = -4.5776f;
        private const float PlatformOffsetX = 7f;
        private static readonly Vector2 PlatformScale = new(8f, 0.7576084f);

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

        [MenuItem("LDY/Rope Test/Upgrade RopeTest To Interact")]
        public static void Upgrade() {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var root = GameObject.Find(RootName);
            if (root == null) {
                Debug.LogWarning($"'{RootName}'가 씬에 없습니다. 먼저 Setup을 실행하세요.");
                return;
            }

            AttachInteraction(root);
            AssignObjectLayer();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Selection.activeGameObject = root;
        }

        [MenuItem("LDY/Rope Test/Assign Player Object Layer (scene only)")]
        public static void AssignPlayerObjectLayer() {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (AssignObjectLayer())
                EditorSceneManager.SaveScene(scene);
        }

        private static bool AssignObjectLayer() {
            var layer = LayerMask.NameToLayer(InteractLayerName);
            var player = GameObject.FindWithTag("Player");
            var interact = player != null ? player.GetComponent<JJBPlayerInteract>() : null;
            if (layer < 0 || interact == null) {
                Debug.LogWarning("Player 태그의 JJBPlayerInteract 또는 Object 레이어를 찾지 못했습니다.");
                return false;
            }

            var serialized = new SerializedObject(interact);
            var mask = serialized.FindProperty("objectLayer");
            if (mask == null) {
                Debug.LogWarning("JJBPlayerInteract에 objectLayer 필드가 없습니다.");
                return false;
            }

            mask.intValue |= 1 << layer;
            serialized.ApplyModifiedProperties();
            PrefabUtility.RecordPrefabInstancePropertyModifications(interact);
            EditorSceneManager.MarkSceneDirty(interact.gameObject.scene);
            return true;
        }

        private static void AttachInteraction(GameObject root) {
            var layer = LayerMask.NameToLayer(InteractLayerName);
            if (layer < 0)
                Debug.LogWarning($"'{InteractLayerName}' 레이어가 없어 레이어를 바꾸지 못했습니다.");
            else
                root.layer = layer;

            if (root.GetComponent<EdgeCollider2D>() == null)
                root.AddComponent<EdgeCollider2D>().isTrigger = true;
            if (root.GetComponent<RopeColliderFollower>() == null)
                root.AddComponent<RopeColliderFollower>();
            if (root.GetComponent<RopeInteractable>() == null)
                root.AddComponent<RopeInteractable>();
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
            AttachInteraction(root);

            var serialized = new SerializedObject(controller);
            serialized.FindProperty("anchor").objectReferenceValue = anchor.transform;
            serialized.FindProperty("settings").objectReferenceValue = settings;
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
