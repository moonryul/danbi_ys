Shader "Yoonsang/Prewarp_YS_"
{
  Properties
  {
      _MainTex("Texture", 2D) = "white" {}
      _CylinderOriginInCamera("Cylinder Origin In Camera Space", Vector) = (0.0, 0.0, 0.0, 0.0)
      _CylinderRadius("Cylinder Radius", Float) = 0.0
      _CylinderHeight("Cylinder Height", Float) = 0.0
      _ConeOriginInCamera("Cone Origin In Camera Space", Vector) = (0.0, 0.0, 0.0, 0.0)
      _ConeRadius("Cone Radius", Float) = 0.0
      _ConeHeight("Cone Height", Float) = 0.0
  }
    SubShader
      {
          Tags { "RenderType" = "Opaque" }
          LOD 300

          Pass
          {
              CGPROGRAM
              #pragma vertex vert
              #pragma fragment frag
              #pragma target 5.0
              #pragma multi_compile CONE_ON CYLINDER_ON
              #include "UnityCG.cginc"

              float4 GetIntersectedPointRayCylinder(float3 origin, float3 dir);
              float4 GetIntersectedPointRayCone(float3 origin, float3 dir);
              float2 Transform2FragCoordsCylinder(float4 intersectedPoint);
              float2 Transform2FragCoordsCone(float4 intersectedPoint);

              struct appdata {
                uint vIndex : SV_VertexID;
                  float4 vertex : POSITION;
                  float2 uv : TEXCOORD0;
                  float3 normal : NORMAL;
              };

              struct v2f {
                  float2 uv : TEXCOORD0;
                  float4 posInClip : SV_POSITION;
                  float4 posInCamera : TEXCOORD1;
                  float3 normalInCamera : TEXCOORD2;
              };

              struct intersection_buf {
                float4 posInObj;
                float4 posInClip;
                float4 posInCam;
                float4 normalInObj;
                float4 normalInCam;
                float4 intersectionPos;
              };

              RWStructuredBuffer<intersection_buf> _IntersectionBuf : register( u1 );
              RWTexture2D<float2> _uvMapBuf : register( u2 );

              sampler2D _MainTex;
              float4 _MainTex_ST;
              float3 _CylinderOriginInCamera, _ConeOriginInCamera;
              float _CylinderRadius, _CylinderHeight, _ConeRadius, _ConeHeight;

              float3 _WorldSpaceCamPos;

              v2f vert(appdata v) {
                  v2f o = (v2f)0;
                  o.posInClip = UnityObjectToClipPos(v.vertex);
                  _WorldSpaceCamPos = _WorldSpaceCameraPos;
                  o.posInCamera = float4(UnityObjectToViewPos(v.vertex), 1.0);
                  o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                  //o.normalInCamera = UnityObjectToViewPos(v.normal).xyz;
                  //o.normalInCamera = mul(UNITY_MATRIX_V, float4(UnityObjectToWorldNormal(v.normal), 1.0));
                  float3 norm = mul(UNITY_MATRIX_V, mul(unity_ObjectToWorld, float4(v.normal, 1.0))).xyz;
                  o.normalInCamera = norm;

                  _IntersectionBuf[v.vIndex].posInObj = v.vertex;
                  _IntersectionBuf[v.vIndex].posInClip = o.posInClip;
                  _IntersectionBuf[v.vIndex].posInCam = o.posInCamera;
                  _IntersectionBuf[v.vIndex].normalInObj = float4( v.normal, 0.0 );
                  _IntersectionBuf[v.vIndex].normalInCam = float4( o.normalInCamera, 0.0 );
                  /*_IntersectionBuf[v.vIndex].intersectionPos = o.hitPosOnCylinder;*/
                  return o;
              }

              float4 frag(v2f i) : SV_Target  
              {
                #ifdef CYLINDER_ON
                float3 camPos = UnityWorldToViewPos(_WorldSpaceCamPos);
                float3 reflDirInCamera = normalize(reflect(-i.posInCamera, i.normalInCamera));
                float4 hitPosOnCylinder = GetIntersectedPointRayCylinder(i.posInCamera,
                                                                  reflDirInCamera);
                if (hitPosOnCylinder.w == 0.0) {
                  return float4(1.0, 1.0, 1.0, 1.0);
                } else {
                  float2 uv = Transform2FragCoordsCylinder(hitPosOnCylinder);
                  //float2 uv = Transform2FragCoordsCylinder(mul(unity_ObjectToWorld, mul(UNITY_MATRIX_IT_MV, hitPosOnCylinder)));
                  return tex2D(_MainTex, uv);
                }
                //#endif
                #elif CONE_ON
                float3 reflDirInCamera = normalize(reflect(-i.posInCamera, i.normalInCamera));
                float4 hitPosOnCone = GetIntersectedPointRayCone(i.posInCamera, reflDirInCamera);
                if (hitPosOnCone.w == 0.0) {
                  return float4(1.0, 1.0, 1.0, 1.0);
                } else {
                  float2 uv = Transform2FragCoordsCone(hitPosOnCone);
                  return tex2D(_MainTex, uv);
                }
                #endif
              }

           float4 GetIntersectedPointRayCylinder(float3 origin, float3 dir) {
            float px = origin.x;
            float px2 = px * px;
            float dx = dir.x;
            float dx2 = dx * dx;
            float pxdx = px * dx;

            float py = origin.y;
            float py2 = py * py;
            float dy = dir.y;
            float dy2 = dy * dy;
            float pydy = py * dy;

            float r = _CylinderRadius;
            float r2 = r * r;

            float t = (-(pxdx + pydy) + sqrt((pxdx + pydy) * (pxdx + pydy) - (dx2 + dy2) * (px2 + py2 - r2))) /
              (dx2 + dy2);
            float3 hitCylPos = float3(
              px + dx * t,
              py + dy * t,
              origin.z + dir.z * t);
            float comp = origin.z + hitCylPos.z * t;
            //comp = max(0.0, comp);
            //return float4(hitCylPos, t);
            if(origin.z <= comp && comp <= origin.z + _CylinderHeight) {
              return float4(hitCylPos, t);
            } else {
              return float4(0.0, 0.0, 0.0, 0.0);
            }
          }

          float4 GetIntersectedPointRayCone(float3 origin, float3 dir) {
            float px = origin.x;
            float px2 = px * px;
            float dx = dir.x;
            float dx2 = dx * dx;
            float pxdx = px * dx;

            float py = origin.y;
            float py2 = py * py;
            float dy = dir.y;
            float dy2 = dy * dy;
            float pydy = py * dy;

            float pz = origin.z;
            float pz2 = pz * pz;
            float dz = dir.z;
            float dz2 = dz * dz;

            float r = _ConeRadius;
            float r2 = r * r;
            float h = _ConeHeight;
            float h2 = h * h;
            float c = origin.z;

            float first = -(h2 * pxdx + h2 * pydy + (h - pz * r + c * r) * dz * r);
            float second = (h2 * pxdx + h2 * pydy + (h - pz * r + c * r) * dz * r);
            second *= second;
            float third = -(h2 * dx2 + h2 * dy2 - r2 * dz2) * (h2 * px2 + h2 * py2 - (h - pz * r + c * r) * (h - pz * r + c * r));
            float fourth = sqrt(second + third);
            float fifth = h2 * dx2 + h2 * dy2 - r2 * dz2;
            float t = (first + fourth) / fifth;
            float3 hitConePos = (px + dx * t, py + dy * t, pz + dz * t);
            float comp = pz + hitConePos.z * t;
            if (pz <= comp && comp <= pz + h) {
              return float4(hitConePos, t);
            }
            else {
              return (float4)0.0;
            }
          }

          float2 Transform2FragCoordsCylinder(float4 intersectedPoint) {
            float theta = acos(intersectedPoint.x / _CylinderRadius);
            //float theta = atan2(intersectedPoint.y, intersectedPoint.x);
             if(theta < 0.0)
              theta += UNITY_TWO_PI;
             float2 res = float2(theta / UNITY_TWO_PI, (intersectedPoint.z - _CylinderOriginInCamera.z) / _CylinderHeight);
            return res;
          }

          float2 Transform2FragCoordsCone(float4 intersectedPoint) {
            float h = _ConeHeight;
            float r = _ConeRadius;
            float x0 = intersectedPoint.x;
            float y0 = intersectedPoint.y;
            float z0 = intersectedPoint.z;
            float2 intersected = float2((-h / (z0 - h)) * x0, (-h / (z0 - h)) * y0);
            float theta = acos((h / h - z0) * (x0 / r));
            float dist = x0 * x0 + y0 * y0 + (z0 - h) * (z0 - h);
            dist = sqrt(dist);
            return float2(dist * cos(r * theta / sqrt(h * h + r * r)),
                          dist * sin(r * theta / sqrt(h * h + r * r)));
            
          }
          ENDCG
      }
      }
}
