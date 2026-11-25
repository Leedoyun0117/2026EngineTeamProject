using System.Collections.Generic;
using System.Linq;
using System.Text;
using LDY.Flow;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace LDY.FlowEditor {
    // 열린 씬(DontDestroyOnLoad 포함)의 모든 TMP 글자를 스킨 기준으로 점검해 표로 출력한다.
    // 탐색기 글자 대부분은 런타임에 만들어지므로 Play 중(스테이지 선택 화면)에 실행해야 전부 잡힌다.
    internal static class TextAudit {
        private const float Epsilon = 0.01f;

        [MenuItem("LDY/StageSelect/Log Text Audit")]
        private static void Run() {
            var skin = AssetDatabase.LoadAssetAtPath<UiSkin>(FlowPaths.Skin);
            var texts = Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Where(t => t.gameObject.scene.IsValid())
                .Select(t => (text: t, path: PathOf(t.transform)))
                .OrderBy(e => e.path)
                .ToList();

            var sb = new StringBuilder("[StageSelect] Text Audit");
            sb.Append(EditorApplication.isPlaying ? " (Play)" : " (Edit: 런타임 글자는 Play 중에 다시 실행)");
            sb.Append("\n  기준: ").Append(DescribeSkin(skin));
            sb.Append("\n  결과 | 폰트 | 크기 | 자동크기 | 여분패딩 | 스냅 | x,y,w,h (캔버스 px) | 정수 | 경로");
            var bad = 0;
            foreach (var (text, path) in texts) {
                var issues = Issues(text, skin, out var rect, out var integral);
                if (issues.Count > 0) bad++;
                sb.Append("\n  ").Append(issues.Count == 0 ? "OK" : "NG")
                    .Append(" | ").Append(text.font != null ? text.font.name : "없음")
                    .Append(" | ").Append(text.fontSize.ToString("0.##"))
                    .Append(" | ").Append(text.enableAutoSizing ? "켬" : "끔")
                    .Append(" | ").Append(text.extraPadding ? "켬" : "끔")
                    .Append(" | ").Append(text.GetComponent<PixelSnapText>() != null ? "O" : "X")
                    .Append(" | ").Append(rect)
                    .Append(" | ").Append(integral ? "O" : "X")
                    .Append(" | ").Append(path);
                if (issues.Count > 0) sb.Append("  <- ").Append(string.Join(", ", issues));
            }

            sb.Append($"\n  합계 {texts.Count}개, NG {bad}개");
            if (bad > 0) Debug.LogWarning(sb.ToString());
            else Debug.Log(sb.ToString());
        }

        private static List<string> Issues(TMP_Text text, UiSkin skin, out string rect, out bool integral) {
            var issues = new List<string>();
            if (skin != null && text.font != skin.font) issues.Add("폰트");
            if (skin != null && !Mathf.Approximately(text.fontSize, skin.fontSize)) issues.Add("크기");
            if (text.enableAutoSizing) issues.Add("자동크기");
            if (text.extraPadding) issues.Add("여분패딩");
            if (text.GetComponent<PixelSnapText>() == null) issues.Add("스냅 없음");

            var r = CanvasRect(text.rectTransform);
            rect = r.HasValue ? $"{r.Value.x:0.##},{r.Value.y:0.##},{r.Value.width:0.##},{r.Value.height:0.##}" : "캔버스 없음";
            integral = r.HasValue && IsInt(r.Value.xMin) && IsInt(r.Value.yMin) && IsInt(r.Value.xMax) &&
                       IsInt(r.Value.yMax);
            if (!integral) issues.Add("정수 아님");
            return issues;
        }

        // 최상위 캔버스 기준, 좌상단 원점 좌표(캔버스 px = 기준 해상도 640x360 단위)
        private static Rect? CanvasRect(RectTransform rect) {
            var canvas = RootCanvas(rect);
            if (canvas == null) return null;
            var root = (RectTransform)canvas.transform;
            var corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            var min = root.InverseTransformPoint(corners[0]);
            var max = root.InverseTransformPoint(corners[2]);
            var bounds = root.rect;
            return Rect.MinMaxRect(min.x - bounds.xMin, bounds.yMax - max.y, max.x - bounds.xMin, bounds.yMax - min.y);
        }

        private static Canvas RootCanvas(Transform transform) {
            Canvas found = null;
            for (var t = transform; t != null; t = t.parent)
                if (t.TryGetComponent<Canvas>(out var canvas))
                    found = canvas;
            return found;
        }

        private static bool IsInt(float value) => Mathf.Abs(value - Mathf.Round(value)) < Epsilon;

        private static string DescribeSkin(UiSkin skin) {
            if (skin == null) return "UiSkin 없음 " + FlowPaths.Skin;
            var font = skin.font;
            if (font == null) return $"폰트 없음 / {skin.fontSize}";
            var atlas = font.atlasTextures != null && font.atlasTextures.Length > 0 ? font.atlasTextures[0] : null;
            return $"{font.name} / {skin.fontSize} (샘플링 {font.faceInfo.pointSize}, {font.atlasRenderMode}, " +
                   $"아틀라스 {(atlas != null ? atlas.filterMode.ToString() : "없음")})";
        }

        private static string PathOf(Transform transform) {
            var names = new List<string>();
            for (var t = transform; t != null; t = t.parent) names.Add(t.name);
            names.Reverse();
            return string.Join("/", names);
        }
    }
}
