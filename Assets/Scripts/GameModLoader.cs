using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ModIO;
using UnityEngine;

public class GameModLoader : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        ModManager.onModBinaryInstalled += ModInstalled;
        ModManager.onModBinariesUninstalled += ModUninstalled;
    }

    private void ModUninstalled(ModfileIdPair[] obj)
    {
        
    }

    private void ModInstalled(ModfileIdPair obj)
    {
        
    }

    public void UpdateResources()
    {
        Song song = Song.instance;

        song.scenes = new Dictionary<string, SceneData>();

        ModManager.QueryInstalledMods(null, value =>
        {
            List<int> modIDs = new List<int>();
            foreach (KeyValuePair<ModfileIdPair,string> pair in value)
            {
                var pairKey = pair.Key;
                ModManager.GetModProfile(pairKey.modId, profile =>
                {
                    var tagNames = profile.tagNames;
                    if (tagNames.Contains("Scene"))
                    {
                        string modPath = ModManager.GetModInstallDirectory(pairKey.modId,pairKey.modfileId);
                        
                    }
                },null);
            }
            
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
