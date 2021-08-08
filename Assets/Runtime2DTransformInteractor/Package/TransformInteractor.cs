using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Runtime2DTransformInteractor
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class TransformInteractor : MonoBehaviour
    {
        // Prefabs
        private GameObject spriteBoundsPrefab;

        [HideInInspector]
        public Interactor interactor;

        // Variables
        [HideInInspector]
        public bool selected;
        private Vector2 lastMousePosition;

        private void Start()
        {
            spriteBoundsPrefab = TransformInteractorController.instance.boundingRectanglePrefab;
            selected = false;
        }

        private void Update()
        {
            if (TransformInteractorController.instance.adjustSizeToZoom)
            {
                if (interactor)
                {
                    ChangeSizeOnZoom();
                }
            }
        }

        public void ResetBoundingRectangle()
        {
            interactor.Settup(interactor.targetGameObject);
        }

        private void ChangeSizeOnZoom()
        {
            float ratio = TransformInteractorController.instance.mainCamera.orthographicSize / Screen.height;

            Vector2 cornerScale = new Vector2(
                ratio * TransformInteractorController.instance.defaultCornerWidth,
                ratio * TransformInteractorController.instance.defaultCornerHeight);
            Vector2 rotationScale = new Vector2(
                ratio * TransformInteractorController.instance.defaultRotationWidth,
                ratio * TransformInteractorController.instance.defaultRotationHeight);
            float lineWidth = ratio * TransformInteractorController.instance.defaultLineWidth;

            interactor.spriteBounds.topLeftCorner.transform.localScale = cornerScale;

            interactor.spriteBounds.bottomLeftCorner.transform.localScale = cornerScale;

            interactor.spriteBounds.topRightCorner.transform.localScale = cornerScale;

            interactor.spriteBounds.bottomRightCorner.transform.localScale = cornerScale;

            interactor.spriteBounds.topLine.lineRenderer.startWidth = lineWidth;
            interactor.spriteBounds.topLine.lineRenderer.endWidth = lineWidth;

            interactor.spriteBounds.bottomLine.lineRenderer.startWidth = lineWidth;
            interactor.spriteBounds.bottomLine.lineRenderer.endWidth = lineWidth;

            interactor.spriteBounds.leftLine.lineRenderer.startWidth = lineWidth;
            interactor.spriteBounds.leftLine.lineRenderer.endWidth = lineWidth;

            interactor.spriteBounds.rightLine.lineRenderer.startWidth = lineWidth;
            interactor.spriteBounds.rightLine.lineRenderer.endWidth = lineWidth;

            interactor.spriteBounds.rotator.transform.localScale = rotationScale;
            interactor.spriteBounds.rotator.transform.localPosition = new Vector2(0,
                ratio * (interactor.spriteBounds.topLine.transform.localPosition.y
                + TransformInteractorController.instance.defaultRotationLineLength * 100
                + TransformInteractorController.instance.defaultRotationHeight + 10));

            interactor.spriteBounds.rotator.lineRenderer.startWidth = lineWidth;
            interactor.spriteBounds.rotator.lineRenderer.endWidth = lineWidth;
            interactor.spriteBounds.rotator.lineRenderer.transform.localScale =
                new Vector2(1, ratio * TransformInteractorController.instance.defaultRotationLineLength * 100);
        }

        private void OnMouseEnter()
        {
            TransformInteractorController.instance.SetMoveMouseCursor();
            if (selected) return;
            interactor = Instantiate(spriteBoundsPrefab).GetComponent<Interactor>();
            interactor.Settup(gameObject);
        }

        private void OnMouseExit()
        {
            TransformInteractorController.instance.SetDefaultMouseCursor();
            if (!selected)
            {
                // Cast a ray to check if one of the bounding rectangle elements is hit
                Collider2D colliderHit = Physics2D.OverlapPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                if (!colliderHit ||
                    (colliderHit.gameObject != interactor.spriteBounds.leftLine.gameObject
                    && colliderHit.gameObject != interactor.spriteBounds.rightLine.gameObject
                    && colliderHit.gameObject != interactor.spriteBounds.topLine.gameObject
                    && colliderHit.gameObject != interactor.spriteBounds.bottomLine.gameObject
                    && colliderHit.gameObject != interactor.spriteBounds.topLeftCorner.gameObject
                    && colliderHit.gameObject != interactor.spriteBounds.topRightCorner.gameObject
                    && colliderHit.gameObject != interactor.spriteBounds.bottomLeftCorner.gameObject
                    && colliderHit.gameObject != interactor.spriteBounds.bottomRightCorner.gameObject)
                    )
                {
                    UnSelect();
                }
            }
        }

        private void OnMouseOver()
        {
            TransformInteractorController.instance.SetMoveMouseCursor();
        }

        private void OnMouseUp()
        {
            if (!selected)
            {
                Select();
            }
        }

        private void OnMouseDown()
        {
            lastMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }

        private void OnMouseDrag()
        {
            if (!selected)
            {
                Select();
            }

            if (selected)
            {
                Vector2 newPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                TransformInteractorController.instance.MoveSelectedObjects(newPosition - lastMousePosition);
                lastMousePosition = newPosition;
            }
        }

        public void Select()
        {
            if (TransformInteractorController.instance.allowMultiSelection)
            {
                bool unselect = true;
                foreach (KeyCode key in TransformInteractorController.instance.multiSelectionKeys)
                {
                    if (Input.GetKey(key))
                    {
                        unselect = false;
                    }
                }

                if (unselect)
                {
                    TransformInteractorController.instance.UnSelectEverything();
                }
            }
            else
            {
                TransformInteractorController.instance.UnSelectEverything();
            }
            selected = true;
            TransformInteractorController.instance.selectedElements.Add(this);
            TransformInteractorController.instance.SetMoveMouseCursor();
        }

        public void UnSelect()
        {
            selected = false;
            TransformInteractorController.instance.selectedElements.Remove(this);
            Destroy(interactor.gameObject);
        }
    }
}
