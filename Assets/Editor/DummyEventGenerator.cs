using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;


public class DummyEventGenerator : Editor
{
    [MenuItem("Tools/Copy Dummy Event Json")]
    public static void CopyDummyEventJson()
    {
        SongEvents events = new SongEvents
        {
            events = new List<SongEvent>
            {
                new SongEvent
                {
                    time = 10000,
                    functionToCall = "DummyFunction",
                    functionArguments = {"String Argument", 10, true}
                }
            }
        };
        
        GUIUtility.systemCopyBuffer = JsonConvert.SerializeObject(events,Formatting.Indented);
    }
}
