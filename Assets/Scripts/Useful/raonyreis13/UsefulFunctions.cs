using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Xml;
using System;
using System.Linq;
using SimpleSpriteAnimator;
using UnityEngine.UI;

namespace raonyreis13.Utils {
    public static class UsefulFunctions {
        
        public static Sprite GetSprite(string path, Vector2 pivot, FilterMode filterMode = FilterMode.Trilinear, int ppu = 100) {
            Texture2D tex = new Texture2D(1, 1);
            tex.LoadImage(File.ReadAllBytes(path));
            tex.filterMode = filterMode;
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), pivot, ppu, 0, SpriteMeshType.FullRect);
        }

        public static Texture2D GetTexture(string path, FilterMode filterMode = FilterMode.Trilinear) {
            Texture2D tex = new Texture2D(1, 1);
            tex.LoadImage(File.ReadAllBytes(path));
            tex.filterMode = filterMode;
            return tex;
        }

        public static Dictionary<string, Dictionary<Vector2, Sprite>> GetSpritesheetXml(string path, string xmlName, Vector2 allPivot, FilterMode filterMode = FilterMode.Trilinear, int ppu = 100) {
            //Parse XML
            List<SubTexture> subTextures = new List<SubTexture>();
            Dictionary<string, Dictionary<Vector2, Sprite>> sprites = new Dictionary<string, Dictionary<Vector2, Sprite>>();
            XmlDocument document = new XmlDocument();
            document.LoadXml(File.ReadAllText(Path.Combine(path, xmlName)));
            XmlElement root = document.DocumentElement;
            if (root == null || root.Name != "TextureAtlas") {
                return null;
            }
            string pngName = root.Attributes["imagePath"].Value;
            Texture2D texture = new Texture2D(1, 1);
            byte[] bytes = File.ReadAllBytes(Path.Combine(path, pngName));
            texture.LoadImage(bytes);

            foreach (XmlNode xmlNode in root.ChildNodes) {
                SubTexture subTexture = new SubTexture();
                subTexture.x = Convert.ToInt32(xmlNode.Attributes["x"]);
                subTexture.y = Convert.ToInt32(xmlNode.Attributes["y"]);
                subTexture.w = Convert.ToInt32(xmlNode.Attributes["w"]);
                subTexture.h = Convert.ToInt32(xmlNode.Attributes["h"]);
                if (xmlNode.Attributes["frameX"] != null)
                    subTexture.fx = Convert.ToInt32(xmlNode.Attributes["frameX"]);
                if (xmlNode.Attributes["frameY"] != null)
                    subTexture.fx = Convert.ToInt32(xmlNode.Attributes["frameY"]);
                if (xmlNode.Attributes["frameWidth"] != null)
                    subTexture.fx = Convert.ToInt32(xmlNode.Attributes["frameWidth"]);
                if (xmlNode.Attributes["frameHeight"] != null)
                    subTexture.fx = Convert.ToInt32(xmlNode.Attributes["frameHeight"]);
                subTextures.Add(subTexture);
            }

            int actualY;

            foreach (SubTexture subTexture in subTextures) {
                sprites.Add(subTexture.name, new Dictionary<Vector2, Sprite>());
                actualY = texture.height - (subTexture.y + subTexture.h);

                Sprite sprite = Sprite.Create(texture, new Rect(subTexture.x, actualY, subTexture.w, subTexture.h), allPivot, ppu,0, SpriteMeshType.FullRect);
                sprite.name = subTexture.name;
                sprites[subTexture.name].Add(new Vector2(subTexture.fx, subTexture.fy), sprite);
            }
            return sprites;
        }



        public static Dictionary<string, Sprite> GetSpritesheetXmlWithoutOffset(string path, string xmlName, Vector2 allPivot, FilterMode filterMode = FilterMode.Trilinear, int ppu = 100) {
            //Parse XML
            List<SubTexture> subTextures = new List<SubTexture>();
            Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();
            
            XmlDocument document = new XmlDocument();
            document.LoadXml(File.ReadAllText(Path.Combine(path, xmlName)));
            XmlElement root = document.DocumentElement;
            if (root == null || root.Name != "TextureAtlas") {
                return null;
            }

            int wantedWidth, wantedHeight;
            string pngName = root.Attributes["imagePath"].Value;
            Texture2D texture = new Texture2D(1, 1);
            byte[] bytes = File.ReadAllBytes(Path.Combine(path, pngName));
            texture.LoadImage(bytes);

            foreach (XmlNode xmlNode in root.ChildNodes) {
                SubTexture subTexture = new SubTexture();
                subTexture.x = Convert.ToInt32(xmlNode.Attributes["x"]);
                subTexture.y = Convert.ToInt32(xmlNode.Attributes["y"]);
                subTexture.w = Convert.ToInt32(xmlNode.Attributes["w"]);
                subTexture.h = Convert.ToInt32(xmlNode.Attributes["h"]);
                if (xmlNode.Attributes["frameX"] != null)
                    subTexture.fx = Convert.ToInt32(xmlNode.Attributes["frameX"]);
                if (xmlNode.Attributes["frameY"] != null)
                    subTexture.fx = Convert.ToInt32(xmlNode.Attributes["frameY"]);
                if (xmlNode.Attributes["frameWidth"] != null)
                    subTexture.fx = Convert.ToInt32(xmlNode.Attributes["frameWidth"]);
                if (xmlNode.Attributes["frameHeight"] != null)
                    subTexture.fx = Convert.ToInt32(xmlNode.Attributes["frameHeight"]);
                subTextures.Add(subTexture);
            }



            wantedWidth = 0;
            wantedHeight = 0;

            foreach (var subTexture in subTextures) {
                var right = subTexture.x + subTexture.w;
                var bottom = subTexture.y + subTexture.h;

                wantedWidth = Mathf.Max(wantedWidth, right);
                wantedHeight = Mathf.Max(wantedHeight, bottom);
            }

            sprites = PerformSlice(texture, subTextures, allPivot, ppu);
            return sprites;
        }

        public static List<Sprite> CreateSmoothAnimationWIthRect(Texture2D texture, bool reverse, Vector2 pivot) {
            List<Sprite> sprites = new List<Sprite>();
            for (int i = 0; i < texture.height; i++) {
                Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height - i), pivot);
                sprites.Add(sprite);
            }

            if (!reverse) {
                sprites.Reverse();
            }

            return sprites;
        }

        public static List<SpriteAnimation> CreateAnimationsForPortraits(Dictionary<string, List<Sprite>> sprites, int fps, SpriteAnimationType type) {
            List<string> keysOf1Dic = sprites.Keys.ToList();
            List<SpriteAnimation> animations = new List<SpriteAnimation>();
            foreach (string key in keysOf1Dic) {
                SpriteAnimation currentAnimation = CreateAnimation(sprites[key], key, fps, type);
                animations.Add(currentAnimation);
            }
            return animations;
        }

        public static SpriteAnimation CreateAnimation(List<Sprite> sprites, string name, int fps, SpriteAnimationType type) {
            SpriteAnimation spriteAnimation = ScriptableObject.CreateInstance<SpriteAnimation>();
            spriteAnimation.Name = name;
            spriteAnimation.name = name;
            spriteAnimation.FPS = fps;
            spriteAnimation.SpriteAnimationType = type;
            spriteAnimation.Frames = new List<SpriteAnimationFrame>();
            foreach (Sprite sprite in sprites) {
                SpriteAnimationFrame frame = new SpriteAnimationFrame();
                frame.Sprite = sprite;
                spriteAnimation.Frames.Add(frame);
            }
            return spriteAnimation;
        }

        public static SpriteAnimation CreateAnimationWithOffsets(List<Dictionary<Vector2, Sprite>> spritesandoffsets, string name, int fps, SpriteAnimationType type) {
            SpriteAnimation spriteAnimation = ScriptableObject.CreateInstance<SpriteAnimation>();
            spriteAnimation.Name = name;
            spriteAnimation.name = name;
            spriteAnimation.FPS = fps;
            spriteAnimation.SpriteAnimationType = type;
            spriteAnimation.Frames = new List<SpriteAnimationFrame>();
            List<Vector2> offsets = new List<Vector2>();
            for (int i = 0; i < spritesandoffsets.Count; i++) {
                offsets = spritesandoffsets[i].Keys.ToList();
                foreach (Vector2 offset in offsets) {
                    SpriteAnimationFrame frame = new SpriteAnimationFrame();
                    frame.Sprite = spritesandoffsets[i][offset];
                    frame.Offset = (-offset / 50f);
                    spriteAnimation.Frames.Add(frame);
                }
            }
            return spriteAnimation;
        }

        static Dictionary<string, Sprite> PerformSlice(Texture2D texture2D, List<SubTexture> subTextures, Vector2 pivot, int pixelsPerUnity) {
            Dictionary<string, Sprite> allSprites = new Dictionary<string, Sprite>();
            int textureHeight = texture2D.height;
            int textureWidth = texture2D.width;
            int actualY;
            int actualX;
            foreach (SubTexture subTexture in subTextures) {
                actualY = textureHeight - (subTexture.y + subTexture.h);
                actualX = subTexture.x;
                allSprites.Add(subTexture.name, Sprite.Create(
                    texture: texture2D,
                    rect: new Rect(actualX, actualY, subTexture.w, subTexture.h),
                    pivot: pivot,
                    pixelsPerUnit: pixelsPerUnity,
                    extrude: 0,
                    meshType: SpriteMeshType.FullRect
                ));
            }
            return allSprites;
        }

        public static List<Sprite> SpriteToList(Dictionary<string, Sprite>.ValueCollection value) => value.ToList();
        public static List<object> DicKeysToList(Dictionary<object, object>.KeyCollection value) => value.ToList();
        public static List<Dictionary<Vector2, Sprite>> DictionaryToList(Dictionary<string, Dictionary<Vector2, Sprite>>.ValueCollection value) => value.ToList();
        public static Image.Type GetImageFillType(string typeInString) {
            switch (typeInString) {
                case "Simple":
                    return Image.Type.Simple;
                case "Tiled":
                    return Image.Type.Tiled;
                case "Sliced":
                    return Image.Type.Sliced;
                case "Filled":
                    return Image.Type.Filled;
                default:
                    return Image.Type.Simple;
            }
        }

        public static Image.FillMethod GetImageFillMethod(string typeInString) {
            switch (typeInString) {
                case "Horizontal":
                    return Image.FillMethod.Horizontal;
                case "Vertical":
                    return Image.FillMethod.Vertical;
                default:
                    return Image.FillMethod.Vertical;
            }
        }

        public static SpriteDrawMode GetSpriteFillType(string typeInString) {
            switch (typeInString) {
                case "Simple":
                    return SpriteDrawMode.Simple;
                case "Sliced":
                    return SpriteDrawMode.Sliced;
                case "Tiled":
                    return SpriteDrawMode.Tiled;
                default:
                    return SpriteDrawMode.Simple;
            }
        }
    }


    struct SubTexture {
        public int w;
        public int h;
        public int x;
        public int y;
        public bool hasFrameCoords;
        public int fx;
        public int fy;
        public int fw;
        public int fh;
        public string name;
    }
}

