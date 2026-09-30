Shader "Custom/Shader"
{
    Properties
    {
        _Color ("Base Color", Color) = (1,1,1,1)
        _MainTex ("Main Texture", 2D) = "white" {}
        _Smoothness ("Smoothness", Range(0,1)) = 0.2
        _Metallic ("Metallic", Range(0,1)) = 0.0

        _RimColor ("Rim Color", Color) = (1,0.85,0.5,1)
        _RimPower ("Rim Power", Range(0.5,8)) = 3.0

        _EmissionColor ("Emission Color", Color) = (0,0,0,1)
        _EmissionStrength ("Emission Strength", Range(0,5)) = 0.0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
            float3 viewDir;
        };

        half _Smoothness;
        half _Metallic;
        fixed4 _Color;

        fixed4 _RimColor;
        half _RimPower;

        fixed4 _EmissionColor;
        half _EmissionStrength;

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;

            o.Albedo = c.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Smoothness;
            o.Alpha = c.a;

            float rim = 1.0 - saturate(dot(normalize(IN.viewDir), o.Normal));
            rim = pow(rim, _RimPower);

            o.Emission = (_RimColor.rgb * rim) + (_EmissionColor.rgb * _EmissionStrength);
        }
        ENDCG
    }
    FallBack "Diffuse"
}
