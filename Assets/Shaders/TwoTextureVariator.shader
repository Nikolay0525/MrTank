Shader "Custom/TwoTexturesVariator"
{
    Properties
    {
        _MainTex ("First Texture", 2D) = "white" {}
        _SecondTex ("Second Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Speed ("Animation Speed", Float) = 5.0
        _Smoothness ("Blend Smoothness", Range(0, 0.5)) = 0.01
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ PIXELSNAP_ON
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            fixed4 _Color;
            sampler2D _MainTex;
            sampler2D _SecondTex;
            float _Speed;
            float _Smoothness;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                
                // Transform vertex position to clip space
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                
                // Apply tint color to vertex color
                OUT.color = IN.color * _Color;
                
                #ifdef PIXELSNAP_ON
                OUT.vertex = UnityPixelSnap (OUT.vertex);
                #endif
                
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                // Sample both textures using the same UV coordinates
                fixed4 c1 = tex2D(_MainTex, IN.texcoord);
                fixed4 c2 = tex2D(_SecondTex, IN.texcoord);

                // Generate a time-based oscillating value between 0 and 1
                float timeFactor = sin(_Time.y * _Speed) * 0.5 + 0.5;

                // Create a sharp step or smooth transition based on the smoothness parameter
                float blend = smoothstep(0.5 - _Smoothness, 0.5 + _Smoothness, timeFactor);

                // Interpolate between the two textures and apply vertex color tint
                fixed4 c = lerp(c1, c2, blend) * IN.color;
                
                // Premultiply RGB by Alpha for proper sprite blending
                c.rgb *= c.a;
                
                return c;
            }
        ENDCG
        }
    }
}