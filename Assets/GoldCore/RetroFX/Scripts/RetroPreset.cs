using UnityEngine;

namespace GoldCore.RetroFX
{
    /// <summary>
    /// One look for <see cref="RetroCamera"/>. Every effect is its own setting, so a preset is
    /// just a mix of them. Make a new one with Assets > Create > GoldCore > Retro Preset.
    /// </summary>
    [CreateAssetMenu(fileName = "RetroPreset", menuName = "GoldCore/Retro Preset", order = 0)]
    public class RetroPreset : ScriptableObject
    {
        [Header("Resolution")]
        [Tooltip("Height in pixels the image is rendered at before it is scaled up. 0 keeps the screen resolution.")]
        [Min(0)] public int verticalResolution = 240;
        [Tooltip("Scale the low resolution image up with hard pixels instead of smoothing.")]
        public bool pixelatedUpscale = true;

        [Header("Color")]
        [Tooltip("Bits per color channel. 5 is the 15-bit color of PS1 and N64, 8 is off.")]
        [Range(1, 8)] public int colorBits = 5;
        [Tooltip("Ordered (Bayer 4x4) dithering that hides the banding from fewer color bits.")]
        [Range(0f, 1f)] public float dither = 1f;
        [Tooltip("Pushes the dark parts of the image toward red, like a lot of PS1 games.")]
        [Range(0f, 1f)] public float warmShadows = 0f;
        [Range(0f, 2f)] public float saturation = 1f;

        [Header("Softness and glow")]
        [Tooltip("Blurs the image a little, like the N64's filtered, soft output.")]
        [Range(0f, 1f)] public float blur = 0f;
        [Tooltip("How much bright parts glow. 0 is off.")]
        [Range(0f, 2f)] public float bloom = 0f;
        [Range(0f, 1f)] public float bloomThreshold = 0.75f;

        [Header("Fog")]
        [Tooltip("Distance fog drawn by the effect, so it works on every material.")]
        public bool fog = false;
        public Color fogColor = new Color(0.55f, 0.6f, 0.7f, 1f);
        [Min(0f)] public float fogStart = 15f;
        [Min(0f)] public float fogEnd = 60f;

        [Header("Screen")]
        [Range(0f, 1f)] public float scanlines = 0f;

        [Header("Geometry (materials using GoldCore/RetroLit only)")]
        [Tooltip("Snaps vertices to a grid this many cells tall, so geometry wobbles. 0 is off.")]
        [Min(0f)] public float vertexSnap = 0f;
        [Tooltip("Mixes in affine (not perspective correct) texture mapping, so textures warp.")]
        [Range(0f, 1f)] public float affineTextures = 0f;
        [Tooltip("Samples textures with hard texels, whatever the texture's import filter is.")]
        public bool pointSampleTextures = false;

        /// <summary>True when the image effect would change nothing, so it can be skipped.</summary>
        public bool IsPassthrough =>
            verticalResolution == 0 && colorBits >= 8 && dither <= 0f && warmShadows <= 0f &&
            Mathf.Approximately(saturation, 1f) && blur <= 0f && bloom <= 0f && !fog && scanlines <= 0f;
    }
}
