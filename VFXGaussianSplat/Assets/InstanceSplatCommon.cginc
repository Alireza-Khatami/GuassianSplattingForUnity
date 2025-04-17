

#include "UnityCG.cginc"


struct InstanceData
{
    float3 position;
    float3 scale;
    float4 rotation;
    float4 color;
};
StructuredBuffer<InstanceData> _InstanceBuffer;
struct appdata
{
    float3 vertex : POSITION; // Unity Quad in XY plane
    float2 uv : TEXCOORD0;
    uint instanceID : SV_InstanceID;
};

// Varying structs
struct v2fDepth
{
    float4 vertex : SV_POSITION;
};

struct v2fColor
{
    float2 uv : TEXCOORD0;
    float4 vertex : SV_POSITION;
    float4 color : COLOR;
};

// --- Math Utilities ---
float2 Rotate2D(float2 pos, float angle)
{
    float s = sin(angle);
    float c = cos(angle);
    return float2(pos.x * c - pos.y * s, pos.x * s + pos.y * c);
}

void QuaternionToBasis(float4 q, out float3 X, out float3 Y, out float3 Z)
{
    float x = q.x, y = q.y, z = q.z, w = q.w;
    float x2 = x + x, y2 = y + y, z2 = z + z;
    float xx = x * x2, yy = y * y2, zz = z * z2;
    float xy = x * y2, xz = x * z2, yz = y * z2;
    float wx = w * x2, wy = w * y2, wz = w * z2;

    X = float3(1.0 - (yy + zz), xy + wz, xz - wy);
    Y = float3(xy - wz, 1.0 - (xx + zz), yz + wx);
    Z = float3(xz + wy, yz - wx, 1.0 - (xx + yy));
}

float3 TransformWorldToViewDir(float3 dir)
{
    return mul((float3x3) UNITY_MATRIX_V, dir);
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

// --- Vertex Functions ---

// Depth pass vertex function
v2fDepth vertDepth(appdata v)
{
    InstanceData d = _InstanceBuffer[v.instanceID];
    float3 X, Y, Z;
    QuaternionToBasis(d.rotation, X, Y, Z);
    float3 axis1, axis2;
    SelectAxes(X, Y, Z, axis1, axis2);
    float3 tem = v.vertex*d.scale;
    float2 vtx = tem.xy;
    float3 offset = vtx.x * axis1 + vtx.y * axis2 ;
    float3 worldPos = d.position + offset;

    v2fDepth o;
    o.vertex = UnityObjectToClipPos(float4(worldPos, 1.0));
    return o;
}

// Color pass vertex function
v2fColor vertColor(appdata v)
{
    InstanceData d = _InstanceBuffer[v.instanceID];
    float3 X, Y, Z;
    QuaternionToBasis(d.rotation, X, Y, Z);
    float3 axis1, axis2;
    SelectAxes(X, Y, Z, axis1, axis2);
    float3 tem = v.vertex * d.scale;
    float2 vtx = tem.xy;
    float3 offset = vtx.x * axis1 + vtx.y * axis2 ;
    float3 worldPos = d.position + offset;

    v2fColor o;
    o.vertex = UnityObjectToClipPos(float4(worldPos, 1.0));
    o.uv = v.uv;
    o.color = d.color;
    return o;
}


