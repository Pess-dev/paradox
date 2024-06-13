Shader "Custom/StencilMask"
{
    Properties
    {
        [IntRange] _StencilID ("Stencil ID", Range(0, 255)) = 0
        [Enum(Off, 0, Back, 1, Front, 2)] _CullMode ("Cull Mode", Int) = 2
    }

    SubShader
    {
        //Tags { "RenderType"="Opaque" "Queue"="Geometry-1" "RenderPipeline" = "UniversalPipeline"}
        Tags { "RenderType"="Opaque" "Queue"="Geometry-1" "RenderPipeline" = "UniversalPipeline"}

        Pass 
        {
            Blend Zero One
            ZWrite Off
            //ColorMask 0
            //ZWrite On
            Cull [_CullMode]

            Stencil
            {
                Ref [_StencilID]
                //Comp Always
                Comp Greater
                Pass Replace
            }
        }
    }
}