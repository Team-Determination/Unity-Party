using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using SimpleSpriteAnimator;
using UnityEditor;

public class CharacterEditorManager : MonoBehaviour
{
    public SpriteAnimator characterAnimator;
    public RuntimeAnimatorController originalController;

    public Dictionary<string, List<Sprite>> CharacterAnimations = new Dictionary<string, List<Sprite>>();

    public Stopwatch beatWatch;
    public float characterHoldTimer;

    public CharacterMeta currentMeta;
    // Start is called before the first frame update
    void Start()
    {
        beatWatch = new Stopwatch();
        beatWatch.Start();
        
        string charactersDir = Application.persistentDataPath+"/Characters";

        if (!Directory.Exists(charactersDir))
        {
            Directory.CreateDirectory(charactersDir);
            
        }

        string charDir = charactersDir + "/char_test";

        if (Directory.Exists(charDir))
        {
            // BEGIN ANIMATIONS IMPORT

            var charMetaPath = charDir + "/char-meta.json";
            currentMeta = File.Exists(charMetaPath) ? JsonConvert.DeserializeObject<CharacterMeta>(File.ReadAllText(charMetaPath)) : null;

            foreach (string directoryPath in Directory.GetDirectories(charDir))
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(directoryPath);

                var files = directoryInfo.GetFiles("*.png");

                List<Sprite> sprites = new List<Sprite>();

                foreach (var file in files)
                {
                    byte[] imageData = File.ReadAllBytes(file.ToString());

                    Texture2D imageTexture = new Texture2D(2, 2);
                    imageTexture.LoadImage(imageData);

                    var sprite = Sprite.Create(imageTexture,
                        new Rect(0, 0, imageTexture.width, imageTexture.height), new Vector2(0.5f, 0.0f), 100);
                    sprites.Add(sprite);
                    
                }

                CharacterAnimations.Add(directoryInfo.Name, sprites);
            }

            foreach (string animationName in CharacterAnimations.Keys)
            {
                print(animationName);
                SpriteAnimation newAnimation = ScriptableObject.CreateInstance<SpriteAnimation>();
                List<SpriteAnimationFrame> frames = new List<SpriteAnimationFrame>();
                for (var index = 0; index < CharacterAnimations[animationName].Count; index++)
                {
                    Sprite sprite = CharacterAnimations[animationName][index];
                    Vector2 animationOffset = Vector2.zero;
                    if (currentMeta != null)
                    {
                        if(currentMeta.Offsets.ContainsKey(animationName))
                        {
                            animationOffset = currentMeta.Offsets[animationName][index];
                        }
                    }

                    SpriteAnimationFrame newFrame = new SpriteAnimationFrame
                    {
                        Sprite = sprite,
                        Offset = animationOffset
                    };

                    frames.Add(newFrame);
                }

                newAnimation.Frames = frames;
                newAnimation.Name = animationName;
                newAnimation.FPS = 24;
                newAnimation.SpriteAnimationType = SpriteAnimationType.PlayOnce;

                characterAnimator.spriteAnimations.Add(newAnimation);
            }

            characterAnimator.Play("Idle");
        }


    }

    public void Character_PlayAnimation(string animationName)
    {
        characterAnimator.Play(animationName);
    }

    // Update is called once per frame
    void Update()
    {
        if (beatWatch.ElapsedMilliseconds >= (float) 60 / 120 * 1000 * 2)
        {
            if (characterHoldTimer <= 0)
            {
                Character_PlayAnimation("Idle");
                beatWatch.Restart();
                
            }
        }
        if(characterHoldTimer > 0)
        {
            characterHoldTimer -= Time.deltaTime; 
        }

        if (Input.GetKeyDown(Player.leftArrowKey) || Input.GetKeyDown(Player.secLeftArrowKey))
        {
            characterHoldTimer = 0.7f;
            characterAnimator.Play("Sing Left");
        }
        if (Input.GetKeyDown(Player.downArrowKey) || Input.GetKeyDown(Player.secDownArrowKey))
        {
            characterHoldTimer = 0.7f;
            characterAnimator.Play("Sing Down");
        }
        if (Input.GetKeyDown(Player.upArrowKey) || Input.GetKeyDown(Player.secUpArrowKey))
        {
            characterHoldTimer = 0.7f;
            characterAnimator.Play("Sing Up");
        }
        if (Input.GetKeyDown(Player.rightArrowKey) || Input.GetKeyDown(Player.secRightArrowKey))
        {
            characterHoldTimer = 0.7f;
            characterAnimator.Play("Sing Right");
        }
    }
}
