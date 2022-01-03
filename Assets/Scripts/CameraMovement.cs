using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public static CameraMovement instance;
    public bool focusOnPlayerOne;

    [Space] public Transform playerOne;
    public Vector3 playerOneOffset;

    [Space] public Transform playerTwo;
    public Vector3 playerTwoOffset;

    [Space] public float speed;

    private Vector3 _defaultPos;
    public Camera myCamera;

    public bool overrideControls;
    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        myCamera = GetComponent<Camera>();
        _defaultPos = myCamera.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (Song.instance == null || overrideControls) return;
        
        if (Song.instance.songStarted & !Options.LiteMode & !Options.Middlescroll)
        {
            Vector3 newOffset;
            newOffset = focusOnPlayerOne ? playerOne.position : playerTwo.position;
            newOffset += focusOnPlayerOne ? playerOneOffset : playerTwoOffset;

            newOffset.z = -10;

            myCamera.transform.position = Vector3.Lerp(myCamera.transform.position, newOffset, speed);
        }
        else
        {
            myCamera.orthographicSize = Song.instance.defaultGameZoom;
            myCamera.transform.position = Vector3.Lerp(myCamera.transform.position, _defaultPos, speed);
        }
        
    }
}
