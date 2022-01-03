using System;
using UnityEngine;

namespace MagicaVoxelTools
{
    [Serializable]
    public class VoxInfo
    {
        public Vector3Int pos;
        public Color32 col;
        public byte idx;
    }

    [Serializable]
    public enum VoxMaterialType
    {
        _diffuse, _metal, _glass, _emit, _plastic, _clouds
    }

    [Serializable]
    public class VoxMaterial
    {
        public VoxMaterialType type;
        public float _weight;
        public float _rough;
        public float _spec;
        public float _ior;
        public float _att;
        public float _flux;
    }

    //[CreateAssetMenu()]
    public class VoxelData : ScriptableObject
    {
        public Vector3 pivot = Vector3.zero;
        public float scale = .1f;

        public Color32[] palette = new Color32[256];
        public VoxMaterial[] materials = new VoxMaterial[256];

        public int width = 1, height = 1, depth = 1;
        public byte[] voxelArray = new byte[1];

        public VoxelData(int w, int h, int d)
        {
            Init(w, h, d);
        }

        public VoxelData(VoxelData vd)
        {
            Init(vd.width, vd.height, vd.depth);
            Array.Copy(vd.voxelArray, voxelArray, vd.voxelArray.Length);
            Array.Copy(vd.palette, palette, vd.palette.Length);
            Array.Copy(vd.materials, materials, vd.materials.Length);
        }

        public VoxelData(int w, int h, int d, Color32[] colors, VoxMaterial[] mats)
        {
            Init(w, h, d);
            Array.Copy(colors, palette, colors.Length);
            Array.Copy(mats, materials, mats.Length);
        }

        public VoxelData(byte[,,] data, Color32[] colors, VoxMaterial[] mats)
        {
            Init(data.GetLength(0), data.GetLength(1), data.GetLength(2));

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int z = 0; z < depth; z++)
                    {
                        SetVoxel(x, y, z, data[x, y, z]);
                    }
                }
            }

            Array.Copy(colors, palette, colors.Length);
            Array.Copy(mats, materials, mats.Length);
        }

        private void Init(int w, int h, int d)
        {
            for (int i = 0; i < 256; i++)
            {
                palette[i] = new Color32(0, 0, 0, 0);
                materials[i] = new VoxMaterial();
            }

            voxelArray = new byte[w * h * d];

            width = w;
            height = h;
            depth = d;
        }

        public bool ValidVoxel(Vector3Int p)
        {
            return ValidVoxel(p.x, p.y, p.z);
        }

        public bool ValidVoxel(int x, int y, int z)
        {
            if (x < 0 || y < 0 || z < 0) return false;
            if (x >= width) return false;
            if (y >= height) return false;
            if (z >= depth) return false;
            return true;
        }

        public byte GetVoxel(Vector3Int p)
        {
            return GetVoxel(p.x, p.y, p.z);
        }

        public byte GetVoxel(int x, int y, int z)
        {
            if (!ValidVoxel(x, y, z)) return 0;
            return voxelArray[x + y * width + (z * width * height)];
        }

        public void SetVoxel(Vector3Int p, byte val)
        {
            SetVoxel(p.x, p.y, p.z, val);
        }

        public void SetVoxel(int x, int y, int z, byte val)
        {
            if (!ValidVoxel(x, y, z)) return;
            voxelArray[x + y * width + (z * width * height)] = val;
        }

        public int VoxelCount()
        {
            int count = 0;
            for (int i = 0; i < voxelArray.Length; i++)
            {
                if (voxelArray[i] > 0) count++;
            }
            return count;
        }

        public Color32 VoxelColor(Vector3Int p)
        {
            return VoxelColor(p.x, p.y, p.z);
        }

        public Color32 VoxelColor(int x, int y, int z)
        {
            if (!ValidVoxel(x, y, z)) return Color.black;
            return palette[GetVoxel(x, y, z) - 1];
        }

        public VoxMaterial VoxelMaterial(Vector3Int p)
        {
            return VoxelMaterial(p.x, p.y, p.z);
        }

        public VoxMaterial VoxelMaterial(int x, int y, int z)
        {
            if (!ValidVoxel(x, y, z)) return materials[0];
            return materials[GetVoxel(x, y, z)];
        }

        public bool FilledVoxel(Vector3Int p)
        {
            return FilledVoxel(p.x, p.y, p.z);
        }

        public bool FilledVoxel(int x, int y, int z)
        {
            if (GetVoxel(x, y, z) == 0) return false;
            return true;
        }

        public bool IsVisible(Vector3Int p)
        {
            return IsVisible(p.x, p.y, p.z);
        }

        public bool IsVisible(int x, int y, int z)
        {
            if (!ValidVoxel(x, y, z)) return false;
            if (!FilledVoxel(x, y, z)) return false;
            if (NeighborCount(x, y, z) == 6) return false;
            return true;
        }

        public int NeighborCount(Vector3Int p)
        {
            return NeighborCount(p.x, p.y, p.z);
        }

        public int NeighborCount(int x, int y, int z)
        {
            if (!ValidVoxel(x, y, z)) return 0;
            int count = 0;
            if (FilledVoxel(x + 1, y, z)) count++;
            if (FilledVoxel(x - 1, y, z)) count++;
            if (FilledVoxel(x, y + 1, z)) count++;
            if (FilledVoxel(x, y - 1, z)) count++;
            if (FilledVoxel(x, y, z + 1)) count++;
            if (FilledVoxel(x, y, z - 1)) count++;
            return count;
        }

        public void ClearByIndex(byte index, bool clearMatching)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int z = 0; z < depth; z++)
                    {
                        if (clearMatching && GetVoxel(x, y, z) == index)
                            SetVoxel(x, y, z, 0);
                        if (!clearMatching && GetVoxel(x, y, z) != index)
                            SetVoxel(x, y, z, 0);
                    }
                }
            }
        }

        public void ClearByMat(VoxMaterialType type, bool clearMatching)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int z = 0; z < depth; z++)
                    {
                        VoxMaterialType mt = VoxelMaterial(x, y, z).type;
                        if (clearMatching && mt == type)
                            SetVoxel(x, y, z, 0);
                        if (!clearMatching && mt != type)
                            SetVoxel(x, y, z, 0);
                    }
                }
            }
        }
    }
}