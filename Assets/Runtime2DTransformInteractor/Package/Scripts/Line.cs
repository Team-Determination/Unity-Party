using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Runtime2DTransformInteractor
{
    public class Line : MonoBehaviour
    {
        public LineRenderer lineRenderer;

        public enum Position
        {
            Left = 0,
            Right = 1,
            Top = 2,
            Bottom = 3
        }

        // Serialized fields

        public SpriteBounds spriteBounds;
        public Position position;

        // Non-Serialized fields

        private Vector2 lastMousePosition;

        private void OnMouseEnter()
        {
            TransformInteractorController.instance.SetLineMouseCursor(position, spriteBounds.interactor.targetGameObject.transform.rotation.eulerAngles.z);
        }

        private void OnMouseExit()
        {
            TransformInteractorController.instance.SetDefaultMouseCursor();

            TransformInteractor transformInteractor = spriteBounds.interactor.targetGameObject.GetComponent<TransformInteractor>();
            if (!transformInteractor.selected)
            {
                // Check if one of the bounding rectangle elements is hit
                Collider2D colliderHit = Physics2D.OverlapPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                if (!colliderHit ||
                    (colliderHit.gameObject != spriteBounds.topLeftCorner.gameObject
                    && colliderHit.gameObject != spriteBounds.topRightCorner.gameObject
                    && colliderHit.gameObject != spriteBounds.bottomLeftCorner.gameObject
                    && colliderHit.gameObject != spriteBounds.bottomRightCorner.gameObject)
                    )
                {
                    transformInteractor.UnSelect();
                }
            }
        }

        private void OnMouseOver()
        {
            TransformInteractorController.instance.SetLineMouseCursor(position, spriteBounds.interactor.targetGameObject.transform.rotation.eulerAngles.z);
        }

        private void OnDisable()
        {
            TransformInteractorController.instance.SetDefaultMouseCursor();
        }

        private void OnMouseDown()
        {
            lastMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            TransformInteractor transformInteractor = spriteBounds.interactor.targetGameObject.GetComponent<TransformInteractor>();
            if (!transformInteractor.selected)
            {
                transformInteractor.Select();
            }
        }

        private void OnMouseDrag()
        {
            Vector2 newPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            
            MoveObjects(newPosition);

            lastMousePosition = newPosition;
        }

        public void MoveObjects(Vector2 newPosition)
        {
            Vector2 worldMoveVector = spriteBounds.transform.worldToLocalMatrix * (newPosition - lastMousePosition);

            if (position == Position.Left)
            {
                // ----- Line we clicked on
                
                transform.Translate(new Vector2(worldMoveVector.x, 0));

                // ----- Corners

                spriteBounds.topLeftCorner.transform.localPosition = new Vector3(
                    spriteBounds.leftLine.transform.localPosition.x,
                    spriteBounds.topLeftCorner.transform.localPosition.y,
                    spriteBounds.topLeftCorner.transform.localPosition.z);
                spriteBounds.bottomLeftCorner.transform.localPosition = new Vector3(
                    spriteBounds.leftLine.transform.localPosition.x,
                    spriteBounds.bottomLeftCorner.transform.localPosition.y,
                    spriteBounds.bottomLeftCorner.transform.localPosition.z);
            }
            else if (position == Position.Right)
            {
                // ----- Line we clicked on

                transform.Translate(new Vector2(worldMoveVector.x, 0));

                // ----- Corners

                spriteBounds.topRightCorner.transform.localPosition = new Vector3(
                    spriteBounds.rightLine.transform.localPosition.x,
                    spriteBounds.topRightCorner.transform.localPosition.y,
                    spriteBounds.topRightCorner.transform.localPosition.z);
                spriteBounds.bottomRightCorner.transform.localPosition = new Vector3(
                    spriteBounds.rightLine.transform.localPosition.x,
                    spriteBounds.bottomRightCorner.transform.localPosition.y,
                    spriteBounds.bottomRightCorner.transform.localPosition.z);
            }
            else if (position == Position.Top)
            {
                // ----- Line we clicked on

                transform.Translate(new Vector2(0, worldMoveVector.y));

                // ----- Corners

                spriteBounds.topLeftCorner.transform.localPosition = new Vector3(
                    spriteBounds.topLeftCorner.transform.localPosition.x,
                    spriteBounds.topLine.transform.localPosition.y,
                    spriteBounds.topLeftCorner.transform.localPosition.z);
                spriteBounds.topRightCorner.transform.localPosition = new Vector3(
                    spriteBounds.topRightCorner.transform.localPosition.x,
                    spriteBounds.topLine.transform.localPosition.y,
                    spriteBounds.topRightCorner.transform.localPosition.z);
            }
            else if (position == Position.Bottom)
            {
                // ----- Line we clicked on

                transform.Translate(new Vector2(0, worldMoveVector.y));

                // ----- Corners

                spriteBounds.bottomLeftCorner.transform.localPosition = new Vector3(
                    spriteBounds.bottomLeftCorner.transform.localPosition.x,
                    spriteBounds.bottomLine.transform.localPosition.y,
                    spriteBounds.bottomLeftCorner.transform.localPosition.z);
                spriteBounds.bottomRightCorner.transform.localPosition = new Vector3(
                    spriteBounds.bottomRightCorner.transform.localPosition.x,
                    spriteBounds.bottomLine.transform.localPosition.y,
                    spriteBounds.bottomRightCorner.transform.localPosition.z);
            }

            // ----- Lines

            // Left line
            spriteBounds.leftLine.transform.localPosition = new Vector3(
                spriteBounds.topLeftCorner.transform.localPosition.x,
                (spriteBounds.topLeftCorner.transform.localPosition.y + spriteBounds.bottomLeftCorner.transform.localPosition.y) / 2,
                spriteBounds.leftLine.transform.localPosition.z);
            spriteBounds.leftLine.transform.localScale =
                    new Vector2(1, spriteBounds.topLeftCorner.transform.localPosition.y - spriteBounds.bottomLeftCorner.transform.localPosition.y);

            // Right line
            spriteBounds.rightLine.transform.localPosition = new Vector3(
                spriteBounds.topRightCorner.transform.localPosition.x,
                (spriteBounds.topRightCorner.transform.localPosition.y + spriteBounds.bottomRightCorner.transform.localPosition.y) / 2,
                spriteBounds.rightLine.transform.localPosition.z);
            spriteBounds.rightLine.transform.localScale =
                new Vector2(1, spriteBounds.topRightCorner.transform.localPosition.y - spriteBounds.bottomRightCorner.transform.localPosition.y);

            // Top line
            spriteBounds.topLine.transform.localPosition = new Vector3(
                (spriteBounds.topRightCorner.transform.localPosition.x + spriteBounds.topLeftCorner.transform.localPosition.x) / 2,
                spriteBounds.topLeftCorner.transform.localPosition.y,
                spriteBounds.topLine.transform.localPosition.z);
            spriteBounds.topLine.transform.localScale =
                new Vector2(spriteBounds.topRightCorner.transform.localPosition.x - spriteBounds.topLeftCorner.transform.localPosition.x, 1);

            // Bottom line
            spriteBounds.bottomLine.transform.localPosition = new Vector3(
                (spriteBounds.bottomRightCorner.transform.localPosition.x + spriteBounds.bottomLeftCorner.transform.localPosition.x) / 2,
                spriteBounds.bottomLeftCorner.transform.localPosition.y,
                spriteBounds.bottomLine.transform.localPosition.z);
            spriteBounds.bottomLine.transform.localScale =
                new Vector2(spriteBounds.bottomRightCorner.transform.localPosition.x - spriteBounds.bottomLeftCorner.transform.localPosition.x, 1);

            // ----- Rotation Tool
            spriteBounds.rotationParent.transform.localPosition = new Vector3(
                spriteBounds.topLine.transform.localPosition.x,
                spriteBounds.topLine.transform.localPosition.y,
                spriteBounds.rotationParent.transform.localPosition.z);

            spriteBounds.interactor.AdaptTransform();
        }
    }
}
