using UnityEngine;

namespace LDY.Script
{
    // CRT 설정과 시간을 셰이더에 전달한다. 시간은 TitleClock을 따르므로 Alt 중에도 흐른다.
    [ExecuteAlways]
    public class CrtMaterialDriver : MonoBehaviour
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

        [SerializeField] private Renderer screenRenderer;
        [SerializeField] private CrtScreen screen;
        [SerializeField] private TitleClock clock;
        [SerializeField] private CrtSettings settings = new CrtSettings();

        private MaterialPropertyBlock _block;

        public float CurvatureValue => settings.curvature;

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
            screenRenderer.SetPropertyBlock(_block);
        }
    }
}
