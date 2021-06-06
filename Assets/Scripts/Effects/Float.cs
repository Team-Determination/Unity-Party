using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Float : MonoBehaviour
{
    public float negY;
    private float _posY;

    public float speed;
    // Start is called before the first frame update
    void Start()
    {
        _posY = transform.position.y;
        DoTheFloating();
    }

    public void DoTheFloating()
    {
        LeanTween.moveY(gameObject, negY, speed).setEaseLinear().setOnComplete(() => LeanTween.moveY(gameObject, _posY, speed).setEaseLinear().setOnComplete(DoTheFloating));
    }

}
