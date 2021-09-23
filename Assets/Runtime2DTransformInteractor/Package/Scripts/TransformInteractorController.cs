using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Runtime2DTransformInteractor
{
    /// <summary>
    /// Class that controls the Transform Interactors and provide settings to customize the interactors
    /// </summary>
    public class TransformInteractorController : MonoBehaviour
    {
        /// <summary>
        /// Instance of the class used to access its methods
        /// </summary>
        public static TransformInteractorController instance;

        /// <summary>
        /// Enumeration of the different mouse cursor types
        /// </summary>
        public enum MouseCursor
        {
            Default = 0,
            Move = 1,
            Resize = 2,
            Rotate = 3
        }

        /// <summary>
        /// The prefab of the bounding rectangle.
        /// </summary>
        public GameObject boundingRectanglePrefab;

        /// <summary>
        /// The sorting layer that will be used for the corner sprites
        /// </summary>
        [Header("Sorting Layers")]
        public string cornerSortingLayer;
        /// <summary>
        /// The sorting layer that will be used for the lineRenderers
        /// </summary>
        public string lineSortingLayer;
        /// <summary>
        /// The sorting layer that will be used for the rotation gizmo sprite
        /// </summary>
        public string rotationGizmoSortinglayer;
        /// <summary>
        /// The difference between the gameObject Z position and the corners of the bounding rectangle Z position
        /// Avoid setting it to 0 as the 2D colliders for the gameObject, Corners, and Lines will overlap, resulting in unexpected behaviors
        /// </summary>
        [Range(0.0f, 1.0f)]
        public float maxZDisplacement;

        /// <summary>
        /// Sprite for the corners of the bounding interactor
        /// </summary>
        [Header("Bounds aspect")]
        public Sprite cornerSprite;
        /// <summary>
        /// Color of the corners of the bounding interactor
        /// </summary>
        public Color cornerColor;
        /// <summary>
        /// Color of the edges of the bounding interactor
        /// </summary>
        public Color lineColor;
        /// <summary>
        /// Sprite for the rotation gizmo
        /// </summary>
        public Sprite rotationSprite;
        /// <summary>
        /// Color of the rotation gizmo
        /// </summary>
        public Color rotationColor;

        /// <summary>
        /// If true, the user can multiselect objects by pressing the multiSelectionKeys and clicking on them
        /// </summary>
        [Header("Multi Selection")]
        public bool allowMultiSelection;
        /// <summary>
        /// Keys that should be used to multiSelect objects
        /// </summary>
        public List<KeyCode> multiSelectionKeys;
        /// <summary>
        /// If true, clicking outside the collider of the target element will unselect the object
        /// </summary>
        public bool unselectWhenClickingOutside;

        /// <summary>
        /// If true, the rotation gizmo will be displayed
        /// </summary>
        [Header("Rotation")]
        public bool allowRotation;

        /// <summary>
        /// If true, the cursor will be changed when it is over a corner, an edge, the rotation gizmo or the collider of the object
        /// </summary>
        [Header("Cursor")]
        public bool changeCursor;
        /// <summary>
        /// The default mouse cursor
        /// </summary>
        public Texture2D defaultCursor;
        /// <summary>
        /// The cursor appearing when the mouse is over the object collider
        /// </summary>
        public Texture2D moveCursor;
        /// <summary>
        /// The cursor for the top and bottom edges of the bounding interactor
        /// </summary>
        public Texture2D resizeTopBottomCursor;
        /// <summary>
        /// The cursor for the top left and bottom right corners
        /// </summary>
        public Texture2D resizeTopLeftBottomRightCursor;
        /// <summary>
        /// The cursor for the left and right edges of the bounding interactor
        /// </summary>
        public Texture2D resizeLeftRightCursor;
        /// <summary>
        /// The cursor for the bottom left and top right corners
        /// </summary>
        public Texture2D resizeBottomLeftTopRightCursor;
        /// <summary>
        /// The cursor for the rotation gizmo
        /// </summary>
        public Texture2D rotateCursor;

        /// <summary>
        /// If true, the size of the bounding interactor will be adjusted to the zoom of the camera, keeping it always the same size on the screen
        /// </summary>
        [Header("Size on zoom")]
        public bool adjustSizeToZoom;
        /// <summary>
        /// The camera that can zoom on the objects
        /// </summary>
        public Camera mainCamera;
        /// <summary>
        /// The default width of the corners of the bounding interactor
        /// </summary>
        public float defaultCornerWidth;
        /// <summary>
        /// The default height of the corners of the bounding interactor
        /// </summary>
        public float defaultCornerHeight;
        /// <summary>
        /// The default width of the edges of the bounding interactor
        /// </summary>
        public float defaultLineWidth;
        /// <summary>
        /// The default width of the rotation gizmo
        /// </summary>
        public float defaultRotationWidth;
        /// <summary>
        /// The default height of the rotation gizmo
        /// </summary>
        public float defaultRotationHeight;
        /// <summary>
        /// The default distance between the top edge of the bounding interactor and the rotation gizmo
        /// </summary>
        public float defaultRotationLineLength;

        /// <summary>
        /// If true, aspect ratio will always be preserved by default
        /// </summary>
        [Header("Aspect ratio")]
        public bool alwaysPreserveAspectRatio;
        /// <summary>
        /// If true, aspect ratio will be preserved only when the user holds one of the specified keys
        /// </summary>
        public bool preserveAspectRatioOnKeyHold;
        /// <summary>
        /// Keys that should be used to preserve aspect ratio
        /// </summary>
        public List<KeyCode> aspectRatioKeys;

        [HideInInspector]
        public List<TransformInteractor> selectedElements;

        [HideInInspector]
        public MouseCursor mouseCursor;

        [Header("Position")] public TMP_InputField positionXField;
        public TMP_InputField positionYField;
        [Header("Rotation")] public TMP_InputField rotationXField;
        public TMP_InputField rotationYField;
        [Header("Scale")] public TMP_InputField scaleXField;
        public TMP_InputField scaleYField;

        private Camera _camera;

        private void Awake()
        {
            _camera = Camera.main;
            instance = this;
        }

        private void Start()
        {
            selectedElements = new List<TransformInteractor>();
            SetDefaultMouseCursor();
        }

        private void Update()
        {
            if (unselectWhenClickingOutside)
            {
                // Detects if the user clicked outside of the selected objects and deselects them if it is the case
                if (Input.GetMouseButtonDown(0))
                {
                    Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
                    RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

                    if (!hit.collider)
                    {
                        List<TransformInteractor> copy = new List<TransformInteractor>(selectedElements);
                        foreach (TransformInteractor elt in copy)
                        {
                            elt.UnSelect();
                        }
                    }
                }
            }

            if (selectedElements.Count == 1)
            {
                if (selectedElements[0].transform.hasChanged)
                {
                    
                }
            }
        }

        /// <summary>
        /// Moves the selected objects.
        /// </summary>
        /// <param name="translateAmount">The vector that is used to translate the selected elements</param>
        public void MoveSelectedObjects(Vector2 translateAmount)
        {
            foreach (TransformInteractor elt in selectedElements)
            {
                elt.transform.Translate(translateAmount, Space.World);
                elt.interactor.transform.Translate(translateAmount, Space.World);
            }
        }

        /// <summary>
        /// Adapts the new position of the corner that has been dragged if the aspect ratio should be kept.
        /// </summary>
        /// <param name="newPosition">The current mouse position</param>
        /// <param name="lastPosition">The previous mouse position (last frame)</param>
        /// <param name="interactor">The interactor that is being modified</param>
        /// <param name="cornerPosition">The poisiton of the corner (top left, top right, bottom left or bottom right)</param>
        public void AdaptNewMousePosition(ref Vector3 newPosition, Vector3 lastPosition, Interactor interactor, Corner.Position cornerPosition)
        {
            bool isPreserveAspectRatioKeyHold = false;
            foreach (KeyCode keyCode in aspectRatioKeys)
            {
                if (Input.GetKey(keyCode))
                {
                    isPreserveAspectRatioKeyHold = true;
                }
            }

            if (alwaysPreserveAspectRatio
                || (preserveAspectRatioOnKeyHold && isPreserveAspectRatioKeyHold))
            {
                if (cornerPosition == Corner.Position.TopRight)
                {
                    var worldToLocalMatrix = interactor.spriteBounds.transform.worldToLocalMatrix;
                    Vector3 newSize = worldToLocalMatrix * newPosition
                                      - worldToLocalMatrix * interactor.spriteBounds.bottomLeftCorner.transform.position;

                    Vector3 firstPossibleResize = new Vector3(
                        newSize.x,
                        newSize.x / interactor.originalAspectRatio,
                        0
                    );

                    Vector3 secondPossibleResize = new Vector3(
                        newSize.y * interactor.originalAspectRatio,
                        newSize.y,
                        0
                    );
                    
                    if (firstPossibleResize.x > secondPossibleResize.x)
                    {
                        var transform1 = interactor.spriteBounds.transform;
                        newPosition = transform1.localToWorldMatrix *
                                      (transform1.worldToLocalMatrix * interactor.spriteBounds.bottomLeftCorner.transform.position
                                       + new Vector4(firstPossibleResize.x, firstPossibleResize.y, firstPossibleResize.z, 0));
                    }
                    else
                    {
                        var transform1 = interactor.spriteBounds.transform;
                        newPosition = transform1.localToWorldMatrix *
                                      (transform1.worldToLocalMatrix * interactor.spriteBounds.bottomLeftCorner.transform.position
                                       + new Vector4(secondPossibleResize.x, secondPossibleResize.y, secondPossibleResize.z, 0));
                    }
                }
                else if (cornerPosition == Corner.Position.BottomLeft)
                {
                    var worldToLocalMatrix = interactor.spriteBounds.transform.worldToLocalMatrix;
                    Vector3 newSize = worldToLocalMatrix * interactor.spriteBounds.topRightCorner.transform.position
                                      - worldToLocalMatrix * newPosition;

                    Vector3 firstPossibleResize = new Vector3(
                        newSize.x,
                        newSize.x / interactor.originalAspectRatio,
                        0
                    );

                    Vector3 secondPossibleResize = new Vector3(
                        newSize.y * interactor.originalAspectRatio,
                        newSize.y,
                        0
                    );

                    if (firstPossibleResize.x > secondPossibleResize.x)
                    {
                        var transform1 = interactor.spriteBounds.transform;
                        newPosition = transform1.localToWorldMatrix *
                                      (transform1.worldToLocalMatrix * interactor.spriteBounds.topRightCorner.transform.position
                                       - new Vector4(firstPossibleResize.x, firstPossibleResize.y, firstPossibleResize.z, 0));
                    }
                    else
                    {
                        var transform1 = interactor.spriteBounds.transform;
                        newPosition = transform1.localToWorldMatrix *
                                      (transform1.worldToLocalMatrix * interactor.spriteBounds.topRightCorner.transform.position
                                       - new Vector4(secondPossibleResize.x, secondPossibleResize.y, secondPossibleResize.z, 0));
                    }
                }
                else if (cornerPosition == Corner.Position.TopLeft)
                {
                    var transform1 = interactor.spriteBounds.transform;
                    var worldToLocalMatrix = transform1.worldToLocalMatrix;
                    Vector3 newSize = worldToLocalMatrix * interactor.spriteBounds.bottomRightCorner.transform.position
                                      - worldToLocalMatrix * newPosition;

                    Vector3 firstPossibleResize = new Vector3(
                        - newSize.x,
                        newSize.x / interactor.originalAspectRatio,
                        0
                    );

                    Vector3 secondPossibleResize = new Vector3(
                        newSize.y * interactor.originalAspectRatio,
                        - newSize.y,
                        0
                    );

                    if (firstPossibleResize.x < secondPossibleResize.x)
                    {
                        var transform2 = interactor.spriteBounds.transform;
                        newPosition = transform2.localToWorldMatrix *
                                      (transform2.worldToLocalMatrix * interactor.spriteBounds.bottomRightCorner.transform.position
                                       + new Vector4(firstPossibleResize.x, firstPossibleResize.y, firstPossibleResize.z, 0));
                    }
                    else
                    {
                        var transform2 = interactor.spriteBounds.transform;
                        newPosition = transform2.localToWorldMatrix *
                                      (transform2.worldToLocalMatrix * interactor.spriteBounds.bottomRightCorner.transform.position
                                       + new Vector4(secondPossibleResize.x, secondPossibleResize.y, secondPossibleResize.z, 0));
                    }
                }
                else if (cornerPosition == Corner.Position.BottomRight)
                {
                    var worldToLocalMatrix = interactor.spriteBounds.transform.worldToLocalMatrix;
                    Vector3 newSize = worldToLocalMatrix * newPosition
                                      - worldToLocalMatrix * interactor.spriteBounds.topLeftCorner.transform.position;

                    Vector3 firstPossibleResize = new Vector3(
                        -newSize.x,
                        newSize.x / interactor.originalAspectRatio,
                        0
                    );

                    Vector3 secondPossibleResize = new Vector3(
                        newSize.y * interactor.originalAspectRatio,
                        -newSize.y,
                        0
                    );

                    if (firstPossibleResize.x < secondPossibleResize.x)
                    {
                        var transform1 = interactor.spriteBounds.transform;
                        newPosition = transform1.localToWorldMatrix *
                                      (transform1.worldToLocalMatrix * interactor.spriteBounds.topLeftCorner.transform.position
                                       - new Vector4(firstPossibleResize.x, firstPossibleResize.y, firstPossibleResize.z, 0));
                    }
                    else
                    {
                        var transform1 = interactor.spriteBounds.transform;
                        newPosition = transform1.localToWorldMatrix *
                                      (transform1.worldToLocalMatrix * interactor.spriteBounds.topLeftCorner.transform.position
                                       - new Vector4(secondPossibleResize.x, secondPossibleResize.y, secondPossibleResize.z, 0));
                    }
                }
            }
        }

        /// <summary>
        /// Unselects every selected transform.
        /// </summary>
        public void UnSelectEverything()
        {
            List<TransformInteractor> copy = new List<TransformInteractor>(selectedElements);
            foreach (TransformInteractor elt in copy)
            {
                elt.UnSelect();
            }
        }

        public void SetDefaultMouseCursor()
        {
            if (changeCursor)
            {
                if (mouseCursor != MouseCursor.Default)
                {
                    Cursor.SetCursor(defaultCursor, Vector2.zero, CursorMode.Auto);
                    mouseCursor = MouseCursor.Default;
                }
            }
        }

        public void SetMoveMouseCursor()
        {
            if (changeCursor)
            {
                if (mouseCursor != MouseCursor.Move)
                {
                    Cursor.SetCursor(moveCursor, new Vector2(moveCursor.width / 2, moveCursor.height / 2), CursorMode.Auto);
                    mouseCursor = MouseCursor.Move;
                }
            }
        }

        public void SetCornerMouseCursor(Corner.Position position, float rotation)
        {
            if (!changeCursor) return;
            if (mouseCursor == MouseCursor.Resize) return;
            // Cursor has to be rotated depending on the transform rotation.

            rotation = rotation % 180;
            Texture2D cursor;
            switch (position)
            {
                case Corner.Position.BottomLeft:
                case Corner.Position.TopRight:
                {
                    if (rotation > 0 && rotation <= 22.5f)
                    {
                        cursor = resizeBottomLeftTopRightCursor;
                    }
                    else if (rotation > 22.5f && rotation <= 67.5f)
                    {
                        cursor = resizeTopBottomCursor;
                    }
                    else if (rotation > 67.5f && rotation <= 112.5f)
                    {
                        cursor = resizeTopLeftBottomRightCursor;
                    }
                    else if (rotation > 112.5f && rotation <= 157.5f)
                    {
                        cursor = resizeLeftRightCursor;
                    }
                    else
                    {
                        cursor = resizeBottomLeftTopRightCursor;
                    }
                    Cursor.SetCursor(cursor, new Vector2(cursor.width / 2, cursor.height / 2), CursorMode.Auto);
                    break;
                }
                case Corner.Position.BottomRight:
                case Corner.Position.TopLeft:
                {
                    if (rotation > 0 && rotation <= 22.5f)
                    {
                        cursor = resizeTopLeftBottomRightCursor;
                    }
                    else if (rotation > 22.5f && rotation <= 67.5f)
                    {
                        cursor = resizeLeftRightCursor;
                    }
                    else if (rotation > 67.5f && rotation <= 112.5f)
                    {
                        cursor = resizeBottomLeftTopRightCursor;
                    }
                    else if (rotation > 112.5f && rotation <= 157.5f)
                    {
                        cursor = resizeTopBottomCursor;
                    }
                    else
                    {
                        cursor = resizeTopLeftBottomRightCursor;
                    }
                    Cursor.SetCursor(cursor, new Vector2(cursor.width / 2, cursor.height / 2), CursorMode.Auto);
                    break;
                }
            }
            mouseCursor = MouseCursor.Resize;
        }

        public void SetLineMouseCursor(Line.Position position, float rotation)
        {
            if (!changeCursor) return;
            if (mouseCursor == MouseCursor.Resize) return;
            // Cursor has to be rotated depending on the transform rotation.

            rotation = rotation % 180;
            Texture2D cursor;
            switch (position)
            {
                case Line.Position.Left:
                case Line.Position.Right:
                {
                    if (rotation > 0 && rotation <= 22.5f)
                    {
                        cursor = resizeLeftRightCursor;
                    }
                    else if (rotation > 22.5f && rotation <= 67.5f)
                    {
                        cursor = resizeBottomLeftTopRightCursor;
                    }
                    else if (rotation > 67.5f && rotation <= 112.5f)
                    {
                        cursor = resizeTopBottomCursor;
                    }
                    else if (rotation > 112.5f && rotation <= 157.5f)
                    {
                        cursor = resizeTopLeftBottomRightCursor;
                    }
                    else
                    {
                        cursor = resizeLeftRightCursor;
                    }
                    Cursor.SetCursor(cursor, new Vector2(cursor.width / 2, cursor.height / 2), CursorMode.Auto);
                    break;
                }
                case Line.Position.Top:
                case Line.Position.Bottom:
                {
                    if (rotation > 0 && rotation <= 22.5f)
                    {
                        cursor = resizeTopBottomCursor;
                    }
                    else if (rotation > 22.5f && rotation <= 67.5f)
                    {
                        cursor = resizeTopLeftBottomRightCursor;
                    }
                    else if (rotation > 67.5f && rotation <= 112.5f)
                    {
                        cursor = resizeLeftRightCursor;
                    }
                    else if (rotation > 112.5f && rotation <= 157.5f)
                    {
                        cursor = resizeBottomLeftTopRightCursor;
                    }
                    else
                    {
                        cursor = resizeTopBottomCursor;
                    }
                    Cursor.SetCursor(cursor, new Vector2(cursor.width / 2, cursor.height / 2), CursorMode.Auto);
                    break;
                }
            }
            mouseCursor = MouseCursor.Resize;
        }

        public void SetRotatorMouseCursor()
        {
            if (!changeCursor) return;
            if (mouseCursor == MouseCursor.Rotate) return;
            Cursor.SetCursor(rotateCursor,
                new Vector2(rotateCursor.width / 2, rotateCursor.height / 2),
                CursorMode.Auto);
            mouseCursor = MouseCursor.Rotate;
        }
    }
}
