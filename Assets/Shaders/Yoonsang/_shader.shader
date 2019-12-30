// Upgrade NOTE: commented out 'float4x4 _CameraToWorld', a built-in variable

// Upgrade NOTE: replaced '_CameraToWorld' with 'unity_CameraToWorld'

Shader "Unlit/_shader"
{
    Properties
    {
        _SkyBoxTexture ("Texture", Cube) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct Ray {
              float3 origin;
              float3 direction;
              float3 energy;
            };

            struct RayHit {
              float3 position;
              float distance;
              float3 normal;
            };

            struct Sphere {
              float3 position;
              float radius;
            };

            struct Cylinder {
              float3 position;
              float radius;
              float height;
              float3 top_position;
            };

            struct Cone {
              float3 position;
              float radius;
              float height;
            };

            Ray CreateRay(float3 origin, float direction);
            RayHit CreateRayHit();
            Ray CreateCameraRay(float2 uv);
            void IntersectSphere(Ray ray, inout RayHit bestHit, Sphere sphere);
            void IntersectGroundPlane(Ray ray, inout RayHit bestHit);
            void IntersectCylinder(Ray ray, inout RayHit bestHit, Cylinder cylinder);
            float IntersectCylinder_internal(Ray ray, float3 center, Cylinder cylinder);
            void IntersectCone(Ray ray, inout RayHit bestHit, Cone cone);
            RayHit Trace(Ray ray);
            float3 Shade(inout Ray ray, RayHit hit);

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            samplerCUBE _SkyBoxTexture;
            // float4x4 _CameraToWorld;
            float4x4 _CameraInverseProjection;
            static const float PI = 3.14159265;
            float2 _PixelOffset;
            static const int EPS = 0.000001;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                //o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                Ray ray = CreateCameraRay(i.uv);
                float3 res = (float3)0;
                for (int i = 0; i < 8; ++i) {
                    RayHit hit = Trace(ray);
                    res += ray.energy * Shade(ray, hit);

                    if (!any(ray.energy))
                      break;
                }
                return float4(res, 1.0);
            }

            Ray CreateRay(float3 origin, float3 direction) {
    Ray ray;
    ray.energy = float3(1.0, 1.0, 1.0);
    ray.origin = origin;
    ray.direction = direction;
    return ray;
}


RayHit CreateRayHit() {
    RayHit hit;
    hit.position = float3 (0.0, 0.0, 0.0);
    hit.distance = 1.#INF;
    hit.normal = float3(0.0, 0.0, 0.0);
    return hit;
}

void IntersectGroundPlane(Ray ray, inout RayHit bestHit) {
    // Calculate distance along the ray where the ground plane is intersected.
    float t = -ray.origin.y / ray.direction.y;
    if (t > 0 && t < bestHit.distance) {
        bestHit.distance = t;
        bestHit.position = ray.origin + t * ray.direction;
        bestHit.normal = float3(0.0, -5.0, 0.0);
    }
}

void IntersectSphere(Ray ray, inout RayHit bestHit, Sphere sphere) {
    // Calculate distance along the ray where the sphere is intersected.
    float3 d = ray.origin - sphere.position;
    float p1 = -dot(ray.direction, d);
    float p2sqr = p1 * p1 - dot(d, d) + sphere.radius * sphere.radius;
    if (p2sqr < 0) return;
    float p2 = sqrt(p2sqr);
    float t = p1 - p2 > 0 ? p1 - p2 : p1 + p2;
    if (t > 0 && t < bestHit.distance) {
        bestHit.distance = t;
        bestHit.position = ray.origin + t * ray.direction;
        bestHit.normal = normalize(bestHit.position - sphere.position);
    }
}

void IntersectCylinder(Ray ray, inout RayHit bestHit, Cylinder cylinder) {
    // Calculate distance along the ray where the cone is intersected.
    // float3 d = ray.origin - cylinder.position;

    // float px = ray.origin.x;
    // float px2 = px * px;
    // float dx = ray.direction.x;
    // float dx2 = dx * dx;
    // float pxdx = px * dx;

    // float py = ray.origin.y;
    // float py2 = py * py;
    // float dy = ray.direction.y;
    // float dy2 = dy * dy;
    // float pydy = py * dy;

    // float r = cylinder.radius;
    // float r2 = r * r;

    // float deter = (pxdx + pydy) * (pxdx + pydy) - (dx2 + dy2) * (px2 + py2 - r2);
    // if (deter < 0) {
    //     return;
    // }
    // float deter2 = sqrt(deter);

    // float t = d - deter2 > 0 ? d - deter2 : d + deter2;

    // // float t = (-(pxdx + pydy) + sqrt((pxdx + pydy) * (pxdx + pydy) - (dx2 + dy2) * (px2 + py2 - r2))) /
    // //   (dx2 + dy2);
    // // float hitPos = float3(px + dx * t, py + dy * t, ray.origin.z + ray.direction.z * t);
    // // float comp = ray.origin.z + hitPos.z * t;
    // // float4 res = float4(0, 0, 0, 0);
    // // if (cylinder.position.z <= comp && cylinder.position.z + cylinder.height) {
    // //     res = float4(hitPos, t);
    // // } else {
    // //     res = float4( 0, 0, 0, 0);
    // // }
    //float3 offset = ray.origin * ray.direction;

    // //float3 offset = -dot(ray.origin, d);

    // // (-b +- sqrt(b ^ 2 - 4ac)) / 2a
    // //float a = offset.x * offset.x + offset.y * offset.y;
    // //float b = 2 * (offset.x * ray.origin.x + offset.y * ray.origin.y);
    // //float c = ray.origin.x * ray.origin.x + ray.origin.y * ray.origin.y - 1;//cylinder.radius * cylinder.radius;

    // //float dter = b * b - 4 * a * c;
    // //if (dter < 0) {
    // //    return;
    // //}

    // //float t = -b + sqrt(dter);
    // //t /= 2 * a;

    // if (t > 0 && t < bestHit.distance) {
    //     bestHit.distance = t;
    //     bestHit.position = ray.origin + t * ray.direction;
    //     bestHit.normal = normalize(bestHit.position - cylinder.position);
    // }

    float a = ray.direction.x * ray.direction.x + ray.direction.z * ray.direction.z;
    float b = 2 * (ray.direction.x * (ray.origin.x - cylinder.position.x) + ray.direction.z * (ray.origin.z - cylinder.position.z));
    float c = (ray.origin.x - cylinder.position.x) * (ray.origin.x - cylinder.position.x) 
      + (ray.origin.z - cylinder.position.z) * (ray.origin.z - cylinder.position.z) 
      - (cylinder.radius * cylinder.radius);

    float d = b * b - 4 * a * c;

    if (d < 0.0) {
        return;
    }

    float t1 = (-b - sqrt(d)) / (2 * a);
    float t2 = (-b + sqrt(d)) / (2 * a);
    float t = 0.0;

    if (t1 > t2) {
        t = t2;
    } else {
        t = t1;
    }

    float r = ray.origin.y + t * ray.direction.y;

    if (r >= cylinder.position.y && r <= cylinder.position.y + cylinder.height) {
        if (t < bestHit.distance) {
            bestHit.distance = t;
            bestHit.position = r;
            bestHit.normal = normalize(bestHit.position - cylinder.position);
        }
    } else {
        return;
    }
}

void IntersectCone(Ray ray, inout RayHit bestHit, Cone cone) {

}

Ray CreateCameraRay(float2 uv) {
    // Transform the camera origin to world space.
    float3 origin = mul(unity_CameraToWorld, float4(0.0f, 0.0f, 0.0f, 1.0f)).xyz;
    // Invert the perspective projection of the view-space position.
    float3 direction = mul(_CameraInverseProjection, float4(uv, 0.0f, 1.0f)).xyz;
    //Transform the direction from camera to world space and normalize.
    direction = mul(unity_CameraToWorld, float4(direction, 0.0f)).xyz;
    direction = normalize(direction);
    return CreateRay(origin, direction);
}

RayHit Trace(Ray ray) {
    Sphere sphere;
    sphere.position = float3(0, -3.0, 0);
    sphere.radius = 1.0;

    Cylinder cylinder;
    cylinder.position = float3(5.0, 6.0, 0);
    cylinder.radius = 4.0;
    cylinder.height = 2.0;
    cylinder.top_position = float3(cylinder.position.x + cylinder.height,
                                  cylinder.position.y + cylinder.height,
                                  cylinder.position.z + cylinder.height);

    RayHit bestHit = CreateRayHit();
    
    IntersectCylinder(ray, bestHit, cylinder);
    cylinder.position.x += 5.0;
    IntersectCylinder(ray, bestHit, cylinder);
    cylinder.position.x += 5.0;
    IntersectCylinder(ray, bestHit, cylinder);

    IntersectSphere(ray, bestHit, sphere);
    sphere.position.y += 3.0;
    IntersectSphere(ray, bestHit, sphere);
    sphere.position.y += 3.0;
    IntersectSphere(ray, bestHit, sphere);

    
    //IntersectGroundPlane(ray, bestHit);
    return bestHit;
}

float3 Shade(inout Ray ray, RayHit hit) {
    if (hit.distance < 1.#INF) {
        float3 spec = float3(0.6, 0.6, 0.6);
        // Reflect the ray and multiply energy with specular reflection.
        ray.origin = hit.position + hit.normal * 0.001;
        ray.direction = reflect(ray.direction, hit.normal);
        ray.energy *= spec;
        // Return nothing.
        return float3( 0, 0, 0);
    } else {
        // Erase the ray's energy - the sky doesn't reflect anything.
        ray.energy = 0.0;
        // Sample the skybox and write it.
        float theta = acos(ray.direction.y) / -PI;
        float phi = atan2(ray.direction.x, -ray.direction.z) / -PI * 0.5;
        //return _SkyboxTexture.(sampler_SkyboxTexture, float2(phi, theta), 0).xyz;
        return texCUBE(_SkyBoxTexture, float3(phi, theta, 1.0)).xyz;
    }
}
            ENDCG
        }
    }
}
