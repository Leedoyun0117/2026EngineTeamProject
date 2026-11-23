using UnityEngine;
using UnityEngine.Tilemaps;

namespace LDY.Stage {
    /// <summary>
    /// 투명 블록용 Tilemap에 붙인다. 에디터에서는 반투명 마커 타일이 보이고,
    /// 플레이 중에는 TilemapRenderer만 꺼서 콜라이더만 남긴다.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TilemapRenderer))]
    public sealed class HideTilemapRendererAtRuntime : MonoBehaviour {
        private void Awake() {
            GetComponent<TilemapRenderer>().enabled = false;
        }
    }
}
