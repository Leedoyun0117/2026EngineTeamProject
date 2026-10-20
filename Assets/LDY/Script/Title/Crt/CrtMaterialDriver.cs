using System.Collections;
using UnityEngine;

namespace LDY.Script
{
    // 시간은 TitleClock을 따라서 Alt 중에도 흐름
    [ExecuteAlways]
    public class CrtMaterialDriver : MonoBehaviour, ICrtPower
    {
        private static readonly int ContentTex = Shader.PropertyToID("_ContentTex");
        private static readonly int Curvature = Shader.PropertyToID("_Curvature");
        private static readonly int ScanlineIntensity = Shader.PropertyToID("_ScanlineIntensity");
        private static readonly int ScanlineCount = Shader.PropertyToID("_ScanlineCount");
        private static readonly int VignetteIntensity = Shader.PropertyToID("_VignetteIntensity");
        private static readonly int VignetteSoftness = Shader.PropertyToID("_VignetteSoftness");
        private static readonly int FlickerIntensity = Shader.PropertyToID("_FlickerIntensity");
        private static readonly int NoiseIntensity = Shader.PropertyToID("_NoiseIntensity");
        private static readonly int CrtTime = Shader.PropertyToID("_CrtTime");
        private static readonly int PowerOff = Shader.PropertyToID("_PowerOff");
        private static readonly int PowerFill = Shader.PropertyToID("_PowerFill");

        [SerializeField] private Renderer screenRenderer;
        [SerializeField] private CrtScreen screen;
        [SerializeField] private TitleClock clock;
        [SerializeField] private CrtSettings settings = new CrtSettings();

        private MaterialPropertyBlock _block;

        public float CurvatureValue => settings.curvature;
        public float Level { get; set; }
        public Vector2 Fill { get; set; } = Vector2.one;

        // Level/Fill은 출력 마스크 값이라 곡률 UV 변환(클릭 좌표)에는 영향 없음
        public IEnumerator AnimateTo(float target, float duration)
        {
            float from = Level;
            yield return UnscaledTween.Run(duration, t => Level = Mathf.Lerp(from, target, t));
            Level = target;
        }

        private void LateUpdate()
        {
            if (clock == null || screen == null)
                return;

            _block ??= new MaterialPropertyBlock();
            screenRenderer.GetPropertyBlock(_block);
            _block.SetTexture(ContentTex, screen.Texture);
            _block.SetFloat(Curvature, settings.curvature);
            _block.SetFloat(ScanlineIntensity, settings.scanlineIntensity);
            _block.SetFloat(ScanlineCount, settings.scanlineCount);
            _block.SetFloat(VignetteIntensity, settings.vignetteIntensity);
            _block.SetFloat(VignetteSoftness, settings.vignetteSoftness);
            _block.SetFloat(FlickerIntensity, settings.flickerIntensity);
            _block.SetFloat(NoiseIntensity, settings.noiseIntensity);
            _block.SetFloat(CrtTime, clock.Now);
            _block.SetFloat(PowerOff, Mathf.Clamp01(Level));
            _block.SetVector(PowerFill, new Vector4(Fill.x, Fill.y, 0f, 0f));
            screenRenderer.SetPropertyBlock(_block);
        }
    }
}
