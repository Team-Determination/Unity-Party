using UnityEditor;
using UnityEngine;

namespace MagicaVoxelTools
{
    [CustomEditor(typeof(VoxelData))]
    public class EditorVoxelData : Editor
    {
        //Preview
        private PreviewRenderUtility preview;

        private float previewRotationX = 0;
        private float previewRotationY = 0;

        private Mesh previewMesh;
        private Material previewMaterial;
        private Texture2D previewTexture;

        private void OnEnable()
        {
            VoxelData vd = (VoxelData)target;

            preview = new PreviewRenderUtility();
            previewMaterial = new Material(Shader.Find("Mobile/Diffuse"));

            //VoxelData data = new VoxelData(vd);
            PreviewVoxelMesh pm = new PreviewVoxelMesh(vd);

            previewMesh = pm.mesh;
            //previewTexture = pm.texture;
            //previewMaterial.mainTexture = previewTexture;
        }

        private void OnDisable()
        {
            preview.Cleanup();
            preview = null;
        }

        public override void OnInspectorGUI()
        {
            VoxelData data = (VoxelData)target;
            GUILayout.Label("Voxel Size: " + data.width + "," + data.height + "," + data.depth);
            HandlePreview();
            //EditorGUILayout.Vector3Field("Pivot", data.pivot);
        }

        private void HandlePreview()
        {
            Rect previewRect = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, 256);

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
            preview.Render();
            preview.EndAndDrawPreview(previewRect);
        }
    }
}