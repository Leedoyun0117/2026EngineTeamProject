using LDY.StageImport;
using UnityEngine;

namespace LDY.StageImportEditor {
    // notes는 EditorOnly 오브젝트(빌드 제외), memo와 원본 정보는 StageRoot의 StageImportInfo에 둔다.
    internal sealed class NoteStep : ISceneStep {
        private const string RootName = "Imported Notes";
        private const string StageRootName = "StageRoot";
        private const string EditorOnlyTag = "EditorOnly";

        public void Apply(SceneContext context) {
            StoreInfo(context);
            if (context.Plan.Notes.Count == 0) return;

            var root = context.EnsureRoot(RootName);
            root.tag = EditorOnlyTag;
            foreach (var note in context.Plan.Notes) {
                var go = new GameObject("Note " + note.Cell);
                go.tag = EditorOnlyTag;
                go.transform.SetParent(root, false);
                go.transform.position = SceneContext.CellCenter(note.Cell);
                go.AddComponent<StageNote>().SetText(note.Text);
            }

            context.Log.Info($"메모 칸 {context.Plan.Notes.Count}개 (EditorOnly)");
        }

        private static void StoreInfo(SceneContext context) {
            var root = context.FindRoot(StageRootName);
            if (root == null) {
                context.Log.Warn("StageRoot가 없어 memo를 저장하지 못했다");
                return;
            }

            var source = context.Plan.Source;
            var info = root.GetComponent<StageImportInfo>();
            if (info == null) info = root.AddComponent<StageImportInfo>();
            info.Fill(source.Id, source.Name, source.Status, source.Dark, source.Memo);
            if (source.Dark) context.Log.Warn("맵 전체 dark=true: 전체 어둠 처리는 손으로 해야 한다");
        }
    }
}
