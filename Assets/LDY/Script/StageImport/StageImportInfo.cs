using UnityEngine;

namespace LDY.StageImport {
    // 툴에서 가져온 원본 정보. StageRoot 오브젝트에 붙는다.
    [DisallowMultipleComponent]
    public sealed class StageImportInfo : MonoBehaviour {
        [SerializeField] private string sourceMapId;
        [SerializeField] private string sourceName;
        [SerializeField] private string status;
        [SerializeField] private bool darkStage;
        [SerializeField, TextArea(3, 12)] private string memo;

        public string SourceMapId => sourceMapId;
        public string SourceName => sourceName;
        public string Memo => memo;
        public bool DarkStage => darkStage;

        public void Fill(string mapId, string mapName, string mapStatus, bool dark, string text) {
            sourceMapId = mapId;
            sourceName = mapName;
            status = mapStatus;
            darkStage = dark;
            memo = text;
        }
    }
}
