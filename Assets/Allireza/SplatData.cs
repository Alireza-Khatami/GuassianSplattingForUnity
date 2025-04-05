using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using System.Text;

public class SplatData : ScriptableObject
{
    public Vector3[] Positions;
    public Vector3[] Axes;
    public Color[] Colors;

    private GraphicsBuffer _positionsBuffer;
    private GraphicsBuffer _axesBuffer;
    private GraphicsBuffer _colorsBuffer;
    public string debugLog;
    public int Count => Positions != null ? Positions.Length : 0;
    //public GraphicsBuffer PositionsBuffer => GetBuffer(ref _positionsBuffer, Positions);
    //public GraphicsBuffer AxesBuffer => GetBuffer(ref _axesBuffer, Axes);
    //public GraphicsBuffer ColorsBuffer => GetBuffer(ref _colorsBuffer, Colors);


    //private GraphicsBuffer GetBuffer<T>(ref GraphicsBuffer buffer, T[] data) where T : unmanaged
    //{
    //    if (buffer == null)
    //    {
    //        buffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, data.Length, Marshal.SizeOf<T>());
    //        buffer.SetData(data);
    //    }
    //    return buffer;
    //}

    //public void Dispose()
    //{
    //    _positionsBuffer?.Dispose();
    //    _axesBuffer?.Dispose();
    //    _colorsBuffer?.Dispose();
    //}

    //private void OnDisable()
    //{
    //    Dispose();
    //}

    public void LoadFromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new SystemException("Invalid PLY file: 'end_header' not found {filePath}");
            return;
        }
        byte[] fileBytes = File.ReadAllBytes(filePath);
        int headerEndIndex = FindHeaderEndIndex(fileBytes);
        if (headerEndIndex == -1)
        {
            throw new SystemException("Invalid PLY file: 'end_header' not found.");
        }
        //createa a new byte array starting from the headerEndIndex
        byte[] dataWihtOutHeaderBytes = new byte[fileBytes.Length - headerEndIndex];
        //excludes the header
        Array.Copy(fileBytes, headerEndIndex, dataWihtOutHeaderBytes, 0, fileBytes.Length - headerEndIndex);
        ReadOnlySpan<ReadData> records = MemoryMarshal.Cast<byte, ReadData>(dataWihtOutHeaderBytes);
        int count = records.Length;
        Positions = new Vector3[count];
        Axes = new Vector3[count * 3];
        Colors = new Color[count];
        debugLog = $"Expected size: 248, actual size: {count}";

        for (int i = 0; i < count; i++)
        {
            var record = records[i];
            Positions[i] = new(record.x, record.z, record.y);
            //float rotX = (record.rx - 128f) / 128f;
            //float rotY = (record.ry - 128f) / 128f;
            //float rotZ = (record.rz - 128f) / 128f;
            //float rotW = (1 - 128f) / 128f;//(record.rz-128)/128f;
            //Quaternion rot = new(-rotX, -rotY, rotZ, rotW);
            //Axes[i * 3 + 0] = rot * new Vector3(record.sx, 0, 0);
            //Axes[i * 3 + 1] = rot * new Vector3(0, record.sy, 0);
            //Axes[i * 3 + 2] = rot * new Vector3(0, 0, record.sz);
            Colors[i] = new Color(record.f_dc_0 , record.f_dc_1 , record.f_dc_2, record.opacity );
        }
    }
    int FindHeaderEndIndex(byte[] fileBytes)
    {
        string fileContent = Encoding.ASCII.GetString(fileBytes);
        int index = fileContent.IndexOf("end_header");
        if (index >= 0)
        {
            int newLineIndex = fileContent.IndexOf('\n', index);
            return newLineIndex >= 0 ? newLineIndex + 1 : -1;
        }
        return -1;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct ReadData
    {
        public float x, y, z; // position
        public float nx, ny, nz;
        public float f_dc_0, f_dc_1, f_dc_2;
        public float f_rest_0, f_rest_1, f_rest_2, f_rest_3, f_rest_4, f_rest_5, f_rest_6, f_rest_7, f_rest_8, f_rest_9;
        public float f_rest_10, f_rest_11, f_rest_12, f_rest_13, f_rest_14, f_rest_15, f_rest_16, f_rest_17, f_rest_18, f_rest_19;
        public float f_rest_20, f_rest_21, f_rest_22, f_rest_23, f_rest_24, f_rest_25, f_rest_26, f_rest_27, f_rest_28, f_rest_29;
        public float f_rest_30, f_rest_31, f_rest_32, f_rest_33, f_rest_34, f_rest_35, f_rest_36, f_rest_37, f_rest_38, f_rest_39;
        public float f_rest_40, f_rest_41, f_rest_42, f_rest_43, f_rest_44;
        public float opacity; //opecity
        public float sx, sy, sz;
        public float rx, ry, rz,rw; 
    }
}
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Runtime.InteropServices;
//using System.Text;
//using UnityEngine;

////[CreateAssetMenu(fileName = "NewMyData", menuName = "MyGame/MyData")]s
//public class SplatData : ScriptableObject
//{
//    public List<InstaneData> instances;
//    public void loadPlyFile(string filePath)
//    {
//        instances = new List<InstaneData>();
//        if (!File.Exists(filePath))
//        {
//            Debug.LogError($"PLY file not found: {filePath}");
//            return;
//        }
//        byte[] fileBytes = File.ReadAllBytes(filePath);
//        int headerEndIndex = FindHeaderEndIndex(fileBytes);
//        if (headerEndIndex == -1)
//        {
//            Debug.LogError("Invalid PLY file: 'end_header' not found.");
//            return;
//        }
//        //createa a new byte array starting from the headerEndIndex
//        byte[] dataWihtOutHeaderBytes = new byte[fileBytes.Length - headerEndIndex];
//        Array.Copy(fileBytes, headerEndIndex, dataWihtOutHeaderBytes, 0, fileBytes.Length - headerEndIndex);
//        //float x = BitConverter.ToSingle(fileBytes, 0);

//        ReadOnlySpan<SlimPlyData> records = MemoryMarshal.Cast<byte, SlimPlyData>( dataWihtOutHeaderBytes);

//        foreach (var record in records)
//        {
//            InstaneData data = parsePlyFileToInstance(record);
//            instances.Add(data);
//        }
//    }
//    public  InstaneData parsePlyFileToInstance( SlimPlyData record)
//    {

//        InstaneData data = new InstaneData();
//        data.position = new Vector3(record.x,record.y, record.z);
//        data.scale = new Vector3(.01f * record.scale_0, .01f *  record.scale_1, .01f * record.scale_2);
//        data.rotation = new Quaternion(record.rot_0, record.rot_1, record.rot_2, record.rot_3);
//        data.color = new Color(record.f_dc_0, record.f_dc_1 , record.f_dc_2 , record.opacity);
//        return data;
//    }

//    [StructLayout(LayoutKind.Sequential, Pack = 1)]
//    public struct SlimPlyData
//    {
//        public float x, y, z;
//        public float nx, ny, nz;
//        public float f_dc_0, f_dc_1, f_dc_2;
//        // Placeholder to skip over 180 bytes of f_rest
//        //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 180)]
//        //private byte[] skip_f_rest;
//        public float f_rest_0, f_rest_1, f_rest_2, f_rest_3, f_rest_4, f_rest_5, f_rest_6, f_rest_7, f_rest_8, f_rest_9;
//        public float f_rest_10, f_rest_11, f_rest_12, f_rest_13, f_rest_14, f_rest_15, f_rest_16, f_rest_17, f_rest_18, f_rest_19;
//        public float f_rest_20, f_rest_21, f_rest_22, f_rest_23, f_rest_24, f_rest_25, f_rest_26, f_rest_27, f_rest_28, f_rest_29;
//        public float f_rest_30, f_rest_31, f_rest_32, f_rest_33, f_rest_34, f_rest_35, f_rest_36, f_rest_37, f_rest_38, f_rest_39;
//        public float f_rest_40, f_rest_41, f_rest_42, f_rest_43, f_rest_44;
//        public float opacity;
//        public float scale_0, scale_1, scale_2;
//        public float rot_0, rot_1, rot_2, rot_3;
//    }
//    [StructLayout(LayoutKind.Sequential, Pack = 1)]
//    public struct InstaneData
//    {
//        public Vector3 position;
//        public Vector3 scale;
//        public Quaternion rotation;
//        public Color color;
//    }
//    int FindHeaderEndIndex(byte[] fileBytes)
//    {
//        string fileContent = Encoding.ASCII.GetString(fileBytes);
//        int index = fileContent.IndexOf("end_header");
//        if (index >= 0)
//        {
//            int newLineIndex = fileContent.IndexOf('\n', index);
//            return newLineIndex >= 0 ? newLineIndex + 1 : -1;
//        }
//        return -1;
//    }

//}
