Shader "Yoonsang/Prewarp_" {
  Properties{
      _MainTex("Texture", 2D) = "white" {}
      _CylinderOriginInCamera("Cylinder Origin In Camera Space", Vector) = (0.0, 0.0, 0.0, 0.0)
      _CylinderRadius("Cylinder Radius", Float) = 0.0
      _CylinderHeight("Cylinder Height", Float) = 0.0
      _ScreenWidth("Screen Width", Float) = 0.0
      _ScreenHeight("Screen Height", Float) = 0.0
  }
    SubShader{
        Tags { "RenderType" = "Opaque" }
        LOD 300

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma enable_d3d11_debug_symbols
            #pragma target 5.0
            #include "UnityCG.cginc"

            float4 GetIntersectedPointRayCylinder(float3 origin,
                                                  float3 dir,
                                                  float cylRadius,
                                                  float cylHeight,
                                                  float4 cylOrigin);
            float2 Transform2FragCoordsCylinder(float4 intersectedPoint);

            /**
            *   SV_VertexID.
            * the vertex id is used by each shader stage to identify each vertex.
            * It's assigned when the primitive is processed in the IA stage.
            * like < Attach the vertices -> ID semantics to the shader input decl to
            * inform the IA stage to generate a per-vertex id.
            *
            * IA will add a vertex ID to each vertex for use by shader stages. for each draw call,
            * the vertex ID is incremented by 1. Throughout indexed draw calls, the cound resets back to
            * the start value. (ID3D11DeviceContext::DrawIndexed(), ID3D11DeciveContext::DrawIndexedInstanced()),
            * the vertexID represents the index value actually. it's wrapped to 0 for overflows (up to 2^32 - 1).
            */
            struct appdata {
                //uint vtxIdx : SV_VertexID;
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };
            /**
            * the semantic << SV_POSITION >> stands for the << System Value Position >> literally but,
            * it also specifies the screen space coordinates that offsets by 0.5.
            */
            struct v2f {
                float2 uv : TEXCOORD0;
                float4 posInClip : SV_POSITION;
                float4 screenPos : TEXCOORD1;
                float3 normalInCamera : NORMAL;
                float3 posInCamera : TEXCOORD2;
                //float4 hitPosOnCylinder : TEXCOORD3;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float4 _CylinderOriginInCamera;
            float _CylinderRadius, _CylinderHeight;
            float _ScreenWidth, _ScreenHeight;
            float2 finalUV;

            /**
            * For the case of writting a output from the pixel shader,
            * the Render-target and the Unordered-Access-Views share the same resourc slots
            * when they're going out of the shader. This means the UAVs must be given an offset so that
            * they are put on the slots after the Render-target-views.
            *
            * RTVs, DSV, and UAVs can't be set separably; it must be happened at the same time.
            */

            /*struct IntersectionBuf {
              float4 posInObj;
              float4 posInClip;
              float4 posInCamera;
              float4 normalInObj;
              float4 normalInCamera;
              float4 intersectedPos;
            };

            RWStructuredBuffer<IntersectionBuf> _IntersectionBuf : register(u1);
            RWTexture2D<float2> _UVMapBuf : register(u2);*/

            v2f vert(appdata v) {
                v2f o = (v2f)0;
                // Retrieve the camera space coordinates of each vertex of the Pyramid.
                o.posInCamera = UnityObjectToViewPos(v.vertex);
                //o.posInCamera = mul(UNITY_MATRIX_V, mul(unity_ObjectToWorld, float4(v.vertex.xyz, 0.0))).xyz;

                // Retrieve the clip sapce coordinates of each vertex of the Pyramid.
                o.posInClip = UnityObjectToClipPos(v.vertex);
                
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                //o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                // Retrieve the normal coordinates of each vertex of the Pyramid.
                //o.normalInCamera = UnityObjectToViewPos(float4(v.normal, 0.0));
                //float3 normalInCamera = mul(UNITY_MATRIX_V, mul(unity_ObjectToWorld, float4(v.normal, 0.0))).xyz;
                float4 normalInCamera = mul(UNITY_MATRIX_V, mul(unity_ObjectToWorld, float4(v.normal, 1.0)));
                o.normalInCamera = normalInCamera.xyz;
                //o.normalInCamera = UnityObjectToViewPos(float4(v.normal, 1.0f));

                _CylinderOriginInCamera = mul(UNITY_MATRIX_V, mul(unity_ObjectToWorld, _CylinderOriginInCamera));
                //_CylinderOriginInCamera = UnityObjectToViewPos(_CylinderOriginInCamera.xyz);
                //_CylinderOriginInCamera = mul(UNITY_MATRIX_MV, _CylinderOriginInCamera);
                //_CylinderOriginInCamera = UnityObjectToViewPos(_CylinderOriginInCamera);
                //o.normalInCamera = v.normal;
                // Remap [-w, w]x [-w, w] x [-w, w] to [0, w]^3 (built-in).
                //o.screenPos = ComputeScreenPos(o.posInClip);
                //float3 reflDirInCamera = normalize(reflect(-o.posInCamera, normalInCamera));
                // float4 hitPosOnCylinder = GetIntersectedPointRayCylinder(o.posInCamera,
                //                                                         reflDirInCamera,
                //                                                         _CylinderRadius,
                //                                                         _CylinderHeight,
                //                                                         _CylinderOriginInCamera);
                // o.hitPosOnCylinder = hitPosOnCylinder;
                // Pull out the variables for debugging.
                // _IntersectionBuf[v.vtxIdx].posInObj = v.vertex;
                // _IntersectionBuf[v.vtxIdx].posInClip = o.posInClip;
                // _IntersectionBuf[v.vtxIdx].posInCamera = float4(o.posInCamera, 1.0);
                // _IntersectionBuf[v.vtxIdx].normalInObj = float4(v.normal, 0.0);
                // _IntersectionBuf[v.vtxIdx].normalInCamera = float4(o.normalInCamera, 0.0);
                // _IntersectionBuf[v.vtxIdx].intersectedPos = o.hitPosOnCylinder;
                return o;
            }

            float4 frag(v2f i) : SV_Target {
              //float3 ndc = i.screenPos.xyz / i.screenPos.w;
              // i.screenPos is in affince space and it can be linearly interpolated;
              //float3 camPos = mul(UNITY_MATRIX_V, float4(_WorldSpaceCameraPos, 1.0)).xyz;
              float3 reflDirInCamera = normalize(reflect(-i.posInCamera, i.normalInCamera));
              float4 hitPosOnCylinder = GetIntersectedPointRayCylinder(i.posInCamera,
                                                                       reflDirInCamera,
                                                                       _CylinderRadius,
                                                                       _CylinderHeight,
                                                                       _CylinderOriginInCamera);

              // If there's no collision, then return the only white colour.
              if(hitPosOnCylinder.w == 0.0) {
                return float4(1.0, 1.0, 1.0, 1.0);
              } else {
                finalUV = Transform2FragCoordsCylinder(hitPosOnCylinder);
                //return tex2D(_MainTex, mul(unity_ObjectToWorld, float4(uv, 0.0, 0.0)).xy);
                //return tex2D(_MainTex, mul(UNITY_MATRIX_V, float4(uv, 0.0, 0.0)));
                //return tex2D(_MainTex, mul(UNITY_MATRIX_V, mul(unity_WorldToObject/*unity_ObjectToWorld*/, float4(finalUV, 0.0, 0.0))));
                return tex2D(_MainTex, finalUV);
              }
            }

            float4 GetIntersectedPointRayCylinder(float3 origin,
                                                  float3 dir,
                                                  float cylRadius,
                                                  float cylHeight,
                                                  float4 cylOrigin) {
            // float px = origin.x;
            // float px2 = px * px;
            // float dx = dir.x;
            // float dx2 = dx * dx;
            // float pxdx = px * dx;

            // float py = origin.y;
            // float py2 = py * py;
            // float dy = dir.y;
            // float dy2 = dy * dy;
            // float pydy = py * dy;

            // float r = cylRadius;
            // float r2 = r * r;

            // float t = (-(pxdx + pydy) + sqrt((pxdx + pydy) * (pxdx + pydy) - (dx2 + dy2) * (px2 + py2 - r2))) /
            //   (dx2 + dy2);
            // float3 hitCylPos = float3(
            //   px + dx * t,
            //   py + dy * t,
            //   origin.z + dir.z * t);

            float a = dir.x * dir.x + dir.z * dir.z;
            float b = 2 * (dir.x * (origin.x - cylOrigin.x) + dir.z * (origin.z - cylOrigin.z));
            float c = (origin.x - cylOrigin.x) * (origin.x - cylOrigin.x) 
            + (origin.z - cylOrigin.z) * (origin.z - cylOrigin.z)
            - (cylRadius * cylRadius);

            float d = b * b - 4 * a * c;
            if (d < 0.0) {
              return (float4)0;
            }

            float t1 = (-b - sqrt(d)) / (2 * a);
            float t2 = (-b + sqrt(d)) / (2 * a);
            float3 res;
            bool bRes = t1 > t2;
            
            if (bRes) {
              res = float3( origin.x + t2 * dir.x, origin.y + t2 * dir.y, origin.z + t2 * dir.z);
            } else {
              res = float3( origin.x + t1 * dir.x, origin.y + t1 * dir.y, origin.z + t1 * dir.z);
            }

            if (origin.z <= res.z && res.z <= origin.z + _CylinderHeight) {
              return float4(res, bRes ? t2 : t1);
            } else {
              return (float4)0;
            }

            // float comp = origin.z + hitCylPos.z * t;
            // //comp = max(0.0, comp);
            // if(origin.z <= comp && comp <= origin.z + _CylinderHeight) {
            //   return float4(hitCylPos, t);
            // } else {
            //   return float4(0.0, 0.0, 0.0, 0.0);
            // }
            }

            float2 Transform2FragCoordsCylinder(float4 intersectedPoint) {
              //float theta = acos(intersectedPoint.x) / _CylinderRadius;
              //float theta = acos(intersectedPoint.x / _CylinderRadius);
              float theta = atan2(intersectedPoint.y, intersectedPoint.x);
               /*if(theta < 0.0)
                theta += UNITY_TWO_PI;*/
              return float2(theta / UNITY_TWO_PI, intersectedPoint.z / _CylinderHeight);
            }
            ENDCG
        }
      }
}
