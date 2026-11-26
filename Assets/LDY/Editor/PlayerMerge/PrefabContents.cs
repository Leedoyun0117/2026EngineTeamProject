using System;
using UnityEditor;
using UnityEngine;

namespace LDY.Script {
    // 프리팹을 격리된 장면에 연다. Save를 부르지 않으면 원본 파일은 바뀌지 않는다.
    internal sealed class PrefabContents : IDisposable {
        private readonly string _path;

        private PrefabContents(string path, GameObject root) {
            _path = path;
            Root = root;
        }

        public GameObject Root { get; }

        public static PrefabContents Open(string path, MergeLog log) {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null) {
                log.Notes.Add($"중단: {path} 프리팹이 없음");
                return null;
            }

            return new PrefabContents(path, PrefabUtility.LoadPrefabContents(path));
        }

        public void Save() => PrefabUtility.SaveAsPrefabAsset(Root, _path);

        public void Dispose() => PrefabUtility.UnloadPrefabContents(Root);
    }
}
