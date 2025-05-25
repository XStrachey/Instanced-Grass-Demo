using UnityEngine;

public class GrassRenderer : MonoBehaviour
{
    public ComputeShader grassCompute;
    public Material grassMaterial;

    public int grassCount = 262144; // 512 x 512
    private ComputeBuffer argsBuffer;
    private ComputeBuffer positionBuffer;  // float4(x, y, z, angle)
    private ComputeBuffer growDirBuffer;   // float3(growth direction)
    private Mesh grassMesh;

    /// <summary>
    /// 构建一撮草的 Mesh：菱形草叶（顶尖 + 中段 + 底尖）
    /// </summary>
    Mesh BuildDiamondGrassBlade(int segments = 4, float height = 0.4f, float width = 0.05f)
    {
        Mesh mesh = new Mesh();

        int middleVertCount = segments * 2;
        int totalVertCount = 1 + middleVertCount + 1; // 顶 + 中段左右点 + 底
        Vector3[] vertices = new Vector3[totalVertCount];
        int totalTriangleCount = 1 + (segments - 1) * 2 + 1;
        int[] triangles = new int[totalTriangleCount * 3];

        float halfWidth = width * 0.5f;

        // 顶点构建
        vertices[0] = new Vector3(0, height, 0);
        for (int i = 0; i < segments; i++)
        {
            float t = (float)(i + 1) / (segments + 1);
            float y = height * (1 - t);
            vertices[1 + i * 2 + 0] = new Vector3(-halfWidth, y, 0);
            vertices[1 + i * 2 + 1] = new Vector3(+halfWidth, y, 0);
        }
        vertices[totalVertCount - 1] = new Vector3(0, 0, 0); // 底尖

        // 三角形索引构建
        int tIndex = 0;
        triangles[tIndex++] = 0; triangles[tIndex++] = 1; triangles[tIndex++] = 2; // 顶三角
        for (int i = 0; i < segments - 1; i++)
        {
            int bl = 1 + i * 2;
            int br = bl + 1;
            int tl = bl + 2;
            int tr = tl + 1;

            triangles[tIndex++] = bl; triangles[tIndex++] = tl; triangles[tIndex++] = br;
            triangles[tIndex++] = br; triangles[tIndex++] = tl; triangles[tIndex++] = tr;
        }
        int bottom = totalVertCount - 1;
        triangles[tIndex++] = bottom;
        triangles[tIndex++] = bottom - 1;
        triangles[tIndex++] = bottom - 2;

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        return mesh;
    }

    void Start()
    {
        // 1. 构建草叶 mesh（每撮草）
        grassMesh = BuildDiamondGrassBlade(8, 0.5f, 0.06f);

        // 2. 初始化草数据 buffer
        positionBuffer = new ComputeBuffer(grassCount, sizeof(float) * 4); // xyz + angle
        growDirBuffer = new ComputeBuffer(grassCount, sizeof(float) * 3);  // grow direction

        // 3. 调用 compute shader 写入草的位置、方向、旋转
        grassCompute.SetBuffer(0, "positionBuffer", positionBuffer);
        grassCompute.SetBuffer(0, "growDirBuffer", growDirBuffer);
        grassCompute.Dispatch(0, grassCount / 8, 1, 1);

        // 4. 设置材质中的 buffer 参数
        grassMaterial.SetBuffer("_GrassData", positionBuffer);
        grassMaterial.SetBuffer("_GrowDirBuffer", growDirBuffer);

        // 5. 初始化 DrawMeshInstancedIndirect 参数
        uint[] args = new uint[5] {
            grassMesh.GetIndexCount(0),
            (uint)grassCount,
            grassMesh.GetIndexStart(0),
            grassMesh.GetBaseVertex(0),
            0
        };
        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        argsBuffer.SetData(args);
    }

    void Update()
    {
        Graphics.DrawMeshInstancedIndirect(
            grassMesh, 0, grassMaterial,
            new Bounds(Vector3.zero, new Vector3(100, 10, 100)),
            argsBuffer
        );
    }

    void OnDestroy()
    {
        argsBuffer?.Release();
        positionBuffer?.Release();
        growDirBuffer?.Release();
    }
}
