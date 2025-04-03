using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Unity.Collections;

public class PlyLoader : MonoBehaviour
{
    public TextAsset plyFile;
    public string radienceFieldFile;

    // public List<PlyPoint> points;
    public List<Vector3> points;
    public List<Color32> colors;

    public void Load()
    {
        int numVertices = 0;
        List<string> properties = new();
        points = new();

        var file = File.Open(radienceFieldFile, FileMode.Open, FileAccess.Read, FileShare.Read);
        var reader = new StreamReader(file);
        int readPos = 0;

        string line = reader.ReadLine();
        readPos += line.Length + 1;

        if (line != "ply")
            throw new ArgumentException("Not given a ply file");

        line = reader.ReadLine();
        readPos += line.Length + 1;
        //Check endianness?

        while (line != "end_header")
        {
            line = reader.ReadLine();
            readPos += line.Length + 1;
            if (line == "end_header")
                break;

                var fields = line.Split();

                if (fields[0] == "element" && fields[1] == "vertex") {
                    numVertices = Convert.ToInt32(fields[2]);
                }

                else if (fields[0] == "property") {
                    if (fields[1] != "float")
                        continue;
                    
                    properties.Add(fields[2]);
                }
        }

        reader.BaseStream.Position = readPos;
        var binaryReader = new BinaryReader(file);

        for (int i = 0; i < numVertices; i++)
        {
            Vector3 point = new();
            Color32 color = new();
            foreach (var property in properties)
            {
                float val = binaryReader.ReadSingle();
                switch (property)
                {
                    case "x": point.x = val;
                        break;
                    case "y": point.y = val;
                        break;
                    case "z": point.z = val;
                        break;
                    case "f_dc_0": color.r = (byte)val;
                        break;
                    case "f_dc_1": color.g = (byte)val;
                        break;
                    case "f_dc_2": color.b = (byte)val;
                        break;
                    case "opacity": color.a = (byte)val;
                        break;
                }
            }

            points.Add(point);
            colors.Add(color);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
