//using Unity.VisualScripting;
//using UnityEngine;

//public class RenderPly : MonoBehaviour
//{

//    ComputeBuffer instanceBuffer;
//    public Material instanceMaterial;
//    [SerializeField] private SplatData plyLoader;
//    private int instanceCount;
//    public Mesh instanceMesh;
//    void Start()
//    {
//        plyLoader.LoadFromFile("//Assets/yourfile.ply");
//        instanceCount = plyLoader.Count;
//        if (instanceBuffer != null)
//            instanceBuffer.Release();

//        instanceBuffer = new ComputeBuffer(instanceCount, sizeof(float) * (3 + 3 + 4 + 4));
//        instanceBuffer.SetData(plyLoader.instances.ToArray());
//        instanceMaterial.SetBuffer("_InstanceBuffer", instanceBuffer);
//        Debug.Log($"Loaded {instanceCount} instances from PLY file.");
//    }

//    // Update is called once per frame
//    void Update()
//    {
//        if (instanceBuffer != null && instanceCount > 0)
//        {
//            Graphics.DrawMeshInstancedProcedural(
//                instanceMesh, 0, instanceMaterial,
//                new Bounds(Vector3.zero, Vector3.one * 10000),
//                instanceCount);
//        }
//        else
//        {
//            Debug.LogWarning("Instance buffer is null or instance count is zero.");
//        }
//    }
//    void OnDestroy()
//    {
//        instanceBuffer?.Release();
//    }

//}
