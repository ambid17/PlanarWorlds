#ifndef MESH_TRIPLANAR_STANDARD_TXTARRAY_INCLUDED
#define MESH_TRIPLANAR_STANDARD_TXTARRAY_INCLUDED

struct Input
{
	half4 splat_control03;
	half4 splat_control47;
	UNITY_FOG_COORDS(5)
	float3 worldPos;
	float3 vertNormal;
};

float _tiles0x;
float _tiles0y;
float _tiles1x;
float _tiles1y;
float _tiles2x;
float _tiles2y;
float _tiles3x;
float _tiles3y;
float _tiles4x;
float _tiles4y;
float _tiles5x;
float _tiles5y;
float _tiles6x;
float _tiles6y;
float _tiles7x;
float _tiles7y;

float _offset0x;
float _offset0y;
float _offset1x;
float _offset1y;
float _offset2x;
float _offset2y;
float _offset3x;
float _offset3y;
float _offset4x;
float _offset4y;
float _offset5x;
float _offset5y;
float _offset6x;
float _offset6y;
float _offset7x;
float _offset7y;

float _NormalScale0;
float _NormalScale1;
float _NormalScale2;
float _NormalScale3;
float _NormalScale4;
float _NormalScale5;
float _NormalScale6;
float _NormalScale7;

UNITY_DECLARE_TEX2DARRAY(_SplatArray);
UNITY_DECLARE_TEX2DARRAY(_NormalArray);

fixed3 normal;
float3 worldPos;

void SplatmapMix(Input IN, 
                half4 defaultAlpha03, 
                half4 defaultAlpha47, 
                half4 splat_control03, 
                half4 splat_control47, 
                out fixed4 mixedDiffuse, 
                inout fixed3 mixedNormal)
{
	worldPos = IN.worldPos;
	normal = abs(IN.vertNormal);
	
	mixedDiffuse = 0.0f;
	
    float y = normal.y;
    float z = normal.z;
    
    float2 tile = float2(_tiles0x, _tiles0y);
    float2 offset = float2(_offset0x, _offset0y);
    float2 y0 = IN.worldPos.zy * tile + offset;
    float2 x0 = IN.worldPos.xz * tile + offset;
    float2 z0 = IN.worldPos.xy * tile + offset;
    fixed4 cX0 = UNITY_SAMPLE_TEX2DARRAY(_SplatArray, float3(y0, 0));
    fixed4 cY0 = UNITY_SAMPLE_TEX2DARRAY(_SplatArray, float3(x0, 0));
    fixed4 cZ0 = UNITY_SAMPLE_TEX2DARRAY(_SplatArray, float3(z0, 0));
    float4 side0 = lerp(cX0, cZ0, z);
    float4 top0 = lerp(side0, cY0, y);
    mixedDiffuse += splat_control03.r * top0 * half4(1.0, 1.0, 1.0, defaultAlpha03.r);
    
    tile = float2(_tiles1x, _tiles1y);
    offset = float2(_offset1x, _offset1y);
    float2 y1 = IN.worldPos.zy * tile + offset;
    float2 x1 = IN.worldPos.xz * tile + offset;
    float2 z1 = IN.worldPos.xy * tile + offset;
    fixed4 cX1 = UNITY_SAMPLE_TEX2DARRAY(_SplatArray, float3(y1, 1));
    fixed4 cY1 = UNITY_SAMPLE_TEX2DARRAY(_SplatArray, float3(x1, 1));
    fixed4 cZ1 = UNITY_SAMPLE_TEX2DARRAY(_SplatArray, float3(z1, 1));
    float4 side1 = lerp(cX1, cZ1, z);
    float4 top1 = lerp(side1, cY1, y);
    mixedDiffuse += splat_control03.g * top1 * half4(1.0, 1.0, 1.0, defaultAlpha03.g); 

    tile = float2(_tiles2x, _tiles2y);
    offset = float2(_offset2x, _offset2y);
    float2 y2 = IN.worldPos.zy * tile + offset;
    float2 x2 = IN.worldPos.xz * tile + offset;
    float2 z2 = IN.worldPos.xy * tile + offset;
    fixed4 cX2 = UNITY_SAMPLE_TEX2DARRAY(_SplatArray, float3(y2, 2));
    fixed4 cY2 = UNITY_SAMPLE_TEX2DARRAY(_SplatArray, float3(x2, 2));
    fixed4 cZ2 = UNITY_SAMPLE_TEX2DARRAY(_SplatArray, float3(z2, 2));
    float4 side2 = lerp(cX2, cZ2, z);
    float4 top2 = lerp(side2, cY2, y);
    mixedDiffuse += splat_control03.b * top2 * half4(1.0, 1.0, 1.0, defaultAlpha03.b); 
    
    tile = float2(_tiles3x, _tiles3y);
    offset = float2(_offset3x, _offset3y);
    float2 y3 = IN.worldPos.zy * tile + offset;
    float2 x3 = IN.worldPos.xz * tile + offset;
    float2 z3 = IN.worldPos.xy * tile + offset;
    fixed4 cX3 = UNITY_SAMPLE_TEX2DARRAY(_SplatArray, float3(y3, 3));
    fixed4 cY3 = UNITY_SAMPLE_TEX2DARRAY(_SplatArray, float3(x3, 3));
    fixed4 cZ3 = UNITY_SAMPLE_TEX2DARRAY(_SplatArray, float3(z3, 3));
    float4 side3 = lerp(cX3, cZ3, z);
    float4 top3 = lerp(side3, cY3, y);
    mixedDiffuse += splat_control03.a * top3 * half4(1.0, 1.0, 1.0, defaultAlpha03.a); 
    
    tile = float2(_tiles4x, _tiles4y);
    offset = float2(_offset4x, _offset4y);
    float2 y4 = IN.worldPos.zy * tile + offset;
    float2 x4 = IN.worldPos.xz * tile + offset;
    float2 z4 = IN.worldPos.xy * tile + offset;
    fixed4 cX4 = UNITY_SAMPLE_TEX2DARRAY(_SplatArray, float3(y4, 4));
    fixed4 cY4 = UNITY_SAMPLE_TEX2DARRAY(_SplatArray, float3(x4, 4));
    fixed4 cZ4 = UNITY_SAMPLE_TEX2DARRAY(_SplatArray, float3(z4, 4));
    float4 side4 = lerp(cX4, cZ4, z);
    float4 top4 = lerp(side4, cY4, y);
    mixedDiffuse += splat_control47.r * top4 * half4(1.0, 1.0, 1.0, defaultAlpha47.r); 
    
    tile = float2(_tiles5x, _tiles5y);
    offset = float2(_offset5x, _offset5y);
    float2 y5 = IN.worldPos.zy * tile + offset;
    float2 x5 = IN.worldPos.xz * tile + offset;
    float2 z5 = IN.worldPos.xy * tile + offset;
    fixed4 cX5 = UNITY_SAMPLE_TEX2DARRAY(_SplatArray, float3(y5, 5));
    fixed4 cY5 = UNITY_SAMPLE_TEX2DARRAY(_SplatArray, float3(x5, 5));
    fixed4 cZ5 = UNITY_SAMPLE_TEX2DARRAY(_SplatArray, float3(z5, 5));
    float4 side5 = lerp(cX5, cZ5, z);
    float4 top5 = lerp(side5, cY5, y);
    mixedDiffuse += splat_control47.g * top5 * half4(1.0, 1.0, 1.0, defaultAlpha47.g); 
    
    tile = float2(_tiles6x, _tiles6y);
    offset = float2(_offset6x, _offset6y);
    float2 y6 = IN.worldPos.zy * tile + offset;
    float2 x6 = IN.worldPos.xz * tile + offset;
    float2 z6 = IN.worldPos.xy * tile + offset;
    fixed4 cX6 = UNITY_SAMPLE_TEX2DARRAY(_SplatArray, float3(y6, 6));
    fixed4 cY6 = UNITY_SAMPLE_TEX2DARRAY(_SplatArray, float3(x6, 6));
    fixed4 cZ6 = UNITY_SAMPLE_TEX2DARRAY(_SplatArray, float3(z6, 6));
    float4 side6 = lerp(cX6, cZ6, z);
    float4 top6 = lerp(side6, cY6, y);
    mixedDiffuse += splat_control47.b * top6 * half4(1.0, 1.0, 1.0, defaultAlpha47.b); 
    
    tile = float2(_tiles7x, _tiles7y);
    offset = float2(_offset7x, _offset7y);
    float2 y7 = IN.worldPos.zy * tile + offset;
    float2 x7 = IN.worldPos.xz * tile + offset;
    float2 z7 = IN.worldPos.xy * tile + offset;
    fixed4 cX7 = UNITY_SAMPLE_TEX2DARRAY(_SplatArray, float3(y7, 7));
    fixed4 cY7 = UNITY_SAMPLE_TEX2DARRAY(_SplatArray, float3(x7, 7));
    fixed4 cZ7 = UNITY_SAMPLE_TEX2DARRAY(_SplatArray, float3(z7, 7));
    float4 side7 = lerp(cX7, cZ7, z);
    float4 top7 = lerp(side7, cY7, y);
    mixedDiffuse += splat_control47.a * top7 * half4(1.0, 1.0, 1.0, defaultAlpha47.a); 
    
    
    
    // NORMAL 
    fixed4 nX0 = UNITY_SAMPLE_TEX2DARRAY(_NormalArray, float3(y0, 0));
    fixed4 nY0 = UNITY_SAMPLE_TEX2DARRAY(_NormalArray, float3(x0, 0));
    fixed4 nZ0 = UNITY_SAMPLE_TEX2DARRAY(_NormalArray, float3(z0, 0));
    float4 side0n = lerp(nX0, nZ0, z);
    float4 top0n = lerp(side0n, nY0, y);
    mixedNormal = splat_control03.r * UnpackNormalWithScale(top0n, _NormalScale0);
    
    fixed4 nX1 = UNITY_SAMPLE_TEX2DARRAY(_NormalArray, float3(y1, 1));
    fixed4 nY1 = UNITY_SAMPLE_TEX2DARRAY(_NormalArray, float3(x1, 1));
    fixed4 nZ1 = UNITY_SAMPLE_TEX2DARRAY(_NormalArray, float3(z1, 1));
    float4 side1n = lerp(nX1, nZ1, z);
    float4 top1n = lerp(side1n, nY1, y);
    mixedNormal += splat_control03.g * UnpackNormalWithScale(top1n, _NormalScale1);
    
    fixed4 nX2 = UNITY_SAMPLE_TEX2DARRAY(_NormalArray, float3(y2, 2));
    fixed4 nY2 = UNITY_SAMPLE_TEX2DARRAY(_NormalArray, float3(x2, 2));
    fixed4 nZ2 = UNITY_SAMPLE_TEX2DARRAY(_NormalArray, float3(z2, 2));
    float4 side2n = lerp(nX2, nZ2, z);
    float4 top2n = lerp(side2n, nY2, y);
    mixedNormal += splat_control03.b * UnpackNormalWithScale(top2n, _NormalScale2);

    fixed4 nX3 = UNITY_SAMPLE_TEX2DARRAY(_NormalArray, float3(y3, 3));
    fixed4 nY3 = UNITY_SAMPLE_TEX2DARRAY(_NormalArray, float3(x3, 3));
    fixed4 nZ3 = UNITY_SAMPLE_TEX2DARRAY(_NormalArray, float3(z3, 3));
    float4 side3n = lerp(nX3, nZ3, z);
    float4 top3n = lerp(side3n, nY3, y);
    mixedNormal += splat_control03.a * UnpackNormalWithScale(top3n, _NormalScale3);
    
    fixed4 nX4 = UNITY_SAMPLE_TEX2DARRAY(_NormalArray, float3(y4, 4));
    fixed4 nY4 = UNITY_SAMPLE_TEX2DARRAY(_NormalArray, float3(x4, 4));
    fixed4 nZ4 = UNITY_SAMPLE_TEX2DARRAY(_NormalArray, float3(z4, 4));
    float4 side4n = lerp(nX4, nZ4, z);
    float4 top4n = lerp(side4n, nY4, y);
    mixedNormal += splat_control47.r * UnpackNormalWithScale(top4n, _NormalScale4);
    
    fixed4 nX5 = UNITY_SAMPLE_TEX2DARRAY(_NormalArray, float3(y5, 5));
    fixed4 nY5 = UNITY_SAMPLE_TEX2DARRAY(_NormalArray, float3(x5, 5));
    fixed4 nZ5 = UNITY_SAMPLE_TEX2DARRAY(_NormalArray, float3(z5, 5));
    float4 side5n = lerp(nX5, nZ5, z);
    float4 top5n = lerp(side5n, nY5, y);
    mixedNormal += splat_control47.g * UnpackNormalWithScale(top5n, _NormalScale5);
    
    fixed4 nX6 = UNITY_SAMPLE_TEX2DARRAY(_NormalArray, float3(y6, 6));
    fixed4 nY6 = UNITY_SAMPLE_TEX2DARRAY(_NormalArray, float3(x6, 6));
    fixed4 nZ6 = UNITY_SAMPLE_TEX2DARRAY(_NormalArray, float3(z6, 6));
    float4 side6n = lerp(nX6, nZ6, z);
    float4 top6n = lerp(side6n, nY6, y);
    mixedNormal += splat_control47.b * UnpackNormalWithScale(top6n, _NormalScale6);
    
    fixed4 nX7 = UNITY_SAMPLE_TEX2DARRAY(_NormalArray, float3(y7, 7));
    fixed4 nY7 = UNITY_SAMPLE_TEX2DARRAY(_NormalArray, float3(x7, 7));
    fixed4 nZ7 = UNITY_SAMPLE_TEX2DARRAY(_NormalArray, float3(z7, 7));
    float4 side7n = lerp(nX7, nZ7, z);
    float4 top7n = lerp(side7n, nY7, y);
    mixedNormal += splat_control47.a * UnpackNormalWithScale(top7n, _NormalScale7);

    mixedNormal.z += 1e-5f; // to avoid nan after normalizing
}

#ifndef TERRAIN_SURFACE_OUTPUT
    #define TERRAIN_SURFACE_OUTPUT SurfaceOutput
#endif

void SplatmapFinalColor(Input IN, TERRAIN_SURFACE_OUTPUT o, inout fixed4 color)
{
    color *= o.Alpha;
    #ifdef TERRAIN_SPLAT_ADDPASS
        UNITY_APPLY_FOG_COLOR(IN.fogCoord, color, fixed4(0,0,0,0));
    #else
        UNITY_APPLY_FOG(IN.fogCoord, color);
    #endif
}

void SplatmapFinalPrepass(Input IN, TERRAIN_SURFACE_OUTPUT o, inout fixed4 normalSpec)
{
    normalSpec *= o.Alpha;
}

void SplatmapFinalGBuffer(Input IN, TERRAIN_SURFACE_OUTPUT o, inout half4 outGBuffer0, inout half4 outGBuffer1, inout half4 outGBuffer2, inout half4 emission)
{
    UnityStandardDataApplyWeightToGbuffer(outGBuffer0, outGBuffer1, outGBuffer2, o.Alpha);
    emission *= o.Alpha;
}

#endif // MESH_TRIPLANAR_STANDARD_TXTARRAY_INCLUDED