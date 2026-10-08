using JJB.Script;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LDY.Script.Editor
{
    public static class AltTestSceneSetup
    {
        private const string PlayerName = "AltTestPlayer";
        private const string GroundLayerName = "Ground";
        private const string InputAssetPath = "Assets/JJB/JJBInput.inputactions";

        [MenuItem("LDY/Alt Test/Setup Test Scene")]
        public static void Setup()
        {
            if (GameObject.Find(PlayerName) != null)
            {
                Selection.activeGameObject = GameObject.Find(PlayerName);
                Debug.LogWarning($"'{PlayerName}'가 이미 씬에 있습니다. 삭제 후 다시 실행하세요.");
                return;
            }

            int groundLayer = EnsureLayer(GroundLayerName);
            if (groundLayer < 0)
            {
                Debug.LogError("Ground 레이어를 만들 수 없습니다 (빈 레이어 슬롯 없음).");
                return;
            }

            Sprite circle = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            Sprite box = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

            Camera cam = EnsureCamera();
            CreateGround("Ground", box, new Vector2(0f, -4f), new Vector2(30f, 1f), groundLayer);
            CreateGround("Platform", box, new Vector2(5f, -1f), new Vector2(5f, 0.6f), groundLayer);
            CreateGround("Wall", box, new Vector2(-8f, -1.5f), new Vector2(1f, 5f), groundLayer);

            GameObject player = CreatePlayer(circle, groundLayer);
            cam.transform.position = new Vector3(0f, 0f, -10f);

            EditorSceneManager.MarkSceneDirty(player.scene);
            Selection.activeGameObject = player;
            Debug.Log("Alt 테스트 세팅 완료. Play 후 A/D, Space, Alt(홀드), L(잠금 토글)로 확인하세요.");
        }

        private static GameObject CreatePlayer(Sprite sprite, int groundLayer)
        {
            var go = new GameObject(PlayerName);
            Undo.RegisterCreatedObjectUndo(go, "Create Alt Test Player");
            go.transform.position = new Vector3(0f, -2f, 0f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 5;

            var col = go.AddComponent<BoxCollider2D>();
            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 3f;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var groundCheck = new GameObject("GroundCheck").transform;
            groundCheck.SetParent(go.transform, false);
            groundCheck.localPosition = new Vector3(col.offset.x, col.offset.y - col.size.y * 0.5f - 0.02f, 0f);

            var playerInput = go.AddComponent<PlayerInput>();
            var inputSo = new SerializedObject(playerInput);
            inputSo.FindProperty("m_Actions").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputAssetPath);
            inputSo.FindProperty("m_DefaultActionMap").stringValue = "Player";
            inputSo.ApplyModifiedPropertiesWithoutUndo();

            go.AddComponent<JJBPlayerInput>();
            var movement = go.AddComponent<JJBPlayerMovement>();
            var jump = go.AddComponent<JJBPlayerJump>();
            go.AddComponent<JJBPlayer>();

            SetFloat(movement, "moveSpeed", 7f);
            var jumpSo = new SerializedObject(jump);
            jumpSo.FindProperty("groundCheck").objectReferenceValue = groundCheck;
            jumpSo.FindProperty("groundCheckSize").vector2Value = new Vector2(col.size.x * 0.9f, 0.1f);
            jumpSo.FindProperty("groundLayer").intValue = 1 << groundLayer;
            jumpSo.ApplyModifiedPropertiesWithoutUndo();

            go.AddComponent<PlayerInputLock>();
            go.AddComponent<PlayerAbilityGate>();
            go.AddComponent<JJBControlGate>();
            go.AddComponent<AltModeController>();
            go.AddComponent<AltLockTester>();
            return go;
        }

        private static void CreateGround(string name, Sprite sprite, Vector2 pos, Vector2 scale, int layer)
        {
            var go = new GameObject(name) { layer = layer };
            Undo.RegisterCreatedObjectUndo(go, "Create Alt Test Ground");
            go.transform.position = pos;
            go.transform.localScale = new Vector3(scale.x, scale.y, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = new Color(0.35f, 0.35f, 0.4f);
            go.AddComponent<BoxCollider2D>();
        }

        private static Camera EnsureCamera()
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                var go = new GameObject("Main Camera") { tag = "MainCamera" };
                Undo.RegisterCreatedObjectUndo(go, "Create Camera");
                cam = go.AddComponent<Camera>();
                go.AddComponent<AudioListener>();
            }

            cam.orthographic = true;
            cam.orthographicSize = 6f;
            return cam;
        }

        private static void SetFloat(Object target, string field, float value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(field).floatValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static int EnsureLayer(string layerName)
        {
            int existing = LayerMask.NameToLayer(layerName);
            if (existing >= 0)
                return existing;

            var tagManager = new SerializedObject(
                AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty layers = tagManager.FindProperty("layers");
            for (int i = 8; i < layers.arraySize; i++)
            {
                SerializedProperty slot = layers.GetArrayElementAtIndex(i);
                if (!string.IsNullOrEmpty(slot.stringValue))
                    continue;

                slot.stringValue = layerName;
                tagManager.ApplyModifiedPropertiesWithoutUndo();
                return i;
            }

            return -1;
        }
    }
}
