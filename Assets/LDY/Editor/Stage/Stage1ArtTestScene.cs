using System.Collections.Generic;
using System.Linq;
using LDY.PlayerMerge;
using LDY.Stage;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace LDY.StageEditor {
    internal sealed class Stage1ArtTestSceneBuilder {
        private const string ScenePath = StagePaths.Scenes + "/Stage1_ArtTest.unity";
        private const string PlayerPath = PlayerPrefabPaths.Shared;
        private const float GimmickSpacing = 3f;
        private const int GimmickFloorY = 10;

        public bool Build(SetupReport report) {
            var grid = AssetDatabase.LoadAssetAtPath<GameObject>(StagePaths.Prefab(Stage1GridBuilder.GridName));
            if (grid == null) {
                report.Warn("Stage1_Grid가 없어 ArtTest 씬을 만들지 못했다. Setup Art를 먼저 실행");
                return false;
            }

            StagePaths.EnsureFolder(StagePaths.Scenes);
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            try {
                AddCamera(scene);
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(grid, scene);
                Paint(instance);
                PlaceGimmicks(scene);
                PlacePlayer(scene, report);
                var created = !System.IO.File.Exists(ScenePath);
                EditorSceneManager.SaveScene(scene, ScenePath);
                report.Record(created, "Scene " + ScenePath);
            }
            finally {
                EditorSceneManager.CloseScene(scene, true);
            }

            return true;
        }

        private static void AddCamera(Scene scene) {
            var go = new GameObject("Main Camera") { tag = "MainCamera" };
            var camera = go.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 9f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.12f, 0.12f, 0.16f);
            go.transform.position = new Vector3(12f, 8f, -10f);
            SceneManager.MoveGameObjectToScene(go, scene);
        }

        private static void Paint(GameObject grid) {
            var ground = grid.transform.Find("Ground").GetComponent<Tilemap>();
            var platform = grid.transform.Find("Platform").GetComponent<Tilemap>();
            var hazard = grid.transform.Find("Hazard").GetComponent<Tilemap>();
            var groundTile = LoadTile("Stage1_Ground");
            var platformTile = LoadTile("Stage1_Platform");

            Fill(ground, groundTile, -4, 30, -2, -1);
            Fill(ground, groundTile, -4, -2, 0, 2);
            Fill(ground, groundTile, 8, 9, 0, 0);
            Fill(ground, groundTile, -4, 45, GimmickFloorY - 1, GimmickFloorY - 1);

            Fill(platform, platformTile, 2, 4, 3, 3);
            Fill(platform, platformTile, 7, 7, 3, 3);
            Fill(platform, platformTile, 11, 12, 3, 3);

            hazard.SetTile(new Vector3Int(14, 0, 0), LoadTile("Stage1_spike_up"));
            hazard.SetTile(new Vector3Int(16, 5, 0), LoadTile("Stage1_spike_down"));
            hazard.SetTile(new Vector3Int(18, 1, 0), LoadTile("Stage1_spike_left"));
            hazard.SetTile(new Vector3Int(20, 1, 0), LoadTile("Stage1_spike_right"));
        }

        private static TileBase LoadTile(string name) {
            return AssetDatabase.LoadAssetAtPath<TileBase>(StagePaths.Tile(name));
        }

        private static void Fill(Tilemap map, TileBase tile, int x0, int x1, int y0, int y1) {
            for (var x = x0; x <= x1; x++)
            for (var y = y0; y <= y1; y++)
                map.SetTile(new Vector3Int(x, y, 0), tile);
        }

        private static void PlaceGimmicks(Scene scene) {
            var paths = AssetDatabase.FindAssets("t:Prefab", new[] { StagePaths.Prefabs })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(p => !p.EndsWith("/" + Stage1GridBuilder.GridName + ".prefab"))
                .OrderBy(p => p)
                .ToList();

            var parent = new GameObject("Gimmicks");
            SceneManager.MoveGameObjectToScene(parent, scene);
            for (var i = 0; i < paths.Count; i++) {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(paths[i]);
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
                instance.transform.SetParent(parent.transform, false);
                var column = i % 14;
                var row = i / 14;
                instance.transform.position = new Vector3(column * GimmickSpacing - 2f, GimmickFloorY + 0.5f + row * 6f, 0f);
            }
        }

        private static void PlacePlayer(Scene scene, SetupReport report) {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPath);
            if (prefab == null) {
                report.Warn("Player 프리팹을 찾지 못해 씬에 놓지 못했다: " + PlayerPath);
                return;
            }

            var player = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            player.transform.position = new Vector3(0f, 2f, 0f);
        }
    }
}
