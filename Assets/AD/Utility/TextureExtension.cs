using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace AD.Utility
{
    public static class TextureExtension
    {
#if UNITY_EDITOR
        /// <summary>
        /// 编辑器模式下Texture转换成Texture2D
        /// </summary>
        /// <param name="texture"></param>
        /// <returns></returns>
        public static Texture2D ToTexture2D_Editor(Texture texture)
        {
            Texture2D texture2d = texture as Texture2D;
            UnityEditor.TextureImporter ti = (UnityEditor.TextureImporter)UnityEditor.AssetImporter.GetAtPath(UnityEditor.AssetDatabase.GetAssetPath(texture2d));
            //图片Read/Write Enable的开关
            ti.isReadable = true;
            UnityEditor.AssetDatabase.ImportAsset(UnityEditor.AssetDatabase.GetAssetPath(texture2d));
            return texture2d;
        }

        public static void SaveRenderTextureToPNG_Editor(this RenderTexture self, string textureName, Action<UnityEditor.TextureImporter> importAction = null)
        {
            string path = UnityEditor.EditorUtility.SaveFilePanel("Save to png", Application.dataPath, textureName + "_painted.png", "png");
            if (path.Length != 0)
            {
                var newTex = new Texture2D(self.width, self.height);
                RenderTexture.active = self;
                newTex.ReadPixels(new Rect(0, 0, self.width, self.height), 0, 0);
                newTex.Apply();

                byte[] pngData = newTex.EncodeToPNG();
                if (pngData != null)
                {
                    File.WriteAllBytes(path, pngData);
                    UnityEditor.AssetDatabase.Refresh();
                    var importer = UnityEditor.AssetImporter.GetAtPath(path) as UnityEditor.TextureImporter;
                    importAction?.Invoke(importer);
                }

                Debug.Log(path);
            }
        }
#endif

        public static Texture2D Clone(this Texture2D self)
        {
            //originTex为克隆对象
            Texture2D result;
            result = new Texture2D(self.width, self.height);
            Color[] colors = self.GetPixels(0, 0, self.width, self.height);
            result.SetPixels(colors);
            result.Apply();//必须apply才生效
            return result;
        }

        public static Texture2D ToTexture2D(this RenderTexture self)
        {
            int width = self.width;
            int height = self.height;
            Texture2D texture2D = new Texture2D(width, height, TextureFormat.ARGB32, false);
            RenderTexture.active = self;
            texture2D.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            texture2D.Apply();
            return texture2D;
        }

        /// <summary>
        /// 运行模式下Texture转换成Texture2D
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static Texture2D ToTexture2D_Runtime(this Texture self)
        {
            Texture2D texture2D = new Texture2D(self.width, self.height, TextureFormat.RGBA32, false);
            RenderTexture currentRT = RenderTexture.active;
            RenderTexture renderTexture = RenderTexture.GetTemporary(self.width, self.height, 32);
            Graphics.Blit(self, renderTexture);

            RenderTexture.active = renderTexture;
            texture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            texture2D.Apply();

            RenderTexture.active = currentRT;
            RenderTexture.ReleaseTemporary(renderTexture);

            return texture2D;
        }

        public static void SaveRenderTextureToPNG_Runtime(this RenderTexture self, string path)
        {
            if (path.Length != 0)
            {
                var newTex = new Texture2D(self.width, self.height);
                RenderTexture.active = self;
                newTex.ReadPixels(new Rect(0, 0, self.width, self.height), 0, 0);
                newTex.Apply();

                byte[] pngData = newTex.EncodeToPNG();
                if (pngData != null)
                {
                    File.WriteAllBytes(path, pngData);
                }

                Debug.Log(path);
            }
        }

        public static Texture2D ToTexture2D(this Sprite self)
        {
            //sprite为图集中的某个子Sprite对象
            Texture2D targetTex = new Texture2D((int)self.rect.width, (int)self.rect.height);
            Color[] pixels = self.texture.GetPixels(
                (int)self.textureRect.x,
                (int)self.textureRect.y,
                (int)self.textureRect.width,
                (int)self.textureRect.height);
            targetTex.SetPixels(pixels);
            targetTex.Apply();
            return targetTex;
        }

        public static Sprite ToSprite(this Texture2D self)
        {
            //t2d为待转换的Texture2D对象
            Sprite result = Sprite.Create(self, new Rect(0, 0, self.width, self.height), Vector2.zero);
            return result;
        }

        public static byte[] Save(this Texture2D self, FileType type = FileType.PNG)
        {
            return type switch
            {
                FileType.EXR => self.EncodeToEXR(),
                FileType.TGA => self.EncodeToTGA(),
                FileType.JPG => self.EncodeToJPG(),
                FileType.PNG => self.EncodeToPNG(),
                _ => null,
            };
        }

        public enum FileType
        {
            PNG, JPG, EXR, TGA
        }

        public static IEnumerator GetTexture(this FileInfo self,UnityAction<Texture2D> action)
        {
            UnityWebRequest request = UnityWebRequestTexture.GetTexture(self.FullName);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                action.Invoke(texture);
            }
            else
            {
                Debug.LogError("Failed To Load " + request.result.ToString());
                action.Invoke(null);
            }
        }
    }
}
