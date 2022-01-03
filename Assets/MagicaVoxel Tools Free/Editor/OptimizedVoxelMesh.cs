using System.Collections.Generic;
using UnityEngine;

namespace MagicaVoxelTools
{
    public class OptimizedVoxelMesh
    {
        public enum VoxelFace
        {
            Front, Back, Left, Right, Top, Bottom
        }

        public Mesh mesh;
        public Texture2D texture;

        //public OptimizedVoxelMesh()
        //{
        //    mesh = new Mesh();
        //    mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        //    texture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        //}

        public OptimizedVoxelMesh(VoxelData vox, Vector3 pivot,int texScale, int pad, float scale = .1f)
        {
            mesh = new Mesh();
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

            texture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            BuildMesh(vox, pivot,texScale, pad, scale);
        }

        private VoxelData data;
        private int w, h, d;
        private bool[,,] map;
        private List<Vector3Int> scanLine;
        private List<Vector3Int> starts;
        private List<Vector3Int> ends;

        private List<Vector3> verts;
        private List<int> idxs;
        private List<Texture2D> texs;

        private void BuildMesh(VoxelData voxData, Vector3 pivot,int texScale, int texPad, float scale = .1f)
        {
            data = voxData;

            verts = new List<Vector3>();
            idxs = new List<int>();
            texs = new List<Texture2D>();

            w = data.width;
            h = data.height;
            d = data.depth;

            map = new bool[w, h, d];

            //Build Verts, Indexes, and Textures
            ProcessAll();

            //Pivot and Scale Mesh...
            for (int i = 0; i < verts.Count; i++)
            {
                verts[i] -= pivot;
                verts[i] *= scale;
            }

            //Create Single Texture and UV's
            List<Vector2> uvs;
            texture = TextureUtility.PackTextures(texs, texScale, texPad, out uvs);

            mesh = new Mesh();
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.vertices = verts.ToArray();
            mesh.SetIndices(idxs.ToArray(), MeshTopology.Triangles, 0);
            mesh.uv = uvs.ToArray();
            mesh.RecalculateNormals();
        }

        private void ProcessAll()
        {
            ProcessFaces(VoxelFace.Front);
            ProcessVerts(VoxelFace.Front);
            ProcessTextures(VoxelFace.Front);

            ProcessFaces(VoxelFace.Back);
            ProcessVerts(VoxelFace.Back);
            ProcessTextures(VoxelFace.Back);

            ProcessFaces(VoxelFace.Left);
            ProcessVerts(VoxelFace.Left);
            ProcessTextures(VoxelFace.Left);

            ProcessFaces(VoxelFace.Right);
            ProcessVerts(VoxelFace.Right);
            ProcessTextures(VoxelFace.Right);

            ProcessFaces(VoxelFace.Top);
            ProcessVerts(VoxelFace.Top);
            ProcessTextures(VoxelFace.Top);

            ProcessFaces(VoxelFace.Bottom);
            ProcessVerts(VoxelFace.Bottom);
            ProcessTextures(VoxelFace.Bottom);
        }

        private void ProcessFaces(VoxelFace face)
        {
            starts = new List<Vector3Int>();
            ends = new List<Vector3Int>();

            if (face == VoxelFace.Front) MakeMap(0, 0, -1);
            if (face == VoxelFace.Back) MakeMap(0, 0, 1);
            if (face == VoxelFace.Top) MakeMap(0, 1, 0);
            if (face == VoxelFace.Bottom) MakeMap(0, -1, 0);
            if (face == VoxelFace.Left) MakeMap(-1, 0, 0);
            if (face == VoxelFace.Right) MakeMap(1, 0, 0);

            switch (face)
            {
                case VoxelFace.Front:
                case VoxelFace.Back:
                    {
                        Vector3Int dir = new Vector3Int(0, 1, 0);

                        for (int z = 0; z < d; z++)
                        {
                            for (int y = 0; y < h; y++)
                            {
                                scanLine = new List<Vector3Int>();

                                for (int x = 0; x < w; x++)
                                {
                                    if (map[x, y, z])
                                    {
                                        map[x, y, z] = false;
                                        scanLine.Add(new Vector3Int(x, y, z));
                                    }
                                    else
                                    {
                                        if (!map[x, y, z] && scanLine.Count > 0)
                                        {
                                            AddScan(dir);
                                            scanLine = new List<Vector3Int>();
                                        }
                                    }
                                }
                                if (scanLine.Count > 0) AddScan(dir);
                            }
                        }
                    }
                    break;

                case VoxelFace.Left:
                case VoxelFace.Right:
                    {
                        Vector3Int dir = new Vector3Int(0, 1, 0);

                        for (int x = 0; x < w; x++)
                        {
                            for (int y = 0; y < h; y++)
                            {
                                scanLine = new List<Vector3Int>();

                                for (int z = 0; z < d; z++)
                                {
                                    if (map[x, y, z])
                                    {
                                        map[x, y, z] = false;
                                        scanLine.Add(new Vector3Int(x, y, z));
                                    }
                                    else
                                    {
                                        if (!map[x, y, z] && scanLine.Count > 0)
                                        {
                                            AddScan(dir);
                                            scanLine = new List<Vector3Int>();
                                        }
                                    }
                                }
                                if (scanLine.Count > 0) AddScan(dir);
                            }
                        }
                    }
                    break;

                case VoxelFace.Top:
                case VoxelFace.Bottom:
                    {
                        Vector3Int dir = new Vector3Int(0, 0, 1);

                        for (int y = 0; y < h; y++)
                        {
                            for (int z = 0; z < d; z++)
                            {
                                scanLine = new List<Vector3Int>();

                                for (int x = 0; x < w; x++)
                                {
                                    if (map[x, y, z])
                                    {
                                        map[x, y, z] = false;
                                        scanLine.Add(new Vector3Int(x, y, z));
                                    }
                                    else
                                    {
                                        if (!map[x, y, z] && scanLine.Count > 0)
                                        {
                                            AddScan(dir);
                                            scanLine = new List<Vector3Int>();
                                        }
                                    }
                                }
                                if (scanLine.Count > 0) AddScan(dir);
                            }
                        }
                    }
                    break;
            }
        }

        private void ProcessTextures(VoxelFace face)
        {
            if (starts.Count <= 0) return;

            for (int i = 0; i < starts.Count; i++)
            {
                int w = 1, h = 1;

                switch (face)
                {
                    case VoxelFace.Front:
                    case VoxelFace.Back:
                        w = ends[i].x - starts[i].x + 1;
                        h = ends[i].y - starts[i].y + 1;
                        break;

                    case VoxelFace.Left:
                    case VoxelFace.Right:
                        w = ends[i].z - starts[i].z + 1;
                        h = ends[i].y - starts[i].y + 1;
                        break;

                    case VoxelFace.Top:
                    case VoxelFace.Bottom:
                        w = ends[i].x - starts[i].x + 1;
                        h = ends[i].z - starts[i].z + 1;
                        break;
                }

                Texture2D tex = new Texture2D(w , h, TextureFormat.ARGB32, false);
                Color32[] tcolor = tex.GetPixels32();
                for (int c = 0; c < tcolor.Length; c++) tcolor[c] = Color.clear;
                tex.SetPixels32(tcolor);

                int px = 0, py = 0;

                switch (face)
                {
                    case VoxelFace.Front:
                    case VoxelFace.Back:
                        {
                            int z = starts[i].z;

                            for (int y = starts[i].y; y <= ends[i].y; y++)
                            {
                                px = 0;
                                for (int x = starts[i].x; x <= ends[i].x; x++)
                                {
                                    tex.SetPixel(px, py, data.VoxelColor(x, y, z));
                                    px++;
                                }
                                py++;
                            }
                        }
                        break;

                    case VoxelFace.Left:
                    case VoxelFace.Right:
                        {
                            int x = starts[i].x;

                            for (int y = starts[i].y; y <= ends[i].y; y++)
                            {
                                px = 0;
                                for (int z = starts[i].z; z <= ends[i].z; z++)
                                {
                                    tex.SetPixel(px, py, data.VoxelColor(x, y, z));
                                    px++;
                                }
                                py++;
                            }
                        }
                        break;

                    case VoxelFace.Top:
                    case VoxelFace.Bottom:
                        {
                            int y = starts[i].y;

                            for (int z = starts[i].z; z <= ends[i].z; z++)
                            {
                                px = 0;
                                for (int x = starts[i].x; x <= ends[i].x; x++)
                                {
                                    tex.SetPixel(px, py, data.VoxelColor(x, y, z));
                                    px++;
                                }
                                py++;
                            }
                        }
                        break;
                }

                tex.Apply();
                texs.Add(tex);
            }
        }

        private void ProcessVerts(VoxelFace face)
        {
            if (starts.Count <= 0) return;

            Vector3Int startOffset = Vector3Int.zero;
            Vector3Int endOffset = Vector3Int.zero;

            switch (face)
            {
                case VoxelFace.Front:
                    startOffset = new Vector3Int(0, 0, 0);
                    endOffset = new Vector3Int(1, 1, 0);
                    break;

                case VoxelFace.Back:
                    startOffset = new Vector3Int(0, 0, 1);
                    endOffset = new Vector3Int(1, 1, 1);
                    break;

                case VoxelFace.Left:
                    startOffset = new Vector3Int(0, 0, 0);
                    endOffset = new Vector3Int(0, 1, 1);
                    break;

                case VoxelFace.Right:
                    startOffset = new Vector3Int(1, 0, 0);
                    endOffset = new Vector3Int(1, 1, 1);
                    break;

                case VoxelFace.Top:
                    startOffset = new Vector3Int(0, 1, 0);
                    endOffset = new Vector3Int(1, 1, 1);
                    break;

                case VoxelFace.Bottom:
                    startOffset = new Vector3Int(0, 0, 0);
                    endOffset = new Vector3Int(1, 0, 1);
                    break;
            }

            for (int i = 0; i < starts.Count; i++)
            {
                int ic = verts.Count;

                Vector3Int start = starts[i] + startOffset;
                Vector3Int end = ends[i] + endOffset;

                verts.Add(start);
                verts.Add(end);

                switch (face)
                {
                    case VoxelFace.Front:
                        verts.Add(new Vector3(start.x, end.y, start.z));
                        verts.Add(new Vector3(end.x, start.y, start.z));
                        break;

                    case VoxelFace.Back:
                        verts.Add(new Vector3(start.x, end.y, start.z));
                        verts.Add(new Vector3(end.x, start.y, start.z));
                        break;

                    case VoxelFace.Left:
                        verts.Add(new Vector3(start.x, end.y, start.z));
                        verts.Add(new Vector3(start.x, start.y, end.z));
                        break;

                    case VoxelFace.Right:
                        verts.Add(new Vector3(start.x, end.y, start.z));
                        verts.Add(new Vector3(start.x, start.y, end.z));
                        break;

                    case VoxelFace.Top:
                        verts.Add(new Vector3(start.x, end.y, end.z));
                        verts.Add(new Vector3(end.x, start.y, start.z));
                        break;

                    case VoxelFace.Bottom:
                        verts.Add(new Vector3(start.x, end.y, end.z));
                        verts.Add(new Vector3(end.x, start.y, start.z));
                        break;
                }

                switch (face)
                {
                    case VoxelFace.Front:
                    case VoxelFace.Right:
                    case VoxelFace.Top:
                        idxs.Add(ic + 0);
                        idxs.Add(ic + 2);
                        idxs.Add(ic + 1);

                        idxs.Add(ic + 0);
                        idxs.Add(ic + 1);
                        idxs.Add(ic + 3);
                        break;

                    case VoxelFace.Back:
                    case VoxelFace.Left:
                    case VoxelFace.Bottom:
                        idxs.Add(ic + 0);
                        idxs.Add(ic + 1);
                        idxs.Add(ic + 2);

                        idxs.Add(ic + 0);
                        idxs.Add(ic + 3);
                        idxs.Add(ic + 1);
                        break;
                }
            }
        }

        private void MakeMap(Vector3Int ov)
        {
            MakeMap(ov.x, ov.y, ov.z);
        }

        private void MakeMap(int ox, int oy, int oz)
        {
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    for (int z = 0; z < d; z++)
                    {
                        if (!data.FilledVoxel(x + ox, y + oy, z + oz))
                        {
                            map[x, y, z] = data.IsVisible(x, y, z);
                        }
                    }
                }
            }
        }

        private void AddScan(Vector3Int o)
        {
            if (scanLine.Count == 0) return;

            Vector3Int start = scanLine[0];
            Vector3Int end = scanLine[scanLine.Count - 1];

            int offset = 1;
            bool valid = true;

            do
            {
                for (int i = 0; i < scanLine.Count; i++)
                {
                    Vector3Int p = scanLine[i] + o * offset;
                    if (!data.ValidVoxel(p) || !map[p.x, p.y, p.z])
                    {
                        valid = false;
                        break;
                    }
                }

                if (valid)
                {
                    for (int i = 0; i < scanLine.Count; i++)
                    {
                        Vector3Int p = scanLine[i] + o * offset;
                        map[p.x, p.y, p.z] = false;
                    }
                    end += o;
                }

                offset++;
            } while (valid);

            starts.Add(start);
            ends.Add(end);
        }
    }
}