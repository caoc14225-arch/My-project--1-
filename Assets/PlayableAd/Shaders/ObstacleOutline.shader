Shader "PlayableAd/ObstacleOutline"
{
    Properties
    {
        _OutlineColor ("Outline Color", Color) = (0,1,0,1)
        _OutlineWidth ("Outline Width", Range(0,0.2)) = 0.05
        _Danger ("Danger", Range(0,1)) = 0
        _PulseSpeed ("Pulse Speed", Float) = 1.8
        _PulseAmount ("Pulse Amount", Range(0,0.5)) = 0.16
        _FlowDirection ("Flow Direction", Range(-1,1)) = 0
    }
    SubShader
    {
        Tags { "Queue"="Geometry+1" "RenderType"="Opaque" }
        Pass
        {
            Cull Front
            ZWrite Off
            ZTest LEqual

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float4 _OutlineColor;
            float _OutlineWidth;
            float _Danger;
            float _PulseSpeed;
            float _PulseAmount;
            float _FlowDirection;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float objectY : TEXCOORD0;
            };

            v2f vert(appdata input)
            {
                v2f output;
                float3 expanded = input.vertex.xyz + input.normal * _OutlineWidth;
                output.vertex = UnityObjectToClipPos(float4(expanded, 1.0));
                output.objectY = input.vertex.y;
                return output;
            }

            fixed4 frag(v2f input) : SV_Target
            {
                float wave = 0.5 + 0.5 * sin(_Time.y * _PulseSpeed * 6.2831853);
                float brightness = lerp(1.0, 1.0 - _PulseAmount + wave * _PulseAmount, _Danger);
                float flow = 0.5 + 0.5 * sin((input.objectY * 9.0 - _Time.y * 5.0 * _FlowDirection) * 6.2831853);
                brightness *= lerp(1.0, 0.82 + flow * 0.28, abs(_FlowDirection));
                return fixed4(_OutlineColor.rgb * brightness, _OutlineColor.a);
            }
            ENDCG
        }
    }
}
