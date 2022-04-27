using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using SimpleSpriteAnimator;
using TMPro;
using UnityEditor;
using UnityEngine.SceneManagement;
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
    [Space] public GameObject preEditScreen; 
    public GameObject selectCharScreen; 
    
    public GameObject selectAnimScreen;

    public GameObject editAnimScreen;

    public CurrentState state = CurrentState.CharacterSelecting;

    public SpriteAnimation currentAnim;
    

    public TMP_Text editingAnimInfoText;

    public int editingCurrentFrame = 0;
    
    public bool enableOnion;
    
    
    public SpriteRenderer onionSprite;
    public SpriteRenderer animationSprite;
    [Header("Meta Editor")] public GameObject metaEditorScreen;
    public TMP_InputField charNameField;
    public TMP_InputField charScaleField;
    public TMP_InputField healthColorField;
    public ColorPicker healthColorPicker;
    
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

        healthColorPicker.onColorChanged += OnHealthColorPicked;
		LoadingTransition.instance.Hide();
    }

    public enum CurrentState
    {
        CharacterSelecting,
        PreEditMenu,
        AnimationTesting,
        AnimationEditing,
        MetaEditing
    }

    private void OnHealthColorPicked(Color newColor)
    {
        string colorString = ColorUtility.ToHtmlStringRGB(newColor);
        healthColorField.SetTextWithoutNotify(colorString);
        currentMeta.Character.healthColor = newColor;
    }

    public void OnHealthColorFieldChanged(string colorString)
    {
        colorString = colorString.Replace("#", "");
        ColorUtility.TryParseHtmlString("#" + colorString, out var newColor);
        currentMeta.Character.healthColor = newColor;
        healthColorPicker.ChangeColorWithoutNotify(newColor);
    }

    public void OnCharNameFieldChanged(string newName)
    {
        currentMeta.Character.characterName = newName;
    }
    public void OnCharScaleFieldChanged(string newScale)
    {
        float parsedScale = float.Parse(newScale);
        currentMeta.Character.scale = parsedScale;

        Vector2 spriteScale = new Vector2(parsedScale, parsedScale);
        animationSprite.transform.localScale = spriteScale;
        onionSprite.transform.localScale = spriteScale;
    }
    
    private void LoadCharacter(string charFolderName)
    {
        selectCharScreen.SetActive(false);
        preEditScreen.SetActive(true);
        
        charDir = charactersDir + "/" + charFolderName;

        CharacterAnimations = new Dictionary<string, List<Sprite>>();

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

            charNameField.text = currentMeta.Character.characterName;

            float characterScale = currentMeta.Character.scale;
            charScaleField.text = characterScale.ToString(CultureInfo.InvariantCulture);

            state = CurrentState.PreEditMenu;

            Vector2 spriteScale = new Vector2(characterScale, characterScale);
            animationSprite.transform.localScale = spriteScale;
            onionSprite.transform.localScale = spriteScale;
            
            characterAnimator.enabled = true;
            characterAnimator.Play("Idle");
        }
    }

    public void OpenAnimations()
    {
        preEditScreen.SetActive(false);
        selectAnimScreen.SetActive(true);

        state = CurrentState.AnimationTesting;
    }

    public void OpenMeta()
    {
        preEditScreen.SetActive(false);
        metaEditorScreen.SetActive(true);

        state = CurrentState.MetaEditing;

        healthColorPicker.color = currentMeta.Character.healthColor;
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
        state = CurrentState.AnimationEditing;
        
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
        if (state == CurrentState.CharacterSelecting)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                SceneManager.LoadScene("Game_Backup3");
            }
        }

        else if (state == CurrentState.PreEditMenu)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                characterAnimator.spriteAnimations = new List<SpriteAnimation>();
                animationSprite.sprite = null;
                state = CurrentState.CharacterSelecting;
                preEditScreen.SetActive(false);
                selectCharScreen.SetActive(true);
            }
        }
        else if(state == CurrentState.AnimationTesting)
        {
            if (beatWatch.ElapsedMilliseconds >= (float) 60 / 120 * 1000 * 2)
            {
                if (characterHoldTimer <= 0)
                {
                    characterAnimator.Play("Idle");
                    beatWatch.Restart();

                }
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                preEditScreen.SetActive(true);
                selectAnimScreen.SetActive(false);

                state = CurrentState.PreEditMenu;
            }

            if (characterHoldTimer > 0)
            {
                characterHoldTimer -= Time.deltaTime;
            }

            if (Input.GetKeyDown(Player.primaryKeyCodes[0]) || Input.GetKeyDown(Player.secondaryKeyCodes[0]))
            {
                characterHoldTimer = 0.7f;
                characterAnimator.Play("Sing Left");
            }

            if (Input.GetKeyDown(Player.primaryKeyCodes[1]) || Input.GetKeyDown(Player.secondaryKeyCodes[1]))
            {
                characterHoldTimer = 0.7f;
                characterAnimator.Play("Sing Down");
            }

            if (Input.GetKeyDown(Player.primaryKeyCodes[2]) || Input.GetKeyDown(Player.secondaryKeyCodes[2]))
            {
                characterHoldTimer = 0.7f;
                characterAnimator.Play("Sing Up");
            }

            if (Input.GetKeyDown(Player.primaryKeyCodes[3]) || Input.GetKeyDown(Player.secondaryKeyCodes[3]))
            {
                characterHoldTimer = 0.7f;
                characterAnimator.Play("Sing Right");
            }
        } else if (state == CurrentState.MetaEditing)
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                SaveCharMeta();
            }
            
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                state = CurrentState.PreEditMenu;
                characterAnimator.enabled = true;
                SaveCharMeta();
                metaEditorScreen.SetActive(false);
                preEditScreen.SetActive(true);
            }
        }
        else if(state == CurrentState.AnimationEditing)
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
                state = CurrentState.AnimationTesting;
                characterAnimator.enabled = true;
                onionSprite.sprite = null;
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
