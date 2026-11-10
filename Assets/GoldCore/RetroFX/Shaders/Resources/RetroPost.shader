// Image effect for RetroCamera on the built-in render pipeline. URP uses RetroPostURP.
// The effect itself is in RetroPostCore.hlsl.
Shader "Hidden/GoldCore/RetroPost"
{
    Properties
    {
        _MainTex ("Source", 2D) = "white" {}
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #pragma target 3.5
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);

            #define RETRO_SAMPLE(uv) tex2D(_MainTex, (uv)).rgb
            #define RETRO_EYE_DEPTH(uv) LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, (uv)))
            #include "../RetroPostCore.hlsl"

            fixed4 frag(v2f_img i) : SV_Target
            {
                return RetroProcess(i.uv);
            }
            ENDCG
        }
    }
    Fallback Off
}
