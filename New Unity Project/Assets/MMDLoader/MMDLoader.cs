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

public class MMDLoader : MonoBehaviour
{
    FileStream fileStream;


    /// <summary>
    /// pmdファイル名
    /// </summary>
    string fileName = "/Lat式ミクver2.31/Lat式ミクVer2.31_Normal.pmd";
    

    /// <summary>
    /// ヘッダー読み込み
    /// </summary>
    void ReadHeader()
    {
        HEADER header;
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
        uint vert_count; // 頂点数
        List<t_vertex> vertexList = new List<t_vertex>(); // 頂点データ(38Bytes/頂点)

        int count = Marshal.SizeOf(typeof(uint));
        byte[] readBuffer = new byte[count];
        BinaryReader reader = new BinaryReader(fileStream);
        readBuffer = reader.ReadBytes(count);
        GCHandle handle = GCHandle.Alloc(readBuffer, GCHandleType.Pinned);
        vert_count = (uint)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(uint));
        UnityEngine.Debug.Log(vert_count);

        for(int i = 0; i < vert_count; i++)
        {
            count = Marshal.SizeOf(typeof(t_vertex));
            readBuffer = new byte[count];
            reader = new BinaryReader(fileStream);
            readBuffer = reader.ReadBytes(count);
            handle = GCHandle.Alloc(readBuffer, GCHandleType.Pinned);
            t_vertex tVertex = (t_vertex)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(t_vertex));
            vertexList.Add(tVertex);
            Debug.Log(tVertex.pos[0] + ", " + tVertex.pos[1] + ", " + tVertex.pos[2]);
        }

        handle.Free();
    }


    // Start is called before the first frame update
    void Start()
    {
        fileStream = new FileStream(Application.dataPath + fileName, FileMode.Open, FileAccess.Read);

        ReadHeader();
        ReadVertexList();

        fileStream.Dispose();

    }

}
