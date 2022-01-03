using SimplePaletteQuantizer.Quantizers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VoxelSystem;

namespace MagicaVoxelTools
{
    public class EditorExportVox : EditorWindow
    {
        [MenuItem("Tools/MagicaVoxel Export")]
        private static void ShowExportVox()
        {
            EditorExportVox w = EditorWindow.GetWindow<EditorExportVox>();

            w.titleContent = new GUIContent("Export", Resources.Load<Texture>("Textures/Icon"), "Export to Magicavoxel");
            w.minSize = new Vector2(372, 363 + 16);
            w.Show();
            w.OnSelectionChange();
        }

        private Texture _logo;
        private Texture _folder;
        private ComputeShader shader;
        private const int _header = 32 + 64 + 2 + 16;

        private bool didWeBake = false;

        //Selection
        private Mesh selectedMesh;

        private Material selectedMaterial;
        private List<VoxInfo> selectedVoxelInfo;
        private List<Color32> selectedPalette;

        //Preview
        private PreviewRenderUtility preview;

        private float previewRotationX = 0;
        private float previewRotationY = 0;

        private Mesh previewMesh;
        private Texture2D previewPalette;
        private Material previewMaterial;

        //Settings...
        private int resolution = 32;

        private int colorTotal = 255;
        private Texture2D texturePalette;
        private string saveName;
        private string folderPath;
        //private bool fillInside = false;

        private void LoadSettings()
        {
            folderPath = EditorPrefs.GetString("VOXFOLDER");
            VerifyPath();
        }

        private void SaveSettings()
        {
            EditorPrefs.SetString("VOXFOLDER", folderPath);
        }

        private void Awake()
        {
            LoadSettings();

            _logo = Resources.Load<Texture>("Textures/Logo");
            _folder = Resources.Load<Texture>("Textures/Folder");
            shader = Resources.Load<ComputeShader>("Shaders/Voxelizer");

            previewMaterial = new Material(Shader.Find("Mobile/Diffuse"));

            preview = new PreviewRenderUtility();
        }

        private void OnDestroy()
        {
            SaveSettings();
            preview.Cleanup();
            preview = null;

            if (didWeBake) GameObject.DestroyImmediate(GameObject.Find("TEMPVOX"));
        }

        private void OnSelectionChange()
        {
            selectedMesh = null;
            selectedMaterial = null;

            ClearPreview();
            Repaint();

            if (Selection.objects.Length > 1) return;
            if (Selection.activeGameObject == null) return;

            saveName = Selection.activeGameObject.name;

            MeshFilter mf = Selection.activeGameObject.GetComponent<MeshFilter>();
            MeshRenderer mr = Selection.activeGameObject.GetComponent<MeshRenderer>();

            if (mf && mr && mf.sharedMesh && mr.sharedMaterial)
            {
                selectedMesh = mf.sharedMesh;
                selectedMaterial = mr.sharedMaterial;
            }

            SkinnedMeshRenderer smr = Selection.activeGameObject.GetComponent<SkinnedMeshRenderer>();

            if (smr && smr.sharedMesh && smr.sharedMaterial)
            {
                selectedMesh = new Mesh();
                smr.BakeMesh(selectedMesh);
                selectedMaterial = smr.sharedMaterial;
            }

            if (selectedMesh == null) return;

            CreatePreview();
        }

        private void CreatePreview()
        {
            GPUVoxelData data = GPUVoxelizer.Voxelize(shader, selectedMesh, resolution, false, false);
            previewMesh = VoxelMesh.Build(data.GetData(), data.UnitLength, true);
            selectedVoxelInfo = BuildVoxInfo(data.GetData(), data.UnitLength);
            CreatePreviewPalette();
        }

        private void OnGUI()
        {
            GUILayout.Box(_logo, GUILayout.Width(this.position.width - 6), GUILayout.Height(32 + 16));

            if (Selection.objects.Length > 1)
            {
                EditorGUILayout.HelpBox("Multible Objects Selected", MessageType.Warning);
                if (GUILayout.Button("Bake"))
                {
                    Selection.activeGameObject = CreateCombinedMesh.CombineSelectedMeshes();
                    didWeBake = true;
                }
                return;
            }

            if (selectedMesh == null)
            {
                EditorGUILayout.HelpBox("Select Mesh", MessageType.Info);
                return;
            }

            GUILayout.BeginHorizontal();

            EditorGUI.BeginChangeCheck();
            GUILayout.Label("Size", GUILayout.Width(50));
            resolution = EditorGUILayout.IntSlider(resolution, 1, 50);
            if (GUILayout.Button("MAX", GUILayout.Width(64))) resolution = 50;
            if (EditorGUI.EndChangeCheck()) CreatePreview();

            //fillInside = EditorGUILayout.Toggle(fillInside, GUILayout.Width(32));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            GUILayout.Label("Palette", GUILayout.Width(50));

            EditorGUI.BeginChangeCheck();
            EditorGUI.BeginDisabledGroup(texturePalette != null);
            colorTotal = EditorGUILayout.IntSlider(colorTotal, 1, 255);
            EditorGUI.EndDisabledGroup();

            texturePalette = (Texture2D)EditorGUILayout.ObjectField(texturePalette, typeof(Texture2D), true, GUILayout.Width(70));

            if (EditorGUI.EndChangeCheck()) CreatePreviewPalette();

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            saveName = GUILayout.TextField(saveName, GUILayout.Width(128), GUILayout.Height(20));

            if (folderPath == "") folderPath = Application.dataPath;

            if (GUILayout.Button("Export"))
            {
                SaveVox();
            }

            GUIContent button = new GUIContent(_folder, folderPath);
            if (GUILayout.Button(button, GUILayout.Width(32), GUILayout.Height(20)))
            {
                VerifyPath();
                string newpath = EditorUtility.SaveFolderPanel("Export folder", folderPath, "");
                if (newpath.Length > 0) folderPath = newpath;
            }

            GUILayout.EndHorizontal();

            DrawPreview();
        }

        private void VerifyPath()
        {
            if (!System.IO.Directory.Exists(folderPath)) folderPath = "";
            if (folderPath == "") folderPath = Application.dataPath;
            if (!folderPath.EndsWith("/")) folderPath += "/";
        }

        private void DrawPreview()
        {
            if (previewMesh != null)
            {
                Rect previewRect = new Rect(0, _header, this.position.width, this.position.height - _header);

                if (Event.current.type == EventType.MouseDrag)
                {
                    if (previewRect.Contains(Event.current.mousePosition))
                    {
                        previewRotationX -= Event.current.delta.y;
                        previewRotationY -= Event.current.delta.x;
                    }
                    Repaint();
                }

                DrawRenderPreview(previewRect);
                if (previewPalette)
                {
                    GUI.DrawTexture(new Rect(0 + 4, 8 * 32 + _header + 4, 8 * 8, -8 * 32), previewPalette);
                }
            }
        }

        private void ClearPreview()
        {
            previewMesh = null;
            previewPalette = null;

            previewRotationX = 0;
            previewRotationY = 0;
        }

        private List<VoxInfo> BuildVoxInfo(Voxel_t[] voxels, float unit)
        {
            List<VoxInfo> vi = new List<VoxInfo>();
            if (!selectedMesh || !selectedMaterial) return vi;

            Vector3 min = selectedMesh.bounds.min;
            Texture2D tex = GetReadableTexture(selectedMaterial.mainTexture);

            float n = 1 / unit;

            for (int i = 0; i < voxels.Length; i++)
            {
                Voxel_t v = voxels[i];
                if (v.fill == 0) continue;
                Vector3 p = v.position - min;
                p *= n;

                int x = Mathf.RoundToInt(p.x);
                int y = Mathf.RoundToInt(p.y);
                int z = Mathf.RoundToInt(p.z);
                Vector3Int vp = new Vector3Int(x, y, z);

                Color col = Color.white;

                if (v.uv.x != -1)//Fill color?
                {
                    if (tex != null)
                        col = tex.GetPixel((int)(v.uv.x * tex.width), (int)(v.uv.y * tex.height));
                    else
                        col = selectedMaterial.color;
                }

                VoxInfo nvi = new VoxInfo
                {
                    pos = vp,
                    col = col,
                    idx = 1
                };
                vi.Add(nvi);
            }

            return vi;
        }

        private Texture2D GetReadableTexture(Texture source)
        {
            if (!source) return null;

            Texture2D texture2D = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
            RenderTexture currentRT = RenderTexture.active;

            RenderTexture renderTexture = new RenderTexture(source.width, source.height, 32);
            Graphics.Blit(source, renderTexture);

            RenderTexture.active = renderTexture;
            texture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            texture2D.Apply();

            RenderTexture.active = currentRT;
            return texture2D;
        }

        private void SaveVox()
        {
            VerifyPath();
            string filepath = folderPath + saveName + ".vox";
            SaveFile(filepath);
            AssetDatabase.Refresh();
        }

        private void CreatePreviewPalette()
        {
            IColorQuantizer quantizer = new PaletteQuantizer();

            //List<Color32> colors;

            if (texturePalette != null)
            {
                Texture2D tp = GetReadableTexture(texturePalette);
                Color32[] cs32 = tp.GetPixels32();

                if (tp.width == 256 && tp.height == 1)
                {
                    //Don't use quantizer
                    selectedPalette = new List<Color32>();
                    foreach (var c32 in cs32)
                    {
                        selectedPalette.Add(c32);
                    }
                }
                else
                {
                    foreach (var c32 in cs32)
                    {
                        quantizer.AddColor(c32);
                    }
                    selectedPalette = quantizer.GetPalette(255);
                }
            }
            else
            {
                for (int i = 0; i < selectedVoxelInfo.Count; i++)
                {
                    VoxInfo v = selectedVoxelInfo[i];
                    quantizer.AddColor(v.col);
                }
                selectedPalette = quantizer.GetPalette(colorTotal);
            }

            previewPalette = new Texture2D(8, 32, TextureFormat.ARGB32, false);
            previewPalette.filterMode = FilterMode.Point;

            int xp = 0;
            int yp = 0;
            foreach (var c in selectedPalette)
            {
                previewPalette.SetPixel(xp, yp, c);
                xp++;
                if (xp >= 8)
                {
                    xp = 0;
                    yp++;
                }
            }
            previewPalette.Apply();

            foreach (VoxInfo v in selectedVoxelInfo)
            {
                v.idx = (Byte)(ClosestColor(selectedPalette, v.col) + 1);
            }
        }

        //private void UpdatePalette()
        //{
        //    IColorQuantizer quantizer = new PaletteQuantizer();
        //    //List<Color32> colors;

        //    if (texturePalette != null)
        //    {
        //        Texture2D tp = GetReadableTexture(texturePalette);
        //        Color32[] cs32 = tp.GetPixels32();

        //        if (tp.width == 256 && tp.height == 1)
        //        {
        //            //Don't use quantizer
        //            selectedPalette = new List<Color32>();
        //            foreach (var c32 in cs32)
        //            {
        //                selectedPalette.Add(c32);
        //            }
        //        }
        //        else
        //        {
        //            foreach (var c32 in cs32)
        //            {
        //                quantizer.AddColor(c32);
        //            }
        //            selectedPalette = quantizer.GetPalette(255);
        //        }
        //    }
        //    else
        //    {
        //        for (int i = 0; i < selectedVoxelInfo.Count; i++)
        //        {
        //            VoxInfo v = selectedVoxelInfo[i];
        //            quantizer.AddColor(v.col);
        //        }
        //        selectedPalette = quantizer.GetPalette(colorTotal);
        //    }

        //    foreach (VoxInfo v in selectedVoxelInfo)
        //    {
        //        v.idx = (Byte)(ClosestColor(selectedPalette, v.col) + 1);
        //    }
        //}

        private int ClosestColor(List<Color32> colors, Color32 target)
        {
            var colorDiffs = colors.Select(n => ColorDiff(n, target)).Min(n => n);
            return colors.FindIndex(n => ColorDiff(n, target) == colorDiffs);
        }

        private float ColorDiff(Color c1, Color c2)
        {
            return Mathf.Sqrt((c1.r - c2.r) * (c1.r - c2.r) + (c1.g - c2.g) * (c1.g - c2.g) + (c1.b - c2.b) * (c1.b - c2.b));
        }

        private void DrawRenderPreview(Rect previewRect)
        {
            if (selectedMaterial)
            {
                previewMaterial.mainTexture = selectedMaterial.mainTexture;
                if (selectedMaterial.HasProperty("_Color")) previewMaterial.color = selectedMaterial.GetColor("_Color");
            }

            Quaternion rotation = Quaternion.Euler(previewRotationX, previewRotationY, 0);
            Matrix4x4 matrix = Matrix4x4.identity;
            matrix.SetTRS(Vector3.zero, rotation, Vector3.one);
            Vector3 position = -matrix.MultiplyPoint(previewMesh.bounds.center);

            float dist = previewMesh.bounds.extents.magnitude * 2;

            preview.camera.transform.position = new Vector3(0, 0, -dist);
            preview.camera.transform.LookAt(Vector3.zero);
            preview.camera.clearFlags = CameraClearFlags.Color;
            preview.camera.backgroundColor = Color.gray;
            preview.camera.fieldOfView = 60;
            preview.camera.nearClipPlane = .3f;
            preview.camera.farClipPlane = 1000f;

            preview.BeginPreview(previewRect, GUIStyle.none);
            preview.DrawMesh(previewMesh, position, rotation, previewMaterial, 0);
            preview.Render();
            preview.EndAndDrawPreview(previewRect);
        }

        public void SaveFile(string fileName)
        {
            Int32 childSize = 24;//size
            childSize += 12 + 4 + selectedVoxelInfo.Count * 4;//xyzi
            childSize += 12 + 1024;//rgba
                                   //Debug.Log("childSize:" + childSize);

            Int32 sizeX = 0;
            Int32 sizeY = 0;
            Int32 sizeZ = 0;
            for (int i = 0; i < selectedVoxelInfo.Count; i++)
            {
                if (selectedVoxelInfo[i].pos.x > sizeX) sizeX = selectedVoxelInfo[i].pos.x;
                if (selectedVoxelInfo[i].pos.y > sizeY) sizeY = selectedVoxelInfo[i].pos.y;
                if (selectedVoxelInfo[i].pos.z > sizeZ) sizeZ = selectedVoxelInfo[i].pos.z;
            }

            using (BinaryWriter bw = new BinaryWriter(File.Open(fileName, FileMode.Create)))
            {
                bw.Write(new char[] { 'V', 'O', 'X', ' ' });
                bw.Write((Int32)150);

                bw.Write(new char[] { 'M', 'A', 'I', 'N' });
                bw.Write((Int32)0);//content size
                bw.Write(childSize);//child size

                bw.Write(new char[] { 'S', 'I', 'Z', 'E' });
                bw.Write((Int32)12);//content size
                bw.Write((Int32)0);//child size

                bw.Write(sizeX + 1);
                bw.Write(sizeZ + 1);
                bw.Write(sizeY + 1);

                bw.Write(new char[] { 'X', 'Y', 'Z', 'I' });

                Int32 xyziSize = 4 + selectedVoxelInfo.Count * 4;
                bw.Write(xyziSize);//content size
                bw.Write((Int32)0);//child size

                bw.Write((Int32)selectedVoxelInfo.Count);
                foreach (VoxInfo v in selectedVoxelInfo)
                {
                    bw.Write((Byte)v.pos.x);
                    bw.Write((Byte)v.pos.z);
                    bw.Write((Byte)v.pos.y);
                    bw.Write(v.idx);
                    //Debug.Log(v.pos + "I:" + v.idx);
                }

                bw.Write(new char[] { 'R', 'G', 'B', 'A' });
                bw.Write((Int32)1024);//content size
                bw.Write((Int32)0);//child size

                for (int i = 0; i < 255; i++)
                {
                    if (i >= selectedPalette.Count)
                    {
                        bw.Write((Int32)0x0);
                    }
                    else
                    {
                        bw.Write((Byte)(selectedPalette[i].r));
                        bw.Write((Byte)(selectedPalette[i].g));
                        bw.Write((Byte)(selectedPalette[i].b));
                        bw.Write((Byte)(selectedPalette[i].a));
                    }
                }
                bw.Write((Int32)0);
            }
        }
    }
}