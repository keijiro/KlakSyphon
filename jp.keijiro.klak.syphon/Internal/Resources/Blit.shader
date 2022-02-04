Shader "Hidden/Klak/Syphon/Blit"
{
    Properties
    {
        _MainTex("", 2D) = "white" {}
    }

    CGINCLUDE

    #include "UnityCG.cginc"

    sampler2D _MainTex;

    fixed4 frag_simple(v2f_img i) : SV_Target
    {
        return tex2D(_MainTex, i.uv);
    }

    fixed4 frag_clear_alpha(v2f_img i) : SV_Target
    {
        return fixed4(tex2D(_MainTex, i.uv).rgb, 1);
    }

    ENDCG

    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag_clear_alpha
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag_simple
            ENDCG
        }
    }
}
