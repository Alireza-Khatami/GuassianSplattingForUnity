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
            Cull Off //  Disables backface culling
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"

            struct InstanceData
            {
                float3 position;
                float3 scale;
                float4 rotation; // quaternion (we'll use .w as Z-rotation)
                float4 color;
            };

            StructuredBuffer<InstanceData> _InstanceBuffer;
            uniform uint _BaseIndex;

            struct appdata
            {
                float3 vertex : POSITION;  // Unity quad in XY plane: (-0.5 to 0.5, -0.5 to 0.5)
                float2 uv : TEXCOORD0;
                uint instanceID : SV_InstanceID;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };

            float2 Rotate2D(float2 pos, float angle)
            {
                float s = sin(angle);
                float c = cos(angle);
                return float2(
                    pos.x * c - pos.y * s,
                    pos.x * s + pos.y * c
                );
            }
            float2 ApplyQuaternionTo2D(float2 pos, float4 q)
            {
                // Convert quaternion rotation to 2D (around Z) using imaginary i & j parts
                // Using standard 2D quaternion rotation formula
                float angle = atan2(2.0 * (q.w * q.z + q.x * q.y), 1.0 - 2.0 * (q.y * q.y + q.z * q.z));
                float s = sin(angle);
                float c = cos(angle);
                return float2(
                    pos.x * c - pos.y * s,
                    pos.x * s + pos.y * c
                );
            }

            // Returns column vectors of the rotation matrix from a quaternion
            void QuaternionToBasis(float4 q, out float3 X, out float3 Y, out float3 Z)
            {
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

                X = float3(1.0 - (yy + zz), xy + wz, xz - wy);
                Y = float3(xy - wz, 1.0 - (xx + zz), yz + wx);
                Z = float3(xz + wy, yz - wx, 1.0 - (xx + yy));
            }

            float3 TransformWorldToViewDir(float3 dir)
            {
                return mul((float3x3)UNITY_MATRIX_V, dir);
            }

            void SelectAxes(float3 X, float3 Y, float3 Z, out float3 x, out float3 y)
            {
                float3 viewX = TransformWorldToViewDir(X);
                float3 viewY = TransformWorldToViewDir(Y);
                float3 viewZ = TransformWorldToViewDir(Z);

                float areaXY = abs(viewX.x * viewY.y - viewX.y * viewY.x);
                float areaYZ = abs(viewY.x * viewZ.y - viewY.y * viewZ.x);
                float areaXZ = abs(viewX.x * viewZ.y - viewX.y * viewZ.x);	

                float xyMask = step(areaYZ, areaXY) * step(areaXZ, areaXY);
                float yzMask = step(areaXY, areaYZ) * step(areaXZ, areaYZ);
                float xzMask = step(areaXY, areaXZ) * step(areaYZ, areaXZ);

                x = xyMask * X + yzMask * Y + xzMask * X;
                y = xyMask * Y + yzMask * Z + xzMask * Z;
            }

            v2f vert(appdata v)
            {
                InstanceData d = _InstanceBuffer[v.instanceID + _BaseIndex];

                // Step 1: convert quaternion to basis
                float3 X, Y, Z;
                QuaternionToBasis(d.rotation, X, Y, Z);

                // Step 2: select optimal facing axes
                float3 axis1, axis2;
                SelectAxes(X, Y, Z, axis1, axis2);

                // Step 3: apply non-uniform scale and vertex projection
                float2 vtx = v.vertex.xy; // quad assumed in XY
                float3 offset = vtx.x * axis1 * d.scale.x + vtx.y * axis2 * d.scale.y;
                float3 worldPos = d.position + offset;
                v2f o;
                o.vertex = UnityObjectToClipPos(float4(worldPos, 1.0));
                o.uv = v.uv;
                o.color = d.color;
                return o;
            }

            sampler2D _MainTex;


            fixed4 frag(v2f i) : SV_Target
            {
                float2 centerUV = i.uv - 0.5;
                float distSq = dot(centerUV, centerUV);
                float gaussian = exp(-distSq );//*25.0f);// adjust falloff sharpness here

                fixed4 texColor = tex2D(_MainTex, i.uv);
                fixed3 linearColor = GammaToLinearSpace(texColor);
                i.color.a *= gaussian;
                texColor = fixed4(linearColor,1) * i.color;
                //clip(texColor.a); optional: discard very transparent pixels
                return texColor;
            }

            ENDCG
        }
    }
}

 /* old way of always facing the camera{
    InstanceData d = _InstanceBuffer[v.instanceID + _BaseIndex];
    // float2 rotated = Rotate2D(v.vertex.xy, d.rotation.w);
    float2 rotated = ApplyQuaternionTo2D(v.vertex.xy, d.rotation);
    rotated.y *= -1.0;
    rotated *= d.scale.xy;
    float3 right = UNITY_MATRIX_IT_MV[0].xyz;
    float3 up = -UNITY_MATRIX_IT_MV[1].xyz;
    // Final world position
    float3 worldPos = d.position + rotated.x * right + rotated.y * up;
    }*/
//original fragment
// fixed4 frag(v2f i) : SV_Target
// {
//     return tex2D(_MainTex, i.uv) * i.color;
// }

// Shader "Custom/InstanceShader"
// {
//     Properties
//     {
//         _MainTex ("Texture", 2D) = "white" {}
//     }
//     SubShader
//     {
//         Tags { "RenderType"="Opaque" }
//         Pass
//         {
//             CGPROGRAM
//             #pragma vertex vert
//             #pragma fragment frag
//             #pragma multi_compile_instancing
//             #include "UnityCG.cginc"

//             struct InstanceData
//             {
//                 float3 position;
//                 float3 scale;
//                 float4 rotation; quaternion
//                 float4 color;
//             };

//             StructuredBuffer<InstanceData> _InstanceBuffer;
//             uniform uint _BaseIndex;

//             struct appdata
//             {
//                 float3 vertex : POSITION;
//                 float2 uv : TEXCOORD0;
//                 uint instanceID : SV_InstanceID;
//             };

//             struct v2f
//             {
//                 float2 uv : TEXCOORD0;
//                 float4 vertex : SV_POSITION;
//                 float4 color : COLOR;
//             };

//             Optional: rotate around view axis (Z) using instance.rotation.w
//             float2 Rotate2D(float2 p, float angle)
//             {
//                 float s = sin(angle);
//                 float c = cos(angle);
//                 return float2(c * p.x - s * p.y, s * p.x + c * p.y);
//             }

//             v2f vert(appdata v)
//             {
//                 InstanceData d = _InstanceBuffer[v.instanceID + _BaseIndex];

//                 Get camera-facing axes
//                 float3 right = UNITY_MATRIX_IT_MV[0].xyz; object-to-view-space inverse transpose
//                 float3 up    = UNITY_MATRIX_IT_MV[1].xyz;

//                 Rotate quad vertices in 2D (optional: Z-rotation from quaternion.w)
//                 float2 rotated = Rotate2D(v.vertex.xz, d.rotation.w); assume plane in XZ
//                 float3 localOffset = rotated.x * right + rotated.y * up;
//                 localOffset *= d.scale.x; assume uniform scale for simplicity, or use d.scale per axis

//                 float3 worldPos = d.position + localOffset;

//                 v2f o;
//                 o.vertex = UnityObjectToClipPos(float4(worldPos, 1.0));
//                 o.uv = v.uv;
//                 o.color = d.color;
//                 return o;
//             }

//             sampler2D _MainTex;

//             fixed4 frag(v2f i) : SV_Target
//             {
//                 return tex2D(_MainTex, i.uv) * i.color;
//             }
//             ENDCG
//         }
//     }
// }



// Shader "Custom/InstanceShader"
// {
//     Properties
//     {
//         _MainTex ("Texture", 2D) = "white" {}
//     }
//     SubShader
//     {
//         Tags { "RenderType"="Opaque" }
//         Pass
//         {
//             CGPROGRAM
//             #pragma vertex vert
//             #pragma fragment frag
//             #pragma multi_compile_instancing
//             #include "UnityCG.cginc"

//             struct InstanceData
//             {
//                 float3 position;
//                 float3 scale;
//                 float4 rotation; quaternion
//                 float4 color;
//             };

//             StructuredBuffer<InstanceData> _InstanceBuffer;
//             uniform uint _BaseIndex;

//             struct appdata
//             {
//                 float3 vertex : POSITION;
//                 float2 uv : TEXCOORD0;
//                 uint instanceID : SV_InstanceID;
//             };

//             struct v2f
//             {
//                 float2 uv : TEXCOORD0;
//                 float4 vertex : SV_POSITION;
//                 float4 color : COLOR;
//             };

//             float4x4 RotationMatrix(float4 q)
//             {
//                 float4x4 m;

//                 float x = q.x, y = q.y, z = q.z, w = q.w;
//                 float x2 = x + x, y2 = y + y, z2 = z + z;

//                 float xx = x * x2;
//                 float yy = y * y2;
//                 float zz = z * z2;
//                 float xy = x * y2;
//                 float xz = x * z2;
//                 float yz = y * z2;
//                 float wx = w * x2;
//                 float wy = w * y2;
//                 float wz = w * z2;

//                 m[0] = float4(1.0 - (yy + zz), xy - wz, xz + wy, 0.0);
//                 m[1] = float4(xy + wz, 1.0 - (xx + zz), yz - wx, 0.0);
//                 m[2] = float4(xz - wy, yz + wx, 1.0 - (xx + yy), 0.0);
//                 m[3] = float4(0.0, 0.0, 0.0, 1.0);

//                 return m;
//             }

//             v2f vert(appdata v)
//             {
//                 InstanceData d = _InstanceBuffer[v.instanceID + _BaseIndex];

//                 float4x4 rot = RotationMatrix(d.rotation);
//                 float4 local = float4(v.vertex * d.scale, 1.0);
//                 float4 world = mul(rot, local);
//                 world.xyz += d.position;

//                 v2f o;
//                 o.vertex = UnityObjectToClipPos(world);
//                 o.uv = v.uv;
//                 o.color = d.color;
//                 return o;
//             }

//             sampler2D _MainTex;

//             fixed4 frag(v2f i) : SV_Target
//             {
//                 return tex2D(_MainTex, i.uv) * i.color;
//             }
//             ENDCG
//         }
//     }
// }
