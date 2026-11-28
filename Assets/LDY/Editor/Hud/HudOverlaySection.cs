using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LDY.Hud.HudEditor {
    // 상단 중앙 토스트, 생각.txt 창.
    internal static class HudOverlaySection {
        public static HudToasts Toasts(HudKit kit, RectTransform canvas) {
            if (kit.Ui(canvas, "Toasts", out var root))
                HudKit.Place(root, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -6f), new Vector2(0f, 16f));

            if (kit.Ui(root, "Toast", out var body)) {
                HudKit.Place(body, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), Vector2.zero, new Vector2(0f, 16f));
                kit.Image(body, "hud_tooltip");
                HudKit.Row(body, new RectOffset(3, 4, 2, 2), 3f, TextAnchor.MiddleLeft);
                HudKit.FitWidth(body);
                body.gameObject.SetActive(false);
            }

            if (kit.Ui(body, "Icon", out var icon)) {
                icon.sizeDelta = new Vector2(12f, 12f);
                kit.Image(icon, "hud_icon_warn");
            }

            if (kit.Ui(body, "Message", out var message)) {
                message.sizeDelta = new Vector2(0f, 12f);
                kit.Text(message, kit.Skin.alreadyHoldingMessage, HudKeyHint.Ink, TextAlignmentOptions.Left);
            }

            var view = HudKit.Component<ToastView>(root.gameObject);
            HudKit.Wire(view, "body", body.gameObject);
            HudKit.Wire(view, "icon", icon.GetComponent<Image>());
            HudKit.Wire(view, "message", message.GetComponent<TMP_Text>());

            var toasts = HudKit.Component<HudToasts>(root.gameObject);
            HudKit.Wire(toasts, "skin", kit.Skin);
            HudKit.Wire(toasts, "view", view);
            return toasts;
        }

        public static ThoughtWindow Thought(HudKit kit, RectTransform canvas) {
            if (kit.Ui(canvas, "Thought", out var root))
                HudKit.Stretch(root, 0f, 0f, 0f, 0f);

            if (kit.Ui(root, "Window", out var window)) {
                HudKit.Place(window, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(135f, 30f), new Vector2(248f, 44f));
                kit.Image(window, "hud_window_frame");
                window.gameObject.SetActive(false);
            }

            if (kit.Ui(window, "Titlebar", out var titlebar)) {
                titlebar.anchorMin = new Vector2(0f, 1f);
                titlebar.anchorMax = Vector2.one;
                titlebar.pivot = new Vector2(0.5f, 1f);
                titlebar.offsetMin = new Vector2(3f, -14f);
                titlebar.offsetMax = new Vector2(-3f, -3f);
                kit.Image(titlebar, "hud_window_titlebar");
            }

            if (kit.Ui(titlebar, "Title", out var title)) {
                HudKit.Stretch(title, 3f, 0f, 3f, 0f);
                kit.Text(title, "생각.txt", Color.white, TextAlignmentOptions.Left);
            }

            if (kit.Ui(window, "Paper", out var paper)) {
                HudKit.Stretch(paper, 3f, 3f, 3f, 15f);
                kit.Image(paper, "hud_window_paper");
            }

            if (kit.Ui(paper, "Body", out var body)) {
                HudKit.Stretch(body, 4f, 0f, 4f, 0f);
                var text = kit.Text(body, "", HudKeyHint.Ink, TextAlignmentOptions.Left);
                text.font = kit.Skin.thoughtFont;
                text.fontSize = kit.Skin.thoughtFontSize;
            }

            var thought = HudKit.Component<ThoughtWindow>(root.gameObject);
            HudKit.Wire(thought, "window", window.gameObject);
            HudKit.Wire(thought, "body", body.GetComponent<TMP_Text>());
            return thought;
        }
    }
}
