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
    private Camera _camera;
    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        _camera = GetComponent<Camera>();
        _defaultPos = _camera.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (Song.instance == null) return;
        
        if (Song.instance.songStarted)
        {
            Vector3 newOffset;
            newOffset = focusOnPlayerOne ? playerOne.position : playerTwo.position;
            newOffset += focusOnPlayerOne ? playerOneOffset : playerTwoOffset;

            newOffset.z = -10;

            _camera.transform.position = Vector3.Lerp(_camera.transform.position, newOffset, speed);
        }
        else
        {
            _camera.orthographicSize = 5;
            _camera.transform.position = Vector3.Lerp(_camera.transform.position, _defaultPos, speed);
        }
        
    }
}
