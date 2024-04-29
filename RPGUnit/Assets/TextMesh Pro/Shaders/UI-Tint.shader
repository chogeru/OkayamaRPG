Shader "UI/Tint" {
    Properties {
        _MainTex ("Base (RGB)", 2D) = "white" {}
     }

    SubShader {
        Pass {
            CGPROGRAM

            #include "UnityCG.cginc"

            #pragma vertex vert_img
            #pragma fragment frag
            
            float GlobalVal;
            float globalR;
            float globalG;
            float globalB;
            float _FloatR;
            float _FloatG;
            float _FloatB;

            sampler2D _MainTex;

            fixed4 frag(v2f_img i) : COLOR {
                fixed4 c = tex2D(_MainTex, i.uv);
                float gray = GlobalVal;
                float r = ((c.r * _FloatR) + (c.g * _FloatG) + (c.b * _FloatB)) / 3 * (gray) + (c.r * _FloatR) * (1 - gray);
                float g = ((c.r * _FloatR) + (c.g * _FloatG) + (c.b * _FloatB)) / 3 * (gray) + (c.g * _FloatG) * (1 - gray);
                float b = ((c.r * _FloatR) + (c.g * _FloatG) + (c.b * _FloatB)) / 3 * (gray) + (c.b * _FloatB) * (1 - gray);
                //float gray = globalR * GlobalVal + globalG * GlobalVal + globalB * GlobalVal;
                //float gray = 0.3 + 0.6 + 0.1;
                //globalR = c.r;
                //c.rgb = fixed3((c.r - (1 - _FloatR)) * GlobalVal, (c.g - (1 - _FloatG)) * GlobalVal, (c.b - (1 - _FloatB) ) * GlobalVal);
                c.rgb = fixed3(r,g,b);
                return c;
            }

            ENDCG
        }
    }
}