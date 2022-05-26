#ifndef UNIVERSAL_MESH_LIT_PASSES_INCLUDED
#define UNIVERSAL_MESH_LIT_PASSES_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"


struct Attributes
{
    float4 positionOS : POSITION;
    half3 normalOS : NORMAL;
    float2 texcoord : TEXCOORD0;
    half4 color        : COLOR;
    half4 texcoord1    : TEXCOORD1;
    half4 texcoord2    : TEXCOORD2;
    half4 texcoord3    : TEXCOORD3;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float4 uvMainAndLM              : TEXCOORD0; // xy: control, zw: lightmap

    half4 splatControl              : TEXCOORD1;

    half4 normal                   : TEXCOORD3;    // xyz: normal, w: viewDir.x
    half4 tangent                  : TEXCOORD4;    // xyz: tangent, w: viewDir.y
    half4 bitangent                : TEXCOORD5;    // xyz: bitangent, w: viewDir.z

    half4 fogFactorAndVertexLight   : TEXCOORD6; // x: fogFactor, yzw: vertex light
    float3 positionWS               : TEXCOORD7;
    float4 shadowCoord              : TEXCOORD8;
    float4 clipPos                  : SV_POSITION;
    UNITY_VERTEX_OUTPUT_STEREO
};

void InitializeInputData(Varyings IN, half3 normalTS, out InputData input)
{
    input = (InputData)0;

    input.positionWS = IN.positionWS;
    half3 SH = half3(0, 0, 0);

    half3 viewDirWS = half3(IN.normal.w, IN.tangent.w, IN.bitangent.w);
    input.normalWS = TransformTangentToWorld(normalTS, half3x3(-IN.tangent.xyz, IN.bitangent.xyz, IN.normal.xyz));
    SH = SampleSH(input.normalWS.xyz);

#if SHADER_HINT_NICE_QUALITY
    viewDirWS = SafeNormalize(viewDirWS);
#endif

    input.normalWS = NormalizeNormalPerPixel(input.normalWS);

    input.viewDirectionWS = viewDirWS;

#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    input.shadowCoord = IN.shadowCoord;
#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
    input.shadowCoord = TransformWorldToShadowCoord(input.positionWS);
#else
    input.shadowCoord = float4(0, 0, 0, 0);
#endif

    input.fogCoord = IN.fogFactorAndVertexLight.x;
    input.vertexLighting = IN.fogFactorAndVertexLight.yzw;

    input.bakedGI = SAMPLE_GI(IN.uvMainAndLM.zw, SH, input.normalWS);
}

#ifndef TERRAIN_SPLAT_BASEPASS

half4 SampleTriplanar(float3 positionWS, half3 normal, Texture2D tex, SamplerState samp, half2 tile, half2 offset) {
    float2 y0 = positionWS.zy * tile + offset;
    float2 x0 = positionWS.xz * tile + offset;
    float2 z0 = positionWS.xy * tile + offset;
    half4 cX0 = SAMPLE_TEXTURE2D(tex, samp, y0);
    half4 cY0 = SAMPLE_TEXTURE2D(tex, samp, x0);
    half4 cZ0 = SAMPLE_TEXTURE2D(tex, samp, z0);
    half4 side0 = lerp(cX0, cZ0, abs(normal.z));
    return lerp(side0, cY0, abs(normal.y));    
}

void SplatmapMix(float3 positionWS, float3 normal, inout half4 splatControl, out half weight, out half4 mixedDiffuse, out half4 defaultSmoothness, inout half3 mixedNormal)
{
    half4 diffAlbedo[4];

    diffAlbedo[0] = SampleTriplanar(positionWS, normal, _Splat0, sampler_Splat0, _Splat0_ST.xy, _Splat0_ST.zw);
    diffAlbedo[1] = SampleTriplanar(positionWS, normal, _Splat1, sampler_Splat0, _Splat1_ST.xy, _Splat1_ST.zw);
    diffAlbedo[2] = SampleTriplanar(positionWS, normal, _Splat2, sampler_Splat0, _Splat2_ST.xy, _Splat2_ST.zw);
    diffAlbedo[3] = SampleTriplanar(positionWS, normal, _Splat3, sampler_Splat0, _Splat3_ST.xy, _Splat3_ST.zw);

    // This might be a bit of a gamble -- the assumption here is that if the diffuseMap has no
    // alpha channel, then diffAlbedo[n].a = 1.0 (and _DiffuseHasAlphaN = 0.0)
    // Prior to coming in, _SmoothnessN is actually set to max(_DiffuseHasAlphaN, _SmoothnessN)
    // This means that if we have an alpha channel, _SmoothnessN is locked to 1.0 and
    // otherwise, the true slider value is passed down and diffAlbedo[n].a == 1.0.
    defaultSmoothness = half4(diffAlbedo[0].a, diffAlbedo[1].a, diffAlbedo[2].a, diffAlbedo[3].a);
    defaultSmoothness *= half4(_Smoothness0, _Smoothness1, _Smoothness2, _Smoothness3);

#ifndef _TERRAIN_BLEND_HEIGHT // density blending
    if(_NumLayersCount <= 4)
    {
        // 20.0 is the number of steps in inputAlphaMask (Density mask. We decided 20 empirically)
        half4 opacityAsDensity = saturate((half4(diffAlbedo[0].a, diffAlbedo[1].a, diffAlbedo[2].a, diffAlbedo[3].a) - (1 - splatControl)) * 20.0);
        opacityAsDensity += 0.001h * splatControl;      // if all weights are zero, default to what the blend mask says
        half4 useOpacityAsDensityParam = { _DiffuseRemapScale0.w, _DiffuseRemapScale1.w, _DiffuseRemapScale2.w, _DiffuseRemapScale3.w }; // 1 is off
        splatControl = lerp(opacityAsDensity, splatControl, useOpacityAsDensityParam);
    }
#endif

    // Now that splatControl has changed, we can compute the final weight and normalize
    weight = dot(splatControl, 1.0h);
    
#ifdef TERRAIN_SPLAT_ADDPASS
    clip(weight <= 0.005h ? -1.0h : 1.0h);
#endif

#ifndef _TERRAIN_BASEMAP_GEN
    // Normalize weights before lighting and restore weights in final modifier functions so that the overal
    // lighting result can be correctly weighted.
    splatControl /= (weight + HALF_MIN);
#endif

    mixedDiffuse = 0.0h;
    mixedDiffuse += diffAlbedo[0] * half4(_DiffuseRemapScale0.rgb * splatControl.rrr, 1.0h);
    mixedDiffuse += diffAlbedo[1] * half4(_DiffuseRemapScale1.rgb * splatControl.ggg, 1.0h);
    mixedDiffuse += diffAlbedo[2] * half4(_DiffuseRemapScale2.rgb * splatControl.bbb, 1.0h);
    mixedDiffuse += diffAlbedo[3] * half4(_DiffuseRemapScale3.rgb * splatControl.aaa, 1.0h);

#ifdef _NORMALMAP
    half3 nrm = 0.0f;
    nrm += splatControl.r * UnpackNormalScale(SampleTriplanar(positionWS, normal, _Normal0, sampler_Normal0, _Splat0_ST.xy, _Splat0_ST.zw), _NormalScale0);
    nrm += splatControl.g * UnpackNormalScale(SampleTriplanar(positionWS, normal, _Normal1, sampler_Normal0, _Splat1_ST.xy, _Splat1_ST.zw), _NormalScale1);
    nrm += splatControl.b * UnpackNormalScale(SampleTriplanar(positionWS, normal, _Normal2, sampler_Normal0, _Splat2_ST.xy, _Splat2_ST.zw), _NormalScale2);
    nrm += splatControl.a * UnpackNormalScale(SampleTriplanar(positionWS, normal, _Normal3, sampler_Normal0, _Splat3_ST.xy, _Splat3_ST.zw), _NormalScale3);

    // avoid risk of NaN when normalizing.
#if HAS_HALF
    nrm.z += 0.01h;     
#else
    nrm.z += 1e-5f;
#endif

    mixedNormal = normalize(nrm.xyz);
#endif
}

#endif

#ifdef _TERRAIN_BLEND_HEIGHT
void HeightBasedSplatModify(inout half4 splatControl, in half4 masks[4])
{
    // heights are in mask blue channel, we multiply by the splat Control weights to get combined height
    half4 splatHeight = half4(masks[0].b, masks[1].b, masks[2].b, masks[3].b) * splatControl.rgba;
    half maxHeight = max(splatHeight.r, max(splatHeight.g, max(splatHeight.b, splatHeight.a)));

    // Ensure that the transition height is not zero.
    half transition = max(_HeightTransition, 1e-5);

    // This sets the highest splat to "transition", and everything else to a lower value relative to that, clamping to zero
    // Then we clamp this to zero and normalize everything
    half4 weightedHeights = splatHeight + transition - maxHeight.xxxx;
    weightedHeights = max(0, weightedHeights);

    // We need to add an epsilon here for active layers (hence the blendMask again)
    // so that at least a layer shows up if everything's too low.
    weightedHeights = (weightedHeights + 1e-6) * splatControl;

    // Normalize (and clamp to epsilon to keep from dividing by zero)
    half sumHeight = max(dot(weightedHeights, half4(1, 1, 1, 1)), 1e-6);
    splatControl = weightedHeights / sumHeight.xxxx;
}
#endif

void SplatmapFinalColor(inout half4 color, half fogCoord)
{
    color.rgb *= color.a;
    #ifdef TERRAIN_SPLAT_ADDPASS
        color.rgb = MixFogColor(color.rgb, half3(0,0,0), fogCoord);
    #else
        color.rgb = MixFog(color.rgb, fogCoord);
    #endif
}


///////////////////////////////////////////////////////////////////////////////
//                  Vertex and Fragment functions                            //
///////////////////////////////////////////////////////////////////////////////

// Used in Standard Terrain shader
Varyings GenericSplatmapVert(Attributes v, float4 control)
{
    Varyings o = (Varyings)0;

    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
    VertexPositionInputs Attributes = GetVertexPositionInputs(v.positionOS.xyz);

    o.splatControl = control;
    o.uvMainAndLM.xy = v.texcoord;
    o.uvMainAndLM.zw = v.texcoord;// * unity_LightmapST.xy + unity_LightmapST.zw;

    half3 viewDirWS = GetCameraPositionWS() - Attributes.positionWS;
#if !SHADER_HINT_NICE_QUALITY
    viewDirWS = SafeNormalize(viewDirWS);
#endif

    float4 vertexTangent = float4(cross(float3(0, 0, 1), v.normalOS), 1.0);
    VertexNormalInputs normalInput = GetVertexNormalInputs(v.normalOS, vertexTangent);

    o.normal = half4(normalInput.normalWS, viewDirWS.x);
    o.tangent = half4(normalInput.tangentWS, viewDirWS.y);
    o.bitangent = half4(normalInput.bitangentWS, viewDirWS.z);

    o.fogFactorAndVertexLight.x = ComputeFogFactor(Attributes.positionCS.z);
    o.fogFactorAndVertexLight.yzw = VertexLighting(Attributes.positionWS, o.normal.xyz);
    o.positionWS = Attributes.positionWS;
    o.clipPos = Attributes.positionCS;

#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    o.shadowCoord = GetShadowCoord(Attributes);
#endif

    return o;
}

Varyings SplatmapVert0(Attributes v)
{
    return GenericSplatmapVert(v, v.color);
}
Varyings SplatmapVert1(Attributes v)
{
    return GenericSplatmapVert(v, v.texcoord1);
}
Varyings SplatmapVert2(Attributes v)
{
    return GenericSplatmapVert(v, v.texcoord2);
}
Varyings SplatmapVert3(Attributes v)
{
    return GenericSplatmapVert(v, v.texcoord3);
}

void ComputeMasks(out half4 masks[4], half4 hasMask, Varyings IN)
{
    masks[0] = 0.5h;
    masks[1] = 0.5h;
    masks[2] = 0.5h;
    masks[3] = 0.5h;

#ifdef _MASKMAP
    masks[0] = lerp(masks[0], SampleTriplanar(IN.positionWS, IN.normal.xyz, _Mask0, sampler_Mask0, _Splat0_ST.xy, _Splat0_ST.zw), hasMask.x);
    masks[1] = lerp(masks[1], SampleTriplanar(IN.positionWS, IN.normal.xyz, _Mask1, sampler_Mask0, _Splat0_ST.xy, _Splat0_ST.zw), hasMask.y);
    masks[2] = lerp(masks[2], SampleTriplanar(IN.positionWS, IN.normal.xyz, _Mask2, sampler_Mask0, _Splat0_ST.xy, _Splat0_ST.zw), hasMask.z);
    masks[3] = lerp(masks[3], SampleTriplanar(IN.positionWS, IN.normal.xyz, _Mask3, sampler_Mask0, _Splat0_ST.xy, _Splat0_ST.zw), hasMask.w);
#endif

    masks[0] *= _MaskMapRemapScale0.rgba;
    masks[0] += _MaskMapRemapOffset0.rgba;
    masks[1] *= _MaskMapRemapScale1.rgba;
    masks[1] += _MaskMapRemapOffset1.rgba;
    masks[2] *= _MaskMapRemapScale2.rgba;
    masks[2] += _MaskMapRemapOffset2.rgba;
    masks[3] *= _MaskMapRemapScale3.rgba;
    masks[3] += _MaskMapRemapOffset3.rgba;
}

// Used in Standard Terrain shader
half4 SplatmapFragment(Varyings IN) : SV_TARGET
{
    half3 normalTS = half3(0.0h, 0.0h, 1.0h);

    half4 hasMask = half4(_LayerHasMask0, _LayerHasMask1, _LayerHasMask2, _LayerHasMask3);
    half4 masks[4];
    ComputeMasks(masks, hasMask, IN);

    half4 splatControl = IN.splatControl;

#ifdef _TERRAIN_BLEND_HEIGHT
    // disable Height Based blend when there are more than 4 layers (multi-pass breaks the normalization)
    if (_NumLayersCount <= 4)
        HeightBasedSplatModify(splatControl, masks);
#endif

    half weight;
    half4 mixedDiffuse;
    half4 defaultSmoothness;
    SplatmapMix(IN.positionWS, IN.normal.xyz, splatControl, weight, mixedDiffuse, defaultSmoothness, normalTS);
    half3 albedo = mixedDiffuse.rgb;

    half4 defaultMetallic = half4(_Metallic0, _Metallic1, _Metallic2, _Metallic3);
    half4 defaultOcclusion = half4(_MaskMapRemapScale0.g, _MaskMapRemapScale1.g, _MaskMapRemapScale2.g, _MaskMapRemapScale3.g) +
                            half4(_MaskMapRemapOffset0.g, _MaskMapRemapOffset1.g, _MaskMapRemapOffset2.g, _MaskMapRemapOffset3.g);

    half4 maskSmoothness = half4(masks[0].a, masks[1].a, masks[2].a, masks[3].a);
    defaultSmoothness = lerp(defaultSmoothness, maskSmoothness, hasMask);
    half smoothness = dot(splatControl, defaultSmoothness);

    half4 maskMetallic = half4(masks[0].r, masks[1].r, masks[2].r, masks[3].r);
    defaultMetallic = lerp(defaultMetallic, maskMetallic, hasMask);
    half metallic = dot(splatControl, defaultMetallic);

    half4 maskOcclusion = half4(masks[0].g, masks[1].g, masks[2].g, masks[3].g);
    defaultOcclusion = lerp(defaultOcclusion, maskOcclusion, hasMask);
    half occlusion = dot(splatControl, defaultOcclusion);

    half alpha = weight;

    InputData inputData;
    InitializeInputData(IN, normalTS, inputData);
    half4 color = UniversalFragmentPBR(inputData, albedo, metallic, /* specular */ half3(0.0h, 0.0h, 0.0h), smoothness, occlusion, /* emission */ half3(0, 0, 0), alpha);

    SplatmapFinalColor(color, inputData.fogCoord);

    return half4(color.rgb, 1.0h);
}

// Shadow pass

// x: global clip space bias, y: normal world space bias
float3 _LightDirection;

struct AttributesLean
{
    float4 position     : POSITION;
    float3 normalOS       : NORMAL;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct VaryingsLean
{
    float4 clipPos      : SV_POSITION;
    UNITY_VERTEX_OUTPUT_STEREO
};

VaryingsLean ShadowPassVertex(AttributesLean v)
{
    VaryingsLean o = (VaryingsLean)0;

    float3 positionWS = TransformObjectToWorld(v.position.xyz);
    float3 normalWS = TransformObjectToWorldNormal(v.normalOS);

    float4 clipPos = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, _LightDirection));

#if UNITY_REVERSED_Z
    clipPos.z = min(clipPos.z, clipPos.w * UNITY_NEAR_CLIP_VALUE);
#else
    clipPos.z = max(clipPos.z, clipPos.w * UNITY_NEAR_CLIP_VALUE);
#endif

    o.clipPos = clipPos;

    return o;
}

half4 ShadowPassFragment(VaryingsLean IN) : SV_TARGET
{
    return 0;
}

// Depth pass

VaryingsLean DepthOnlyVertex(AttributesLean v)
{
    VaryingsLean o = (VaryingsLean)0;
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
    o.clipPos = TransformObjectToHClip(v.position.xyz);
    return o;
}

half4 DepthOnlyFragment(VaryingsLean IN) : SV_TARGET
{
    return 0;
}

#endif
