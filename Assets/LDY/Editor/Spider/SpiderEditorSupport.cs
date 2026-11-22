using System;
using System.IO;
using System.Linq;
using LDY.PlayerMerge;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LDY.SpiderEditor {
    internal static class SpiderPaths {
        public const string Prefabs = "Assets/LDY/Prefabs/Stage2";
        public const string Data = "Assets/LDY/Data/Spider";
        public const string Scenes = "Assets/LDY/Scenes";
        public const string CircleSprite = "Assets/LDY/Sprites/Circle.png";
        public const string PlayerPrefab = PlayerPrefabPaths.Shared;

        public static void EnsureFolder(string path) {
            if (AssetDatabase.IsValidFolder(path))
                return;

            var parent = Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }

        public static T LoadOrCreate<T>(string path) where T : ScriptableObject {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
                return asset;

            EnsureFolder(Path.GetDirectoryName(path).Replace('\\', '/'));
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        public static GameObject SavePrefab(GameObject root, string path) {
            EnsureFolder(Path.GetDirectoryName(path).Replace('\\', '/'));
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return prefab;
        }
    }

    internal static class Wire {
        public static void Ref(Object target, string field, Object value) {
            var so = new SerializedObject(target);
            so.FindProperty(field).objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void Refs(Object target, string field, Object[] values) {
            var so = new SerializedObject(target);
            var property = so.FindProperty(field);
            property.arraySize = values.Length;
            for (var i = 0; i < values.Length; i++)
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void Float(Object target, string field, float value) {
            var so = new SerializedObject(target);
            so.FindProperty(field).floatValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void Mask(Object target, string field, int mask) {
            var so = new SerializedObject(target);
            so.FindProperty(field).FindPropertyRelative("m_Bits").intValue = mask;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    // 팀원 코드(HES KillHitbox, HTY DragObj)는 어셈블리가 달라 이름으로 찾아 붙인다. 팀원 코드는 건드리지 않는다.
    internal static class ExternalComponents {
        public const string DeathPresenter = "LDY.Script.PlayerDeathPresenter";
        public const string DragObj = "DragObj";

        public static bool HasComponent(GameObject target, string typeName) {
            var type = FindType(typeName);
            return type != null && target.GetComponent(type) != null;
        }

        private static Type FindType(string typeName) {
            return AppDomain.CurrentDomain.GetAssemblies()
                .Select(assembly => assembly.GetType(typeName)).FirstOrDefault(t => t != null);
        }

        public static bool TryAdd(GameObject target, string typeName) {
            var type = FindType(typeName);
            if (type == null) {
                Debug.LogWarning($"[Stage2] {typeName} 타입을 찾지 못해 {target.name}에 붙이지 못했다");
                return false;
            }

            target.AddComponent(type);
            return true;
        }
    }

    internal static class SpriteObjects {
        public static SpriteRenderer Add(Transform parent, string name, Sprite sprite, int order) {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = order;
            return renderer;
        }

        public static Vector2 PixelsToLocal(Vector2 pixel, Sprite sprite) {
            var pixels = sprite.rect.size;
            return new Vector2(pixel.x - pixels.x * 0.5f, pixels.y * 0.5f - pixel.y) / sprite.pixelsPerUnit;
        }
    }
}
