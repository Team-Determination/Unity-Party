using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleSpriteAnimator;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using raonyreis13.Utils;

[System.Serializable]
public class PortraitData {
    [JsonProperty("Character Name")] public string characterName = "Template";
    [JsonProperty("Use Default Beats")] public bool useDefaultBeats = true;
    [JsonProperty("Animation Type")] public string animationType = "Beat Based";
    [JsonProperty("Have Winning")] public bool haveWinning = false;
    [JsonProperty("Frames Per Second")] public int fps = 24;
    [JsonProperty("Sizes")] public Dictionary<string, List<Vector2>> sizes = new Dictionary<string, List<Vector2>>();
    [JsonProperty("Offsets")] public Dictionary<string, List<Vector2>> offsets = new Dictionary<string, List<Vector2>>();

    [JsonIgnore] public List<SpriteAnimation> animations;
}
[System.Serializable]
public class PortraitRoot {
    [JsonProperty("Portrait")] public PortraitData portraitData = new PortraitData();
    [JsonIgnore] public string path;
    [JsonIgnore] public string pathToSprites;

    public void GetAnimations() {
        pathToSprites = Path.Combine(path, "Sprites");
        string[] directories = Directory.GetDirectories(pathToSprites);
        Dictionary<string, List<Sprite>> animationSprites = new Dictionary<string, List<Sprite>>();
        foreach (string directory in directories) {
            DirectoryInfo directoryInfo = new DirectoryInfo(directory);
            animationSprites.Add(directoryInfo.Name, new List<Sprite>());
            string currentAnimationsFolder = directory;
            string[] files = Directory.GetFiles(currentAnimationsFolder, "*.png");
            foreach (string file in files) {
                animationSprites[directoryInfo.Name].Add(GetSpriteForPortrait(file, new Vector2(0.5f, 0.5f)));
            }
        }

        List<SpriteAnimation> animations = new List<SpriteAnimation>();
        animations = UsefulFunctions.CreateAnimationsForPortraits(animationSprites, portraitData.sizes, portraitData.offsets, 24, portraitData.animationType == "Beat Based" ? SpriteAnimationType.PlayOnce : SpriteAnimationType.Looping);
        portraitData.animations = animations;

        Sprite GetSpriteForPortrait(string path, Vector2 pivot) {
            Sprite sprite;
            Texture2D texture = new Texture2D(1, 1);
            if (Preloaded.preloadedAssets["Images"].ContainsKey(path)) {
                texture = Preloaded.preloadedAssets["Images"][path] as Texture2D;
            } else {
                texture.LoadImage(File.ReadAllBytes(path));
            }

            if (Preloaded.preloadedAssets["Sprites"].ContainsKey(path)) {
                sprite = Preloaded.preloadedAssets["Sprites"][path] as Sprite;
            } else {
                sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), pivot, 100, 0, SpriteMeshType.FullRect, Vector4.zero, false);
            }
            return sprite;
        }
    }
}
