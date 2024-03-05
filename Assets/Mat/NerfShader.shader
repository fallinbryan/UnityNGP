Shader "Unlit/NerfShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _DepthTex("Depth Texture", 2D) = "white" {}
        _AlphaThreshold("Alpha Threshold", Range(0, 1)) = 0.01
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        Pass
        {
            //Cull off
            ZWrite On
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            float _AlphaThreshold;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float clipPosZ : TEXCOORD1;
                float clipPosW : TEXCOORD2;
            };

            sampler2D _MainTex;
            sampler2D _DepthTex;
            float4 _MainTex_ST;


            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);

                 // float2 center = float2(0.75, 0.5);
                 // float2 uvDist = v.uv - center;

                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.clipPosZ = o.vertex.z;
                o.clipPosW = o.vertex.w;
  
                return o;
            }

            struct fragOutput {
                fixed4 color : SV_Target;
                float depth : SV_Depth;
            };

            fragOutput frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                float depth = tex2D(_DepthTex, i.uv).r;
        
                float clipDepth = i.clipPosZ / i.clipPosW;
                float finalDepth = lerp(clipDepth, 1.0, depth);
                // if the color is black, set alpha to 0
                // if (col.r == 0.0 && col.g == 0.0 && col.b == 0.0) {

                //   col.a = 0.0;
                // }
                  // Convert the color to grayscale
                float grayscale = dot(col.rgb, fixed3(0.299, 0.587, 0.114));

           


                // If the grayscale value is below a certain threshold, scale the alpha
                if (grayscale < _AlphaThreshold) {
                    col.a *= grayscale;
                }
           
                

                fragOutput o;
                o.color = col;
                o.depth = depth;
                return o;

            }

            ENDCG
        }
    }
}
