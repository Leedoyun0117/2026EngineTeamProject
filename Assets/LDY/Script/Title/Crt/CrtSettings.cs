using System;
using UnityEngine;

namespace LDY.Script
{
    [Serializable]
    public class CrtSettings
    {
        [Range(0f, 0.4f)] public float curvature = 0.03f;
        [Range(0f, 1f)] public float scanlineIntensity = 0.12f;
        [Min(1f)] public float scanlineCount = 360f;
        [Range(0f, 1f)] public float vignetteIntensity = 0.2f;
        [Range(0.05f, 1f)] public float vignetteSoftness = 0.5f;
        [Range(0f, 0.2f)] public float flickerIntensity = 0.008f;
        [Range(0f, 0.3f)] public float noiseIntensity = 0.006f;
    }
}
