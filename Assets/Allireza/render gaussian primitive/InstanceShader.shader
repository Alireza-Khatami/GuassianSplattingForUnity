Shader "Custom/InstanceShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"

            struct InstanceData
            {
                float3 position;
                float3 scale;
                float4 rotation; // quaternion
                float4 color;
            };

            StructuredBuffer<InstanceData> _InstanceBuffer;

            struct appdata
            {
                float3 vertex : POSITION;
                float2 uv : TEXCOORD0;
                uint instanceID : SV_InstanceID;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };

            float4x4 RotationMatrix(float4 q)
            {
                float4x4 m;

                float x = q.x, y = q.y, z = q.z, w = q.w;
                float x2 = x + x, y2 = y + y, z2 = z + z;

                float xx = x * x2;
                float yy = y * y2;
                float zz = z * z2;
                float xy = x * y2;
                float xz = x * z2;
                float yz = y * z2;
                float wx = w * x2;
                float wy = w * y2;
                float wz = w * z2;

                m[0] = float4(1.0 - (yy + zz), xy - wz, xz + wy, 0.0);
                m[1] = float4(xy + wz, 1.0 - (xx + zz), yz - wx, 0.0);
                m[2] = float4(xz - wy, yz + wx, 1.0 - (xx + yy), 0.0);
                m[3] = float4(0.0, 0.0, 0.0, 1.0);

                return m;
            }

            v2f vert(appdata v)
            {
                InstanceData d = _InstanceBuffer[v.instanceID];

                float4x4 rot = RotationMatrix(d.rotation);
                float4 local = float4(v.vertex * d.scale, 1.0);
                float4 world = mul(rot, local);
                world.xyz += d.position;

                v2f o;
                o.vertex = UnityObjectToClipPos(world);
                o.uv = v.uv;
                o.color = d.color;
                return o;
            }

            sampler2D _MainTex;

            fixed4 frag(v2f i) : SV_Target
            {
                return tex2D(_MainTex, i.uv) * i.color;
            }
            ENDCG
        }
    }
}
