using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeckerSong : MonoBehaviour
{
    private GameObject _playerOne;

    private GameObject _playerTwo;
    public float gameZoom = 6.4f;

    [SerializeField] private Transform playerOneTransform;
    [SerializeField] private Transform playerTwoTransform;
    // Start is called before the first frame update
    void Start()
    {
        _playerOne = Song.instance.boyfriendObject.transform.parent.gameObject;
        _playerTwo = Song.instance.opponentObject.transform.parent.gameObject;

        Transform boyfriendTransform = _playerOne.transform;
        Transform heckerTransform = _playerTwo.transform;

        boyfriendTransform.position = playerOneTransform.position;
        boyfriendTransform.localScale = playerOneTransform.localScale;
        
        heckerTransform.position = playerTwoTransform.position;
        heckerTransform.localScale = playerTwoTransform.localScale;

        CameraMovement.instance.enableMovement = false;

        
        Song.instance.defaultGameZoom = gameZoom;
        Song.instance.mainCamera.orthographicSize = gameZoom;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
