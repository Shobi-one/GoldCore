using UnityEngine;
using UnityEngine.InputSystem;

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

        static readonly System.Collections.Generic.Dictionary<Camera, RetroCamera> byCamera =
            new System.Collections.Generic.Dictionary<Camera, RetroCamera>();

        /// <summary>The RetroCamera on a camera, or null. Used by the URP renderer feature.</summary>
        public static RetroCamera For(Camera camera) =>
            camera != null && byCamera.TryGetValue(camera, out RetroCamera retro) ? retro : null;

        void OnEnable()
        {
            cam = GetComponent<Camera>();
            byCamera[cam] = this;
            if (Application.isPlaying && targetFrameRate > 0) Application.targetFrameRate = targetFrameRate;
            ApplyGlobals();
        }

        void OnDisable()
        {
            if (cam != null && byCamera.TryGetValue(cam, out RetroCamera current) && current == this) byCamera.Remove(cam);
            ClearGlobals();
        }

        static void ClearGlobals()
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
            if (Application.isPlaying && CycleKeyPressed()) NextPreset();
#if UNITY_EDITOR
            // Pick up edits made to the preset asset while it is in use.
            if (!Application.isPlaying) ApplyGlobals();
#endif
        }

        bool CycleKeyPressed()
        {
            Keyboard keyboard = Keyboard.current;
            if (cycleKey == KeyCode.None || keyboard == null) return false;
            string name = cycleKey.ToString();
            if (name.StartsWith("Alpha")) name = "Digit" + name.Substring(5);
            if (!System.Enum.TryParse(name, out Key key) || key == Key.None) return false;
            return keyboard[key].wasPressedThisFrame;
        }

        void ApplyGlobals()
        {
            if (preset == null)
            {
                ClearGlobals();
                return;
            }
            Shader.SetGlobalFloat(VertexSnapId, preset.vertexSnap);
            Shader.SetGlobalFloat(AffineId, preset.affineTextures);
            Shader.SetGlobalFloat(PointSampleId, preset.pointSampleTextures ? 1f : 0f);
            if (cam != null && preset.fog && postProcess) cam.depthTextureMode |= DepthTextureMode.Depth;
        }

        /// <summary>True when this camera should draw the image effect this frame.</summary>
        public bool WantsPostProcess => isActiveAndEnabled && postProcess && preset != null && !preset.IsPassthrough;

        /// <summary>Size the effect renders at for a source of this size.</summary>
        public Vector2Int TargetSize(int sourceWidth, int sourceHeight)
        {
            int height = preset != null && preset.verticalResolution > 0 ? Mathf.Min(preset.verticalResolution, sourceHeight) : sourceHeight;
            int width = Mathf.Max(1, Mathf.RoundToInt(height * (float)sourceWidth / Mathf.Max(1, sourceHeight)));
            return new Vector2Int(width, Mathf.Max(1, height));
        }

        /// <summary>Copies the preset into the post material. Used by both render pipelines.</summary>
        public void ApplyPostSettings(Material material, Vector2Int target)
        {
            material.SetVector(TargetSizeId, new Vector4(target.x, target.y, 1f / target.x, 1f / target.y));
            material.SetFloat(ColorStepsId, (1 << Mathf.Clamp(preset.colorBits, 1, 8)) - 1);
            material.SetFloat(DitherId, preset.dither);
            material.SetFloat(WarmShadowsId, preset.warmShadows);
            material.SetFloat(SaturationId, preset.saturation);
            material.SetFloat(BlurId, preset.blur);
            material.SetFloat(BloomId, preset.bloom);
            material.SetFloat(BloomThresholdId, preset.bloomThreshold);
            material.SetFloat(ScanlinesId, preset.scanlines);
            material.SetColor(FogColorId, preset.fogColor);
            float fogEnd = Mathf.Max(preset.fogEnd, preset.fogStart + 0.01f);
            material.SetVector(FogRangeId, new Vector4(preset.fog ? 1f : 0f, preset.fogStart, fogEnd, 0f));
        }

        // Built-in render pipeline path. URP uses RetroFXFeature instead and never calls this.
        void OnRenderImage(RenderTexture src, RenderTexture dst)
        {
            if (!WantsPostProcess || !EnsureMaterial())
            {
                Graphics.Blit(src, dst);
                return;
            }

            Vector2Int target = TargetSize(src.width, src.height);
            ApplyPostSettings(postMaterial, target);
            if (preset.fog) cam.depthTextureMode |= DepthTextureMode.Depth;

            if (target.y == src.height)
            {
                Graphics.Blit(src, dst, postMaterial);
                return;
            }

            RenderTexture low = RenderTexture.GetTemporary(target.x, target.y, 0, src.format);
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
