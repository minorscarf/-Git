Shader "Custom/SoftEdges" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _Radius ("Edge Radius", Range(0.0, 0.1)) = 0.03
    }
    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float _Radius;

            v2f vert (appdata_t v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            half4 frag (v2f i) : SV_Target {
                float2 center = float2(0.5, 0.5);
                float radius = _Radius;
                float dist = distance(i.uv, center);

                // カスタマイズ可能なエッジのソフトネスを設定
                float edgeSoftness = smoothstep(radius - 0.005, radius + 0.005, dist);

                half4 col = tex2D(_MainTex, i.uv);
                col.a = col.a * edgeSoftness;

                return col;
            }
            ENDCG
        }
    }
}
