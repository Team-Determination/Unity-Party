using System.Collections.Generic;
using UnityEngine;

namespace MagicaVoxelTools
{
    public class PreviewVoxelMesh
    {
        public Mesh mesh;

        public PreviewVoxelMesh(VoxelData data)
        {
            List<Vector3> verts = new List<Vector3>();
            List<int> idxs = new List<int>();
            List<Color32> colors = new List<Color32>();

            for (int x = 0; x < data.width; x++)
            {
                for (int y = 0; y < data.height; y++)
                {
                    for (int z = 0; z < data.depth; z++)
                    {
                        if (data.IsVisible(x, y, z))
                        {
                            int i = verts.Count;

                            Color32 col = data.VoxelColor(x, y, z);

                            float c = ((float)( (data.GetVoxel(x, y, z)-1) )) / 256;
                            Vector2 uv = new Vector2(c, 0);

                            //Top...
                            verts.Add(new Vector3(x, y + 1, z));
                            verts.Add(new Vector3(x, y + 1, z + 1));
                            verts.Add(new Vector3(x + 1, y + 1, z + 1));
                            verts.Add(new Vector3(x + 1, y + 1, z));

                            colors.Add(col); colors.Add(col); colors.Add(col); colors.Add(col);

                            idxs.Add(i + 0);
                            idxs.Add(i + 1);
                            idxs.Add(i + 2);
                            idxs.Add(i + 0);
                            idxs.Add(i + 2);
                            idxs.Add(i + 3);
                            i += 4;

                            //Bottom...
                            verts.Add(new Vector3(x, y, z));
                            verts.Add(new Vector3(x, y, z + 1));
                            verts.Add(new Vector3(x + 1, y, z + 1));
                            verts.Add(new Vector3(x + 1, y, z));

                            colors.Add(col); colors.Add(col); colors.Add(col); colors.Add(col);

                            idxs.Add(i + 0);
                            idxs.Add(i + 2);
                            idxs.Add(i + 1);
                            idxs.Add(i + 0);
                            idxs.Add(i + 3);
                            idxs.Add(i + 2);
                            i += 4;

                            //Front...
                            verts.Add(new Vector3(x, y, z));
                            verts.Add(new Vector3(x, y + 1, z));
                            verts.Add(new Vector3(x + 1, y + 1, z));
                            verts.Add(new Vector3(x + 1, y, z));

                            colors.Add(col); colors.Add(col); colors.Add(col); colors.Add(col);

                            idxs.Add(i + 0);
                            idxs.Add(i + 1);
                            idxs.Add(i + 2);
                            idxs.Add(i + 0);
                            idxs.Add(i + 2);
                            idxs.Add(i + 3);
                            i += 4;

                            //Back...
                            verts.Add(new Vector3(x, y, z + 1));
                            verts.Add(new Vector3(x, y + 1, z + 1));
                            verts.Add(new Vector3(x + 1, y + 1, z + 1));
                            verts.Add(new Vector3(x + 1, y, z + 1));

                            colors.Add(col); colors.Add(col); colors.Add(col); colors.Add(col);

                            idxs.Add(i + 0);
                            idxs.Add(i + 2);
                            idxs.Add(i + 1);
                            idxs.Add(i + 0);
                            idxs.Add(i + 3);
                            idxs.Add(i + 2);
                            i += 4;

                            //Left...
                            verts.Add(new Vector3(x, y, z));
                            verts.Add(new Vector3(x, y + 1, z));
                            verts.Add(new Vector3(x, y + 1, z + 1));
                            verts.Add(new Vector3(x, y, z + 1));

                            colors.Add(col); colors.Add(col); colors.Add(col); colors.Add(col);

                            idxs.Add(i + 0);
                            idxs.Add(i + 3);
                            idxs.Add(i + 2);
                            idxs.Add(i + 0);
                            idxs.Add(i + 2);
                            idxs.Add(i + 1);
                            i += 4;

                            //Right...
                            verts.Add(new Vector3(x + 1, y, z));
                            verts.Add(new Vector3(x + 1, y + 1, z));
                            verts.Add(new Vector3(x + 1, y + 1, z + 1));
                            verts.Add(new Vector3(x + 1, y, z + 1));

                            colors.Add(col); colors.Add(col); colors.Add(col); colors.Add(col);

                            idxs.Add(i + 0);
                            idxs.Add(i + 1);
                            idxs.Add(i + 2);
                            idxs.Add(i + 0);
                            idxs.Add(i + 2);
                            idxs.Add(i + 3);
                            i += 4;
                        }
                    }
                }
            }

            //Debug.Log(verts.Count);
            //Debug.Log(uvs.Count);
            //Debug.Log(idxs.Count);

            mesh = new Mesh();
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.subMeshCount = 1;

            mesh.vertices = verts.ToArray();
            mesh.SetIndices(idxs.ToArray(), MeshTopology.Triangles, 0);
            mesh.SetColors(colors);

            mesh.RecalculateNormals();
        }
    }
}