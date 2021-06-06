using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vector2 = System.Numerics.Vector2;

public class CameraController : MonoBehaviour
{

    public int currentLayer;

    [Space] public Vector3 playerOneOffset;
    public bool followPlayerOne;

    public Vector3 playerTwoOffset;
    public bool followPlayerTwo;

    [Space] public float speed;

    private Vector3 _boyfriendPos;
    private Vector3 _enemyPos;

    private bool _initialized;

    // ReSharper disable once InconsistentNaming
    public static CameraController Instance;
    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (!_initialized)
        {
            if (Song.instance == null) return;

            _boyfriendPos = Song.instance.boyfriendSpriteRenderer.bounds.center;
            _enemyPos = Song.instance.enemySpriteRenderer.bounds.center;
            _initialized = true;
        }
        if (currentLayer == 1)
        {
            if(!followPlayerOne)
                transform.position = Vector3.Lerp(transform.position,
                    _boyfriendPos + playerOneOffset, speed);
            else
                transform.position = Vector3.Lerp(transform.position,
                    Song.instance.boyfriendSpriteRenderer.bounds.center + playerOneOffset, speed);
        }
        else if(currentLayer == 2)
        {
            if(!followPlayerTwo)
                transform.position = Vector3.Lerp(transform.position,
                    _enemyPos + playerTwoOffset, speed);
            else
                transform.position = Vector3.Lerp(transform.position,
                    Song.instance.enemySpriteRenderer.bounds.center + playerTwoOffset, speed);
        }
    }
}
