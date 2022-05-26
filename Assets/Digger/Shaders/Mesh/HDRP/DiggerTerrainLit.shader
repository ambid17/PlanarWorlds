Shader "Digger/HDRP/Mesh/Lit"
{
    Properties
    {
         _Splat0("Layer 0", 2D) = "grey" {}
         _Splat1("Layer 1", 2D) = "grey" {}
         _Splat2("Layer 2", 2D) = "grey" {}
         _Splat3("Layer 3", 2D) = "grey" {}
         _Splat4("Layer 4", 2D) = "grey" {}
         _Splat5("Layer 5", 2D) = "grey" {}
         _Splat6("Layer 6", 2D) = "grey" {}
         _Splat7("Layer 7", 2D) = "grey" {}
         
         _Normal0("Normal 0", 2D) = "bump" {}
         _Normal1("Normal 1", 2D) = "bump" {}
         _Normal2("Normal 2", 2D) = "bump" {}
         _Normal3("Normal 3", 2D) = "bump" {}
         _Normal4("Normal 4", 2D) = "bump" {}
         _Normal5("Normal 5", 2D) = "bump" {}
         _Normal6("Normal 6", 2D) = "bump" {}
         _Normal7("Normal 7", 2D) = "bump" {}
         
         _Mask0("Mask 0", 2D) = "clear" {}
         _Mask1("Mask 1", 2D) = "clear" {}
         _Mask2("Mask 2", 2D) = "clear" {}
         _Mask3("Mask 3", 2D) = "clear" {}
         _Mask4("Mask 4", 2D) = "clear" {}
         _Mask5("Mask 5", 2D) = "clear" {}
         _Mask6("Mask 6", 2D) = "clear" {}
         _Mask7("Mask 7", 2D) = "clear" {}
         
        [Gamma] _Metallic0("Metallic 0", Range(0.0, 1.0)) = 0.0
        [Gamma] _Metallic1("Metallic 1", Range(0.0, 1.0)) = 0.0
        [Gamma] _Metallic2("Metallic 2", Range(0.0, 1.0)) = 0.0
        [Gamma] _Metallic3("Metallic 3", Range(0.0, 1.0)) = 0.0
        [Gamma] _Metallic4("Metallic 4", Range(0.0, 1.0)) = 0.0
        [Gamma] _Metallic5("Metallic 5", Range(0.0, 1.0)) = 0.0
        [Gamma] _Metallic6("Metallic 6", Range(0.0, 1.0)) = 0.0
        [Gamma] _Metallic7("Metallic 7", Range(0.0, 1.0)) = 0.0
        
         _Smoothness0("Smoothness 0", Range(0.0, 1.0)) = 0.0
         _Smoothness1("Smoothness 1", Range(0.0, 1.0)) = 0.0
         _Smoothness2("Smoothness 2", Range(0.0, 1.0)) = 0.0
         _Smoothness3("Smoothness 3", Range(0.0, 1.0)) = 0.0
         _Smoothness4("Smoothness 4", Range(0.0, 1.0)) = 0.0
         _Smoothness5("Smoothness 5", Range(0.0, 1.0)) = 0.0
         _Smoothness6("Smoothness 6", Range(0.0, 1.0)) = 0.0
         _Smoothness7("Smoothness 7", Range(0.0, 1.0)) = 0.0
         
         _NormalScale0("NormalScale 0", float) = 1.0
         _NormalScale1("NormalScale 1", float) = 1.0
         _NormalScale2("NormalScale 2", float) = 1.0
         _NormalScale3("NormalScale 3", float) = 1.0
         _NormalScale4("NormalScale 4", float) = 1.0
         _NormalScale5("NormalScale 5", float) = 1.0
         _NormalScale6("NormalScale 6", float) = 1.0
         _NormalScale7("NormalScale 7", float) = 1.0
         
         _LayerHasMask0("LayerHasMask 0", float) = 0.0
         _LayerHasMask1("LayerHasMask 1", float) = 0.0
         _LayerHasMask2("LayerHasMask 2", float) = 0.0
         _LayerHasMask3("LayerHasMask 3", float) = 0.0
         _LayerHasMask4("LayerHasMask 4", float) = 0.0
         _LayerHasMask5("LayerHasMask 5", float) = 0.0
         _LayerHasMask6("LayerHasMask 6", float) = 0.0
         _LayerHasMask7("LayerHasMask 7", float) = 0.0
         
         _MaskMapRemapOffset0("MaskMapRemapOffset 0", Vector) = (0,0,0,0)
         _MaskMapRemapOffset1("MaskMapRemapOffset 1", Vector) = (0,0,0,0)
         _MaskMapRemapOffset2("MaskMapRemapOffset 2", Vector) = (0,0,0,0)
         _MaskMapRemapOffset3("MaskMapRemapOffset 3", Vector) = (0,0,0,0)
         _MaskMapRemapOffset4("MaskMapRemapOffset 4", Vector) = (0,0,0,0)
         _MaskMapRemapOffset5("MaskMapRemapOffset 5", Vector) = (0,0,0,0)
         _MaskMapRemapOffset6("MaskMapRemapOffset 6", Vector) = (0,0,0,0)
         _MaskMapRemapOffset7("MaskMapRemapOffset 7", Vector) = (0,0,0,0)
         
         _MaskMapRemapScale0("MaskMapRemapScale 0", Vector) = (1,1,1,1)
         _MaskMapRemapScale1("MaskMapRemapScale 1", Vector) = (1,1,1,1)
         _MaskMapRemapScale2("MaskMapRemapScale 2", Vector) = (1,1,1,1)
         _MaskMapRemapScale3("MaskMapRemapScale 3", Vector) = (1,1,1,1)
         _MaskMapRemapScale4("MaskMapRemapScale 4", Vector) = (1,1,1,1)
         _MaskMapRemapScale5("MaskMapRemapScale 5", Vector) = (1,1,1,1)
         _MaskMapRemapScale6("MaskMapRemapScale 6", Vector) = (1,1,1,1)
         _MaskMapRemapScale7("MaskMapRemapScale 7", Vector) = (1,1,1,1)
         
         _DiffuseRemapScale0("DiffuseRemapScale 0", Vector) = (1,1,1,1)
         _DiffuseRemapScale1("DiffuseRemapScale 1", Vector) = (1,1,1,1)
         _DiffuseRemapScale2("DiffuseRemapScale 2", Vector) = (1,1,1,1)
         _DiffuseRemapScale3("DiffuseRemapScale 3", Vector) = (1,1,1,1)
         _DiffuseRemapScale4("DiffuseRemapScale 4", Vector) = (1,1,1,1)
         _DiffuseRemapScale5("DiffuseRemapScale 5", Vector) = (1,1,1,1)
         _DiffuseRemapScale6("DiffuseRemapScale 6", Vector) = (1,1,1,1)
         _DiffuseRemapScale7("DiffuseRemapScale 7", Vector) = (1,1,1,1)
        
        [HideInInspector] [ToggleUI] _EnableHeightBlend("EnableHeightBlend", Float) = 0.0
        _HeightTransition("Height Transition", Range(0, 1.0)) = 0.0
        [HideInInspector] [Enum(Off, 0, From Ambient Occlusion, 1)]  _SpecularOcclusionMode("Specular Occlusion Mode", Int) = 1

         // Following are builtin properties

        // Stencil state
        // Forward
        [HideInInspector] _StencilRef("_StencilRef", Int) = 0  // StencilUsage.Clear
        [HideInInspector] _StencilWriteMask("_StencilWriteMask", Int) = 3 // StencilUsage.RequiresDeferredLighting | StencilUsage.SubsurfaceScattering
        // GBuffer
        [HideInInspector] _StencilRefGBuffer("_StencilRefGBuffer", Int) = 2 // StencilUsage.RequiresDeferredLighting
        [HideInInspector] _StencilWriteMaskGBuffer("_StencilWriteMaskGBuffer", Int) = 3 // StencilUsage.RequiresDeferredLighting | StencilUsage.SubsurfaceScattering
        // Depth prepass
        [HideInInspector] _StencilRefDepth("_StencilRefDepth", Int) = 0 // Nothing
        [HideInInspector] _StencilWriteMaskDepth("_StencilWriteMaskDepth", Int) = 8 // StencilUsage.TraceReflectionRay

        // Blending state
        [HideInInspector] _ZWrite ("__zw", Float) = 1.0
        [HideInInspector][ToggleUI] _TransparentZWrite("_TransparentZWrite", Float) = 0.0
        [HideInInspector] _CullMode("__cullmode", Float) = 2.0
        [HideInInspector] _ZTestDepthEqualForOpaque("_ZTestDepthEqualForOpaque", Int) = 4 // Less equal
        [HideInInspector] _ZTestGBuffer("_ZTestGBuffer", Int) = 4

        [ToggleUI] _EnableInstancedPerPixelNormal("Instanced per pixel normal", Float) = 1.0

		[HideInInspector] _TerrainHolesTexture("Holes Map (RGB)", 2D) = "white" {}

        // Caution: C# code in BaseLitUI.cs call LightmapEmissionFlagsProperty() which assume that there is an existing "_EmissionColor"
        // value that exist to identify if the GI emission need to be enabled.
        // In our case we don't use such a mechanism but need to keep the code quiet. We declare the value and always enable it.
        // TODO: Fix the code in legacy unity so we can customize the behavior for GI
        [HideInInspector] _EmissionColor("Color", Color) = (1, 1, 1)

        // HACK: GI Baking system relies on some properties existing in the shader ("_MainTex", "_Cutoff" and "_Color") for opacity handling, so we need to store our version of those parameters in the hard-coded name the GI baking system recognizes.
        [HideInInspector] _MainTex("Albedo", 2D) = "white" {}
        [HideInInspector] _Color("Color", Color) = (1,1,1,1)

        [HideInInspector] [ToggleUI] _SupportDecals("Support Decals", Float) = 1.0
        [HideInInspector] [ToggleUI] _ReceivesSSR("Receives SSR", Float) = 1.0
        [HideInInspector] [ToggleUI] _AddPrecomputedVelocity("AddPrecomputedVelocity", Float) = 0.0

    }

    HLSLINCLUDE

    #pragma target 4.5
    #pragma only_renderers d3d11 playstation xboxone xboxseries vulkan metal switch

    // Terrain builtin keywords
    #define _TERRAIN_8_LAYERS
	#define _NORMALMAP
    #define _MASKMAP
    #pragma shader_feature_local _SPECULAR_OCCLUSION_NONE

    #pragma shader_feature_local _TERRAIN_BLEND_HEIGHT
    // Sample normal in pixel shader when doing instancing
    //#pragma shader_feature_local _TERRAIN_INSTANCED_PERPIXEL_NORMAL

    //#pragma shader_feature _ _LAYER_MAPPING_PLANAR0 _LAYER_MAPPING_TRIPLANAR0
    //#pragma shader_feature _ _LAYER_MAPPING_PLANAR1 _LAYER_MAPPING_TRIPLANAR1
    //#pragma shader_feature _ _LAYER_MAPPING_PLANAR2 _LAYER_MAPPING_TRIPLANAR2
    //#pragma shader_feature _ _LAYER_MAPPING_PLANAR3 _LAYER_MAPPING_TRIPLANAR3

    #pragma shader_feature_local _DISABLE_DECALS
    #pragma shader_feature_local _ADD_PRECOMPUTED_VELOCITY

    //enable GPU instancing support
    #pragma multi_compile_instancing
    #pragma instancing_options assumeuniformscaling nomatrices nolightprobe nolightmap

	// #pragma multi_compile _ _ALPHATEST_ON
	
    #define ATTRIBUTES_NEED_COLOR
	#define VARYINGS_NEED_COLOR
	#define ATTRIBUTES_NEED_TEXCOORD2
	#define VARYINGS_NEED_TEXCOORD2
	#define ATTRIBUTES_NEED_TEXCOORD3
	#define VARYINGS_NEED_TEXCOORD3

    // All our shaders use same name for entry point
    #pragma vertex Vert
    #pragma fragment Frag

    // Define _DEFERRED_CAPABLE_MATERIAL for shader capable to run in deferred pass
    #define _DEFERRED_CAPABLE_MATERIAL

    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/TerrainLit/TerrainLit_Splatmap_Includes.hlsl"

    ENDHLSL

    SubShader
    {
        // This tags allow to use the shader replacement features
        Tags
        {
            "RenderPipeline" = "HDRenderPipeline"
            "RenderType" = "Opaque"
            "SplatCount" = "8"
            "MaskMapR" = "Metallic"
            "MaskMapG" = "AO"
            "MaskMapB" = "Height"
            "MaskMapA" = "Smoothness"
            "DiffuseA" = "Smoothness (becomes Density when Mask map is assigned)"   // when MaskMap is disabled
            "DiffuseA_MaskMapUsed" = "Density"                                      // when MaskMap is enabled
            "DisableBatching" = "True"
        }

        // Caution: The outline selection in the editor use the vertex shader/hull/domain shader of the first pass declare. So it should not bethe  meta pass.
        Pass
        {
            Name "GBuffer"
            Tags { "LightMode" = "GBuffer" } // This will be only for opaque object based on the RenderQueue index

            Cull [_CullMode]
            ZTest [_ZTestGBuffer]

            Stencil
            {
                WriteMask [_StencilWriteMaskGBuffer]
                Ref [_StencilRefGBuffer]
                Comp Always
                Pass Replace
            }

            HLSLPROGRAM

            #pragma multi_compile _ DEBUG_DISPLAY
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ DYNAMICLIGHTMAP_ON
            #pragma multi_compile_fragment _ SHADOWS_SHADOWMASK
            #pragma multi_compile_fragment PROBE_VOLUMES_OFF PROBE_VOLUMES_L1 PROBE_VOLUMES_L2
            // Setup DECALS_OFF so the shader stripper can remove variants
            #pragma multi_compile_fragment DECALS_OFF DECALS_3RT DECALS_4RT
            #pragma multi_compile_fragment _ DECAL_SURFACE_GRADIENT
            #pragma multi_compile_fragment _ LIGHT_LAYERS

            #define SHADERPASS SHADERPASS_GBUFFER
            #include "DiggerTerrainLitTemplate.hlsl"
            #include "DiggerTerrainLit_Splatmap.hlsl"

            ENDHLSL
        }

        // Extracts information for lightmapping, GI (emission, albedo, ...)
        // This pass it not used during regular rendering.
        Pass
        {
            Name "META"
            Tags{ "LightMode" = "META" }

            Cull Off

            HLSLPROGRAM

            // Lightmap memo
            // DYNAMICLIGHTMAP_ON is used when we have an "enlighten lightmap" ie a lightmap updated at runtime by enlighten.This lightmap contain indirect lighting from realtime lights and realtime emissive material.Offline baked lighting(from baked material / light,
            // both direct and indirect lighting) will hand up in the "regular" lightmap->LIGHTMAP_ON.

            #define SHADERPASS SHADERPASS_LIGHT_TRANSPORT
            #include "DiggerTerrainLitTemplate.hlsl"
            #include "DiggerTerrainLit_Splatmap.hlsl"

            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags{ "LightMode" = "ShadowCaster" }

            Cull[_CullMode]

            ZClip [_ZClip]
            ZWrite On
            ZTest LEqual

            ColorMask 0

            HLSLPROGRAM

            #define SHADERPASS SHADERPASS_SHADOWS
            #include "DiggerTerrainLitTemplate.hlsl"
            #include "DiggerTerrainLit_Splatmap.hlsl"

            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags{ "LightMode" = "DepthOnly" }

            Cull[_CullMode]

            // To be able to tag stencil with disableSSR information for forward
            Stencil
            {
                WriteMask [_StencilWriteMaskDepth]
                Ref [_StencilRefDepth]
                Comp Always
                Pass Replace
            }

            ZWrite On

            HLSLPROGRAM

            // In deferred, depth only pass don't output anything.
            // In forward it output the normal buffer
            #pragma multi_compile _ WRITE_NORMAL_BUFFER
            #pragma multi_compile _ WRITE_DECAL_BUFFER
            #pragma multi_compile _ WRITE_MSAA_DEPTH

            #define SHADERPASS SHADERPASS_DEPTH_ONLY
            #include "DiggerTerrainLitTemplate.hlsl"
            #ifdef WRITE_NORMAL_BUFFER
                #if defined(_NORMALMAP)
                    #define OVERRIDE_SPLAT_SAMPLER_NAME sampler_Normal0
                #elif defined(_MASKMAP)
                    #define OVERRIDE_SPLAT_SAMPLER_NAME sampler_Mask0
                #endif
            #endif
            #include "DiggerTerrainLit_Splatmap.hlsl"

            ENDHLSL
        }

        Pass
        {
            Name "Forward"
            Tags { "LightMode" = "Forward" } // This will be only for transparent object based on the RenderQueue index

            Stencil
            {
                WriteMask [_StencilWriteMask]
                Ref [_StencilRef]
                Comp Always
                Pass Replace
            }

            // In case of forward we want to have depth equal for opaque mesh
            ZTest [_ZTestDepthEqualForOpaque]
            ZWrite [_ZWrite]
            Cull [_CullMode]

            HLSLPROGRAM

            #pragma multi_compile _ DEBUG_DISPLAY
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ DYNAMICLIGHTMAP_ON
            #pragma multi_compile_fragment _ SHADOWS_SHADOWMASK
            #pragma multi_compile_fragment PROBE_VOLUMES_OFF PROBE_VOLUMES_L1 PROBE_VOLUMES_L2
            #pragma multi_compile_fragment SCREEN_SPACE_SHADOWS_OFF SCREEN_SPACE_SHADOWS_ON
            // Setup DECALS_OFF so the shader stripper can remove variants
            #pragma multi_compile_fragment DECALS_OFF DECALS_3RT DECALS_4RT
            #pragma multi_compile_fragment _ DECAL_SURFACE_GRADIENT

            // Supported shadow modes per light type
            #pragma multi_compile_fragment SHADOW_LOW SHADOW_MEDIUM SHADOW_HIGH SHADOW_VERY_HIGH

            #pragma multi_compile USE_FPTL_LIGHTLIST USE_CLUSTERED_LIGHTLIST

            #define SHADERPASS SHADERPASS_FORWARD
            #include "DiggerTerrainLitTemplate.hlsl"
            #include "DiggerTerrainLit_Splatmap.hlsl"

            ENDHLSL
        }
    }

    Dependency "BaseMapShader" = "Hidden/HDRP/TerrainLit_Basemap"
    Dependency "BaseMapGenShader" = "Hidden/HDRP/TerrainLit_BasemapGen"
    CustomEditor "UnityEditor.Rendering.HighDefinition.TerrainLitGUI"
}
