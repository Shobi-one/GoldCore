// Image effect for RetroCamera on URP, drawn by RetroFXFeature. The effect is in RetroPostCore.hlsl.
Shader "Hidden/GoldCore/RetroPostURP"
{
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            Name "RetroFX"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma target 3.5
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            // The scene depth, bound by RetroFXFeature when fog is on.
            TEXTURE2D_X_FLOAT(_RetroDepthTexture);

            #define RETRO_SAMPLE(uv) SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, (uv)).rgb
            #define RETRO_EYE_DEPTH(uv) LinearEyeDepth(SAMPLE_TEXTURE2D_X(_RetroDepthTexture, sampler_PointClamp, (uv)).r, _ZBufferParams)
            #include "../RetroPostCore.hlsl"

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                return RetroProcess(input.texcoord);
            }
            ENDHLSL
        }
    }
    Fallback Off
}
