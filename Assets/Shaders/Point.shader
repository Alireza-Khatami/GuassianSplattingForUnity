Shader "PointCloudShader"
{
    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 color : COLOR0;
            };

            // StructuredBuffer<int> _Triangles;
            StructuredBuffer<float3> _Positions;
            StructuredBuffer<float4> _Colors;
            // uniform uint _StartIndex;
            uniform uint _BaseVertexIndex;
            uniform uint _BaseColorIndex;
            uniform float4x4 _ObjectToWorld;
            // uniform float _NumInstances;

            v2f vert(uint vertexID: SV_VertexID, uint instanceID : SV_InstanceID)
            {
                v2f o;
                // float3 pos = _Positions[_Triangles[vertexID + _StartIndex] + _BaseVertexIndex];
                float3 pos = _Positions[vertexID + _BaseVertexIndex];
                float4 wpos = mul(_ObjectToWorld, float4(pos, 1.0f));
                o.pos = mul(UNITY_MATRIX_VP, wpos);
                // o.color = float4(instanceID / _NumInstances, 0.0f, 0.0f, 0.0f);
                o.color = _Colors[vertexID + _BaseColorIndex];
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                return i.color;
            }
            ENDCG
        }
    }
}