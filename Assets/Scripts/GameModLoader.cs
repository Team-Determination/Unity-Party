using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ModIO;
using Newtonsoft.Json;
using SimpleSpriteAnimator;
using UnityEngine;

public class GameModLoader : MonoBehaviour
{

    public static Dictionary<string, string> bundleModDirectories = new Dictionary<string, string>();

    // Start is called before the first frame update
    void Start()
    {
        ModManager.onModBinaryInstalled += ModInstalled;
        ModManager.onModBinariesUninstalled += ModUninstalled;

        RefreshResources();
    }

    private void ModUninstalled(ModfileIdPair[] pairs)
    {
        RefreshResources();
    }

    private void ModInstalled(ModfileIdPair pair)
    {
        RefreshResources();
    }

    public static void RefreshResources()
    {
        bundleModDirectories.Clear();
        ModManager.QueryInstalledMods(null, mods =>
        {
            foreach (var pair in mods)
            {
                var idPair = pair.Key;
                var modInstallDirectory = ModManager.GetModInstallDirectory(idPair.modId, idPair.modfileId);
                ModManager.GetModProfile(idPair.modId, profile =>
                {
                    bundleModDirectories.Add(modInstallDirectory,profile.profileURL);
                }, error =>
                {
                    print(error.displayMessage);
                });
                
                

                /*ModManager.GetModProfile(idPair.modId, profile =>
                {
                    if (profile.tagNames.ToList().Contains("Bundle"))
                    {
                        bundleModDirectories.Add(ModManager.GetModInstallDirectory(idPair.modId, idPair.modfileId));
                    }
                }, null);*/
            }
        });
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}