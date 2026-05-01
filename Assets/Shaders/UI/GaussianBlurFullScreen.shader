Shader "Angel/UI/GaussianBlurFullScreen"
{
    Properties
    {
        _Radius ("Radius", Range(0, 6)) = 0
        _Darken ("Darken", Range(0, 1)) = 0
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }
        ZWrite Off
        Cull Off
        ZTest Always

        Pass
        {
            Name "GaussianBlur"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            float4 _BlitTexture_TexelSize;
            float _Radius;
            float _Darken;

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 uv = input.texcoord.xy;
                float2 stepUV = _BlitTexture_TexelSize.xy * _Radius;

                half4 color = 0;

                [unroll]
                for (int y = 0; y < 5; y++)
                {
                    [unroll]
                    for (int x = 0; x < 5; x++)
                    {
                        float wx = x == 2 ? 6.0 : (x == 1 || x == 3 ? 4.0 : 1.0);
                        float wy = y == 2 ? 6.0 : (y == 1 || y == 3 ? 4.0 : 1.0);
                        float weight = wx * wy;
                        float2 offset = float2(x - 2, y - 2) * stepUV;

                        color += SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + offset) * weight;
                    }
                }

                color *= 1.0 / 256.0;
                color.rgb *= 1.0 - _Darken;
                color.a = 1.0;
                return color;
            }
            ENDHLSL
        }
    }
}
