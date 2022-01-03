using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

namespace MagicaVoxelTools
{
    public class TextureUtility
    {
        public static Texture2D PackTextures(List<Texture2D> texs, int size, int pad,  out List<Vector2> uvs)
        {
            List<Texture2D> newTexs = new List<Texture2D>();

            foreach(Texture2D tex in texs)
            {
                newTexs.Add(ResizeAndPad(tex, size, pad));
            }

            Texture2D packedTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            packedTexture.filterMode = FilterMode.Point;

            Rect[] rects = packedTexture.PackTextures(newTexs.ToArray(), 0);

            uvs = new List<Vector2>();

            for (int i = 0; i < rects.Length; i++)
            {
                Rect r = rects[i];

                float pw = r.width * (1.0f / (float)newTexs[i].width);
                float ph = r.height * (1.0f / (float)newTexs[i].height);

                ////Full Texture
                //uvs.Add(new Vector2(r.x, r.y));
                //uvs.Add(new Vector2(r.x + r.width, r.y + r.height));
                //uvs.Add(new Vector2(r.x, r.y + r.height));
                //uvs.Add(new Vector2(r.x + r.width, r.y));

                //offset
                uvs.Add(new Vector2(r.x + pw * pad, r.y + ph * pad));
                uvs.Add(new Vector2(r.x + r.width - pw * pad, r.y + r.height - ph * pad));
                uvs.Add(new Vector2(r.x + pw * pad, r.y + r.height - ph * pad));
                uvs.Add(new Vector2(r.x + r.width - pw * pad, r.y + ph * pad));
            }

            return packedTexture;
        }

        public static Texture2D ResizeAndPad(Texture2D tex, int size, int pad)
        {
            tex = Resize(tex, size);
            tex = Pad(tex, pad);
            return tex;
        }
        public static Texture2D Resize(Texture2D tex, int size)
        {
            if (size <= 1) return tex;

            return ResizeTexture(tex, tex.width * size, tex.height * size);
        }

        public static Texture2D Pad(Texture2D tex, int pad)
        {
            if (pad <= 0) return tex;

            Texture2D tmp = new Texture2D(tex.width + pad * 2, tex.height + pad * 2, tex.format, true);
            Graphics.CopyTexture(tex, 0, 0, 0, 0, tex.width, tex.height, tmp, 0, 0, pad, pad);

            for (int i = 0; i < pad; i++)
            {
                Color[] ct = tex.GetPixels(0, 0, tex.width, 1);
                tmp.SetPixels(pad, i, tex.width, 1, ct);

                Color[] cb = tex.GetPixels(0, tex.height - 1, tex.width, 1);
                tmp.SetPixels(pad, tex.height + pad + i, tex.width, 1, cb);

                Color[] cl = tex.GetPixels(0, 0, 1, tex.height);
                tmp.SetPixels(i, pad, 1, tex.height, cl);

                Color[] cr = tex.GetPixels(tex.width - 1, 0, 1, tex.height);
                tmp.SetPixels(tex.width + pad + i, pad, 1, tex.height, cr);
            }

            Color[] c = new Color[pad * pad];
            Color f = Color.black;

            f = tex.GetPixel(0, 0);
            for (int i = 0; i < pad * pad; i++) c[i] = f;
            tmp.SetPixels(0, 0, pad, pad, c);

            f = tex.GetPixel(tex.width - 1, 0);
            for (int i = 0; i < pad * pad; i++) c[i] = f;
            tmp.SetPixels(tex.width + pad, 0, pad, pad, c);

            f = tex.GetPixel(0, tex.height - 1);
            for (int i = 0; i < pad * pad; i++) c[i] = f;
            tmp.SetPixels(0, tex.height + pad, pad, pad, c);

            f = tex.GetPixel(tex.width - 1, tex.height - 1);
            for (int i = 0; i < pad * pad; i++) c[i] = f;
            tmp.SetPixels(tex.width + pad, tex.height + pad, pad, pad, c);

            tmp.Apply();
            return tmp;
        }


        private class ThreadData
        {
            public int start;
            public int end;
            public ThreadData(int s, int e)
            {
                start = s;
                end = e;
            }
        }

        private static Color[] texColors;
        private static Color[] newColors;
        private static int w;
        private static float ratioX;
        private static float ratioY;
        private static int w2;
        private static int finishCount;
        private static Mutex mutex;

        private static Texture2D ResizeTexture(Texture2D tex, int newWidth, int newHeight)
        {
            ThreadedScale(tex, newWidth, newHeight);
            return tex;
        }

        private static void ThreadedScale(Texture2D tex, int newWidth, int newHeight)
        {
            texColors = tex.GetPixels();
            newColors = new Color[newWidth * newHeight];
            ratioX = ((float)tex.width) / newWidth;
            ratioY = ((float)tex.height) / newHeight;
            w = tex.width;
            w2 = newWidth;
            var cores = Mathf.Min(SystemInfo.processorCount, newHeight);
            var slice = newHeight / cores;

            finishCount = 0;
            if (mutex == null)
            {
                mutex = new Mutex(false);
            }
            if (cores > 1)
            {
                int i = 0;
                ThreadData threadData;
                for (i = 0; i < cores - 1; i++)
                {
                    threadData = new ThreadData(slice * i, slice * (i + 1));
                    ParameterizedThreadStart ts = new ParameterizedThreadStart(PointScale);
                    Thread thread = new Thread(ts);
                    thread.Start(threadData);
                }
                threadData = new ThreadData(slice * i, newHeight);
                PointScale(threadData);
                while (finishCount < cores)
                {
                    Thread.Sleep(1);
                }
            }
            else
            {
                ThreadData threadData = new ThreadData(0, newHeight);
                PointScale(threadData);
            }

            tex.Resize(newWidth, newHeight);
            tex.SetPixels(newColors);
            tex.Apply();

            texColors = null;
            newColors = null;
        }
        private static void PointScale(System.Object obj)
        {
            ThreadData threadData = (ThreadData)obj;
            for (var y = threadData.start; y < threadData.end; y++)
            {
                var thisY = (int)(ratioY * y) * w;
                var yw = y * w2;
                for (var x = 0; x < w2; x++)
                {
                    newColors[yw + x] = texColors[(int)(thisY + ratioX * x)];
                }
            }

            mutex.WaitOne();
            finishCount++;
            mutex.ReleaseMutex();
        }
    }

}
