using LDY.Flow;
using LDY.StageImport.Core;
using UnityEditor;
using UnityEngine;

namespace LDY.StageImportEditor {
    // 템플릿의 PlayerStart·Player·StageGoal을 툴 위치로 옮기고, 골이 더 있으면 같은 구성으로 추가한다.
    internal sealed class StartGoalStep : ISceneStep {
        private const string StartName = "PlayerStart";
        private const string GoalName = "StageGoal";

        public void Apply(SceneContext context) {
            PlaceStart(context);
            PlaceGoals(context);
        }

        private static void PlaceStart(SceneContext context) {
            var starts = context.Plan.Starts;
            if (starts.Count == 0) {
                context.Log.Warn("start 칸이 없어 템플릿 시작 위치를 그대로 둔다");
                return;
            }

            if (starts.Count > 1) context.Log.Warn($"start 칸 {starts.Count}개 중 첫 번째 {starts[0]}만 쓴다");
            var cell = starts[0];
            var marker = context.FindRoot(StartName);
            if (marker != null) marker.transform.position = new Vector3(cell.X + 0.5f, cell.Y, 0f);
            var player = context.FindPlayer();
            if (player != null) PrefabAligner.Bottom(player, cell.X + 0.5f, cell.Y);
            context.Log.Info($"시작 위치 {cell}");
        }

        private static void PlaceGoals(SceneContext context) {
            var goals = context.Plan.Goals;
            var template = context.FindRoot(GoalName);
            if (goals.Count == 0) {
                context.Log.Warn("goal 칸이 없어 템플릿 골을 그대로 둔다");
                return;
            }

            for (var i = 0; i < goals.Count; i++) {
                var goal = i == 0 && template != null ? template : NewGoal(context, i);
                goal.transform.position = SceneContext.CellCenter(goals[i]);
                EnsureVisual(context, goal);
            }

            context.Log.Info($"골 {goals.Count}개");
        }

        private static GameObject NewGoal(SceneContext context, int index) {
            var go = new GameObject($"{GoalName} ({index})");
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(go, context.Scene);
            go.AddComponent<BoxCollider2D>().isTrigger = true;
            go.AddComponent<StageGoal>();
            return go;
        }

        private static void EnsureVisual(SceneContext context, GameObject goal) {
            if (goal.transform.childCount > 0) return;
            var prefab = context.Assets.Prefab(ToolKeys.Goal);
            if (prefab == null) return;
            var visual = (GameObject)PrefabUtility.InstantiatePrefab(prefab, context.Scene);
            visual.transform.SetParent(goal.transform, false);
            visual.transform.localPosition = Vector3.zero;
        }
    }
}
