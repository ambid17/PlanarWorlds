Shader "Custom/TerrainBrushHighlight"
{
    Properties
    {
        // Control Texture ("Splat Map")
        [HideInInspector] _Control("Control (RGBA)", 2D) = "red" {}

        // Terrain textures - each weighted according to the corresponding colour
        // channel in the control texture
        [HideInInspector] _Splat3("Layer 3 (A)", 2D) = "white" {}
        [HideInInspector] _Splat2("Layer 2 (B)", 2D) = "white" {}
        [HideInInspector] _Splat1("Layer 1 (G)", 2D) = "white" {}
        [HideInInspector] _Splat0("Layer 0 (R)", 2D) = "white" {}

        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _Center("Center", Vector) = (0,0,0,0)
        _Radius("Radius", Range(0,200)) = 10
    }
    SubShader
    {
        Tags {
            "SplatCount" = "4"
            "Queue" = "Geometry-100"
            "RenderType"="Opaque"
        }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

             
        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.5

        sampler2D _MainTex;
        fixed4 _Color;
        float3 _Center;
        float _Radius;
        half _Glossiness;
        half _Metallic;

        uniform sampler2D _Control;
        uniform sampler2D _Splat0, _Splat1, _Splat2, _Splat3;

        struct Input
        {
            float2 uv_Control : TEXCOORD0;
            float2 uv_Splat0 : TEXCOORD1;
            float2 uv_Splat1 : TEXCOORD2;
            float2 uv_Splat2 : TEXCOORD3;
            float2 uv_Splat3 : TEXCOORD4;
            float3 worldPos;
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 splat_control = tex2D(_Control, IN.uv_Control);
            fixed3 col;

            if(_Center.x > IN.worldPos.x - _Radius 
                && _Center.x < IN.worldPos.x + _Radius
                && _Center.z > IN.worldPos.z - _Radius
                && _Center.z < IN.worldPos.z + _Radius
                )
                o.Albedo = (1,0,1,1);
            else
                col = splat_control.r * tex2D(_Splat0, IN.uv_Splat0).rgb;
                col += splat_control.g * tex2D(_Splat1, IN.uv_Splat1).rgb;
                col += splat_control.b * tex2D(_Splat2, IN.uv_Splat2).rgb;
                col += splat_control.a * tex2D(_Splat3, IN.uv_Splat3).rgb;
                o.Albedo = col;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = 1;
        }
        ENDCG
    }
            Dependency "AddPassShader" = "MyShaders/Terrain/Editor/AddPass"
            Dependency "BaseMapShader" = "MyShaders/Terrain/Editor/BaseMap"

    FallBack "Diffuse"
}
