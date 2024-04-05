Shader "Unlit/EqualizeShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Size("Screen Size", int) = 0
        _Brightness("Brightness", float) = 1
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

            sampler2D _MainTex;
            float4 _MainTex_ST;

            int _Size;
            int _RProps[256];
            int _GProps[256];
            int _BProps[256];

            int _CProps[256 * 4];

            float _Brightness;

            float equalizer(float val, int colIdx){
                float sum = 0;
                float adjVal = 256 * val;
                
                for (int i = 0; i <= adjVal*4; i+=4) {
                    sum += _CProps[i + colIdx];
                }
                return _Brightness * 255 * sum / (float)_Size;
            }
            float4 colEqualizer(float4 col) {
                return float4(equalizer(col.r, 0), 
                    equalizer(col.g, 1), 
                    equalizer(col.b, 2), 
                    1);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                col = colEqualizer(col);
                return col;
            }
            ENDCG
        }
    }
}
