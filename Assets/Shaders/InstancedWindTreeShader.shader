Shader "Custom/InstancedWindTree"
{
    Properties
    {
        _MainTex ("Albedo (Texture)", 2D) = "white" {}
        _Color ("Tint Color", Color) = (1,1,1,1)
        
        [Header(Wind Settings)]
        _WindSpeed ("Wind Speed", Float) = 2.0
        _WindStrength ("Wind Strength", Float) = 0.2
        _TreeHeight ("Tree Height (for bending)", Float) = 2.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // vertex:vert - кажемо Unity, що ми хочемо змінювати вершини
        // addshadow - щоб тіні хиталися разом з деревом
        #pragma surface surf Standard vertex:vert addshadow fullforwardshadows
        
        // ВАЖЛИВО: Цей рядок дозволяє шейдеру працювати з Graphics.DrawMeshInstanced!
        #pragma multi_compile_instancing 
        #pragma target 3.0

        sampler2D _MainTex;
        fixed4 _Color;
        float _WindSpeed;
        float _WindStrength;
        float _TreeHeight;

        struct Input
        {
            float2 uv_MainTex;
        };

        // Блок для змінних Instancing (навіть якщо він порожній, він потрібен)
        UNITY_INSTANCING_BUFFER_START(Props)
        UNITY_INSTANCING_BUFFER_END(Props)

        // Функція зміни вершин (тут відбувається магія вітру)
        void vert(inout appdata_full v)
        {
            // Отримуємо локальну висоту вершини. 
            // Якщо Y = 0 (корінь), heightFactor = 0 (не хитається).
            // Чим вище вершина, тим більше вона хитається.
            float heightFactor = saturate(v.vertex.y / _TreeHeight);
            
            // Отримуємо глобальну позицію дерева, щоб вони не хиталися всі абсолютно синхронно
            float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
            
            // Рахуємо синусоїду на основі часу та позиції дерева у світі
            float wave = sin(_Time.y * _WindSpeed + worldPos.x) * _WindStrength;

            // Рухаємо вершину по осі X (ліво/право)
            // Множимо на квадрат висоти (heightFactor * heightFactor), щоб стовбур гнувся плавно
            v.vertex.x += wave * (heightFactor * heightFactor);
        }

        // Стандартна функція малювання текстури
        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}