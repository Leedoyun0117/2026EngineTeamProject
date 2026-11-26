using System.IO;
using System.Linq;
using LDY.Flow;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace LDY.FlowEditor {
    // Ready 씬을 하나씩 열어 메인 카메라를 192x108로 찍는다. 씬은 저장하지 않고, 끝나면 원래 열린 씬 구성으로 돌린다.
    internal static class ThumbnailCapture {
        private const int Width = 192;
        private const int Height = 108;
        private const float PixelsPerUnit = 100f;

        // OpenScene이 참조 없는 에셋을 언로드해서 카탈로그가 파괴된다. 씬을 열기 전에 값만 복사해 두고, 끝나면 다시 읽는다.
        private readonly struct Target {
            public Target(StageEntry entry, StageCatalog catalog) {
                Id = entry.id;
                ScenePath = entry.scenePath;
                SubRoot = entry.subRootName;
                KnownRoots = catalog.SubStagesIn(entry.scenePath).Select(e => e.subRootName).ToArray();
            }

            public string Id { get; }
            public string ScenePath { get; }
            public string SubRoot { get; }
            public string[] KnownRoots { get; }
            public bool UsesSubRoot => !string.IsNullOrEmpty(SubRoot);
        }

        public static void Run(FlowLog log) {
            var catalog = LoadCatalog();
            if (catalog == null) {
                log.Warn("카탈로그가 없다. LDY/Flow/Build Flow Assets 먼저 실행");
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var targets = catalog.Entries.Where(e => e.IsReady).Select(e => new Target(e, catalog)).ToList();
            var setup = EditorSceneManager.GetSceneManagerSetup();
            FlowPaths.EnsureFolder(FlowPaths.Thumbnails);
            var captured = targets.Where(t => Capture(t, log)).Select(t => t.Id).ToList();

            if (setup.Length > 0) EditorSceneManager.RestoreSceneManagerSetup(setup);
            AssetDatabase.Refresh();
            catalog = LoadCatalog();
            foreach (var id in captured) Link(catalog, id, log);
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            log.Info($"찍음 {captured.Count}개, 대상 {targets.Count}개");
        }

        private static StageCatalog LoadCatalog() => AssetDatabase.LoadAssetAtPath<StageCatalog>(FlowPaths.Catalog);

        private static bool Capture(Target target, FlowLog log) {
            if (!File.Exists(target.ScenePath)) {
                log.Warn($"{target.Id}: 씬 파일 없음 {target.ScenePath}");
                return false;
            }

            var scene = EditorSceneManager.OpenScene(target.ScenePath, OpenSceneMode.Single);
            if (target.UsesSubRoot && SubStageRoots.Apply(scene, target.SubRoot, target.KnownRoots) == null) {
                log.Warn($"{target.Id}: 루트 {target.SubRoot} 없음");
                return false;
            }

            var camera = FindCamera(scene);
            if (camera == null) {
                log.Warn($"{target.Id}: 카메라 없음");
                return false;
            }

            File.WriteAllBytes(PathOf(target.Id), Render(camera));
            log.Info($"{target.Id}: {target.ScenePath}{(target.UsesSubRoot ? $" [{target.SubRoot}]" : "")} -> {PathOf(target.Id)}");
            return true;
        }

        private static Camera FindCamera(Scene scene) {
            var cameras = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Camera>(true)).ToList();
            return cameras.FirstOrDefault(c => c.CompareTag("MainCamera")) ?? cameras.FirstOrDefault();
        }

        private static byte[] Render(Camera camera) {
            var target = new RenderTexture(Width, Height, 24, RenderTextureFormat.ARGB32) { filterMode = FilterMode.Point };
            var previousTarget = camera.targetTexture;
            var previousActive = RenderTexture.active;
            try {
                var request = new RenderPipeline.StandardRequest { destination = target };
                if (RenderPipeline.SupportsRenderRequest(camera, request)) {
                    RenderPipeline.SubmitRenderRequest(camera, request);
                }
                else {
                    camera.targetTexture = target;
                    camera.Render();
                }

                RenderTexture.active = target;
                var texture = new Texture2D(Width, Height, TextureFormat.RGBA32, false);
                texture.ReadPixels(new Rect(0, 0, Width, Height), 0, 0);
                texture.Apply();
                var png = texture.EncodeToPNG();
                Object.DestroyImmediate(texture);
                return png;
            }
            finally {
                camera.targetTexture = previousTarget;
                RenderTexture.active = previousActive;
                target.Release();
                Object.DestroyImmediate(target);
            }
        }

        private static void Link(StageCatalog catalog, string id, FlowLog log) {
            var path = PathOf(id);
            if (AssetImporter.GetAtPath(path) is TextureImporter importer) {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = PixelsPerUnit;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.mipmapEnabled = false;
                importer.SaveAndReimport();
            }

            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null) {
                log.Warn($"{id}: 스프라이트로 읽지 못함 {path}");
                return;
            }

            catalog.Find(id).thumbnail = sprite;
        }

        private static string PathOf(string id) => $"{FlowPaths.Thumbnails}/{id}.png";
    }
}
