using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

//• ▌ ▄ ·.  ▄▄▄·  ▄▄ • ▪   ▄▄·  ▄▄▄·  ▌ ▐·      ▐▄• ▄ ▄▄▄ .▄▄▌      ▄▄▄▄▄            ▄▄▌  .▄▄ ·
//·██ ▐███▪▐█ ▀█ ▐█ ▀ ▪██ ▐█ ▌▪▐█ ▀█ ▪█·█▌▪      █▌█▌▪▀▄.▀·██•      •██  ▪     ▪     ██•  ▐█ ▀.
//▐█ ▌▐▌▐█·▄█▀▀█ ▄█ ▀█▄▐█·██ ▄▄▄█▀▀█ ▐█▐█• ▄█▀▄  ·██· ▐▀▀▪▄██▪       ▐█.▪ ▄█▀▄  ▄█▀▄ ██▪  ▄▀▀▀█▄
//██ ██▌▐█▌▐█ ▪▐▌▐█▄▪▐█▐█▌▐███▌▐█ ▪▐▌ ███ ▐█▌.▐▌▪▐█·█▌▐█▄▄▌▐█▌▐▌     ▐█▌·▐█▌.▐▌▐█▌.▐▌▐█▌▐▌▐█▄▪▐█
//▀▀  █▪▀▀▀ ▀  ▀ ·▀▀▀▀ ▀▀▀·▀▀▀  ▀  ▀ . ▀   ▀█▄▀▪•▀▀ ▀▀ ▀▀▀ .▀▀▀      ▀▀▀  ▀█▄▀▪ ▀█▄▀▪.▀▀▀  ▀▀▀▀

namespace MagicaVoxelTools
{
    public class MagicaVoxel
    {
        public Color32[] palette = new Color32[256];
        public VoxMaterial[] materials = new VoxMaterial[256];
        public List<byte[,,]> models = new List<byte[,,]>();
        public List<String> modelNames = new List<string>();
        public List<Vector3Int> modelOffsets = new List<Vector3Int>();
        public int totalVoxels = 0;

        public bool IsValid
        {
            get
            {
                if (models.Count == 0) return false;
                if (modelNames.Count != models.Count) return false;
                if (modelOffsets.Count != models.Count) return false;
                return true;
            }
        }

        public Vector3Int Size(int index)
        {
            return new Vector3Int(models[index].GetLength(0), models[index].GetLength(1), models[index].GetLength(2));
        }

        public VoxelData GetData(int index)
        {
            return new VoxelData(models[index], palette, materials);
        }

        private void Init()
        {
            for (int i = 0; i < 256; i++)
            {
                palette[i] = new Color32(0, 0, 0, 0);
                materials[i] = new VoxMaterial();
            }
        }

        public MagicaVoxel()
        {
            Init();
        }

        public MagicaVoxel(string fileName)
        {
            Init();
            LoadFile(fileName);
        }

        public void LoadFile(string fileName)
        {
            //Debug.Log("LoadFile: " + fileName);

            if (File.Exists(fileName))
            {
                using (BinaryReader br = new BinaryReader(File.Open(fileName, FileMode.Open)))
                {
                    string magic = new string(br.ReadChars(4));
                    //Debug.Log(magic);
                    if (magic != "VOX ")
                    {
                        //Debug.Log("Not Magicavoxel File!");
                        return;
                    }

                    br.ReadInt32();//Version
                                   //int version = br.ReadInt32();
                                   //Debug.Log("Version: " + version);

                    br.ReadChars(4);//mainChunkID
                                    //string mainChunkID = new string(br.ReadChars(4));
                                    //Debug.Log("mainChunkID:" + mainChunkID);

                    int mainChunkSize = br.ReadInt32();
                    int mainChildrenSize = br.ReadInt32();
                    //Debug.Log("mainChunkSize: " + mainChunkSize);
                    //Debug.Log("mainChildrenSize: " + mainChildrenSize);
                    if (mainChunkSize > 0)
                    {
                        Debug.Log("Invalid Magicavoxel File!");
                        return;
                    }

                    int totalRead = 0;
                    while (totalRead < mainChildrenSize)
                    {
                        string chunkID = new string(br.ReadChars(4));
                        //Debug.Log("Chunk:" + chunkID);

                        int chunkSize = br.ReadInt32();
                        int childrenSize = br.ReadInt32();

                        //Debug.Log("ChunkSize: " + chunkSize);
                        //Debug.Log("ChildrenSize: " + childrenSize);

                        int dataSize = chunkSize + childrenSize;
                        byte[] data = br.ReadBytes(dataSize);
                        ProcessChunk(chunkID, data);

                        totalRead += 12 + dataSize;//Chunk ID + Sizes + Data...
                    }
                }

                Finialize();
            }
        }

        private void Finialize()
        {
            if (models.Count == 0) return;//something went wrong..

            //Remove Empty Root Object! (no model)
            if (modelNames.Count > 1) modelNames.RemoveAt(0);
            if (modelOffsets.Count > 1) modelOffsets.RemoveAt(0);

            //missing "nTRN" sections? won't have names or offsets
            if (modelNames.Count == 0)
            {
                for (int i = 0; i < models.Count; i++) modelNames.Add("vox");
            }
            if (modelOffsets.Count == 0)
            {
                //Center offset
                for (int i = 0; i < models.Count; i++)
                {
                    Vector3Int o = Size(i);
                    o.x /= 2;
                    o.y /= 2;
                    o.z /= 2;
                    modelOffsets.Add(o);
                }
            }

            //Check dup names, "by design" unity doesn't like it so it won't be fixed (ie for editor popup)
            for (int i = 0; i < modelNames.Count; i++)
            {
                for (int j = 0; j < modelNames.Count; j++)
                {
                    if (j == i) continue;// don't modify current name

                    if (modelNames[i] == modelNames[j]) modelNames[j] = modelNames[i] + j;
                }
            }
        }

        private void ProcessChunk(string type, byte[] data)
        {
            Stream s = new MemoryStream(data);
            BinaryReader br = new BinaryReader(s);

            //Debug.Log("Type: " + type);

            if (type == "PACK")
            {
                int count = br.ReadInt32();
                //Debug.Log("PACK: " + count);
            }

            if (type == "nTRN")
            {
                //Debug.Log("nTRN");

                br.ReadInt32();//id
                               //int id = br.ReadInt32();

                string name = "VOX";
                Vector3Int offset = Vector3Int.zero;

                //DICT: node attributes
                {
                    int numPairs = br.ReadInt32();
                    for (int i = 0; i < numPairs; i++)
                    {
                        string key = ReadDictString(br);
                        string val = ReadDictString(br);
                        //Debug.Log(String.Format("node Key:{0} Val:{1}", key, val));

                        if (key == "_name") name = val;
                    }
                }

                br.ReadInt32();//childId
                br.ReadInt32();//reserved
                br.ReadInt32();//layer
                br.ReadInt32();//frames
                               //int childId = br.ReadInt32();
                               //int reserved = br.ReadInt32();
                               //int layer = br.ReadInt32();
                               //int frames = br.ReadInt32();

                //Debug.Log(String.Format("{0} {1} {2} {3} {4}",id, childId,reserved,layer,frames));

                //DICT: frame attributes
                {
                    int numPairs = br.ReadInt32();
                    for (int i = 0; i < numPairs; i++)
                    {
                        string key = ReadDictString(br);
                        string val = ReadDictString(br);

                        //Debug.Log(String.Format("frame Key:{0} Val:{1}", key, val));

                        if (key == "_t")
                        {
                            string[] v3 = val.Split(' ');
                            offset.x = int.Parse(v3[0]);
                            offset.y = int.Parse(v3[2]);
                            offset.z = int.Parse(v3[1]);
                        }
                    }
                }

                modelNames.Add(name);
                modelOffsets.Add(offset);
            }

            if (type == "SIZE")
            {
                int x = br.ReadInt32();
                int z = br.ReadInt32();
                int y = br.ReadInt32();

                //Debug.Log("SIZE:" + x + "," + y + "," + z);
                //voxels = new byte[x, y, z];

                models.Add(new byte[x, y, z]);
            }

            if (type == "XYZI")
            {
                //Debug.Log("XYZI");

                int model = models.Count - 1;
                if (model >= 0)
                {
                    int numVoxels = br.ReadInt32();
                    for (int i = 0; i < numVoxels; i++)
                    {
                        int x = (int)br.ReadByte();
                        int z = (int)br.ReadByte();
                        int y = (int)br.ReadByte();
                        byte colorIndex = br.ReadByte();
                        models[model][x, y, z] = colorIndex;
                        //voxels[x, y, z] = colorIndex;
                        totalVoxels++;
                    }
                }
            }

            if (type == "RGBA")
            {
                //Debug.Log("RGBA");
                for (int i = 0; i < 256; i++)
                {
                    byte r = br.ReadByte();
                    byte g = br.ReadByte();
                    byte b = br.ReadByte();
                    byte a = br.ReadByte();
                    palette[i] = new Color32(r, g, b, a);

                    //Debug.Log(palette[i]);
                }
            }

            if (type == "MATT")
            {
                int matID = Mathf.Clamp(br.ReadInt32(), 0, 255);
                int matType = br.ReadInt32();
                //Debug.Log("MATT id: " + matID + " (" + matType + ")");

                float weight = br.ReadSingle();
                int matBits = br.ReadInt32();

                //Debug.Log("MATT weight: " + weight + " (" + matBits + ")");
                //float normalized = br.ReadSingle();

                materials[matID]._weight = weight;

                if (matType == 1) materials[matID].type = VoxMaterialType._metal;
                if (matType == 2) materials[matID].type = VoxMaterialType._glass;
                if (matType == 3) materials[matID].type = VoxMaterialType._emit;

            }

            if (type == "MATL")
            {
                int matID = Mathf.Clamp(br.ReadInt32(), 0, 255);

                int numPairs = br.ReadInt32();
                //Debug.Log("MATL id: " + matID + " (" + numPairs + ")");

                for (int i = 0; i < numPairs; i++)
                {
                    string key = ReadDictString(br);
                    string val = ReadDictString(br);

                    //Debug.Log("Key: " + key + " = " + val);

                    if (key == "_type")
                    {
                        //Debug.Log("Key: " + key + " = " + val);
                        if (val == "_diffuse") materials[matID].type = VoxMaterialType._diffuse;
                        if (val == "_metal") materials[matID].type = VoxMaterialType._metal;
                        if (val == "_glass") materials[matID].type = VoxMaterialType._glass;
                        if (val == "_emit") materials[matID].type = VoxMaterialType._emit;
                        if (val == "_plastic") materials[matID].type = VoxMaterialType._plastic;
                        if (val == "_media") materials[matID].type = VoxMaterialType._clouds;


                    }
                    if (key == "_weight") materials[matID]._weight = float.Parse(val);
                    if (key == "_rough") materials[matID]._rough = float.Parse(val);
                    if (key == "_spec") materials[matID]._spec = float.Parse(val);
                    if (key == "_ior") materials[matID]._ior = float.Parse(val);
                    if (key == "_att") materials[matID]._att = float.Parse(val);
                    if (key == "_flux") materials[matID]._flux = float.Parse(val);
                }
            }
        }

        private string ReadDictString(BinaryReader br)
        {
            int len = br.ReadInt32();
            char[] s = br.ReadChars(len);
            return new string(s);
        }
    }
}