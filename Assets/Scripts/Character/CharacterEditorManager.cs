using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using SimpleSpriteAnimator;
using TMPro;
using UnityEditor;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class CharacterEditorManager : MonoBehaviour
{
    public SpriteAnimator characterAnimator;
    public RuntimeAnimatorController originalController;

    public Dictionary<string, List<Sprite>> CharacterAnimations = new Dictionary<string, List<Sprite>>();

    public Stopwatch beatWatch;
    public float characterHoldTimer;

    public CharacterMeta currentMeta;

    [FormerlySerializedAs("editAnimPrefab")] [Space] public GameObject editorSelectionPrefab;

    public RectTransform selectCharListRect;

    public RectTransform editAnimListRect;
    [Space] public GameObject selectCharScreen; 
    
    public GameObject selectAnimScreen;

    public GameObject editAnimScreen;

    public bool editingAnimation;

    public SpriteAnimation currentAnim;
    

    public TMP_Text editingAnimInfoText;

    public int editingCurrentFrame = 0;
    
    public bool enableOnion;
    
    public SpriteRenderer onionSprite;
    public SpriteRenderer animationSprite;
    [Space] public string charactersDir;
    public string charMetaPath;
    public string charDir;

    // Start is called before the first frame update
    void Start()
    {
        beatWatch = new Stopwatch();
        beatWatch.Start();
        
        charactersDir = Application.persistentDataPath+"/Characters";

        if (!Directory.Exists(charactersDir))
        {
            Directory.CreateDirectory(charactersDir);
            
        }

        foreach (string directoryPath in Directory.GetDirectories(charactersDir))
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(directoryPath);

            GameObject newCharSelection = Instantiate(editorSelectionPrefab, selectCharListRect);
            string metaPath = directoryInfo.FullName + "/char-meta.json";
            string realCharName = string.Empty;
            if (File.Exists(metaPath))
            {
                CharacterMeta meta = JsonConvert.DeserializeObject<CharacterMeta>(File.ReadAllText(metaPath));
                realCharName = meta != null ? meta.Character.characterName : "<color=red>Meta Error</color>";
            }
            else
            {
                realCharName = "<color=blue>Meta Missing</color>";
            }
            newCharSelection.GetComponentInChildren<TMP_Text>().text = $"{directoryInfo.Name}\n({realCharName})";
            newCharSelection.GetComponent<Button>().onClick.AddListener(() => LoadCharacter(directoryInfo.Name));
            
        }
    }

    private void LoadCharacter(string charFolderName)
    {
        selectCharScreen.SetActive(false);
        selectAnimScreen.SetActive(true);
        
        charDir = charactersDir + "/" + charFolderName;

        if (Directory.Exists(charDir))
        {
            // BEGIN ANIMATIONS IMPORT

            charMetaPath = charDir + "/char-meta.json";
            bool createMeta = false;
            currentMeta = File.Exists(charMetaPath)
                ? JsonConvert.DeserializeObject<CharacterMeta>(File.ReadAllText(charMetaPath))
                : null;

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

            CharacterMeta potentialNewMeta = ScriptableObject.CreateInstance<CharacterMeta>();
            potentialNewMeta.Character = ScriptableObject.CreateInstance<Character>();

            foreach (string animationName in CharacterAnimations.Keys)
            {
                List<Vector2> offsets = new List<Vector2>();
                SpriteAnimation newAnimation = ScriptableObject.CreateInstance<SpriteAnimation>();
                newAnimation.name = animationName;
                List<SpriteAnimationFrame> frames = new List<SpriteAnimationFrame>();
                for (var index = 0; index < CharacterAnimations[animationName].Count; index++)
                {
                    Sprite sprite = CharacterAnimations[animationName][index];
                    Vector2 animationOffset = Vector2.zero;
                    if (currentMeta != null)
                    {
                        var metaOffsets = currentMeta.Offsets;
                        if (metaOffsets.ContainsKey(animationName))
                        {
                            animationOffset = index < metaOffsets[animationName].Count
                                ? metaOffsets[animationName][index]
                                : Vector2.zero;
                        }
                    }
                    else
                    {
                        offsets.Add(animationOffset);
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

                potentialNewMeta.Offsets.Add(animationName, offsets);

                GameObject newAnimObj = Instantiate(editorSelectionPrefab, editAnimListRect);
                newAnimObj.GetComponentInChildren<TMP_Text>().text = animationName;
                newAnimObj.GetComponent<Button>().onClick.AddListener(() => EditAnimation(animationName));
            }

            if (currentMeta == null)
            {
                currentMeta = potentialNewMeta;

                File.Create(charMetaPath).Dispose();
                File.WriteAllText(charMetaPath, JsonConvert.SerializeObject(potentialNewMeta, Formatting.Indented));
            }

            characterAnimator.Play("Idle");
        }
    }

    public void EditAnimation(string animationName)
    {
        editAnimScreen.SetActive(true);
        selectAnimScreen.SetActive(false);

        foreach (SpriteAnimation spriteAnimation in characterAnimator.spriteAnimations)
        {
            if (spriteAnimation.name == animationName)
            {
                currentAnim = spriteAnimation;
                break;
            }
        }

        editingCurrentFrame = 0;
        characterAnimator.enabled = false;
        editingAnimation = true;
        
        var animFrames = currentAnim.Frames;

        Vector3 position = animFrames[editingCurrentFrame].Offset;
        position.z = 0;

        animationSprite.transform.localPosition = position;

        animationSprite.sprite = animFrames[editingCurrentFrame].Sprite;

        UpdateEditInfo();
        
        RefreshOnionLayer();
    }

    public void UpdateEditInfo()
    {
        var offset = currentAnim.Frames[editingCurrentFrame].Offset;
        editingAnimInfoText.text =
            $"Frame: {editingCurrentFrame}\nOffset: {offset.x},{offset.y}\nOnion Layer: {(enableOnion ? "Enabled" : "Disabled")}";
        
    }

    public void RefreshOnionLayer()
    {
        if (enableOnion)
        {
            if (editingCurrentFrame != 0)
            {
                var animFrame = currentAnim.Frames[editingCurrentFrame - 1];
                onionSprite.sprite = animFrame.Sprite;
                Vector3 position = animFrame.Offset;
                position.z = 0;
                onionSprite.transform.localPosition = position;
                onionSprite.enabled = true;
            }
            else
            {
                onionSprite.enabled = false;
            }

            UpdateEditInfo();
        }
    }

    public void UpdateOffset(Vector2 change, bool additive = true)
    {
        var animFrame = currentAnim.Frames[editingCurrentFrame];
        var animFrameOffset = animFrame.Offset;
        if (additive)
        {
            animFrameOffset += change;
        }
        else
        {
            animFrameOffset = change;
        }

        animFrameOffset.x = Mathf.Round(animFrameOffset.x * 100f) / 100f;
        animFrameOffset.y = Mathf.Round(animFrameOffset.y * 100f) / 100f;

        currentMeta.Offsets[currentAnim.Name][editingCurrentFrame] = animFrameOffset;

        animationSprite.transform.localPosition = animFrameOffset;

        animFrame.Offset = animFrameOffset;
        UpdateEditInfo();
    }

    public void ChangeFrame(int val)
    {
        editingCurrentFrame += val;
        var animFrames = currentAnim.Frames;
        if (editingCurrentFrame < 0)
        {
            editingCurrentFrame = animFrames.Count - 1;
        } else if (editingCurrentFrame > animFrames.Count - 1)
        {
            editingCurrentFrame = 0;
        }

        Vector3 position = animFrames[editingCurrentFrame].Offset;
        position.z = 0;

        animationSprite.transform.localPosition = position;

        animationSprite.sprite = animFrames[editingCurrentFrame].Sprite;

        UpdateEditInfo();
        RefreshOnionLayer();
    }
    
    // Update is called once per frame
    void Update()
    {
        if(!editingAnimation)
        {
            if (beatWatch.ElapsedMilliseconds >= (float) 60 / 120 * 1000 * 2)
            {
                if (characterHoldTimer <= 0)
                {
                    characterAnimator.Play("Idle");
                    beatWatch.Restart();

                }
            }

            if (characterHoldTimer > 0)
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
        else
        {
            float change = Input.GetKey(KeyCode.LeftControl) ? 10 : 100;
            
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                UpdateOffset(Vector2.left/change);
            }
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                UpdateOffset(Vector2.right/change);
            }
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                UpdateOffset(Vector2.up/change);
            }
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                UpdateOffset(Vector2.down/change);
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                SaveCharMeta();
            }

            if (Input.GetKeyDown(KeyCode.Tab))
            {
                enableOnion = !enableOnion;
                onionSprite.enabled = enableOnion;
                RefreshOnionLayer();
            }

            if (Input.GetKeyDown(KeyCode.A))
            {
                ChangeFrame(-1);
            } else if (Input.GetKeyDown(KeyCode.D))
            {
                ChangeFrame(1);
            }

            if (Input.GetKeyDown(KeyCode.LeftAlt))
            {
                if(editingCurrentFrame != 0)
                {
                    Vector2 position = currentAnim.Frames[editingCurrentFrame - 1].Offset;
                    UpdateOffset(position,false);
                }
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                editingAnimation = false;
                characterAnimator.enabled = true;
                SaveCharMeta();
                editAnimScreen.SetActive(false);
                selectAnimScreen.SetActive(true);
            }
        }
    }

    private void SaveCharMeta()
    {
        File.Delete(charMetaPath);
        File.Create(charMetaPath).Dispose();
        File.WriteAllText(charMetaPath, JsonConvert.SerializeObject(currentMeta, Formatting.Indented));
    }
}
