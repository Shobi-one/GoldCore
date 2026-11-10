// Unlit particle shader for GoldCore Effects. EffectBuilder sets every property from the preset,
// so it is hidden from the shader menu.
Shader "Hidden/GoldCore/Particle"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Shape ("Shape", Float) = 0
        _Intensity ("Intensity", Float) = 1
        _Pixelate ("Pixelate", Float) = 0
        _Additive ("Additive (fogs to black)", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Src Blend", Float) = 5
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dst Blend", Float) = 1
    }

    // URP
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" "PreviewType" = "Plane" }
        Blend [_SrcBlend] [_DstBlend]
        ZWrite Off
        Cull Off

        Pass
        {
            Name "ForwardUnlit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "../GoldCoreParticleCore.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float _Shape;
                float _Intensity;
                float _Pixelate;
                float _Additive;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                half4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                half4 color : COLOR;
                float2 uv : TEXCOORD0;
                float fogFactor : TEXCOORD1;
            };

            Varyings vert(Attributes v)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.color = v.color;
                o.uv = v.uv;
                o.fogFactor = ComputeFogFactor(o.positionCS.z);
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                float2 uv = GCPixelate(i.uv, _Pixelate);
                half4 col = i.color * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, TRANSFORM_TEX(uv, _MainTex));
                col.a *= GCShapeAlpha(uv, _Shape);
                col.rgb *= _Intensity;
                col.rgb = _Additive > 0.5 ? MixFogColor(col.rgb, half3(0, 0, 0), i.fogFactor) : MixFog(col.rgb, i.fogFactor);
                return col;
            }
            ENDHLSL
        }
    }

    // Built-in render pipeline
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" "PreviewType" = "Plane" }
        Blend [_SrcBlend] [_DstBlend]
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #include "UnityCG.cginc"
            #include "../GoldCoreParticleCore.hlsl"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Shape;
            float _Intensity;
            float _Pixelate;
            float _Additive;

            struct appdata
            {
                float4 vertex : POSITION;
                fixed4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                fixed4 color : COLOR;
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.color = v.color;
                o.uv = v.uv;
                UNITY_TRANSFER_FOG(o, o.pos);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = GCPixelate(i.uv, _Pixelate);
                fixed4 col = i.color * tex2D(_MainTex, TRANSFORM_TEX(uv, _MainTex));
                col.a *= GCShapeAlpha(uv, _Shape);
                col.rgb *= _Intensity;
                if (_Additive > 0.5)
                {
                    UNITY_APPLY_FOG_COLOR(i.fogCoord, col, fixed4(0, 0, 0, 0));
                }
                else
                {
                    UNITY_APPLY_FOG(i.fogCoord, col);
                }
                return col;
            }
            ENDCG
        }
    }
}
