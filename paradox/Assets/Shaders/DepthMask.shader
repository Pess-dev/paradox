
Shader "DepthMask"
{
    Properties
    {
        [IntRange] _StencilID ("Stencil ID", Range(0, 255)) = 0
        [Enum(Off, 0, Back, 1, Front, 2)] _CullMode ("Cull Mode", Int) = 2
    }

   SubShader {
        Tags { "RenderType"="Opaque" "Queue"="Transparent-1"}
        colormask 0
        Pass {
            Stencil {
                Ref [_StencilID]
                Comp NotEqual
                Pass Zero
            }
            
            Cull [_CullMode]
     
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            struct appdata {
                float4 vertex : POSITION;
            };
            struct v2f {
                float4 pos : SV_POSITION;
            };
            v2f vert(appdata v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }
            half4 frag(v2f i) : SV_Target {
                return half4(1,0,0,1);
            }
            ENDCG
        }
    }
}
