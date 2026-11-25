using UnityEngine;

namespace LDY.Background {
    [CreateAssetMenu(menuName = "LDY/Background/StageBackgroundSource", fileName = "StageBackgroundSource")]
    public sealed class StageBackgroundSource : ScriptableObject {
        [SerializeField] private string folderPath;
        [SerializeField] private string jsonPath;
        [SerializeField] private string prefabPath;
        [SerializeField] private string textSetPath;
        [SerializeField, Min(1f)] private float pixelsPerUnit = 40f;
        [SerializeField] private string fontPath;
        [SerializeField, Min(1)] private int fontPixelSize = 12;

        public string FolderPath => folderPath;
        public string JsonPath => jsonPath;
        public string PrefabPath => prefabPath;
        public string TextSetPath => textSetPath;
        public float PixelsPerUnit => pixelsPerUnit;
        public string FontPath => fontPath;
        public int FontPixelSize => fontPixelSize;
    }
}
