using UnityEngine;

namespace GoldCore.RetroFX
{
    /// <summary>
    /// Applies a <see cref="RetroPreset"/> to the camera it sits on (built-in render pipeline).
    /// It sets the global values the GoldCore/RetroLit shader reads, and draws the image effect.
    /// Swap looks at runtime with <see cref="SetPreset"/> or the cycle key.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    [ExecuteAlways]
    [ImageEffectAllowedInSceneView]
    public class RetroCamera : MonoBehaviour
    {
        public RetroPreset preset;

        [Tooltip("Draw the image effect on this camera. Turn it off on overlay cameras, such as a weapon camera.")]
        public bool postProcess = true;

        [Tooltip("Presets the cycle key steps through.")]
        public RetroPreset[] cyclePresets;
        public KeyCode cycleKey = KeyCode.None;

        [Tooltip("Sets Application.targetFrameRate when this starts. 0 or less leaves it alone.")]
        public int targetFrameRate = -1;

        static readonly int VertexSnapId = Shader.PropertyToID("_RetroVertexSnap");
        static readonly int AffineId = Shader.PropertyToID("_RetroAffine");
        static readonly int PointSampleId = Shader.PropertyToID("_RetroPointSample");

        static readonly int TargetSizeId = Shader.PropertyToID("_RetroTargetSize");
        static readonly int ColorStepsId = Shader.PropertyToID("_RetroColorSteps");
        static readonly int DitherId = Shader.PropertyToID("_RetroDither");
        static readonly int WarmShadowsId = Shader.PropertyToID("_RetroWarmShadows");
        static readonly int SaturationId = Shader.PropertyToID("_RetroSaturation");
        static readonly int BlurId = Shader.PropertyToID("_RetroBlur");
        static readonly int BloomId = Shader.PropertyToID("_RetroBloom");
        static readonly int BloomThresholdId = Shader.PropertyToID("_RetroBloomThreshold");
        static readonly int FogColorId = Shader.PropertyToID("_RetroFogColor");
        static readonly int FogRangeId = Shader.PropertyToID("_RetroFogRange");
        static readonly int ScanlinesId = Shader.PropertyToID("_RetroScanlines");

        Camera cam;
        Material postMaterial;

        public RetroPreset Preset => preset;

        public void SetPreset(RetroPreset newPreset)
        {
            preset = newPreset;
            ApplyGlobals();
        }

        public void NextPreset()
        {
            if (cyclePresets == null || cyclePresets.Length == 0) return;
            int next = (System.Array.IndexOf(cyclePresets, preset) + 1) % cyclePresets.Length;
            SetPreset(cyclePresets[next]);
        }

        void OnEnable()
        {
            cam = GetComponent<Camera>();
            if (Application.isPlaying && targetFrameRate > 0) Application.targetFrameRate = targetFrameRate;
            ApplyGlobals();
        }

        void OnDisable()
        {
            Shader.SetGlobalFloat(VertexSnapId, 0f);
            Shader.SetGlobalFloat(AffineId, 0f);
            Shader.SetGlobalFloat(PointSampleId, 0f);
        }

        void OnDestroy()
        {
            if (postMaterial != null) DestroyImmediate(postMaterial);
        }

        void OnValidate()
        {
            if (isActiveAndEnabled) ApplyGlobals();
        }

        void Update()
        {
            if (Application.isPlaying && cycleKey != KeyCode.None && Input.GetKeyDown(cycleKey)) NextPreset();
#if UNITY_EDITOR
            // Pick up edits made to the preset asset while it is in use.
            if (!Application.isPlaying) ApplyGlobals();
#endif
        }

        void ApplyGlobals()
        {
            if (preset == null)
            {
                OnDisable();
                return;
            }
            Shader.SetGlobalFloat(VertexSnapId, preset.vertexSnap);
            Shader.SetGlobalFloat(AffineId, preset.affineTextures);
            Shader.SetGlobalFloat(PointSampleId, preset.pointSampleTextures ? 1f : 0f);
            if (cam != null && preset.fog && postProcess) cam.depthTextureMode |= DepthTextureMode.Depth;
        }

        void OnRenderImage(RenderTexture src, RenderTexture dst)
        {
            if (!postProcess || preset == null || preset.IsPassthrough || !EnsureMaterial())
            {
                Graphics.Blit(src, dst);
                return;
            }

            int height = preset.verticalResolution > 0 ? Mathf.Min(preset.verticalResolution, src.height) : src.height;
            int width = Mathf.Max(1, Mathf.RoundToInt(height * (float)src.width / src.height));

            postMaterial.SetVector(TargetSizeId, new Vector4(width, height, 1f / width, 1f / height));
            postMaterial.SetFloat(ColorStepsId, (1 << Mathf.Clamp(preset.colorBits, 1, 8)) - 1);
            postMaterial.SetFloat(DitherId, preset.dither);
            postMaterial.SetFloat(WarmShadowsId, preset.warmShadows);
            postMaterial.SetFloat(SaturationId, preset.saturation);
            postMaterial.SetFloat(BlurId, preset.blur);
            postMaterial.SetFloat(BloomId, preset.bloom);
            postMaterial.SetFloat(BloomThresholdId, preset.bloomThreshold);
            postMaterial.SetFloat(ScanlinesId, preset.scanlines);
            postMaterial.SetColor(FogColorId, preset.fogColor);
            float fogEnd = Mathf.Max(preset.fogEnd, preset.fogStart + 0.01f);
            postMaterial.SetVector(FogRangeId, new Vector4(preset.fog ? 1f : 0f, preset.fogStart, fogEnd, 0f));
            if (preset.fog) cam.depthTextureMode |= DepthTextureMode.Depth;

            if (height == src.height)
            {
                Graphics.Blit(src, dst, postMaterial);
                return;
            }

            RenderTexture low = RenderTexture.GetTemporary(width, height, 0, src.format);
            low.filterMode = preset.pixelatedUpscale ? FilterMode.Point : FilterMode.Bilinear;
            Graphics.Blit(src, low, postMaterial);
            Graphics.Blit(low, dst);
            RenderTexture.ReleaseTemporary(low);
        }

        bool EnsureMaterial()
        {
            if (postMaterial != null) return true;
            Shader shader = Shader.Find("Hidden/GoldCore/RetroPost");
            if (shader == null || !shader.isSupported) return false;
            postMaterial = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
            return true;
        }
    }
}
