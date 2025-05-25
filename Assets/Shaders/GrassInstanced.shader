Shader "Custom/GrassInstanced"
{
    Properties
    {
        _ColorBottom ("Bottom Color", Color) = (0.2, 0.5, 0.1, 1)
        _ColorTop ("Top Color", Color) = (0.4, 1.0, 0.4, 1)
        _WindStrength ("Wind Strength", Float) = 0.05
        _WindSpeed ("Wind Speed", Float) = 2.0
        _SpringDamping ("Spring Damping", Float) = 4.0
        _GroundTex ("Ground Color Texture", 2D) = "white" {}
        _GroundTexScale ("Ground Tex Scale", Range(0.005, 0.5)) = 0.1
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"

            StructuredBuffer<float4> _GrassData;      // xyz = pos, w = rotationAngle
            StructuredBuffer<float3> _GrowDirBuffer;  // growth direction

            sampler2D _GroundTex;
            float _GroundTexScale;
            float4 _ColorBottom;
            float4 _ColorTop;
            float _WindStrength;
            float _WindSpeed;
            float _SpringDamping;

            struct appdata
            {
                float3 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float heightFactor : TEXCOORD0;
                float2 worldXZ : TEXCOORD1;
            };

            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float, _Dummy)
            UNITY_INSTANCING_BUFFER_END(Props)

            v2f vert(appdata v, uint id : SV_InstanceID)
            {
                v2f o;

                float4 data = _GrassData[id];
                float3 offset = data.xyz;
                float angle = data.w; // rotation angle in radians
                float3 growDir = normalize(_GrowDirBuffer[id]);

                // 草体局部坐标（未缩放）
                float3 local = float3(v.vertex.x, v.vertex.y, v.vertex.z);

                // 局部 xz 平面旋转（绕 Y 轴）
                float cosA = cos(angle);
                float sinA = sin(angle);
                float xRot = local.x * cosA - local.z * sinA;
                float zRot = local.x * sinA + local.z * cosA;
                float3 rotatedLocal = float3(xRot, local.y, zRot);

                // 计算风相位 + 弹性
                float heightFactor = saturate(local.y);
                float phase = _Time.y * _WindSpeed + dot(offset.xz, float2(4.0, 4.0));
                float sway = sin(phase) * _WindStrength * pow(heightFactor, 1.5); // 非线性
                float damping = exp(-_SpringDamping * (1.0 - heightFactor));
                sway *= damping;

                // 构建稳定的局部坐标系（TBN）
                float3 up = float3(0, 1, 0);
                float3 forward = normalize(abs(dot(growDir, up)) > 0.99 ? float3(1, 0, 0) : cross(up, growDir));
                float3 right = normalize(cross(growDir, forward));

                // 风作用于 x 方向
                rotatedLocal.x += sway;

                // 构建 worldPos
                float3 worldPos = offset
                    + rotatedLocal.y * growDir
                    + rotatedLocal.x * right
                    + rotatedLocal.z * forward;

                o.vertex = UnityObjectToClipPos(float4(worldPos, 1));
                o.heightFactor = heightFactor;
                o.worldXZ = offset.xz * _GroundTexScale;
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float4 gradColor = lerp(_ColorBottom, _ColorTop, i.heightFactor);
                float4 groundColor = tex2D(_GroundTex, i.worldXZ);
                return lerp(groundColor, gradColor, 0.5);
            }

            ENDHLSL
        }
    }
}
