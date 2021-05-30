using UnityEngine;
 
public class SpriteParallax : MonoBehaviour {
 
    private Vector3 _pz;
    private Vector3 _startPos;
    private Camera _mainCamera;
     
    public float modifier;
     
    void Start ()
    {
        _startPos = transform.position;
        _mainCamera = Camera.main;
    }
     
    void Update ()
    {
        var pz = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
        pz.z = 0;
        gameObject.transform.position = pz;
        transform.position = new Vector3(_startPos.x + (pz.x * (modifier/10)), _startPos.y + (pz.y * (modifier/10)), 0);
    }
     
}