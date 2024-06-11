Shader "Custom/SceneColorCopy"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        [Enum(Off, 0, Back, 1, Front, 2)] _CullMode ("Cull Mode", Int) = 2
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Overlay" "RenderPipeline" = "UniversalRenderPipeline" }

        Pass
        {
            Name "SceneColorCopy"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_MainTex);
            TEXTURE2D(_CameraOpaqueTexture);
            SAMPLER(sampler_MainTex);
            SAMPLER(sampler_CameraOpaqueTexture);

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float2 uv           : TEXCOORD0;
                float4 positionHCS  : SV_POSITION;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // Получаем цвет сцены из текстуры _CameraOpaqueTexture
                half4 sceneColor = SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, IN.uv);
                return sceneColor;
            }
            ENDHLSL

            Cull [_CullMode]
        }
    }
    FallBack "Diffuse"
}