using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Xml;
using System;
using System.Linq;
using SimpleSpriteAnimator;

namespace raonyreis13.Utils {
    public static class UsefulFunctions {
        
        public static Sprite GetSprite(string path, Vector2 pivot, FilterMode filterMode = FilterMode.Trilinear, int ppu = 100) {
            Texture2D tex = new Texture2D(1, 1);
            tex.LoadImage(File.ReadAllBytes(path));
            tex.filterMode = filterMode;
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), pivot, ppu);
        }

        public static Texture2D GetTexture(string path, FilterMode filterMode = FilterMode.Trilinear) {
            Texture2D tex = new Texture2D(1, 1);
            tex.LoadImage(File.ReadAllBytes(path));
            tex.filterMode = filterMode;
            return tex;
        }

        public static Dictionary<string, Dictionary<Vector2, Sprite>> GetSpritesheetXml(string path, string xmlName, string pngName, Vector2 allPivot, FilterMode filterMode = FilterMode.Trilinear, int ppu = 100) {
            //Parse XML
            SubTexture[] subTextures;
            Dictionary<string, Dictionary<Vector2, Sprite>> sprites = new Dictionary<string, Dictionary<Vector2, Sprite>>();
            Texture2D texture = new Texture2D(1, 1);
            byte[] bytes = File.ReadAllBytes(Path.Combine(path, pngName));
            texture.LoadImage(bytes);
            XmlDocument document = new XmlDocument();
            document.LoadXml(File.ReadAllText(Path.Combine(path, xmlName)));
            XmlElement root = document.DocumentElement;
            if (root == null || root.Name != "TextureAtlas") {
                return null;
            }

            subTextures = root.ChildNodes
                              .Cast<XmlNode>()
                              .Where(childNode => childNode.Name == "SubTexture")
                              .Select(childNode => new SubTexture {
                                  w = Convert.ToInt32(childNode.Attributes["width"].Value),
                                  h = Convert.ToInt32(childNode.Attributes["height"].Value),
                                  x = Convert.ToInt32(childNode.Attributes["x"].Value),
                                  y = Convert.ToInt32(childNode.Attributes["y"].Value),
                                  fx = Convert.ToInt32(childNode.Attributes["frameX"].Value),
                                  fy = Convert.ToInt32(childNode.Attributes["frameY"].Value),
                                  name = childNode.Attributes["name"].Value
                              }).ToArray();
            int right;
            int bottom;
            int wantedWidth, wantedHeight;
            wantedWidth = 0;
            wantedHeight = 0;
            int actualY;

            foreach (SubTexture subTexture in subTextures) {
                sprites.Add(subTexture.name, new Dictionary<Vector2, Sprite>());
                right = subTexture.x + subTexture.w;
                bottom = subTexture.y + subTexture.h;

                wantedWidth = Mathf.Max(wantedWidth, right);
                wantedHeight = Mathf.Max(wantedHeight, bottom);
                actualY = texture.height - (subTexture.y + subTexture.h);

                Sprite sprite = Sprite.Create(texture, new Rect(subTexture.x, actualY, subTexture.w, subTexture.h), allPivot, ppu);
                sprite.name = subTexture.name;
                sprites[subTexture.name].Add(new Vector2(subTexture.fx, 0), sprite);
            }
            return sprites;
        }

        public static Dictionary<string, Sprite> GetSpritesheetXmlWithoutOffset(string path, string xmlName, string pngName, Vector2 allPivot, FilterMode filterMode = FilterMode.Trilinear, int ppu = 100) {
            //Parse XML
            SubTexture[] subTextures;
            Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();
            Texture2D texture = new Texture2D(1, 1);
            byte[] bytes = File.ReadAllBytes(Path.Combine(path, pngName));
            texture.LoadImage(bytes);
            XmlDocument document = new XmlDocument();
            document.LoadXml(File.ReadAllText(Path.Combine(path, xmlName)));
            XmlElement root = document.DocumentElement;
            if (root == null || root.Name != "TextureAtlas") {
                return null;
            }

            subTextures = root.ChildNodes
                              .Cast<XmlNode>()
                              .Where(childNode => childNode.Name == "SubTexture")
                              .Select(childNode => new SubTexture {
                                  w = Convert.ToInt32(childNode.Attributes["width"].Value),
                                  h = Convert.ToInt32(childNode.Attributes["height"].Value),
                                  x = Convert.ToInt32(childNode.Attributes["x"].Value),
                                  y = Convert.ToInt32(childNode.Attributes["y"].Value),
                                  name = childNode.Attributes["name"].Value
                              }).ToArray();
            int right;
            int bottom;
            int wantedWidth, wantedHeight;
            wantedWidth = 0;
            wantedHeight = 0;
            int actualY;
            int actualX;

            foreach (SubTexture subTexture in subTextures) {
                right = subTexture.x + subTexture.w;
                bottom = subTexture.y + subTexture.h;

                wantedWidth = Mathf.Max(wantedWidth, right);
                wantedHeight = Mathf.Max(wantedHeight, bottom);
                actualY = texture.height - (subTexture.y + subTexture.h);
                actualX = texture.width - (subTexture.x + subTexture.w);

                Sprite sprite = Sprite.Create(texture, new Rect(actualX, actualY, subTexture.w, subTexture.h), allPivot, ppu);
                sprite.name = subTexture.name;
                sprites.Add(subTexture.name, sprite);
            }
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

        public static List<Sprite> SpriteToList(Dictionary<string, Sprite>.ValueCollection value) => value.ToList();
        public static List<object> DicKeysToList(Dictionary<object, object>.KeyCollection value) => value.ToList();
        public static List<Dictionary<Vector2, Sprite>> DictionaryToList(Dictionary<string, Dictionary<Vector2, Sprite>>.ValueCollection value) => value.ToList();
    }

    struct SubTexture {
        public int w;
        public int h;
        public int x;
        public int y;
        public int fx;
        public int fy;
        public string name;
    }
}

