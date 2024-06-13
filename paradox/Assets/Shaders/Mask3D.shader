Shader "Masked/Mask" { 
    Properties
    {
        [Enum(Off, 0, Back, 1, Front, 2)] _CullMode ("Cull Mode", Int) = 2
    }
     SubShader {
        // Render the mask after regular geometry, but before masked geometry and
        // transparent things.
        Tags {"Queue" = "Geometry+501"}
        // Don't draw in the RGBA channels; just the depth buffer
        ColorMask 0
        ZWrite On
        Cull [_CullMode]
        // Do nothing specific in the pass:
        Pass {}
    }
}

//Cull [_CullMode]

// Shader "Masked/Mask" { 
//     Properties
//     {
//         [Enum(Off, 0, Back, 1, Front, 2)] _CullMode ("Cull Mode", Int) = 2
//     }
//     SubShader {
//         // Render the mask after the skybox, but before masked geometry and transparent things.
//         Tags {"Queue" = "Geometry+10"}
//         // Don't draw in the RGBA channels; just the depth buffer
//         ColorMask 0
//         ZWrite On
        
//         // Do nothing specific in the pass:
//         Pass {
//             // Ensure depth testing is enabled
//             ZTest LEqual
//         }
        
//     }
// }
