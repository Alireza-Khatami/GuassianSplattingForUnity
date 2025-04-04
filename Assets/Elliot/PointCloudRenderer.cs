using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PointCloudRenderer : MonoBehaviour
{
    public Material material;
    public PlyLoader pointCloudLoader;

    private GraphicsBuffer vertBuffer;
    private GraphicsBuffer colorBuffer;

    // Start is called before the first frame update
    void Start()
    {
        pointCloudLoader.Load();
        vertBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, pointCloudLoader.points.Count, sizeof(float)*3);
        vertBuffer.SetData(pointCloudLoader.points);
        colorBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, pointCloudLoader.points.Count, sizeof(float)*3);
        colorBuffer.SetData(pointCloudLoader.colors);
    }

    // Update is called once per frame
    void Update()
    {
        RenderParams rp = new(material);
        rp.worldBounds = new Bounds(Vector3.zero, 10000*Vector3.one);
        rp.matProps = new MaterialPropertyBlock();
        rp.matProps.SetBuffer("_Positions", vertBuffer);
        rp.matProps.SetBuffer("_Colors", colorBuffer);
        rp.matProps.SetInt("_BaseVertexIndex", 0);
        rp.matProps.SetInt("_BaseColorIndex", 0);
        rp.matProps.SetMatrix("_ObjectToWorld", Matrix4x4.Translate(new Vector3(4.5f, 0, 0)));
        rp.matProps.SetFloat("_NumInstances", pointCloudLoader.points.Count);
        Graphics.RenderPrimitives(rp, MeshTopology.Triangles, pointCloudLoader.points.Count, 1);
    }
}
