using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Runtime2DTransformInteractor
{
    public class SpriteBounds : MonoBehaviour
    {
        // Serialized fields

        [Header("Lines")]
        public Line leftLine;
        public Line rightLine;
        public Line topLine;
        public Line bottomLine;

        [Header("Corners")]
        public Corner topLeftCorner;
        public Corner topRightCorner;
        public Corner bottomLeftCorner;
        public Corner bottomRightCorner;

        [Header("Rotator")]
        public GameObject rotationParent;
        public Rotator rotator;

        // Non-Serialized fields

        [HideInInspector]
        public Interactor interactor;
        private float originalWidth;
        private float originalHeight;
        private Vector2 originalScale;

        /// <summary>
        /// Snaps the corners and lines around the sprite of the spriteRenderer parameter.
        /// </summary>
        public void SetBounds()
        {
            // Sets the positions of the sprite bounds

            transform.localPosition = interactor.colliderCenter;

            // Sets the sorting layer of each component
            
            topLeftCorner.gameObject.GetComponent<SpriteRenderer>().sortingLayerName = TransformInteractorController.instance.cornerSortingLayer;
            topRightCorner.gameObject.GetComponent<SpriteRenderer>().sortingLayerName = TransformInteractorController.instance.cornerSortingLayer;
            bottomLeftCorner.gameObject.GetComponent<SpriteRenderer>().sortingLayerName = TransformInteractorController.instance.cornerSortingLayer;
            bottomRightCorner.gameObject.GetComponent<SpriteRenderer>().sortingLayerName = TransformInteractorController.instance.cornerSortingLayer;

            topLine.lineRenderer.sortingLayerName = TransformInteractorController.instance.lineSortingLayer;
            bottomLine.lineRenderer.sortingLayerName = TransformInteractorController.instance.lineSortingLayer;
            leftLine.lineRenderer.sortingLayerName = TransformInteractorController.instance.lineSortingLayer;
            rightLine.lineRenderer.sortingLayerName = TransformInteractorController.instance.lineSortingLayer;

            rotator.gameObject.GetComponent<SpriteRenderer>().sortingLayerName = TransformInteractorController.instance.rotationGizmoSortinglayer;
            rotator.lineRenderer.sortingLayerName = TransformInteractorController.instance.lineSortingLayer;

            // Apply the user aspect choices for the bounds

            if (TransformInteractorController.instance.cornerSprite)
            {
                topLeftCorner.gameObject.GetComponent<SpriteRenderer>().sprite = TransformInteractorController.instance.cornerSprite;
                topRightCorner.gameObject.GetComponent<SpriteRenderer>().sprite = TransformInteractorController.instance.cornerSprite;
                bottomLeftCorner.gameObject.GetComponent<SpriteRenderer>().sprite = TransformInteractorController.instance.cornerSprite;
                bottomRightCorner.gameObject.GetComponent<SpriteRenderer>().sprite = TransformInteractorController.instance.cornerSprite;
            }
            
            topLeftCorner.gameObject.GetComponent<SpriteRenderer>().color = TransformInteractorController.instance.cornerColor;
            topRightCorner.gameObject.GetComponent<SpriteRenderer>().color = TransformInteractorController.instance.cornerColor;
            bottomLeftCorner.gameObject.GetComponent<SpriteRenderer>().color = TransformInteractorController.instance.cornerColor;
            bottomRightCorner.gameObject.GetComponent<SpriteRenderer>().color = TransformInteractorController.instance.cornerColor;

            leftLine.lineRenderer.startColor = TransformInteractorController.instance.lineColor;
            leftLine.lineRenderer.endColor = TransformInteractorController.instance.lineColor;

            rightLine.lineRenderer.startColor = TransformInteractorController.instance.lineColor;
            rightLine.lineRenderer.endColor = TransformInteractorController.instance.lineColor;

            topLine.lineRenderer.startColor = TransformInteractorController.instance.lineColor;
            topLine.lineRenderer.endColor = TransformInteractorController.instance.lineColor;

            bottomLine.lineRenderer.startColor = TransformInteractorController.instance.lineColor;
            bottomLine.lineRenderer.endColor = TransformInteractorController.instance.lineColor;

            if (TransformInteractorController.instance.rotationSprite)
            {
                rotator.gameObject.GetComponent<SpriteRenderer>().sprite = TransformInteractorController.instance.rotationSprite;
            }

            rotator.gameObject.GetComponent<SpriteRenderer>().color = TransformInteractorController.instance.rotationColor;

            rotator.lineRenderer.startColor = TransformInteractorController.instance.lineColor;
            rotator.lineRenderer.endColor = TransformInteractorController.instance.lineColor;

            if (!TransformInteractorController.instance.allowRotation)
            {
                rotationParent.SetActive(false);
            }

            // Sets the positions of the corners

            topLeftCorner.transform.position = new Vector3(interactor.colliderCenter.x - interactor.originalTargetSizeScaled.x / 2,
                interactor.colliderCenter.y + interactor.originalTargetSizeScaled.y / 2,
                transform.position.z - TransformInteractorController.instance.maxZDisplacement);

            topRightCorner.transform.position = new Vector3(interactor.colliderCenter.x + interactor.originalTargetSizeScaled.x / 2,
                interactor.colliderCenter.y + interactor.originalTargetSizeScaled.y / 2,
                transform.position.z - TransformInteractorController.instance.maxZDisplacement);

            bottomLeftCorner.transform.position = new Vector3(interactor.colliderCenter.x - interactor.originalTargetSizeScaled.x / 2,
                interactor.colliderCenter.y - interactor.originalTargetSizeScaled.y / 2,
                transform.position.z - TransformInteractorController.instance.maxZDisplacement);

            bottomRightCorner.transform.position = new Vector3(interactor.colliderCenter.x + interactor.originalTargetSizeScaled.x / 2,
                interactor.colliderCenter.y - interactor.originalTargetSizeScaled.y / 2,
                transform.position.z - TransformInteractorController.instance.maxZDisplacement);

            // Sets the positions and scale of the lines

            Vector3[] positions = new Vector3[2];

            leftLine.transform.position = new Vector3(
                interactor.colliderCenter.x - interactor.originalTargetSizeScaled.x / 2,
                interactor.colliderCenter.y,
                transform.position.z - (TransformInteractorController.instance.maxZDisplacement/2));
            leftLine.transform.localScale = new Vector2(1, interactor.originalTargetSizeScaled.y);

            rightLine.transform.position = new Vector3(
                interactor.colliderCenter.x + interactor.originalTargetSizeScaled.x / 2,
                interactor.colliderCenter.y,
                transform.position.z - (TransformInteractorController.instance.maxZDisplacement / 2));
            rightLine.transform.localScale = new Vector2(1, interactor.originalTargetSizeScaled.y);

            topLine.transform.position = new Vector3(
                interactor.colliderCenter.x,
                interactor.colliderCenter.y + interactor.originalTargetSizeScaled.y / 2,
                transform.position.z - (TransformInteractorController.instance.maxZDisplacement / 2));
            topLine.transform.localScale = new Vector2(interactor.originalTargetSizeScaled.x, 1);

            bottomLine.transform.position = new Vector3(
                interactor.colliderCenter.x,
                interactor.colliderCenter.y - interactor.originalTargetSizeScaled.y / 2,
                transform.position.z - (TransformInteractorController.instance.maxZDisplacement / 2));
            bottomLine.transform.localScale = new Vector2(interactor.originalTargetSizeScaled.x, 1);

            // Sets the position of the rotation tool

            rotationParent.transform.position = new Vector3(
                interactor.colliderCenter.x,
                interactor.colliderCenter.y + interactor.originalTargetSizeScaled.y / 2,
                transform.position.z - (TransformInteractorController.instance.maxZDisplacement / 2));

            transform.rotation = interactor.targetGameObject.transform.rotation;

            if (interactor.targetGameObject.transform.localScale.y < 0)
            {
                rotationParent.transform.localRotation = Quaternion.Euler(0, 0, 180);
            }
            else
            {
                rotationParent.transform.localRotation = Quaternion.Euler(0, 0, 0);
            }
        }
    }
}
