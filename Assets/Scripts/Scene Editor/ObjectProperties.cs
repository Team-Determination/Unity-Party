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
    [Header("Object Properties")] public RectTransform objPropertiesRect;
    public bool propertiesWindowToggled;
    public bool objectsWindowToggled;
    public GameObject objectsWindowObject;
    public RectTransform objectsList;
    public bool objectPropertiesToggled;
    public GameObject objectPropertiesObject;
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
    public static ObjectProperties instance;

    private void Start()
    {
        _sceneEditor = GetComponent<SceneEditor>();

        instance = this;
        
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
        deleteButton.onClick.AddListener(() => DeleteObject());

        objPropertiesRect.anchoredPosition = new Vector3(180, 0);
    }

    public void ToggleObjectProperties()
    {
        
        
        if(!propertiesWindowToggled)
        {
            LeanTween.value(objPropertiesRect.gameObject, 180f, -180f, .55f).setOnUpdate(val =>
            {
                objPropertiesRect.anchoredPosition = new Vector3(val, 0);
            });
            propertiesWindowToggled = true;

        }

        objectsWindowObject.SetActive(false);
        objectPropertiesObject.SetActive(true);

    }

    public void ToggleObjects()
    {
        if(!propertiesWindowToggled)
        {
            LeanTween.value(objPropertiesRect.gameObject, 180f, -180f, .55f).setOnUpdate(val =>
            {
                objPropertiesRect.anchoredPosition = new Vector3(val, 0);
            });
            propertiesWindowToggled = true;

        }
        objectsWindowObject.SetActive(true);
        objectPropertiesObject.SetActive(false);
        LayoutRebuilder.ForceRebuildLayoutImmediate(objectsList);
    }

    public void ToggleWindow()
    {
        if(!propertiesWindowToggled)
        {
            LeanTween.value(objPropertiesRect.gameObject, 180f, -180f, .55f).setOnUpdate(val =>
            {
                objPropertiesRect.anchoredPosition = new Vector3(val, 0);
            });
            propertiesWindowToggled = true;
        }
        else
        {
            LeanTween.value(objPropertiesRect.gameObject, -180f, 180f, .55f).setOnUpdate(val =>
            {
                objPropertiesRect.anchoredPosition = new Vector3(val, 0);
            });
            propertiesWindowToggled = false;

        }
    }

    private void ResetPosition()
    {
        TransformInteractorController.instance.selectedElements[0].transform.position = Vector2.zero;
    
        TransformInteractorController.instance.selectedElements[0].ResetBoundingRectangle();
        TransformInteractorController.instance.selectedElements[0].interactor.AdaptTransform();
        
    }

    private void ResetRotation()
    {
        TransformInteractorController.instance.selectedElements[0].transform.rotation = Quaternion.identity;

        TransformInteractorController.instance.selectedElements[0].ResetBoundingRectangle();
        TransformInteractorController.instance.selectedElements[0].interactor.AdaptTransform();
    }

    private void ResetScale()
    {
        TransformInteractorController.instance.selectedElements[0].transform.localScale = Vector2.one;

        TransformInteractorController.instance.selectedElements[0].ResetBoundingRectangle();
        TransformInteractorController.instance.selectedElements[0].interactor.AdaptTransform();
    }

    private void UpdateScaleY(string value)
    {
        if (TransformInteractorController.instance.selectedElements.Count != 1) return;
        Vector2 old = TransformInteractorController.instance.selectedElements[0].transform.localScale;
        old.y = float.Parse(value);
        TransformInteractorController.instance.selectedElements[0].transform.localScale = old;
        TransformInteractorController.instance.selectedElements[0].ResetBoundingRectangle();
        
        TransformInteractorController.instance.selectedElements[0].interactor.AdaptTransform();
    }

    private void UpdateScaleX(string value)
    {
        if (TransformInteractorController.instance.selectedElements.Count != 1) return;
        Vector2 old = TransformInteractorController.instance.selectedElements[0].transform.localScale;
        old.x = float.Parse(value);
        TransformInteractorController.instance.selectedElements[0].transform.localScale = old;
        TransformInteractorController.instance.selectedElements[0].ResetBoundingRectangle();
        TransformInteractorController.instance.selectedElements[0].interactor.AdaptTransform();
    }

    private void UpdateRotZ(string value)
    {
        if (TransformInteractorController.instance.selectedElements.Count != 1) return;
        var instanceSelectedElement = TransformInteractorController.instance.selectedElements[0];
        Quaternion rotation = instanceSelectedElement.transform.rotation;
        Vector3 old = rotation.eulerAngles;
        old.z = float.Parse(value);
        rotation.eulerAngles = old;
        instanceSelectedElement.transform.rotation = rotation;
        instanceSelectedElement.ResetBoundingRectangle();
        instanceSelectedElement.interactor.AdaptTransform();
    }

    private void UpdatePosY(string value)
    {
        if (TransformInteractorController.instance.selectedElements.Count != 1) return;
        var instanceSelectedElement = TransformInteractorController.instance.selectedElements[0];
        Vector2 old = instanceSelectedElement.transform.position;
        old.y = float.Parse(value);
        instanceSelectedElement.transform.position = old;
        instanceSelectedElement.ResetBoundingRectangle();
        instanceSelectedElement.interactor.AdaptTransform();
    }

    private void UpdatePosX(string value)
    {
        if (TransformInteractorController.instance.selectedElements.Count != 1) return;
        var instanceSelectedElement = TransformInteractorController.instance.selectedElements[0];
        Vector2 old = instanceSelectedElement.transform.position;
        old.x = float.Parse(value);
        instanceSelectedElement.transform.position = old;
        instanceSelectedElement.ResetBoundingRectangle();
        instanceSelectedElement.interactor.AdaptTransform();
    }

    public void UpdateLayer(string value)
    {
        if (TransformInteractorController.instance.selectedElements.Count != 1) return;
        var instanceSelectedElement = TransformInteractorController.instance.selectedElements[0];
        instanceSelectedElement.GetComponent<SpriteRenderer>().sortingOrder =
            int.Parse(value);
        instanceSelectedElement.ResetBoundingRectangle();
        instanceSelectedElement.interactor.AdaptTransform();
    }
    
    
    public void DeleteObject(GameObject specificObject = null)
    {
        if(specificObject == null)
            specificObject = TransformInteractorController.instance.selectedElements[0].gameObject;
        _sceneEditor.objects.Remove(specificObject);
        
        Destroy(specificObject);
        //TransformInteractorController.instance.selectedElements.RemoveAt(0);
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
                rotZField.text = rotation.eulerAngles.z.ToString(CultureInfo.InvariantCulture);
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
