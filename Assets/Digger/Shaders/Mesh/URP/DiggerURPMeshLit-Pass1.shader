Shader "Digger/URP/Mesh-Pass1"
{
    Properties
    {
        // Layer count is passed down to guide height-blend enable/disable, due
        // to the fact that heigh-based blend will be broken with multipass.
        [HideInInspector] [PerRendererData] _NumLayersCount ("Total Layer Count", Float) = 1.0
    
        // set by terrain engine
        [HideInInspector] _Splat3("Layer 3 (A)", 2D) = "white" {}
        [HideInInspector] _Splat2("Layer 2 (B)", 2D) = "white" {}
        [HideInInspector] _Splat1("Layer 1 (G)", 2D) = "white" {}
        [HideInInspector] _Splat0("Layer 0 (R)", 2D) = "white" {}
        [HideInInspector] _Normal3("Normal 3 (A)", 2D) = "bump" {}
        [HideInInspector] _Normal2("Normal 2 (B)", 2D) = "bump" {}
        [HideInInspector] _Normal1("Normal 1 (G)", 2D) = "bump" {}
        [HideInInspector] _Normal0("Normal 0 (R)", 2D) = "bump" {}
        [HideInInspector] _Mask3("Mask 3 (A)", 2D) = "grey" {}
        [HideInInspector] _Mask2("Mask 2 (B)", 2D) = "grey" {}
        [HideInInspector] _Mask1("Mask 1 (G)", 2D) = "grey" {}
        [HideInInspector] _Mask0("Mask 0 (R)", 2D) = "grey" {}
        [HideInInspector][Gamma] _Metallic0("Metallic 0", Range(0.0, 1.0)) = 0.0
        [HideInInspector][Gamma] _Metallic1("Metallic 1", Range(0.0, 1.0)) = 0.0
        [HideInInspector][Gamma] _Metallic2("Metallic 2", Range(0.0, 1.0)) = 0.0
        [HideInInspector][Gamma] _Metallic3("Metallic 3", Range(0.0, 1.0)) = 0.0
        [HideInInspector] _Smoothness0("Smoothness 0", Range(0.0, 1.0)) = 0.5
        [HideInInspector] _Smoothness1("Smoothness 1", Range(0.0, 1.0)) = 0.5
        [HideInInspector] _Smoothness2("Smoothness 2", Range(0.0, 1.0)) = 0.5
        [HideInInspector] _Smoothness3("Smoothness 3", Range(0.0, 1.0)) = 0.5
        [HideInInspector] _LayerHasMask0("LayerHasMask 0", Range(0.0, 1.0)) = 0
        [HideInInspector] _LayerHasMask1("LayerHasMask 1", Range(0.0, 1.0)) = 0
        [HideInInspector] _LayerHasMask2("LayerHasMask 2", Range(0.0, 1.0)) = 0
        [HideInInspector] _LayerHasMask3("LayerHasMask 3", Range(0.0, 1.0)) = 0
        
        [HideInInspector]  _DiffuseRemapScale0("DiffuseRemapScale  0", Vector) = (1,1,1,1)
        [HideInInspector] _MaskMapRemapOffset0("MaskMapRemapOffset 0", Vector) = (0,0,0,0)
        [HideInInspector]  _MaskMapRemapScale0("MaskMapRemapScale  0", Vector) = (1,1,1,1)
        
        [HideInInspector]  _DiffuseRemapScale1("DiffuseRemapScale  1", Vector) = (1,1,1,1)
        [HideInInspector] _MaskMapRemapOffset1("MaskMapRemapOffset 1", Vector) = (0,0,0,0)
        [HideInInspector]  _MaskMapRemapScale1("MaskMapRemapScale  1", Vector) = (1,1,1,1)
        
        [HideInInspector]  _DiffuseRemapScale2("DiffuseRemapScale  2", Vector) = (1,1,1,1)
        [HideInInspector] _MaskMapRemapOffset2("MaskMapRemapOffset 2", Vector) = (0,0,0,0)
        [HideInInspector]  _MaskMapRemapScale2("MaskMapRemapScale  2", Vector) = (1,1,1,1)
        
        [HideInInspector]  _DiffuseRemapScale3("DiffuseRemapScale  3", Vector) = (1,1,1,1)
        [HideInInspector] _MaskMapRemapOffset3("MaskMapRemapOffset 3", Vector) = (0,0,0,0)
        [HideInInspector]  _MaskMapRemapScale3("MaskMapRemapScale  3", Vector) = (1,1,1,1)

        // used in fallback on old cards & base map
        [HideInInspector] _MainTex("BaseMap (RGB)", 2D) = "grey" {}
        [HideInInspector] _BaseColor("Main Color", Color) = (1,1,1,1)

        _NormalScale0("NormalScale 0", float) = 1.0
        _NormalScale1("NormalScale 1", float) = 1.0
        _NormalScale2("NormalScale 2", float) = 1.0
        _NormalScale3("NormalScale 3", float) = 1.0
    }

    SubShader
    {
        Tags { "Queue" = "Geometry-99" "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" "DisableBatching" = "True" }

        Pass
        {
            Name "TerrainAddLit"
            Tags { "LightMode" = "UniversalForward" }
            Blend One One
            HLSLPROGRAM
            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 3.0

            #pragma vertex SplatmapVert1
            #pragma fragment SplatmapFragment

            // -------------------------------------
            // Universal Pipeline keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile_fog

            #pragma shader_feature_local _TERRAIN_BLEND_HEIGHT
            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local _MASKMAP
            
            #define TERRAIN_SPLAT_ADDPASS

            #include "DiggerURPMeshLitInput.hlsl"
            #include "DiggerURPMeshLitPasses.hlsl"
            
            ENDHLSL
        }
    }

    Fallback "Hidden/Universal Render Pipeline/FallbackError"
}
