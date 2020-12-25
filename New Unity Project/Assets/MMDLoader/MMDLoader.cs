using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;


public class MMDLoader : MonoBehaviour
{
    /// <summary>
    /// pmdファイル名
    /// </summary>
    string fileName = "/Lat式ミクver2.31/Lat式ミクVer2.31_Normal.pmd";

    /// <summary>
    /// ヘッダー情報クラス
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private class HEADER
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
    /// ヘッダー読み込み
    /// </summary>
    void ReadHeader()
    {
        FileStream fs = new FileStream(Application.dataPath+ fileName, FileMode.Open, FileAccess.Read);
        HEADER header;
        int count = Marshal.SizeOf(typeof(HEADER));
        byte[] readBuffer = new byte[count];
        BinaryReader reader = new BinaryReader(fs);
        readBuffer = reader.ReadBytes(count);
        GCHandle handle = GCHandle.Alloc(readBuffer, GCHandleType.Pinned);
        header = (HEADER)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(HEADER));
        handle.Free();
        fs.Dispose();
    }

    // Start is called before the first frame update
    void Start()
    {
        ReadHeader();
    }

}
