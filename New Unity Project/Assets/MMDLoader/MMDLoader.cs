using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

/// <summary>
/// ヘッダー情報クラス
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class HEADER
{
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
    char[] magic; // "Pmd"
    float version; // 00 00 80 3F == 1.00 // 訂正しました。コメントありがとうございます
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
    char[] model_name;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
    char[] comment;
};


/// <summary>
/// ヘッダー情報クラス
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class t_vertex
{
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
    public float[] pos; // x, y, z // 座標

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
    public float[] normal_vec; // nx, ny, nz // 法線ベクトル

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
    public float[] uv; // u, v // UV座標 // MMDは頂点UV

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
    public ushort[] bone_num; // ボーン番号1、番号2 // モデル変形(頂点移動)時に影響

    public byte bone_weight; // ボーン1に与える影響度 // min:0 max:100 // ボーン2への影響度は、(100 - bone_weight)
    public byte edge_flag; // 0:通常、1:エッジ無効 // エッジ(輪郭)が有効の場合
};

/// <summary>
/// ヘッダー情報クラス
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class t_material
{
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
    public float[] diffuse_color; // dr, dg, db // 減衰色

    public float alpha; // 減衰色の不透明度
    public float specularity;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
    public float[] specular_color; // sr, sg, sb // 光沢色

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
    public float[] mirror_color; // mr, mg, mb // 環境色(ambient)

    public byte toon_index; // toon??.bmp // 0.bmp:0xFF, 1(01).bmp:0x00 ・・・ 10.bmp:0x09
    public byte edge_flag; // 輪郭、影
    public uint face_vert_count; // 面頂点数 // 面数ではありません。この材質で使う、面頂点リストのデータ数です。

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
    public char[] texture_file_name; // テクスチャファイル名またはスフィアファイル名 // 20バイトぎりぎりまで使える(終端の0x00は無くても動く)

};


public class MMDLoader : MonoBehaviour
{
    FileStream fileStream;


    /// <summary>
    /// pmdファイル名
    /// </summary>
    string fileName = "/Lat式ミクver2.31/Lat式ミクVer2.31_Normal.pmd";

    HEADER header;
    uint vert_count; // 頂点数
    List<t_vertex> tVertexList = new List<t_vertex>(); // 頂点データ(38Bytes/頂点)
    uint face_vert_count; // 頂点数 // 面数ではありません
    List<int> face_vert_index = new List<int>(); // 頂点番号(3個/面)
    uint material_count; // 材質数
    List<t_material> tMaterialiList = new List<t_material>();


    /// <summary>
    /// ヘッダー読み込み
    /// </summary>
    void ReadHeader()
    {
        int count = Marshal.SizeOf(typeof(HEADER));
        byte[] readBuffer = new byte[count];
        BinaryReader reader = new BinaryReader(fileStream);
        readBuffer = reader.ReadBytes(count);
        GCHandle handle = GCHandle.Alloc(readBuffer, GCHandleType.Pinned);
        header = (HEADER)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(HEADER));
        handle.Free();
    }

    /// <summary>
    /// 頂点リスト読み込み
    /// </summary>
    void ReadVertexList()
    {
       int count = Marshal.SizeOf(typeof(uint));
        byte[] readBuffer = new byte[count];
        BinaryReader reader = new BinaryReader(fileStream);
        readBuffer = reader.ReadBytes(count);
        GCHandle handle = GCHandle.Alloc(readBuffer, GCHandleType.Pinned);
        vert_count = (uint)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(uint));

        for(int i = 0; i < vert_count; i++)
        {
            count = Marshal.SizeOf(typeof(t_vertex));
            readBuffer = new byte[count];
            reader = new BinaryReader(fileStream);
            readBuffer = reader.ReadBytes(count);
            handle = GCHandle.Alloc(readBuffer, GCHandleType.Pinned);
            t_vertex tVertex = (t_vertex)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(t_vertex));
            tVertexList.Add(tVertex);
        }

        handle.Free();
    }

    /// <summary>
    /// 面情報読み込み
    /// </summary>
    void ReadFace()
    {
        int count = Marshal.SizeOf(typeof(uint));
        byte[] readBuffer = new byte[count];
        BinaryReader reader = new BinaryReader(fileStream);
        readBuffer = reader.ReadBytes(count);
        GCHandle handle = GCHandle.Alloc(readBuffer, GCHandleType.Pinned);
        face_vert_count = (uint)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(uint));

        for(int i = 0; i < face_vert_count; i++)
        {
            count = Marshal.SizeOf(typeof(ushort));
            readBuffer = new byte[count];
            reader = new BinaryReader(fileStream);
            readBuffer = reader.ReadBytes(count);
            handle = GCHandle.Alloc(readBuffer, GCHandleType.Pinned);
            ushort face_vert = (ushort)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(ushort));
            face_vert_index.Add((int)face_vert);
        }

        handle.Free();
    }

    /// <summary>
    /// マテリアルリスト読み込み
    /// </summary>
    void ReadMaterial()
    {
        int count = Marshal.SizeOf(typeof(uint));
        byte[] readBuffer = new byte[count];
        BinaryReader reader = new BinaryReader(fileStream);
        readBuffer = reader.ReadBytes(count);
        GCHandle handle = GCHandle.Alloc(readBuffer, GCHandleType.Pinned);
        material_count = (uint)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(uint));

        for (int i = 0; i < material_count; i++)
        {
            count = Marshal.SizeOf(typeof(t_material));
            readBuffer = new byte[count];
            reader = new BinaryReader(fileStream);
            readBuffer = reader.ReadBytes(count);
            handle = GCHandle.Alloc(readBuffer, GCHandleType.Pinned);
            t_material tMateriali = (t_material)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(t_material));
            string textureFileName = "";
            for (int j = 0; j < 20; j++)
            {
                textureFileName += tMateriali.texture_file_name[j];
            }
            UnityEngine.Debug.Log(textureFileName);
            tMaterialiList.Add(tMateriali);
        }
    }

    void DrawMesh(int offsetPos, int faceVertCount, string textureName)
    {
        GameObject obj = new GameObject();
        obj.transform.parent = this.transform;
        obj.transform.localScale = Vector3.one;
        obj.AddComponent<MeshRenderer>();
        obj.AddComponent<MeshFilter>();

        Mesh mesh = new Mesh();

        // 頂点リストを作成
        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvList = new List<Vector2>();

        int i = 0;

        foreach (t_vertex t_Vertex in tVertexList)
        {
            Vector3 vertex = new Vector3(t_Vertex.pos[0], t_Vertex.pos[1], t_Vertex.pos[2]);
            vertices.Add(vertex);

            Vector3 uv = new Vector3(t_Vertex.uv[0], 1 - t_Vertex.uv[1]);

            uvList.Add(uv);
        }

        List<int> facevertList = new List<int>();
        for(i = offsetPos; i < offsetPos + faceVertCount; i++)
        {
            facevertList.Add(face_vert_index[i]);
        }
        mesh.SetVertices(vertices);
        //mesh.SetUVs(0, uvList);
        mesh.uv = uvList.ToArray();
        mesh.SetTriangles(facevertList, 0);
        MeshFilter meshFilter = obj.GetComponent<MeshFilter>();

        Texture J_SEIKAI01;
        string fileName = textureName.Split('*')[0];
        fileName = fileName.Split('.')[0];
        J_SEIKAI01 = Resources.Load("lat/"+fileName) as Texture;
        obj.GetComponent<Renderer>().material.shader = Shader.Find("Unlit/Texture");
        obj.name = fileName;
        obj.GetComponent<Renderer>().material.SetTexture("_MainTex", J_SEIKAI01);

        meshFilter.mesh = mesh;
        meshFilter.mesh.RecalculateBounds();
    }


    // Start is called before the first frame update
    void Start()
    {
        fileStream = new FileStream(Application.dataPath + fileName, FileMode.Open, FileAccess.Read);
        ReadHeader();
        ReadVertexList();
        ReadFace();
        ReadMaterial();
        fileStream.Dispose();

        int offsetPos = 0;

        foreach(t_material tMaterial in tMaterialiList)
        {
            DrawMesh(offsetPos, (int)tMaterial.face_vert_count, new string(tMaterial.texture_file_name));
            offsetPos += (int)tMaterial.face_vert_count;
        }

    }

}
