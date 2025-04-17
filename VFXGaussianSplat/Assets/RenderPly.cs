using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;

public class RenderPly : MonoBehaviour
{

    GraphicsBuffer instanceBuffer;
    //public Material instanceMaterial;
    [SerializeField] private AKPyloader plyLoader;
    [SerializeField] private VisualEffect vfx;
    [SerializeField] private ExposedProperty bufferProperty = "DataSource";
    private int instanceCount;
    //public Mesh instanceMesh;
    void Awake()
    {
        plyLoader.LoadAndCreateBuffer();
        instanceCount = plyLoader.instances.Count;
        if (instanceBuffer != null)
            instanceBuffer.Release();

        instanceBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, instanceCount, sizeof(float)*(5*3 + 4));
        instanceBuffer.SetData(plyLoader.instances.ToArray());


        vfx.SetGraphicsBuffer(bufferProperty, instanceBuffer);
        Debug.Log($"Loaded {instanceCount} instances from PLY file.");
    }

    // Update is called once per frame
    void Update()
    {
        /*
        if (instanceBuffer != null && instanceCount > 0)
        {
            int increment = 3000000;
            for (int i = 0; i < instanceCount; i += increment)
            {
                //instanceMaterial.SetInteger("_BaseIndex", i);
                var matProps = new MaterialPropertyBlock();
                matProps.SetInt("_BaseIndex", i);

                Graphics.DrawMeshInstancedProcedural(
                    instanceMesh, 0, instanceMaterial,
                    new Bounds(Vector3.zero, Vector3.one * 10000),
                    (instanceCount - i < increment) ? instanceCount - i : increment,
                    matProps);
            }
        }
        else
        {
            Debug.LogWarning("Instance buffer is null or instance count is zero.");
        } */
    }
    void OnDestroy()
    {
        instanceBuffer?.Release();
    }

}
