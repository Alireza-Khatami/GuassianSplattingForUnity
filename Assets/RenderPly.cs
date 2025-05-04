using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class RenderPly : MonoBehaviour
{

    ComputeBuffer instanceBuffer;
    public Material instanceMaterial;
    [SerializeField] private PlyLoader plyLoader;
    private int instanceCount;
    public Mesh instanceMesh;
    void Start()
    {
        plyLoader.LoadAndCreateBuffer();
        instanceCount = plyLoader.instances.Count;
        if (instanceBuffer != null)
            instanceBuffer.Release();

        instanceBuffer = new ComputeBuffer(instanceCount, sizeof(float) * (5*3 + 4));
        instanceBuffer.SetData(plyLoader.instances.ToArray());
        instanceMaterial.SetBuffer("_InstanceBuffer", instanceBuffer);
        Debug.Log($"Loaded {instanceCount} instances from PLY file.");
    }

    // Update is called once per frame
    void Update()
    {
        // Manually reorder splats by depth
        if (Input.GetKeyDown(KeyCode.R)) {
            plyLoader.FullySortInstances();
            instanceBuffer.SetData(plyLoader.instances.ToArray());
        }
        if (instanceBuffer != null && instanceCount > 0)
        {
            int increment = 3000000;
            for (int i = 0; i < instanceCount; i += increment)
            {
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
        }
    }
    void OnDestroy()
    {
        instanceBuffer?.Release();
    }

}
