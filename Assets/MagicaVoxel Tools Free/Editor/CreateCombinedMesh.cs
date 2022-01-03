using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MagicaVoxelTools
{
    public class CreateCombinedMesh
    {
        private class TriangleInfo
        {
            public Vector3 p1;
            public Vector3 p2;
            public Vector3 p3;
            public Vector2 uv1;
            public Vector2 uv2;
            public Vector2 uv3;
        }

        private static Dictionary<Texture2D, List<TriangleInfo>> meshes;

        //[MenuItem("Tools/Create Combined")]
        [MenuItem("GameObject/Bake", false, 0)]
        public static GameObject CombineSelectedMeshes()
        {
            GameObject.DestroyImmediate(GameObject.Find("TEMPVOX"));
            meshes = new Dictionary<Texture2D, List<TriangleInfo>>();

            //Collect Meshes from selected objects...
            foreach (var obj in Selection.gameObjects)
            {
                MeshFilter[] mfs = obj.transform.root.GetComponentsInChildren<MeshFilter>();
                foreach (var mf in mfs)
                {
                    if (mf.sharedMesh == null) continue;
                    MeshRenderer mr = mf.gameObject.GetComponent<MeshRenderer>();
                    if (mr == null) continue;
                    if (mr.sharedMaterials == null) continue;
                    if (mr.sharedMaterials.Length != mf.sharedMesh.subMeshCount) continue;

                    //if (mf.gameObject.name == "")
                    AddMesh(mf.sharedMesh, mr.sharedMaterials, mf.gameObject.transform.localToWorldMatrix);
                }

                SkinnedMeshRenderer[] smrs = obj.transform.root.GetComponentsInChildren<SkinnedMeshRenderer>();
                foreach (var smr in smrs)
                {
                    if (smr.sharedMesh == null) continue;
                    if (smr.sharedMaterials == null) continue;
                    if (smr.sharedMaterials.Length != smr.sharedMesh.subMeshCount) continue;

                    Mesh mesh = new Mesh();
                    smr.BakeMesh(mesh);
                    AddMesh(mesh, smr.sharedMaterials, smr.gameObject.transform.localToWorldMatrix);
                }
            }

            if (meshes.Keys.Count == 0) return null;//No mesh/skins found

            List<Texture2D> texList = new List<Texture2D>();
            foreach (var tex in meshes.Keys)
            {
                //This will resize if needed, and create a readable texture.
                texList.Add(ResizeTexture(tex, tex.width, tex.height));

                //if (tex.width > 256 || tex.height > 256)
                //    texList.Add(ResizeTexture(tex, 256, 256));
                //else
                //    texList.Add(ResizeTexture(tex, tex.width, tex.height));
            }

            Texture2D atlas = new Texture2D(1, 1);
            Rect[] uvRects = atlas.PackTextures(texList.ToArray(), 0, 8192);
            //Debug.Log("Rect Count:" + uvRects.Length);

            //Build a VERY unoptimized mesh
            List<Vector3> buildVerts = new List<Vector3>();
            List<int> buildIdxs = new List<int>();
            List<Vector2> buildUvs = new List<Vector2>();

            int buildIdx = 0;
            int uvCount = 0;

            foreach (var tex in meshes.Keys)
            {
                Rect r = uvRects[uvCount++];

                //if (tex.name == "")
                {
                    //Debug.Log(tex.name + ": " + tex.width + "," + tex.height + " Rect: " + r.ToString());

                    foreach (var t in meshes[tex])
                    {
                        buildVerts.Add(t.p1);
                        buildVerts.Add(t.p2);
                        buildVerts.Add(t.p3);

                        //buildVerts.Add(new Vector3((t.uv1.x * r.width) + r.x, (t.uv1.y * r.height) + r.y, 0));
                        //buildVerts.Add(new Vector3((t.uv2.x * r.width) + r.x, (t.uv2.y * r.height) + r.y, 0));
                        //buildVerts.Add(new Vector3((t.uv3.x * r.width) + r.x, (t.uv3.y * r.height) + r.y, 0));
                        //Debug.Log(t.uv1.ToString() + " " + t.uv2.ToString() + " " + t.uv3.ToString() + " ");

                        buildIdxs.Add(buildIdx++);
                        buildIdxs.Add(buildIdx++);
                        buildIdxs.Add(buildIdx++);

                        buildUvs.Add(new Vector2((t.uv1.x * r.width) + r.x, (t.uv1.y * r.height) + r.y));
                        buildUvs.Add(new Vector2((t.uv2.x * r.width) + r.x, (t.uv2.y * r.height) + r.y));
                        buildUvs.Add(new Vector2((t.uv3.x * r.width) + r.x, (t.uv3.y * r.height) + r.y));
                    }
                }
            }

            //Create Final scene game object
            GameObject objBuild = new GameObject("TEMPVOX");
            MeshFilter objMf = objBuild.AddComponent<MeshFilter>();
            MeshRenderer objMr = objBuild.AddComponent<MeshRenderer>();
            //Material mat = new Material(Shader.Find("Mobile/Diffuse"));
            Material mat = new Material(Shader.Find("Unlit/Texture"));
            mat.mainTexture = atlas;
            objMr.sharedMaterial = mat;

            Mesh objMesh = new Mesh();
            objMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            objMesh.vertices = buildVerts.ToArray();
            objMesh.SetIndices(buildIdxs.ToArray(), MeshTopology.Triangles, 0);
            objMesh.uv = buildUvs.ToArray();
            objMesh.RecalculateNormals();

            objMf.sharedMesh = objMesh;

            return objBuild;
        }

        private static Vector2 FixUV(Vector2 uv)
        {
            while (uv.x < 0) uv.x += 1;
            while (uv.y < 0) uv.y += 1;

            while (uv.x > 1) uv.x -= 1;
            while (uv.y > 1) uv.y -= 1;

            return uv;
        }

        private static void AddMesh(Mesh mesh, Material[] mats, Matrix4x4 trans)
        {
            Vector3[] v = mesh.vertices;
            Vector2[] uv = mesh.uv;

            Debug.Log(v.Length + ":" + uv.Length);

            if (v.Length == 0) return;//no verts wtf?
            if (v.Length != uv.Length) uv = new Vector2[v.Length];//no uv's, fix yo model!

            for (int i = 0; i < mesh.subMeshCount; i++)
            {
                Texture2D tex = mats[i].mainTexture as Texture2D;
                if (tex == null)
                {
                    //Build a small solid color texture
                    tex = new Texture2D(16, 16, TextureFormat.ARGB32, false);
                    tex.filterMode = FilterMode.Point;
                    Color32[] pixs = new Color32[16 * 16];
                    for (int p = 0; p < pixs.Length; p++) pixs[p] = mats[i].color;
                    tex.SetPixels32(pixs);
                    tex.Apply();
                }

                if (!meshes.ContainsKey(tex))
                {
                    meshes.Add(tex, new List<TriangleInfo>());
                }

                int[] idxs = mesh.GetIndices(i);

                for (int idx = 0; idx < idxs.Length; idx += 3)
                {
                    TriangleInfo ti = new TriangleInfo();
                    ti.p1 = trans.MultiplyPoint(v[idxs[idx + 0]]);
                    ti.p2 = trans.MultiplyPoint(v[idxs[idx + 1]]);
                    ti.p3 = trans.MultiplyPoint(v[idxs[idx + 2]]);

                    ti.uv1 = FixUV(uv[idxs[idx + 0]]);
                    ti.uv2 = FixUV(uv[idxs[idx + 1]]);
                    ti.uv3 = FixUV(uv[idxs[idx + 2]]);

                    meshes[tex].Add(ti);
                }
            }
        }

        //public static void SetTextureImporterFormat(Texture2D texture, bool isReadable)
        //{
        //    if (null == texture) return;

        //    string assetPath = AssetDatabase.GetAssetPath(texture);
        //    var tImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        //    if (tImporter != null)
        //    {
        //        if (tImporter.isReadable == isReadable) return;

        //        tImporter.isReadable = isReadable;
        //        AssetDatabase.ImportAsset(assetPath);
        //        AssetDatabase.Refresh();
        //    }
        //}

        public static Texture2D ResizeTexture(Texture2D tex, int width, int height)
        {
            RenderTexture rt = RenderTexture.GetTemporary(width, height);
            rt.filterMode = FilterMode.Point;
            RenderTexture.active = rt;

            Texture2D newTex = new Texture2D(width, height, TextureFormat.ARGB32, false);
            newTex.filterMode = FilterMode.Point;

            Graphics.Blit(tex, rt);
            newTex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            newTex.Apply();

            RenderTexture.active = null;

            return newTex;
        }
    }
}