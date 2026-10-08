using TMPro;
using UnityEngine;

namespace LDY.Script
{
    // 메뉴 아이콘의 모양(타일, 심볼, 이름)과 크기를 정하고, 발판/판정 영역을 타일 크기에 맞춘다.
    public class MenuIconVisual : MonoBehaviour
    {
        [SerializeField] private BoxCollider2D platform;
        [SerializeField] private BoxCollider2D hitArea;
        [SerializeField] private SpriteRenderer tile;
        [SerializeField] private SpriteRenderer symbol;
        [SerializeField] private TMP_Text label;
        [Tooltip("이름을 타일 아래가 아니라 타일 안에 표시한다 (닫기 버튼용)")]
        [SerializeField] private bool labelInside;
        [SerializeField, Min(0f)] private float hitPadding = 0.2f;

        public void SetContent(TitleIconSlot slot)
        {
            label.text = slot.labelText;
            tile.sprite = slot.tileSprite;
            tile.color = slot.tileTint;
            symbol.sprite = slot.symbolSprite;
            symbol.color = slot.symbolTint;
        }

        public void Layout(Vector2 tileSize, float symbolRatio, float labelGap, float labelFontSize)
        {
            platform.size = tileSize;
            platform.offset = Vector2.zero;
            SpriteFit.Stretch(tile, tileSize);

            float symbolBox = Mathf.Min(tileSize.x, tileSize.y) * symbolRatio;
            SpriteFit.Contain(symbol, new Vector2(symbolBox, symbolBox));
            symbol.transform.localPosition = new Vector3(0f, labelInside ? 0f : tileSize.y * 0.04f, 0f);

            float labelHeight = labelFontSize * 0.12f;
            label.fontSize = labelFontSize;
            label.rectTransform.sizeDelta = new Vector2(Mathf.Max(tileSize.x * 3f, 3f), labelHeight);
            label.transform.localPosition = labelInside
                ? Vector3.zero
                : new Vector3(0f, -(tileSize.y * 0.5f + labelGap + labelHeight * 0.5f), 0f);

            float labelWidth = label.GetPreferredValues(label.text).x;
            float extraHeight = labelInside ? 0f : labelGap + labelHeight;
            hitArea.size = new Vector2(Mathf.Max(tileSize.x, labelWidth) + hitPadding * 2f,
                tileSize.y + extraHeight + hitPadding * 2f);
            hitArea.offset = new Vector2(0f, -extraHeight * 0.5f);
        }
    }
}
