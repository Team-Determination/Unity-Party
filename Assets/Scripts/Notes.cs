using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Notes : MonoBehaviour
{
    public float speed;
    public RectTransform target;
    public bool isActive;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(isActive)
            target.Translate(Vector3.up * (speed * Time.deltaTime));
    }
}
