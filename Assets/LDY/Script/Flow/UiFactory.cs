using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LDY.Flow {
    // 좌상단 기준 픽셀 좌표로 uGUI 요소를 만든다. 캔버스 기준 해상도는 640x360.
    public sealed class UiFactory {
        public static readonly Vector2 ReferenceResolution = new Vector2(640f, 360f);

        private readonly UiSkin _skin;

        public UiFactory(UiSkin skin) {
            _skin = skin;
        }

        public UiSkin Skin => _skin;

        public static Canvas CreateCanvas(string name, int sortingOrder, Transform parent = null) {
            var go = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler),
                typeof(GraphicRaycaster));
            if (parent != null) go.transform.SetParent(parent, false);
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;
            canvas.pixelPerfect = true;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            scaler.referencePixelsPerUnit = 100f;
            return canvas;
        }

        public static RectTransform Rect(string name, Transform parent) {
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = LayerMask.NameToLayer("UI");
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            return rect;
        }

        public static RectTransform Stretch(RectTransform rect, float left = 0f, float top = 0f, float right = 0f,
            float bottom = 0f) {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(-right, -top);
            return rect;
        }

        public static RectTransform Place(RectTransform rect, float x, float y, float width, float height) {
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(x, -y);
            rect.sizeDelta = new Vector2(width, height);
            return rect;
        }

        public static RectTransform TopStrip(RectTransform rect, float top, float height, float left = 0f,
            float right = 0f) {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 1f);
            rect.offsetMin = new Vector2(left, -top - height);
            rect.offsetMax = new Vector2(-right, -top);
            return rect;
        }

        public static RectTransform LeftColumn(RectTransform rect, float left, float width, float top = 0f,
            float bottom = 0f) {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 0.5f);
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(left + width, -top);
            return rect;
        }

        public static RectTransform BottomStrip(RectTransform rect, float bottom, float height, float left = 0f,
            float right = 0f) {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(-right, bottom + height);
            return rect;
        }

        public Image NewImage(string name, Transform parent, Sprite sprite, Color? fallback = null) {
            var rect = Rect(name, parent);
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.color = sprite != null ? Color.white : fallback ?? _skin.face;
            image.raycastTarget = false;
            if (sprite != null && sprite.border != Vector4.zero) image.type = UnityEngine.UI.Image.Type.Sliced;
            image.pixelsPerUnitMultiplier = 1f;
            return image;
        }

        public Image Panel(string name, Transform parent) => NewImage(name, parent, _skin.panelRaised, _skin.face);

        public Image Sunken(string name, Transform parent) => NewImage(name, parent, _skin.fieldSunken, _skin.paper);

        public Image Solid(string name, Transform parent, Color color) {
            var image = NewImage(name, parent, null, color);
            image.color = color;
            return image;
        }

        // 세로 정렬은 줄 기준 가운데(Left/Center/Right)를 쓴다. 글자 모양 기준(Midline)은 문자열마다 기준선이 달라진다.
        public TextMeshProUGUI NewText(string name, Transform parent, string text, Color? color = null,
            TextAlignmentOptions alignment = TextAlignmentOptions.Left) {
            var rect = Rect(name, parent);
            var label = rect.gameObject.AddComponent<TextMeshProUGUI>();
            Style(label);
            label.color = color ?? _skin.text;
            label.alignment = alignment;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.overflowMode = TextOverflowModes.Ellipsis;
            label.raycastTarget = false;
            label.text = text;
            return label;
        }

        // 폰트·크기는 스킨 값 하나만 쓴다. 픽셀 폰트가 번지지 않게 자동 크기·여분 패딩을 끄고 글리프를 정수 좌표에 맞춘다.
        public void Style(TMP_Text label) {
            if (_skin.font != null) label.font = _skin.font;
            label.fontSize = _skin.fontSize;
            label.enableAutoSizing = false;
            label.extraPadding = false;
            label.isOrthographic = true;
            label.margin = Vector4.zero;
            if (label.GetComponent<PixelSnapText>() == null) label.gameObject.AddComponent<PixelSnapText>();
        }

        // 창 안쪽 위의 제목표시줄(바탕 + 제목). 닫기 버튼 자리만큼 제목 오른쪽을 비운다.
        public Image WindowTitleBar(Transform window, string title, out TextMeshProUGUI label) {
            var bar = NewImage("Titlebar", window, _skin.titlebar, _skin.selection);
            TopStrip(bar.rectTransform, WindowChrome.TitleBarInset, WindowChrome.TitleBarHeight,
                WindowChrome.TitleBarInset, WindowChrome.TitleBarInset);
            label = NewText("Title", bar.transform, title, _skin.titleText);
            Stretch(label.rectTransform, WindowChrome.TitleLeft, 0f, WindowChrome.TitleRightInset(false), 0f);
            return bar;
        }

        // 표시만 하는 제목표시줄 버튼(누를 수 없음)
        public Image TitleButtonImage(Transform bar, WindowChrome.TitleButton kind) {
            var image = NewImage(kind.ToString(), bar, TitleSprite(kind), _skin.face);
            WindowChrome.PlaceButton(image.rectTransform, kind);
            return image;
        }

        public Button TitleButton(Transform bar, WindowChrome.TitleButton kind) {
            var image = TitleButtonImage(bar, kind);
            image.raycastTarget = true;
            var button = image.gameObject.AddComponent<Button>();
            button.transition = Selectable.Transition.None;
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            return button;
        }

        private Sprite TitleSprite(WindowChrome.TitleButton kind) {
            switch (kind) {
                case WindowChrome.TitleButton.Minimize: return _skin.titleMin;
                case WindowChrome.TitleButton.Maximize: return _skin.titleMax;
                default: return _skin.titleClose;
            }
        }

        public Button NewButton(string name, Transform parent, string caption, Sprite icon = null) {
            var image = NewImage(name, parent, _skin.button, _skin.face);
            image.raycastTarget = true;
            var button = image.gameObject.AddComponent<Button>();
            button.transition = Selectable.Transition.SpriteSwap;
            button.spriteState = new SpriteState { pressedSprite = _skin.buttonPressed };
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            if (icon != null) Place(NewImage("Icon", image.transform, icon).rectTransform, 3f, 3f, 12f, 12f);
            if (!string.IsNullOrEmpty(caption))
                Stretch(NewText("Caption", image.transform, caption, null, TextAlignmentOptions.Center).rectTransform,
                    icon != null ? 14f : 2f, 0f, 2f, 0f);
            return button;
        }
    }
}
