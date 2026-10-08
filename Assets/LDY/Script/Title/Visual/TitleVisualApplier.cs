using TMPro;
using UnityEngine;

namespace LDY.Script
{
    // TitleVisualSettings의 값을 씬 오브젝트에 반영한다. 화면 대비 비율은 콘텐츠 카메라가 보는 영역 기준이다.
    [RequireComponent(typeof(TitleVisualSettings))]
    public class TitleVisualApplier : MonoBehaviour
    {
        [SerializeField] private Camera contentCamera;
        [SerializeField] private CrtScreen crtScreen;
        [SerializeField] private SpriteRenderer wallpaper;
        [SerializeField] private TMP_Text logoLabel;
        [SerializeField] private SpriteRenderer logoSprite;
        [SerializeField] private SpriteRenderer underline;
        [SerializeField] private GameObject player;

        private void Awake()
        {
            Apply(true);
        }

#if UNITY_EDITOR
        // 인스펙터에서 값을 바꾸면 바로 반영한다. 플레이 중에는 플레이어 위치를 건드리지 않는다.
        private void OnValidate()
        {
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (this != null)
                    Apply(!Application.isPlaying);
            };
        }
#endif

        [ContextMenu("Apply")]
        private void ApplyFromMenu()
        {
            Apply(true);
        }

        public void Apply(bool placePlayer)
        {
            TitleVisualSettings settings = GetComponent<TitleVisualSettings>();
            float height = contentCamera.orthographicSize * 2f;
            var size = new Vector2(height * crtScreen.Aspect, height);
            Vector2 origin = (Vector2)contentCamera.transform.position - size * 0.5f;

            Vector3 At(Vector2 ratio) => new Vector3(origin.x + ratio.x * size.x, origin.y + ratio.y * size.y, 0f);

            ApplyWallpaper(settings, size, origin + size * 0.5f);
            ApplyLogo(settings, size, At);
            ApplyIcons(settings, size, At);
            ApplyPlayer(settings, At, placePlayer);
        }

        private void ApplyWallpaper(TitleVisualSettings settings, Vector2 size, Vector2 center)
        {
            wallpaper.sprite = settings.wallpaperSprite;
            wallpaper.color = settings.wallpaperTint;
            wallpaper.transform.position = new Vector3(center.x, center.y, 0f);
            SpriteFit.Stretch(wallpaper, size);
        }

        private void ApplyLogo(TitleVisualSettings settings, Vector2 size, System.Func<Vector2, Vector3> at)
        {
            bool text = settings.logoMode == TitleLogoMode.Text;
            logoLabel.gameObject.SetActive(text);
            logoSprite.gameObject.SetActive(!text);

            Vector3 position = at(settings.logoAnchor);
            logoLabel.transform.position = position;
            logoSprite.transform.position = position;

            logoLabel.text = settings.logoText;
            logoLabel.fontSize = settings.logoFontSize;
            logoLabel.color = settings.logoColor;
            logoSprite.sprite = settings.logoSprite;
            SpriteFit.Contain(logoSprite, new Vector2(size.x * settings.logoSpriteWidthRatio, size.y * 0.3f));

            underline.color = settings.underlineColor;
            underline.transform.position = position + Vector3.down * (size.y * settings.underlineOffsetRatio);
            underline.transform.localScale = new Vector3(size.x * settings.underlineWidthRatio,
                size.y * settings.underlineThicknessRatio, 1f);
        }

        private static void ApplyIcons(TitleVisualSettings settings, Vector2 size, System.Func<Vector2, Vector3> at)
        {
            float tile = size.x * settings.iconSizeRatio;
            foreach (TitleIconSlot slot in settings.icons)
            {
                if (slot.visual == null)
                    continue;

                slot.visual.transform.position = at(slot.anchor);
                slot.visual.SetContent(slot);
                slot.visual.Layout(new Vector2(tile, tile), settings.symbolRatio, settings.labelGap,
                    settings.labelFontSize);
            }
        }

        private void ApplyPlayer(TitleVisualSettings settings, System.Func<Vector2, Vector3> at, bool placePlayer)
        {
            var renderer = player.GetComponent<SpriteRenderer>();
            renderer.sprite = settings.playerSprite;
            renderer.color = settings.playerColor;

            // 충돌체를 스프라이트 크기에 맞추고, 바닥 판정 위치를 충돌체 아래쪽 끝으로 옮긴다.
            var body = player.GetComponent<BoxCollider2D>();
            if (settings.playerSprite != null)
            {
                body.size = settings.playerSprite.bounds.size;
                body.offset = Vector2.zero;
                Transform groundCheck = player.transform.Find("GroundCheck");
                if (groundCheck != null)
                    groundCheck.localPosition = new Vector3(0f, -body.size.y * 0.5f, 0f);
            }

            if (!placePlayer)
                return;

            float halfHeight = body.size.y * 0.5f * player.transform.lossyScale.y;
            player.transform.position = at(settings.playerStartRatio) + Vector3.up * halfHeight;
        }
    }
}
