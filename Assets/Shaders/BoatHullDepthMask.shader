Shader "Custom/BoatHullDepthMask"
{
    SubShader
    {
        // Geometry-1 で描画することで、Transparent の水シェーダーより前に深度を書き込む
        Tags
        {
            "RenderType" = "Opaque"
            "Queue" = "Geometry-1"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "DepthMask"

            ZWrite On
            ZTest LEqual
            ColorMask 0      // 色を一切書き込まない（完全に不可視）
            Cull Back        // 上向き法線のクワッドを上方カメラから見た場合のみ有効

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
