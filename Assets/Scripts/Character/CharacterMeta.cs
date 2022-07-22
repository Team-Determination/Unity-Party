using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using SimpleSpriteAnimator;
using Newtonsoft.Json;
[Serializable, CreateAssetMenu(menuName = "Create New Character Meta", fileName = "New Character Meta")]
public class CharacterMeta : ScriptableObject
{
    public Character Character;
    public Dictionary<string, List<Vector2>> Offsets = new Dictionary<string, List<Vector2>>();
    public string path;

    public void GenerateSpritesOfCharacter() {
        string basePath = Path.GetDirectoryName(path);
        string animationsPath = Path.Combine(basePath, "Animations").Replace('\\', '/');
        Dictionary<string, List<Sprite>> CharacterAnimations = new Dictionary<string, List<Sprite>>();
        Character.animations = new List<SpriteAnimation>();

        foreach (string directoryPath in Directory.GetDirectories(animationsPath)) {
            DirectoryInfo directoryInfo = new DirectoryInfo(directoryPath);

            FileInfo[] files = directoryInfo.GetFiles("*.png");

            List<Sprite> sprites = new List<Sprite>();
            foreach (FileInfo file in files) {
                if (Preloaded.preloadedAssets["Sprites"].ContainsKey(file.ToString().Replace('\\', '/'))) {
                    Sprite sprite = Preloaded.preloadedAssets["Sprites"][file.ToString().Replace('\\', '/')] as Sprite;
                    sprites.Add(sprite);
                } else {
                    byte[] imageData = File.ReadAllBytes(file.ToString());

                    Texture2D imageTexture = new Texture2D(2, 2);
                    imageTexture.LoadImage(imageData);

                    Sprite sprite = Sprite.Create(imageTexture,
                        new Rect(0, 0, imageTexture.width, imageTexture.height), Character.allPivot, 100, 0, SpriteMeshType.FullRect);
                    sprites.Add(sprite);
                }
            }

            CharacterAnimations.Add(directoryInfo.Name, sprites);
        }

        foreach (string animationName in CharacterAnimations.Keys) {
            List<Vector2> offsets = new List<Vector2>();
            SpriteAnimation newAnimation = CreateInstance<SpriteAnimation>();
            List<SpriteAnimationFrame> frames = new List<SpriteAnimationFrame>();
            for (var index = 0; index < CharacterAnimations[animationName].Count; index++) {
                Sprite sprite = CharacterAnimations[animationName][index];
                Vector2 animationOffset = Vector2.zero;
                if (Offsets.ContainsKey(animationName)) {
                    //animationOffset = currentCharacter.Offsets.ContainsKey(animationName) ? currentCharacter.Offsets[animationName][index] != null ? currentCharacter.Offsets[animationName][index] : new Vector2(0, 0) : new Vector2(0, 0);
                }

                SpriteAnimationFrame newFrame = new SpriteAnimationFrame {
                    Sprite = sprite,
                    Offset = animationOffset
                };

                frames.Add(newFrame);
            }

            newAnimation.Frames = frames;
            newAnimation.Name = animationName;
            newAnimation.FPS = 24;
            newAnimation.SpriteAnimationType = SpriteAnimationType.PlayOnce;

            Character.animations.Add(newAnimation);
        }
    }
}
