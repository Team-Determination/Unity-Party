using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SqrRoomScene : MonoBehaviour
{
    public bool cutscene = true;

    private void Update()
    {
        if (cutscene)
        {
            if (!Options.LiteMode & !Options.Middlescroll)
            {
                var cameraMovement = CameraMovement.instance;
                cameraMovement.overrideControls = true;
                cameraMovement.focusOnPlayerOne = false;
                Vector3 newOffset;
                newOffset = cameraMovement.playerTwo.position;
                newOffset += cameraMovement.playerTwoOffset;

                newOffset.z = -10;

                var instanceMyCamera = cameraMovement.myCamera;
                instanceMyCamera.transform.position = Vector3.Lerp(instanceMyCamera.transform.position, newOffset, cameraMovement.speed);
            }
            if (!Song.instance.songStarted) return;
            cutscene = false;
            CameraMovement.instance.overrideControls = false;
        }
    }
}
