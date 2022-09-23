using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ModIO;
using ModIOBrowser;
using Newtonsoft.Json;
using SimpleSpriteAnimator;
using UnityEngine;

public class GameModLoader : MonoBehaviour
{

    public static Dictionary<string, string> bundleModDirectories = new Dictionary<string, string>();

    // Start is called before the first frame update
    void Start()
    {
        ModIOUnity.EnableModManagement(ModManagementEventDelegate);

        RefreshResources();
    }

    public void OpenModBrowser()
    {
        Browser.OpenBrowser(null);
    }

    private void ModManagementEventDelegate(ModManagementEventType eventtype, ModId modid, Result eventresult)
    {
        switch (eventtype)
        {
            case ModManagementEventType.InstallStarted:
                break;
            case ModManagementEventType.Installed:
                RefreshResources();
                break;
            case ModManagementEventType.InstallFailed:
                break;
            case ModManagementEventType.DownloadStarted:
                break;
            case ModManagementEventType.Downloaded:
                break;
            case ModManagementEventType.DownloadFailed:
                break;
            case ModManagementEventType.UninstallStarted:
                break;
            case ModManagementEventType.Uninstalled:
                RefreshResources();
                break;
            case ModManagementEventType.UninstallFailed:
                break;
            case ModManagementEventType.UpdateStarted:
                break;
            case ModManagementEventType.Updated:
                break;
            case ModManagementEventType.UpdateFailed:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(eventtype), eventtype, null);
        }
    }
    public static void RefreshResources()
    {
        bundleModDirectories.Clear();
        SubscribedMod[] mods = ModIOUnity.GetSubscribedMods(out var result);
        
        foreach (var mod in mods)
        {
            bundleModDirectories.Add(mod.directory,"https://mod.io/g/up");
        }
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}