using System;
using System.Collections.Generic;
using System.Globalization;
using Runtime2DTransformInteractor;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ObjectProperties : MonoBehaviour
{
    [Header("Position")] public TMP_InputField posXField;
    public TMP_InputField posYField;
    public Button posResetButton;

    [FormerlySerializedAs("rotXField")] [Header("Rotation")]
    public TMP_InputField rotZField;
    public Button rotResetButton;

    [Header("Scale")] public TMP_InputField scaleXField;
    public TMP_InputField scaleYField;
    public Button scaleResetButton;

    [Header("Misc")] public TMP_InputField layerField;
    public Button deleteButton;

    private bool _updateFields;
    private bool _objectInitialized;
    private SceneEditor _sceneEditor;

    private void Start()
    {
        _sceneEditor = GetComponent<SceneEditor>();
        
        layerField.onEndEdit.AddListener(UpdateLayer);
        layerField.onSelect.AddListener(val => TransformInteractorController.instance.selectedElements[0].transform.hasChanged = false);
        posXField.onEndEdit.AddListener(UpdatePosX);
        posXField.onSelect.AddListener(val => TransformInteractorController.instance.selectedElements[0].transform.hasChanged = false);
        posYField.onEndEdit.AddListener(UpdatePosY);
        posYField.onSelect.AddListener(val => TransformInteractorController.instance.selectedElements[0].transform.hasChanged = false);
        rotZField.onEndEdit.AddListener(UpdateRotZ);
        rotZField.onSelect.AddListener(val => TransformInteractorController.instance.selectedElements[0].transform.hasChanged = false);
        scaleXField.onEndEdit.AddListener(UpdateScaleX);
        scaleXField.onSelect.AddListener(val => TransformInteractorController.instance.selectedElements[0].transform.hasChanged = false);
        scaleYField.onEndEdit.AddListener(UpdateScaleY);
        scaleYField.onSelect.AddListener(val => TransformInteractorController.instance.selectedElements[0].transform.hasChanged = false);

        posResetButton.onClick.AddListener(ResetPosition);
        rotResetButton.onClick.AddListener(ResetRotation);
        scaleResetButton.onClick.AddListener(ResetScale);
        deleteButton.onClick.AddListener(DeleteObject);
        
    }

    private void ResetPosition()
    {
        TransformInteractorController.instance.selectedElements[0].transform.position = Vector2.zero;
    
        TransformInteractorController.instance.selectedElements[0].ResetBoundingRectangle();
        
    }

    private void ResetRotation()
    {
        TransformInteractorController.instance.selectedElements[0].transform.rotation = Quaternion.identity;

        TransformInteractorController.instance.selectedElements[0].ResetBoundingRectangle();
    }

    private void ResetScale()
    {
        TransformInteractorController.instance.selectedElements[0].transform.localScale = Vector2.one;

        TransformInteractorController.instance.selectedElements[0].ResetBoundingRectangle();
    }

    private void UpdateScaleY(string value)
    {
        if (TransformInteractorController.instance.selectedElements.Count != 1) return;
        Vector2 old = TransformInteractorController.instance.selectedElements[0].transform.localScale;
        old.y = float.Parse(value);
        TransformInteractorController.instance.selectedElements[0].transform.localScale = old;
        TransformInteractorController.instance.selectedElements[0].ResetBoundingRectangle();
    }

    private void UpdateScaleX(string value)
    {
        if (TransformInteractorController.instance.selectedElements.Count != 1) return;
        Vector2 old = TransformInteractorController.instance.selectedElements[0].transform.localScale;
        old.x = float.Parse(value);
        TransformInteractorController.instance.selectedElements[0].transform.localScale = old;
        TransformInteractorController.instance.selectedElements[0].ResetBoundingRectangle();
    }

    private void UpdateRotZ(string value)
    {
        if (TransformInteractorController.instance.selectedElements.Count != 1) return;
        Quaternion old = TransformInteractorController.instance.selectedElements[0].transform.rotation;
        old.z = float.Parse(value);
        TransformInteractorController.instance.selectedElements[0].transform.rotation = old;
        TransformInteractorController.instance.selectedElements[0].ResetBoundingRectangle();
    }

    private void UpdatePosY(string value)
    {
        if (TransformInteractorController.instance.selectedElements.Count != 1) return;
        Vector2 old = TransformInteractorController.instance.selectedElements[0].transform.position;
        old.y = float.Parse(value);
        TransformInteractorController.instance.selectedElements[0].transform.position = old;
        TransformInteractorController.instance.selectedElements[0].ResetBoundingRectangle();
    }

    private void UpdatePosX(string value)
    {
        if (TransformInteractorController.instance.selectedElements.Count != 1) return;
        Vector2 old = TransformInteractorController.instance.selectedElements[0].transform.position;
        old.x = float.Parse(value);
        TransformInteractorController.instance.selectedElements[0].transform.position = old;
        TransformInteractorController.instance.selectedElements[0].ResetBoundingRectangle();
    }

    public void UpdateLayer(string value)
    {
        if (TransformInteractorController.instance.selectedElements.Count != 1) return;
        TransformInteractorController.instance.selectedElements[0].GetComponent<SpriteRenderer>().sortingOrder =
            int.Parse(value);
        TransformInteractorController.instance.selectedElements[0].ResetBoundingRectangle();
    }
    
    public void DeleteObject()
    {
        _sceneEditor.objects.Remove(TransformInteractorController.instance.selectedElements[0].gameObject);
        
        Destroy(TransformInteractorController.instance.selectedElements[0].gameObject);
        //TransformInteractorController.instance.selectedElements.RemoveAt(0);
        TransformInteractorController.instance.UnSelectEverything();
    }

    private void Update()
    {
        List<TransformInteractor> instanceSelectedElements = TransformInteractorController.instance.selectedElements;
        if (instanceSelectedElements.Count == 1)
        {
            Transform objectTransform = instanceSelectedElements[0].transform;
            if (objectTransform.hasChanged || !_objectInitialized)
            {
                var position = objectTransform.position;
                posXField.text = position.x.ToString(CultureInfo.InvariantCulture);
                posYField.text = position.y.ToString(CultureInfo.InvariantCulture);
                var rotation = objectTransform.rotation;
                rotZField.text = rotation.z.ToString(CultureInfo.InvariantCulture);
                var scale = objectTransform.localScale;
                scaleXField.text = scale.x.ToString(CultureInfo.InvariantCulture);
                scaleYField.text = scale.y.ToString(CultureInfo.InvariantCulture);

                layerField.text = instanceSelectedElements[0].GetComponent<SpriteRenderer>().sortingOrder
                    .ToString();

                posXField.interactable = true;

                posYField.interactable = true;

                rotZField.interactable = true;

                scaleXField.interactable = true;

                scaleYField.interactable = true;

                layerField.interactable = true;

                deleteButton.interactable = true;

                _objectInitialized = true;
            }
        }
        else
        {
            _objectInitialized = false;
            
            posXField.interactable = false;
            posXField.text = "0";

            posYField.interactable = false;
            posYField.text = "0";
           
            rotZField.interactable = false;
            rotZField.text = "0";

            scaleXField.interactable = false;
            scaleXField.text = "0";
           
            scaleYField.interactable = false;
            scaleYField.text = "0";
           
            layerField.interactable = false;
            layerField.text = "0";

            deleteButton.interactable = false;
        }
    }
}
