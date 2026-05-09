Shader "Custom/NPCShadow"
{
    Properties
    {
        _MainTex("Sprite Texture", 2D) = "white" {}
        _ShadowColor("Shadow Color", Color) = (0,0,0,0.3)
        _ShadowOffsetY("Shadow Offset Y", Float) = -0.1
        _ShadowScaleX("Shadow Scale X", Float) = 1
        _ShadowScaleY("Shadow Scale Y", Float) = 0.4
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "Queue"="Transparent"
        }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float4 _ShadowColor;
            float _ShadowOffsetY;
            float _ShadowScaleX;
            float _ShadowScaleY;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                float4 pos = IN.vertex;

                // 局部缩放形成扁椭圆
                pos.x *= _ShadowScaleX;
                pos.y *= _ShadowScaleY;

                // Y方向偏移到脚下
                pos.y += _ShadowOffsetY;

                OUT.vertex = UnityObjectToClipPos(pos);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                // ShadowColor 自带 Alpha 控制透明度
                fixed4 col = _ShadowColor;
                return col;
            }
            ENDHLSL
        }
    }
}