using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace LDY.Script
{
    // 영역 밖 줄은 마스크 대신 알파로 서서히 투명하게 처리
    public class CreditPanel : WindowPanel
    {
        private const float ViewHalfWidth = 3.7f;
        private const float FadeDistance = 0.45f;
        private const float WheelStep = 0.8f;
        private const float KeyScrollSpeed = 3f;
        private const float ManualPause = 2f;

        private const float TopPadding = 0.2f;
        private const float RoleHeight = 0.6f;
        private const float NameHeight = 0.5f;
        private const float EntryGap = 0.5f;
        private const float RoleFont = 4.5f;
        private const float NameFont = 4.5f;

        private static readonly Color RoleColor = new Color(0.1f, 0.2f, 0.5f);

        [SerializeField] private CreditData data;
        [Tooltip("자동 스크롤 속도 (유닛/초). 0이면 자동으로 올라가지 않는다")]
        [SerializeField, Min(0f)] private float scrollSpeed = 0.6f;
        [Tooltip("창이 열린 뒤 자동 스크롤이 시작되기까지의 시간(초)")]
        [SerializeField, Min(0f)] private float startDelay = 0.5f;

        private readonly List<Line> _lines = new List<Line>();
        private float _viewTop;
        private float _viewBottom;
        private float _contentHeight;
        private float _offset;
        private float _hold;

        private struct Line
        {
            public TMP_Text Text;
            public float Top;
            public float Height;
        }

        protected override string TitleText => WindowText.CreditTitle;
        protected override string ControlHint => WindowText.CreditControlHint;
        protected override WindowLayout Layout => WindowLayout.Credit;
        // 자동 스크롤 창이라 행 이동 없이 물리 점프 + S 내려가기
        protected override bool UsesRowMovement => false;

        private float ViewHeight => _viewTop - _viewBottom;
        // 마지막 줄까지 올라온 위치(목록이 짧으면 0)
        private float EndOffset => Mathf.Max(0f, _contentHeight - ViewHeight);

        protected override void BuildBody(WindowKit kit, Transform root, List<WindowWidget> widgets)
        {
            _viewTop = Layout.CloseTop - 0.1f;
            _viewBottom = Layout.FooterTop + 0.7f;

            if (data == null)
            {
                Debug.LogWarning("[Credit] CreditData 연결 안 됨");
                return;
            }

            float y = TopPadding;
            foreach (CreditEntry entry in data.entries)
            {
                AddLine(kit, root, entry.role, RoleFont, RoleColor, y, RoleHeight);
                y += RoleHeight;
                foreach (string person in entry.names)
                {
                    AddLine(kit, root, person, NameFont, WindowStyle.Text, y, NameHeight);
                    y += NameHeight;
                }

                y += EntryGap;
            }

            _contentHeight = y;
            if (_lines.Count == 0)
                Debug.LogWarning("[Credit] 표시할 항목 없음");
        }

        // 맨 위 줄이 영역 아래 끝에 닿은 위치에서 시작(짧은 목록도 같음)
        protected override void OnWindowOpened()
        {
            _offset = -ViewHeight;
            _hold = startDelay;
            LayoutLines();
        }

        // 자동 스크롤은 Time.deltaTime이라 Alt(timeScale 0) 동안 멈추고 떼면 이어짐
        // 휠, W/S는 Alt 중에도 동작. 끝에 닿으면 멈추고 반복 없음
        protected override void OnWindowTick(float dt)
        {
            IWindowInput input = Services.Input;
            float manual = input.Scroll * WheelStep + input.VerticalHeld * KeyScrollSpeed * dt;
            float before = _offset;

            if (manual != 0f)
            {
                // 휠 위/W는 목록이 아래로 내려옴
                _offset = Mathf.Clamp(_offset - manual, -ViewHeight, EndOffset);
                _hold = ManualPause;
            }
            else if (_hold > 0f)
            {
                _hold -= Mathf.Min(Time.deltaTime, TitleTuning.MaxDeltaTime);
            }
            else
            {
                _offset = Mathf.Min(_offset + scrollSpeed * Mathf.Min(Time.deltaTime, TitleTuning.MaxDeltaTime), EndOffset);
            }

            if (!Mathf.Approximately(before, _offset))
                LayoutLines();
        }

        private void AddLine(WindowKit kit, Transform root, string text, float fontSize, Color color, float top,
            float height)
        {
            TextMeshPro tmp = kit.Text("Line", root, Vector2.zero, text, fontSize, color, 12,
                TextAlignmentOptions.Center, ViewHalfWidth * 2f);
            _lines.Add(new Line { Text = tmp, Top = top, Height = height });
        }

        private void LayoutLines()
        {
            foreach (Line line in _lines)
            {
                float centerY = _viewTop - line.Top - line.Height * 0.5f + _offset;
                float inside = Mathf.Min(centerY - line.Height * 0.5f - _viewBottom,
                    _viewTop - (centerY + line.Height * 0.5f));
                float fade = Mathf.Clamp01(inside / FadeDistance);
                line.Text.transform.localPosition = new Vector3(0f, centerY, 0f);
                line.Text.enabled = fade > 0f;
                line.Text.alpha = fade;
            }
        }
    }
}
