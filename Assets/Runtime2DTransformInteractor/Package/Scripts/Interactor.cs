using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Runtime2DTransformInteractor
{
    public class Interactor : MonoBehaviour
    {
        /// <summary>
        /// The transform interactor UI.
        /// </summary>
        public SpriteBounds spriteBounds;

        /// <summary>
        /// The GameObject that has the transform we want to modify.
        /// </summary>
        [HideInInspector]
        public GameObject targetGameObject;

        /// <summary>
        /// Original size in world units of the targetGameObject multiplied by its scale
        /// </summary>
        [HideInInspector]
        public Vector2 originalTargetSizeScaled;
        /// <summary>
        /// Original size in world units of the targetGameObject
        /// </summary>
        [HideInInspector]
        public Vector2 originalTargetSize;
        /// <summary>
        /// Original aspect ratio of the transform (x/y)
        /// </summary>
        [HideInInspector]
        public float originalAspectRatio;

        /// <summary>
        /// Center of the collider in world units
        /// </summary>
        [HideInInspector]
        public Vector2 colliderCenter;

        /// <summary>
        /// Offset of the collider
        /// </summary>
        [HideInInspector]
        public Vector3 localColliderOffset;

        /// <summary>
        /// Sets the transform interactor around the target GameObject.
        /// </summary>
        /// <param name="target"></param>
        public void Settup(GameObject target)
        {
            targetGameObject = target;
            
            BoxCollider2D boxCollider = target.GetComponent<BoxCollider2D>();
            if (boxCollider)
            {
                localColliderOffset = new Vector3(boxCollider.offset.x, boxCollider.offset.y, 0);
                Vector2 worldColliderOffset = target.transform.localToWorldMatrix * localColliderOffset;
                Vector3 worldColliderCenter = target.transform.position + new Vector3(worldColliderOffset.x, worldColliderOffset.y, 0);
                colliderCenter = new Vector2(worldColliderCenter.x, worldColliderCenter.y);
                
                originalTargetSize = boxCollider.size;

                originalTargetSizeScaled = originalTargetSize;
                originalTargetSizeScaled.x *= target.transform.localScale.x;
                originalTargetSizeScaled.y *= target.transform.localScale.y;
            }
            else
            {
                target.AddComponent<BoxCollider2D>();

                colliderCenter = Vector2.zero;

                originalTargetSize = Vector2.one;

                originalTargetSizeScaled = originalTargetSize;
                originalTargetSizeScaled.x *= target.transform.localScale.x;
                originalTargetSizeScaled.y *= target.transform.localScale.y;
            }

            originalAspectRatio = originalTargetSizeScaled.x / originalTargetSizeScaled.y;

            spriteBounds.interactor = this;
            spriteBounds.SetBounds();
        }

        /// <summary>
        /// Changes the transform of the target GameObject to match the bounding interactor
        /// </summary>
        public void AdaptTransform()
        {
            float newWidth = spriteBounds.topRightCorner.transform.localPosition.x - spriteBounds.topLeftCorner.transform.localPosition.x;
            float newHeight = spriteBounds.topLeftCorner.transform.localPosition.y - spriteBounds.bottomLeftCorner.transform.localPosition.y;
            
            if (newHeight < 0)
            {
                spriteBounds.rotationParent.transform.localRotation = Quaternion.Euler(0, 0, 180);
            }
            else
            {
                spriteBounds.rotationParent.transform.localRotation = Quaternion.Euler(0, 0, 0);
            }

            Vector3 newScale = new Vector3(
                newWidth / originalTargetSize.x,
                newHeight / originalTargetSize.y,
                targetGameObject.transform.localScale.z
            );
            targetGameObject.transform.localScale = newScale;

            Vector3 worldColliderOffsetBis = targetGameObject.transform.localToWorldMatrix * localColliderOffset;
            Vector3 newPosition = new Vector3(
                (spriteBounds.topLeftCorner.transform.position.x + spriteBounds.bottomRightCorner.transform.position.x) / 2
                - (worldColliderOffsetBis.x),
                (spriteBounds.topLeftCorner.transform.position.y + spriteBounds.bottomRightCorner.transform.position.y) / 2 
                - (worldColliderOffsetBis.y),
                targetGameObject.transform.position.z
            );
            
            targetGameObject.transform.position = newPosition;

            Quaternion rotation = spriteBounds.transform.rotation;
            targetGameObject.transform.rotation = rotation;

            originalAspectRatio = newWidth / newHeight;
        }
    }
}
