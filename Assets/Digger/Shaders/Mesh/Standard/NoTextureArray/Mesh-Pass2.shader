Shader "Digger/Standard/Mesh-Pass2" {
    Properties {
    
        // set by digger engine
        _Splat0 ("Layer 0", 2D) = "white" {}
        _Splat1 ("Layer 1", 2D) = "white" {}
        _Splat2 ("Layer 2", 2D) = "white" {}
        _Splat3 ("Layer 3", 2D) = "white" {}
        
        _Normal0 ("Normal 0", 2D) = "bump" {}
        _Normal1 ("Normal 1", 2D) = "bump" {}
        _Normal2 ("Normal 2", 2D) = "bump" {}
        _Normal3 ("Normal 3", 2D) = "bump" {}
        
        [Gamma] _Metallic0 ("Metallic 0", Range(0.0, 1.0)) = 0.0
        _Smoothness0 ("Smoothness 0", Range(0.0, 1.0)) = 0.0
        
        [Gamma] _Metallic1 ("Metallic 1", Range(0.0, 1.0)) = 0.0
        _Smoothness1 ("Smoothness 1", Range(0.0, 1.0)) = 0.0
        
        [Gamma] _Metallic2 ("Metallic 2", Range(0.0, 1.0)) = 0.0
        _Smoothness2 ("Smoothness 2", Range(0.0, 1.0)) = 0.0
        
        [Gamma] _Metallic3 ("Metallic 3", Range(0.0, 1.0)) = 0.0
        _Smoothness3 ("Smoothness 3", Range(0.0, 1.0)) = 0.0
      
        
        // used in fallback on old cards & base map
        _MainTex ("BaseMap (RGB)", 2D) = "white" {}
        _Color ("Main Color", Color) = (1,1,1,1)
        
        _tiles0x ("tile0X", float) = 0.03
        _tiles0y ("tile0Y", float) = 0.03
        _tiles1x ("tile1X", float) = 0.03
        _tiles1y ("tile1Y", float) = 0.03
        _tiles2x ("tile2X", float) = 0.03
        _tiles2y ("tile2Y", float) = 0.03
        _tiles3x ("tile3X", float) = 0.03
        _tiles3y ("tile3Y", float) = 0.03
        [HideInInspector] _offset0x ("offset0X", float) = 0
        [HideInInspector] _offset0y ("offset0Y", float) = 0
        [HideInInspector] _offset1x ("offset1X", float) = 0
        [HideInInspector] _offset1y ("offset1Y", float) = 0
        [HideInInspector] _offset2x ("offset2X", float) = 0
        [HideInInspector] _offset2y ("offset2Y", float) = 0
        [HideInInspector] _offset3x ("offset3X", float) = 0
        [HideInInspector] _offset3y ("offset3Y", float) = 0

        [HideInInspector] _NormalScale0 ("normalScale0", float) = 1
        [HideInInspector] _NormalScale1 ("normalScale1", float) = 1
        [HideInInspector] _NormalScale2 ("normalScale2", float) = 1
        [HideInInspector] _NormalScale3 ("normalScale3", float) = 1
    }

    SubShader {
        Tags {
            "Queue" = "Geometry-103"
            "IgnoreProjector" = "True"
            "RenderType" = "Opaque"
            "DisableBatching" = "True"
        }

        CGPROGRAM
        #pragma surface surf Standard decal:add vertex:SplatmapVert finalcolor:SplatmapFinalColor finalgbuffer:SplatmapFinalGBuffer fullforwardshadows nometa
        #pragma instancing_options assumeuniformscaling nomatrices nolightprobe nolightmap forwardadd
        #pragma multi_compile_fog
        #pragma target 3.0
        // needs more than 8 texcoords
        #pragma exclude_renderers gles
        #include "UnityPBSLighting.cginc"

        #pragma multi_compile __ _NORMALMAP

        #define TERRAIN_SPLAT_ADDPASS
        #define TERRAIN_SURFACE_OUTPUT SurfaceOutputStandard
        #include "MeshTriplanarStandard.cginc"
        

        half _Metallic0;
        half _Metallic1;
        half _Metallic2;
        half _Metallic3;

        half _Smoothness0;
        half _Smoothness1;
        half _Smoothness2;
        half _Smoothness3;
        
        void SplatmapVert(inout appdata_full v, out Input data) {
            UNITY_INITIALIZE_OUTPUT(Input, data);
            data.splat_control = v.texcoord2;
            float4 pos = UnityObjectToClipPos (v.vertex); 
            UNITY_TRANSFER_FOG(data, pos);
            v.tangent.xyz = cross(v.normal, float3(0,0,1));
            v.tangent.w = -1;
            data.vertNormal = v.normal;
        }

        void surf (Input IN, inout SurfaceOutputStandard o) {
            half4 splat_control03 = IN.splat_control;
            half weight = dot(splat_control03, half4(1,1,1,1));
            // Normalize weights before lighting and restore weights in final modifier functions so that the overal
	        // lighting result can be correctly weighted.
            splat_control03 /= (weight + 1e-3f);
            fixed4 mixedDiffuse;
            half4 defaultSmoothness03 = half4(_Smoothness0, _Smoothness1, _Smoothness2, _Smoothness3);
            SplatmapMix(IN, 
                        defaultSmoothness03, 
                        splat_control03, 
                        mixedDiffuse, o.Normal);
            o.Albedo = mixedDiffuse.rgb;
            o.Alpha = weight;
            o.Smoothness = mixedDiffuse.a;
            o.Metallic = dot(splat_control03, half4(_Metallic0, _Metallic1, _Metallic2, _Metallic3));
        }
        ENDCG
    }

    FallBack "Diffuse"
}
