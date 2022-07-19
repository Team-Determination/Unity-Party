using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleSpriteAnimator;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using raonyreis13.Utils;

public static class PortraitsParser
{
    public static string[] defaultAnimations = { "Winning", "Normal", "Losing" };
    public static PortraitData ParsePortraits(string songPath, string characterName, string ofCharacterType = "Opponent") {
        string currentPath = Path.Combine(songPath, ofCharacterType, "portraits");
        SpriteAnimator animator = new SpriteAnimator();
        
        PortraitRoot portraitRoot = JsonConvert.DeserializeObject<PortraitRoot>(File.ReadAllText(Path.Combine(currentPath, "portraits-controller.json")));
        PortraitData data = portraitRoot.portraitData;

        List<string> animationNames = new List<string>();
        animationNames.AddRange(defaultAnimations);

        Dictionary<string, Sprite> sprites = UsefulFunctions.GetSpritesheetXmlWithoutOffset(currentPath, data.xmlFileName, new Vector2(.0f, .0f), FilterMode.Trilinear, 100);
        List<string> keys = sprites.Keys.ToList();

        Dictionary<string, List<Sprite>> spritesParsed = new Dictionary<string, List<Sprite>>();
        if (!data.haveWinning)
            animationNames.Remove("Winning");
        foreach (string name in animationNames) {
            spritesParsed.Add(name, new List<Sprite>());
            foreach (string key in keys) {
                if (key.Contains(name)) {
                    spritesParsed[name].Add(sprites[key]);
                }
            }
        }

        data.animations = UsefulFunctions.CreateAnimationsForPortraits(spritesParsed, data.fps, data.animationType != "Beat Based" ? SpriteAnimationType.Looping : SpriteAnimationType.PlayOnce);
        return data;
    }
}

public class PortraitData {
    [JsonProperty("Character Name")] public string characterName = "Template";
    [JsonProperty("Xml File Name")] public string xmlFileName = "template.xml";
    [JsonProperty("Use Default Beats")] public bool useDefaultBeats = true;
    [JsonProperty("Animation Type")] public string animationType = "Beat Based";
    [JsonProperty("Have Winning")] public bool haveWinning = false;
    [JsonProperty("Frames Per Second")] public int fps = 24;
    [JsonProperty("Animation Names")] public List<string> animationNames = new List<string>();
    [JsonProperty("Triggers")] public List<string> triggers = new List<string>();
    [JsonProperty("Size Delta")] public Vector2 sizeDelta = new Vector2(100, 100);
    [JsonIgnore] public List<SpriteAnimation> animations;
}

public class PortraitRoot {
    [JsonProperty("Portrait")] public PortraitData portraitData = new PortraitData();
}
