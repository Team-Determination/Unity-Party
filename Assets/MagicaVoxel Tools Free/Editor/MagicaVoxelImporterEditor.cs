using UnityEditor;

using UnityEngine;

namespace MagicaVoxelTools
{
    [CustomEditor(typeof(MagicaVoxelImporter))]
    public class MagicaVoxelImporterEditor : UnityEditor.AssetImporters.ScriptedImporterEditor
    {
        private enum ImportType
        {
            Single,
            Multi,
            Merge
        }

        private enum ImportCollider
        {
            None,
            Box,
            Sphere,
            Cylinder,
            Mesh
        }

        private MagicaVoxel importer = new MagicaVoxel();

        private string importAssetName;
        private string importAssetPath;

        //Prefab ImportSettings
        private ImportType importType = ImportType.Single;

        private ImportCollider importCollider = ImportCollider.None;
        private float importScale = .1f;
        private Vector3 importPivot = Vector3.zero;
        private bool importStatic = false;
        private bool import2Sided = true;
        private int importPadding = 1;
        private int importTextureScale = 1;
        private bool importRemoveHidden = false;
        private bool importRigidbody = false;

        private Shader importDiffuseShader = null;

        private void LoadSettings()
        {
            string shaderNameDiffuse = EditorPrefs.GetString("VOXIMPORT_importDiffuseShader", "Standard");

            importDiffuseShader = Shader.Find(shaderNameDiffuse);
            VerifyShaders();

            importScale = EditorPrefs.GetFloat("VOXIMPORT_SCALE", .1f);
            importRemoveHidden = EditorPrefs.GetBool("VOXIMPORT_HIDDEN", false);
            importRigidbody = EditorPrefs.GetBool("VOXIMPORT_RIGIDBODY", false);
            importStatic = EditorPrefs.GetBool("VOXIMPORT_STATIC", false);
            import2Sided = EditorPrefs.GetBool("VOXIMPORT_SHADOW2SIDED", true);
            importPadding = EditorPrefs.GetInt("VOXIMPORT_TEXUREPAD", 1);
            importTextureScale = EditorPrefs.GetInt("VOXIMPORT_TEXURESCALE", 1);

            importCollider = (ImportCollider)EditorPrefs.GetInt("VOXIMPORT_COLLIDER", 0);
        }

        private void SaveSettings()
        {
            EditorPrefs.SetString("VOXIMPORT_importDiffuseShader", importDiffuseShader.name);

            EditorPrefs.SetFloat("VOXIMPORT_SCALE", importScale);
            EditorPrefs.SetBool("VOXIMPORT_HIDDEN", importRemoveHidden);
            EditorPrefs.SetBool("VOXIMPORT_RIGIDBODY", importRigidbody);
            EditorPrefs.SetBool("VOXIMPORT_STATIC", importStatic);
            EditorPrefs.SetBool("VOXIMPORT_SHADOW2SIDED", import2Sided);
            EditorPrefs.SetInt("VOXIMPORT_TEXUREPAD", importPadding);
            EditorPrefs.SetInt("VOXIMPORT_TEXURESCALE", importTextureScale);
            EditorPrefs.SetInt("VOXIMPORT_COLLIDER", (int)importCollider);
        }

        private void DefaultSettings()
        {
            EditorPrefs.DeleteKey("VOXIMPORT_importDiffuseShader");
            EditorPrefs.DeleteKey("VOXIMPORT_importMetalShader");
            EditorPrefs.DeleteKey("VOXIMPORT_importGlassShader");
            EditorPrefs.DeleteKey("VOXIMPORT_importEmissionShader");

            EditorPrefs.DeleteKey("VOXIMPORT_SCALE");
            EditorPrefs.DeleteKey("VOXIMPORT_MAT");
            EditorPrefs.DeleteKey("VOXIMPORT_HIDDEN");
            EditorPrefs.DeleteKey("VOXIMPORT_RIGIDBODY");
            EditorPrefs.DeleteKey("VOXIMPORT_STATIC");
            EditorPrefs.DeleteKey("VOXIMPORT_SHADOW2SIDED");
            EditorPrefs.DeleteKey("VOXIMPORT_TEXUREPAD");
            EditorPrefs.DeleteKey("VOXIMPORT_TEXURESCALE");
            EditorPrefs.DeleteKey("VOXIMPORT_COLLIDER");
            LoadSettings();
        }

        //Preview
        private PreviewRenderUtility preview;

        private float previewRotationX = 0;
        private float previewRotationY = 0;
        private int selectedModel = 0;

        private Mesh previewMesh;
        private Material previewMaterial;
        private Texture2D previewTexture;

        private Mesh previewPivotMesh;
        private Material previewPivotMaterial;

        private void ShowWait()
        {
            EditorUtility.DisplayProgressBar("Please Wait", "Working...", 1);
        }

        private void ClearWait()
        {
            EditorUtility.ClearProgressBar();
        }

        public override void OnEnable()
        {
            base.OnEnable();

            ShowWait();

            LoadSettings();

            preview = new PreviewRenderUtility();

            previewMesh = CreatePrimitiveMesh(PrimitiveType.Cube);
            previewPivotMesh = CreatePrimitiveMesh(PrimitiveType.Sphere);
            //previewMaterial = new Material(Shader.Find("Mobile/Diffuse"));
            previewMaterial = new Material(Shader.Find("Particles/Standard Surface"));

            previewPivotMaterial = new Material(Shader.Find("GUI/Text Shader"));
            previewPivotMaterial.color = Color.red;

            string path = AssetDatabase.GetAssetPath(target);
            importAssetName = path.Substring(path.LastIndexOf('/') + 1);
            importAssetPath = path.Substring(0, path.Length - importAssetName.Length);
            importAssetName = importAssetName.Substring(0, importAssetName.Length - 4);
            //Debug.Log(path);
            //Debug.Log(importAssetPath);
            //Debug.Log(importAssetName);

            importer.LoadFile(path);

            if (importer.IsValid)
            {
                CreatePreviewMesh(importer.GetData(0));
                CenterPivot();
            }

            ClearWait();
        }

        public override void OnDisable()
        {
            base.OnDisable();

            SaveSettings();
            preview.Cleanup();
            preview = null;
        }

        public override void OnInspectorGUI()
        {
            if (!importer.IsValid)
            {
                EditorGUILayout.LabelField("Invalid Vox file!");
                return;
            }

            if (importer.models.Count == 1)
            {
                EditorGUILayout.BeginHorizontal();
                string info = string.Format("{0} Voxels {1}", importer.totalVoxels, importer.Size(selectedModel).ToString());
                EditorGUILayout.LabelField(info);
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.BeginHorizontal();

                EditorGUI.BeginChangeCheck();
                importType = (ImportType)EditorGUILayout.EnumPopup(importType, GUILayout.Width(58));
                if (EditorGUI.EndChangeCheck())
                {
                    if (importType == ImportType.Merge)
                    {
                        CreatePreviewMesh(MergedModels());
                        CenterPivot();
                    }
                    else
                    {
                        CreatePreviewMesh(importer.GetData(selectedModel));
                        CenterPivot();
                    }
                }

                if (importType == ImportType.Merge)
                {
                    Vector3Int previewSize = new Vector3Int((int)previewMesh.bounds.size.x, (int)previewMesh.bounds.size.y, (int)previewMesh.bounds.size.z);
                    string info = string.Format("{0} Models, {1} Voxels {2}", importer.models.Count, importer.totalVoxels, previewSize.ToString());
                    EditorGUILayout.LabelField(info);
                }
                else
                {
                    EditorGUI.BeginChangeCheck();
                    selectedModel = EditorGUILayout.Popup(selectedModel, importer.modelNames.ToArray());
                    if (EditorGUI.EndChangeCheck())
                    {
                        CreatePreviewMesh(importer.GetData(selectedModel));
                        CenterPivot();
                    }

                    string info = string.Format("{0} Models {1}", importer.models.Count, importer.Size(selectedModel).ToString());
                    EditorGUILayout.LabelField(info);
                }

                EditorGUILayout.EndHorizontal();
            }

            HandlePreview();

            importScale = EditorGUILayout.FloatField("Voxel Scale", importScale);
            importPadding = EditorGUILayout.IntSlider("Texture Padding", importPadding, 0, 8);
            importTextureScale = EditorGUILayout.IntSlider("Texture Scale", importTextureScale, 1, 16);

            GUILayout.BeginHorizontal();
            importPivot = EditorGUILayout.Vector3Field("Pivot", importPivot);
            if (GUILayout.Button("Center", GUILayout.Width(50))) CenterPivot();
            GUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            importStatic = EditorGUILayout.Toggle("Static", importStatic);
            importRemoveHidden = EditorGUILayout.Toggle("Remove Hidden", importRemoveHidden);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            import2Sided = EditorGUILayout.Toggle("2 Sided", import2Sided);
            EditorGUILayout.EndHorizontal();

            importRigidbody = EditorGUILayout.Toggle("Rigid Body", importRigidbody);

            importCollider = (ImportCollider)EditorGUILayout.EnumPopup("Collider", importCollider);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Create Prefab"))
            {
                CreatePrefab();
            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Default Settings"))
            {
                DefaultSettings();
            }

            GUILayout.Space(10);
            importDiffuseShader = (Shader)EditorGUILayout.ObjectField("Diffuse", importDiffuseShader, typeof(Shader), true);

        }

        private void CreatePrefab()
        {
            ShowWait();

            if (importType == ImportType.Single)
            {
                VoxelData data = importer.GetData(selectedModel);
                if (importRemoveHidden) RemoveHidden(data);
                CreatePrefab(data, importAssetName + "_" + importer.modelNames[selectedModel]);
            }
            if (importType == ImportType.Multi)
            {
                for (int i = 0; i < importer.models.Count; i++)
                {
                    VoxelData data = importer.GetData(i);
                    if (importRemoveHidden) RemoveHidden(data);
                    CreatePrefab(data, importAssetName + "_" + importer.modelNames[i]);
                }
            }
            if (importType == ImportType.Merge)
            {
                VoxelData data = MergedModels();
                if (importRemoveHidden) RemoveHidden(data);
                CreatePrefab(data, importAssetName + "_merged");
            }

            ClearWait();
        }

        private void CreateData()
        {
            ShowWait();

            if (importType == ImportType.Single)
            {
                VoxelData data = importer.GetData(selectedModel);
                if (importRemoveHidden) RemoveHidden(data);
                CreateData(data, importAssetName + "_" + importer.modelNames[selectedModel] + "_data");
            }
            if (importType == ImportType.Multi)
            {
                for (int i = 0; i < importer.models.Count; i++)
                {
                    VoxelData data = importer.GetData(i);
                    if (importRemoveHidden) RemoveHidden(data);
                    CreateData(data, importAssetName + "_" + importer.modelNames[i] + "_data");
                }
            }
            if (importType == ImportType.Merge)
            {
                VoxelData data = MergedModels();
                if (importRemoveHidden) RemoveHidden(data);
                CreateData(data, importAssetName + "_merged" + "_data");
            }

            ClearWait();
        }

        private void HandlePreview()
        {
            Rect previewRect = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, 256 + 100);

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
        }

        private void DrawRenderPreview(Rect previewRect)
        {
            Quaternion rotation = Quaternion.Euler(previewRotationX, previewRotationY, 0);
            Matrix4x4 matrix = Matrix4x4.identity;
            matrix.SetTRS(Vector3.zero, rotation, Vector3.one);
            Vector3 position = -matrix.MultiplyPoint(previewMesh.bounds.center);
            Vector3 position2 = matrix.MultiplyPoint(importPivot - previewMesh.bounds.center);

            float dist = previewMesh.bounds.extents.magnitude * 2;

            preview.camera.transform.position = new Vector3(0, 0, -dist);
            preview.camera.transform.LookAt(Vector3.zero);
            preview.camera.clearFlags = CameraClearFlags.Color;
            preview.camera.backgroundColor = Color.gray;
            preview.camera.fieldOfView = 60;
            preview.camera.nearClipPlane = .3f;
            preview.camera.farClipPlane = 10000f;

            preview.BeginPreview(previewRect, GUIStyle.none);

            preview.DrawMesh(previewMesh, position, rotation, previewMaterial, 0);
            preview.DrawMesh(previewPivotMesh, position2, rotation, previewPivotMaterial, 0);

            preview.Render();
            preview.EndAndDrawPreview(previewRect);
        }

        private void CenterPivot()
        {
            importPivot = previewMesh.bounds.size / 2;
        }

        private static Mesh CreatePrimitiveMesh(PrimitiveType type)
        {
            GameObject gameObject = GameObject.CreatePrimitive(type);
            Mesh mesh = gameObject.GetComponent<MeshFilter>().sharedMesh;
            GameObject.DestroyImmediate(gameObject);
            return mesh;
        }

        private VoxelData MergedModels()
        {
            Vector3Int min = new Vector3Int(4096, 4096, 4096);
            Vector3Int max = new Vector3Int(-4096, -4096, -4096);

            for (int i = 0; i < importer.models.Count; i++)
            {
                Vector3Int s = importer.Size(i);
                Vector3Int c = importer.modelOffsets[i];//centers

                int xmin = c.x - (s.x / 2);
                int xmax = c.x + (s.x / 2);
                if (min.x > xmin) min.x = xmin;
                if (max.x < xmax) max.x = xmax;

                int ymin = c.y - (s.y / 2);
                int ymax = c.y + (s.y / 2);
                if (min.y > ymin) min.y = ymin;
                if (max.y < ymax) max.y = ymax;

                int zmin = c.z - (s.z / 2);
                int zmax = c.z + (s.z / 2);
                if (min.z > zmin) min.z = zmin;
                if (max.z < zmax) max.z = zmax;
            }

            //Debug.Log(min.ToString());
            //Debug.Log(max.ToString());

            int w = Mathf.Abs(max.x - min.x) + 1;
            int h = Mathf.Abs(max.y - min.y) + 1;
            int d = Mathf.Abs(max.z - min.z) + 1;
            //Debug.Log(w + "," + h + "," + d);

            VoxelData data = new VoxelData(w, h, d, importer.palette, importer.materials);

            //Merge all models...
            for (int i = 0; i < importer.models.Count; i++)
            {
                Vector3Int s = importer.Size(i);
                Vector3Int o = importer.modelOffsets[i];
                o.x -= min.x;
                o.y -= min.y;
                o.z -= min.z;

                o.x -= s.x / 2;
                o.y -= s.y / 2;
                o.z -= s.z / 2;

                for (int x = 0; x < s.x; x++)
                {
                    for (int y = 0; y < s.y; y++)
                    {
                        for (int z = 0; z < s.z; z++)
                        {
                            if (importer.models[i][x, y, z] > 0)
                            {
                                data.SetVoxel(x + o.x, y + o.y, z + o.z, importer.models[i][x, y, z]);
                            }
                        }
                    }
                }
            }

            return data;
        }

        private void RemoveHidden(VoxelData data)
        {
            for (int x = 0; x < data.width; x++)
            {
                for (int y = 0; y < data.height; y++)
                {
                    for (int z = 0; z < data.depth; z++)
                    {
                        if (!data.IsVisible(x, y, z))
                        {
                            data.SetVoxel(x, y, z, 0);
                        }
                    }
                }
            }
        }

        private void CreatePreviewMesh(VoxelData data)
        {
            ShowWait();

            PreviewVoxelMesh pm = new PreviewVoxelMesh(data);
            previewMesh = pm.mesh;

            ClearWait();
        }

        private void VerifyShaders()
        {
            //Fallback to Standard Shaders...
            if (importDiffuseShader == null) importDiffuseShader = Shader.Find("Standard");
        }

        private void CreateData(VoxelData data, string pfName)
        {
            string assetPath = importAssetPath + pfName + ".asset";

            AssetDatabase.CreateAsset(data, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public void CreatePrefab(VoxelData data, string pfName)
        {
            VoxelData dd = new VoxelData(data);
            VerifyShaders();

            GameObject obj = new GameObject(pfName);
            obj.isStatic = importStatic;

            OptimizedVoxelMesh assetDifuse = null;
            Material diffuseMat = new Material(importDiffuseShader);
            diffuseMat.name = "Diffuse";
            diffuseMat.enableInstancing = true;

            if (dd.VoxelCount() > 0)
            {
                assetDifuse = new OptimizedVoxelMesh(dd, importPivot, importTextureScale, importPadding, importScale);
                assetDifuse.mesh.name = "Diffuse";
                assetDifuse.texture.name = "Diffuse";

                MeshFilter mf = obj.AddComponent<MeshFilter>();
                mf.mesh = assetDifuse.mesh;

                diffuseMat.mainTexture = assetDifuse.texture;
                diffuseMat.SetFloat("_Metallic", 0);
                diffuseMat.SetFloat("_Glossiness", 0);

                MeshRenderer mr = obj.AddComponent<MeshRenderer>();
                if (import2Sided) mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;

                mr.material = diffuseMat;

                AddCollider(obj);
                if (importRigidbody) obj.AddComponent<Rigidbody>();
            }

 

            //create prefab

            if (assetDifuse != null)
            {
                string assetPath = importAssetPath + pfName + ".asset";
                string prefabPath = importAssetPath + pfName + ".prefab";

                AssetDatabase.CreateAsset(new Mesh(), assetPath);

                if (assetDifuse != null)
                {
                    AssetDatabase.AddObjectToAsset(assetDifuse.mesh, assetPath);
                    AssetDatabase.AddObjectToAsset(assetDifuse.texture, assetPath);
                    AssetDatabase.AddObjectToAsset(diffuseMat, assetPath);
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                PrefabUtility.CreatePrefab(prefabPath, obj);
                //GameObject prefab = PrefabUtility.CreatePrefab(prefabPath, obj);
                //Selection.activeObject = prefab;
            }

            GameObject.DestroyImmediate(obj);
        }

        void AddCollider(GameObject obj)
        {
            switch (importCollider)
            {
                case ImportCollider.None:
                    break;

                case ImportCollider.Box:
                    obj.AddComponent<BoxCollider>();
                    break;

                case ImportCollider.Sphere:
                    obj.AddComponent<SphereCollider>();
                    break;

                case ImportCollider.Mesh:
                    obj.AddComponent<MeshCollider>();
                    break;
            }
        }
    }
}