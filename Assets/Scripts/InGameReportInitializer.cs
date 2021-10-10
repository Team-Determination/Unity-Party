using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameReportInitializer : MonoBehaviour
{
    public CrittrSDK crittr;
    // Start is called before the first frame update
    void Start()
    {
        crittr.ConnectionURI = "";
    }
}
