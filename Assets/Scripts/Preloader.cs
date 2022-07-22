using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using raonyreis13.Utils;
using FridayNightFunkin;
using Newtonsoft.Json;
using ModIO.API;

public class Preloader : MonoBehaviour
{
    string basePath;
    string dataBasePath;
    string modpath;

    void Start() {
        basePath = Application.persistentDataPath;
        dataBasePath = Application.dataPath;
        modpath = "mod.io/editor/3160/mods";
        if (!Preloaded.createdEntries) {
            Preloaded.preloadedAssets.Add("Characters", new Dictionary<string, object>());
            Preloaded.preloadedAssets.Add("Scenes", new Dictionary<string, object>());
            Preloaded.preloadedAssets.Add("Portraits", new Dictionary<string, object>());
            Preloaded.preloadedAssets.Add("Bundles Meta", new Dictionary<string, object>());
            Preloaded.preloadedAssets.Add("Songs Meta", new Dictionary<string, object>());
            Preloaded.preloadedAssets.Add("Songs Data", new Dictionary<string, object>());
            Preloaded.preloadedAssets.Add("Images", new Dictionary<string, object>());
            Preloaded.preloadedAssets.Add("Sprites", new Dictionary<string, object>());
            Preloaded.preloadedAssets.Add("Audios", new Dictionary<string, object>());
            Preloaded.preloadedAssets.Add("Unknown", new Dictionary<string, object>());
            Preloaded.createdEntries = true;
        }
        List<string> bundlesPath = new List<string>();
        bundlesPath.Add(Path.Combine(basePath, "Bundles").Replace('\\', '/'));

        foreach (string pathBase in bundlesPath) {
            if (!Preloaded.preloadedImages) {
                string[] allPngFiles = Directory.GetFiles(pathBase, "*.png", SearchOption.AllDirectories);
                PreloadAllTextures(allPngFiles);
            }

            if (!Preloaded.preloadedJsons) {
                string[] allJsonFiles = Directory.GetFiles(pathBase, "*.json", SearchOption.AllDirectories);
                PreloadAllObjects(allJsonFiles);
            }

            if (!Preloaded.preloadedAudios) {
                StartCoroutine(PreloadSongAudios());
            }
        }
    }

    IEnumerator PreloadSongAudios() {
        List<string> keysOfMetas = Preloaded.preloadedAssets["Songs Meta"].Keys.ToList();
        foreach (string path in keysOfMetas) {
            string fixedPath = Path.GetDirectoryName(path).Replace('\\', '/');
            string instPath = Path.Combine(fixedPath, "Inst.ogg").Replace('\\', '/');
            string secInstPath = Path.Combine(fixedPath, "SecInst.ogg").Replace('\\', '/');
            string voicesPath = Path.Combine(fixedPath, "Voices.ogg").Replace('\\', '/');
            string secVoicesPath = Path.Combine(fixedPath, "SecVoices.ogg").Replace('\\', '/');

            if (File.Exists(instPath)) {
                WWW www = new WWW(instPath) {
                    threadPriority = ThreadPriority.High
                };
                AudioClip audio = www.GetAudioClip(false, true);
                yield return new WaitWhile(() => audio.loadState != AudioDataLoadState.Loaded);
                Preloaded.preloadedAssets["Audios"].Add(instPath, audio);
            }

            if (File.Exists(secInstPath)) {
                WWW www = new WWW(secInstPath) {
                    threadPriority = ThreadPriority.High
                };
                AudioClip audio = www.GetAudioClip(false, true);
                yield return new WaitWhile(() => audio.loadState != AudioDataLoadState.Loaded);
                Preloaded.preloadedAssets["Audios"].Add(secInstPath, audio);
            }

            if (File.Exists(voicesPath)) {
                WWW www = new WWW(voicesPath) {
                    threadPriority = ThreadPriority.High
                };
                AudioClip audio = www.GetAudioClip(false, true);
                yield return new WaitWhile(() => audio.loadState != AudioDataLoadState.Loaded);
                Preloaded.preloadedAssets["Audios"].Add(voicesPath, audio);
            }

            if (File.Exists(secVoicesPath)) {
                WWW www = new WWW(secVoicesPath) {
                    threadPriority = ThreadPriority.High
                };
                AudioClip audio = www.GetAudioClip(false, true);
                yield return new WaitWhile(() => audio.loadState != AudioDataLoadState.Loaded);
                Preloaded.preloadedAssets["Audios"].Add(secVoicesPath, audio);
            }
        }
        Preloaded.preloadedAudios = true;
        print("Loaded Audios");
    }

    void PreloadAllObjects(string[] paths) {
        foreach (string path in paths) {
            PreloadOptions preloadOptions = JsonConvert.DeserializeObject<PreloadOptions>(File.ReadAllText(path).Replace('\\', '/'));
            switch (preloadOptions.type) {
                case "Character": {
                    CharacterMeta character = JsonConvert.DeserializeObject<CharacterMeta>(File.ReadAllText(path.Replace('\\', '/')));
                    character.path = path.Replace('\\', '/');
                    character.GenerateSpritesOfCharacter();
                    Preloaded.preloadedAssets["Characters"].Add(path.Replace('\\', '/'), character);
                    break;
                }
                case "Scene": {
                    SceneData scene = JsonConvert.DeserializeObject<SceneData>(File.ReadAllText(path.Replace('\\', '/')));
                    Preloaded.preloadedAssets["Scenes"].Add(path.Replace('\\', '/'), scene);
                    break;
                }
                case "Portrait": {
                    PortraitRoot portrait = JsonConvert.DeserializeObject<PortraitRoot>(File.ReadAllText(path.Replace('\\', '/')));
                    portrait.path = Path.GetDirectoryName(path.Replace('\\', '/'));
                    portrait.GetAnimations();
                    Preloaded.preloadedAssets["Portraits"].Add(portrait.portraitData.characterName, portrait);
                    break;
                }
                case "Bundle Meta": {
                    break;
                }
                case "Song Meta": {
                    SongMetaV2 songMetaV2 = JsonConvert.DeserializeObject<SongMetaV2>(File.ReadAllText(path.Replace('\\', '/')));
                    Preloaded.preloadedAssets["Songs Meta"].Add(path.Replace('\\', '/'), songMetaV2);
                    break;
                }
                case "Song Data": {
                    FNFSong song = new FNFSong(path);
                    Preloaded.preloadedAssets["Songs Data"].Add(path.Replace('\\', '/'), song);
                    break;
                }
                case "Unknown": {
                    break;
                }
            }
        }
        Preloaded.preloadedJsons = true;
        print("Loaded Objects");
    }

    void PreloadAllTextures(string[] paths) {
        foreach (string path in paths) {
            string fixedPath = path.Replace('\\', '/');
            Texture2D texture = UsefulFunctions.GetTexture(fixedPath, OptionsV2.LiteMode ? OptionsV2.DesperateMode ? FilterMode.Point : FilterMode.Bilinear : FilterMode.Trilinear);
            Preloaded.preloadedAssets["Images"].Add(fixedPath, texture);
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(.5f, .5f), 100, 0, SpriteMeshType.FullRect, Vector4.zero, false);
            Preloaded.preloadedAssets["Sprites"].Add(fixedPath, sprite);
        }

        Preloaded.preloadedImages = true;
        print("Loaded Textures");
    }

    string GetFileName(string path) {
        return Path.GetFileName(path);
    }
}

[System.Serializable]
public static class Preloaded {
    public static bool preloadedImages = false;
    public static bool preloadedJsons = false;
    public static bool preloadedAudios = false;
    public static bool createdEntries = false;
    public static Dictionary<string, Dictionary<string, object>> preloadedAssets = new Dictionary<string, Dictionary<string, object>>();
}

public class PreloadOptions {
    [JsonProperty("Ignore Preload")] public bool ignorePreload = false;
    [JsonProperty("Type of Preload")] public string type = "Unknown";
}