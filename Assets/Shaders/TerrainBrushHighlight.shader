Shader "Custom/TerrainBrushHighlight"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _Center("Center", Vector) = (0,0,0,0)
        _Radius("Radius", Range(0,200)) = 10
        _Thickness("Thickness", Range(0,100)) = 5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
        fixed4 _Color;
        float3 _Center;
        float _Thickness;
        float _Radius;
        half _Glossiness;
        half _Metallic;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            half4 c = tex2D (_MainTex, IN.uv_MainTex);
            float dist = distance(_Center.xz, IN.worldPos.xz);

            if(_Center.x > IN.worldPos.x - _Radius 
                && _Center.x < IN.worldPos.x + _Radius
                && _Center.z > IN.worldPos.z - _Radius
                && _Center.z < IN.worldPos.z + _Radius
                )
                o.Albedo = _Color;
            else
                o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
