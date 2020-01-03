Shader "danbi/VaryingColorByScreenPos" {
  Properties {      
      _BaseColor("Base Color", COLOR) = (1, 1, 1, 1)
      _TestImage("Test Image", 2D) = "white" {}
  }
    SubShader {
      Tags { "RenderType" = "Opaque" }
      LOD 100
      Cull off
      
      Pass {
          CGPROGRAM
          #pragma vertex vert
          #pragma fragment frag          

          #include "UnityCG.cginc"
          

          struct appdata {
              float4 vertex : POSITION;
              float2 uv : TEXCOORD0;
              float4 col : COLOR;
          };

          struct v2f {
              float2 uv : TEXCOORD0;              
              float4 vertex : SV_POSITION;
              float4 col : COLOR;
              //float4 screenPos : TEXCOORD1;
          };          
          
          uint _ScreenDimensionX;
          uint _ScreenDimensionY;
          float4 _BaseColor;
          float4 _VertexCol;
          sampler2D _TestImage;
          float4 _TestImage_ST;

          v2f vert(appdata v) {
              v2f o;
              o.vertex = UnityObjectToClipPos(v.vertex);
              o.uv = TRANSFORM_TEX(v.uv, _TestImage);
              //o.uv = (float2)0;
              o.col = v.col;
              //o.screenPos = ComputeScreenPos(v.vertex);
              return o;
          }

          fixed4 frag(v2f i) : SV_Target {
            /*float3 pixelPos = i.screenPos.xyz / i.screenPos.w;
            float3 offset = i.col;
            for (uint x = 0; x < _ScreenDimensionX; ++x) {
              for (uint y = 0; y < _ScreenDimensionY; ++y) {
                offset.y *= _BaseColor * (pixelPos.y / _ScreenDimensionY);
              }
            }
            return fixed4(offset, 0.0);*/
            return tex2D(_TestImage, i.uv);// *_BaseColor;
          }
          ENDCG
      }
  }
}
