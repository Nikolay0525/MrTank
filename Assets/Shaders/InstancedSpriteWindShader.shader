Shader "Custom/InstancedSpriteWindShader" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        [Header(Wind Settings)]
        _WindSpeed ("Wind Speed", Float) = 2.0
        _WindStrength ("Wind Strength", Float) = 0.2
        _TreeHeight ("Tree Height (for bending)", Float) = 2.0
    }
    SubShader {
        // Твої ідеальні налаштування для 2D спрайтів та прозорості
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
        LOD 100
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        
        // Вимикаємо відсікання "спини"
        Cull Off 

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            // Підтримка GPU Instancing
            #pragma multi_compile_instancing 
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            
            // Змінні для вітру
            float _WindSpeed;
            float _WindStrength;
            float _TreeHeight;

            v2f vert (appdata v) {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                
                // === МАГІЯ ВІТРУ ПОЧИНАЄТЬСЯ ===
                
                // Визначаємо, наскільки високо знаходиться точка (від 0 до 1)
                float heightFactor = saturate(v.vertex.y / _TreeHeight);
                
                // Отримуємо глобальну позицію дерева у світі (щоб хиталися в різнобій)
                float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
                
                // Синусоїда: Час + Позиція X
                float wave = sin(_Time.y * _WindSpeed + worldPos.x) * _WindStrength;
                
                // Зміщуємо X (ліво/право). Множимо на квадрат висоти для плавного згину
                v.vertex.x += wave * (heightFactor * heightFactor);
                
                // === МАГІЯ ВІТРУ ЗАКІНЧУЄТЬСЯ ===

                // Стандартна конвертація позиції
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                UNITY_SETUP_INSTANCE_ID(i);
                
                // Читаємо текстуру і множимо на колір
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                return col;
            }
            ENDCG
        }
    }
}