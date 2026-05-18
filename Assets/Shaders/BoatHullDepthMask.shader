Shader "Custom/BoatHullStencilMask"
{
    SubShader
    {
        // Geometry-1 で水（Transparent）より前に描画し、ステンシルを書き込む
        Tags
        {
            "RenderType" = "Opaque"
            "Queue" = "Geometry-1"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "StencilWrite"

            Stencil
            {
                Ref 1
                Comp Always    // 常に書き込む
                Pass Replace   // ステンシルバッファを 1 に上書き
                Fail Keep
                ZFail Keep
            }

            ZWrite Off      // 深度は書き込まない（ボート本体の描画を妨げない）
            ColorMask 0     // 色も書き込まない（完全に不可視）
            Cull Off        // 両面有効（カメラ角度を問わない）

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }
    }
}
