Shader "Custom/TriplanarMapping"
{
    Properties
    {
      _Color("Main Color", Color) = (1,1,1,1)
      _MainTex("Base (RGB)", 2D) = "white" {}
      _Scale("Texture Scale", Float) = 1.0
      _OutlineColor("Outline color", Color) = (1,0,0,0.5)
      _OutlineWidth("Outlines width", Range(0.0, 2)) = 1.04
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque"}
        LOD 200

        CGPROGRAM
        #pragma surface surf NoLighting vertex:vert 

        sampler2D _MainTex;
        fixed4 _Color;
        float _Scale;

        struct Input
        {
          float3 worldNormal;
          float3 worldPos;
        };

        void vert(inout appdata_full v) {

            // Get the worldspace normal after transformation, and ensure it's unit length.
            float3 n = normalize(mul(unity_ObjectToWorld, v.normal).xyz);

            // Pick a direction for our texture's vertical "v" axis. 
            // Default for floors/ceilings:
            float3 vDirection = float3(0, 0, 1);

            // For non-horizontal planes, we'll choose
            // the closest vector in the polygon's plane to world up.
            if (abs(n.y) < 1.0f) {
                vDirection = normalize(float3(0, 1, 0) - n.y * n);
            }

            // Get the perpendicular in-plane vector to use as our "u" direction.
            float3 uDirection = normalize(cross(n, vDirection));

            // Get the position of the vertex in worldspace.
            float3 worldSpace = mul(unity_ObjectToWorld, v.vertex).xyz;

            // Project the worldspace position of the vertex into our texturing plane,
            // and use this result as the primary texture coordinate.
            v.texcoord.xy = float2(dot(worldSpace, uDirection), dot(worldSpace, vDirection));
        }

        void surf(Input IN, inout SurfaceOutput o)
        {
          float2 UV;
          fixed4 c;

          if (abs(IN.worldNormal.x) > 0.5)
          {
            UV = IN.worldPos.yz; // side
            c = tex2D(_MainTex, UV * _Scale); // use WALLSIDE texture
          }
          else if (abs(IN.worldNormal.z) > 0.5)
          {
            UV = IN.worldPos.xy; // front
            c = tex2D(_MainTex, UV * _Scale); // use WALL texture
          }
          else
          {
            UV = IN.worldPos.xz; // top
            c = tex2D(_MainTex, UV * _Scale); // use FLR texture
          }

          o.Albedo = c.rgb * _Color;
        }

        fixed4 LightingNoLighting(SurfaceOutput s, fixed3 lightDir, fixed atten)
        {
            fixed4 c;
            c.rgb = s.Albedo;
            c.a = s.Alpha;
            return c;
        }
        ENDCG

            //The second pass where we render the outlines
            Pass{
                Cull Front

                CGPROGRAM

            #include "UnityCG.cginc"

            #pragma vertex vert
            #pragma fragment frag

            fixed4 _OutlineColor;
            float _OutlineWidth;

            struct appdata {
                float4 vertex : POSITION;
                float4 normal : NORMAL;
            };

            struct v2f {
                float4 position : SV_POSITION;
            };

            v2f vert(appdata v) {
                v2f o;
                v.vertex.xyz *= _OutlineWidth;
                o.position = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_TARGET{
                return _OutlineColor;
            }

            ENDCG
        }
    }
    
    Fallback "VertexLit"
}