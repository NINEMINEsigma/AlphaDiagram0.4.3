using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using AD.BASE;
using AD.Utility;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace AD.UI
{
    public interface ICanGetArchitecture
    {
        IADArchitecture ADinstance();
    }

    public interface ICanTransformSprite
    {
        void TransformSprite(Sprite target, Image image);
    }

    [Serializable]
    public class ImagePair
    {
        public Sprite SpriteSource = null;
        public ImagePair ChangeSprite(Sprite newSprite)
        {
            SpriteSource = newSprite;
            return this;
        }
        public string Name = "New Pair";
        public string SpriteName = "New Sprite";
    }

    [Serializable]
    [RequireComponent(typeof(Image))]
    [AddComponentMenu("UI/AD/ViewController", 100)]
    public sealed class ViewController : ADUI, IADController
    {
        #region Attribute 

        private Image _ViewImage;
        public Image ViewImage
        {
            get
            {
                if (_ViewImage == null) _ViewImage = GetComponent<Image>();
                return _ViewImage;
            }
            private set
            {
                _ViewImage = value;
            }
        }

        public List<ImagePair> SourcePairs = new List<ImagePair>();
        private int CurrentPairIndex = 0;

        public ImagePair CurrentImagePair
        {
            get
            {
                if (SourcePairs.Count > 0) return SourcePairs[CurrentPairIndex];
                else return null;
            }
            set
            {
                SourcePairs[CurrentPairIndex] = value;
            }
        }
        public Sprite CurrentImage
        {
            get
            {
                if (SourcePairs.Count > 0) return SourcePairs[CurrentPairIndex].SpriteSource;
                else return null;
            }
            set
            {
                if (SourcePairs.Count > 0) SourcePairs[CurrentPairIndex].SpriteSource = value;
                else Debug.LogWarning("this Image's list of Source is empty,but you try to change it");
            }
        }
        public int CurrentIndex
        {
            get { return CurrentPairIndex; }
        }

        [HideInInspector] public ICanGetArchitecture architectureObtainer = null;
        [HideInInspector] public ICanTransformSprite canTransformSprite = null;

        #endregion

        #region Function

        public ViewController()
        {
            ElementArea = "Image";
        }

        private void Start()
        {
            AD.UI.ADUI.Initialize(this);

            ViewImage = GetComponent<Image>();
            ViewImage.sprite = CurrentImage;
        }
        private void OnDestroy()
        {
            AD.UI.ADUI.Destory(this);
        }

#if UNITY_EDITOR
        [MenuItem("GameObject/AD/Image", false, 10)]
        private static void ADD(MenuCommand menuCommand)
        {
            AD.UI.ViewController obj = null;
            if (ADGlobalSystem.instance._Image != null)
            {
                obj = GameObject.Instantiate(ADGlobalSystem.instance._Image);
                obj.name = "New Image";
            }
            else
            {
                obj = new GameObject("New Image").AddComponent<AD.UI.ViewController>();
            }
            GameObjectUtility.SetParentAndAlign(obj.gameObject, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(obj.gameObject, "Create " + obj.name);
            Selection.activeObject = obj.gameObject;
        }
#endif

        public static ViewController Generate(string name = "New Image", Transform parent = null, params System.Type[] components)
        {
            ViewController source = new GameObject(name, components).AddComponent<ViewController>();
            source.transform.SetParent(parent, false);

            return source;
        }

        public IADArchitecture ADinstance()
        {
            if (architectureObtainer == null) return null;
            else return architectureObtainer.ADinstance();
        }

        public ViewController SetTransparentChannelCollisionThreshold(float value)
        {
            ViewImage.alphaHitTestMinimumThreshold = value;
            return this;
        }

        public ViewController SetMaterial(Material material)
        {
            ViewImage.material = material;
            return this;
        }

        public ViewController RandomPairs()
        {
            SourcePairs.Sort((T, P) => { if (UnityEngine.Random.Range(-1, 1) > 0) return 1; else return -1; });
            return this;
        }

        public ViewController Refresh()
        {
            ViewImage.sprite = CurrentImage;
            return this;
        }

        public ViewController NextPair()
        {
            if (SourcePairs.Count == 0) return this;
            if (CurrentPairIndex < SourcePairs.Count - 1) CurrentPairIndex++;
            else CurrentPairIndex = 0;
            if (canTransformSprite == null) Refresh();
            else canTransformSprite.TransformSprite(CurrentImage, ViewImage);
            return this;
        }
        public ViewController PreviousPair()
        {
            if (SourcePairs.Count == 0) return this;
            if (CurrentPairIndex > 0) CurrentPairIndex--;
            else CurrentPairIndex = SourcePairs.Count - 1;
            if (canTransformSprite == null) Refresh();
            else canTransformSprite.TransformSprite(CurrentImage, ViewImage);
            return this;
        }
        public ViewController RandomPair()
        {
            if (SourcePairs.Count == 0) return this;
            CurrentPairIndex = UnityEngine.Random.Range(0, SourcePairs.Count);
            if (canTransformSprite == null) Refresh();
            else canTransformSprite.TransformSprite(CurrentImage, ViewImage);
            return this;
        }
        public ViewController SetPair(int index)
        {
            if (SourcePairs.Count == 0) return this;
            CurrentPairIndex = Mathf.Clamp(index, 0, SourcePairs.Count - 1);
            if (canTransformSprite == null) Refresh();
            else canTransformSprite.TransformSprite(CurrentImage, ViewImage);
            return this;
        }

        public ViewController SetAlpha(float alpha)
        {
            ViewImage.color = new Color(ViewImage.color.r, ViewImage.color.g, ViewImage.color.b, alpha);
            return this;
        }
        public ViewController SetRed(float red)
        {
            ViewImage.color = new Color(red, ViewImage.color.g, ViewImage.color.b, ViewImage.color.a);
            return this;
        }
        public ViewController SetGreen(float green)
        {
            ViewImage.color = new Color(ViewImage.color.r, green, ViewImage.color.b, ViewImage.color.a);
            return this;
        }
        public ViewController SetBlue(float blue)
        {
            ViewImage.color = new Color(ViewImage.color.r, ViewImage.color.g, blue, ViewImage.color.a);
            return this;
        }

        public ViewController BakeAudioWaveformFormAudioCilp(AudioClip clip)
        {
            ViewImage.color = new Color();
            ViewImage.sprite = null;
            ViewImage.sprite = AudioSourceController.BakeAudioWaveform(clip).ToSprite();
            return this;
        }

        public void Init()
        {

        }

        public void SendCommand<T>() where T : class, IADCommand, new()
        {
            Architecture.SendCommand<T>();
        }

        public T GetSystem<T>() where T : class, IADSystem, new()
        {
            return Architecture.GetSystem<T>();
        }

        public IADArchitecture ADInstance()
        {
            return Architecture;
        }

        public IADArchitecture Architecture { get; private set; } = null;

        public void SetArchitecture(IADArchitecture target)
        {
            Architecture = target;
        }

        public void RegisterCommand<T>() where T : class, IADCommand, new()
        {
            Architecture.RegisterCommand<T>();
        }

        public void RegisterModel<T>() where T : class, IADModel, new()
        {
            Architecture.RegisterModel<T>();
        }

        public T GetModel<T>() where T : class, IADModel, new()
        {
            return Architecture.GetModel<T>();
        }

        public void RegisterCommand<T>(T _Command) where T : class, IADCommand, new()
        {
            Architecture.RegisterCommand<T>(_Command);
        }

        public void RegisterModel<T>(T _Model) where T : class, IADModel, new()
        {
            Architecture.RegisterModel<T>(_Model);
        }

        #endregion

        #region Resource

        public void LoadOnResource(string source, bool isCurrent = true)
        {
            string finalPath = Application.dataPath + "/Resources/" + source;
            StartCoroutine(LoadTexture2D(finalPath, isCurrent));
        }

        public void LoadOnUrl(string url, bool isCurrent = true)
        {
            StartCoroutine(LoadTexture2D(url, isCurrent));
        }

        public IEnumerator LoadTexture2D(string path, bool isCurrent)
        {
            UnityWebRequest request = UnityWebRequestTexture.GetTexture(path);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                var texture = DownloadHandlerTexture.GetContent(request);
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                if (isCurrent && SourcePairs.Count > 0)
                    CurrentImage = sprite;
                else
                    SourcePairs.Add(new ImagePair() { SpriteName = path, SpriteSource = sprite });
                Refresh();
            }
            else
                Debug.LogError("Failed To Load "+ request.result.ToString());
        }

        public void LoadByIo(string path, int width, int height, bool isCurrent = true)
        {
            byte[] bytes;
            using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                bytes = new byte[fileStream.Length];
                fileStream.Read(bytes, 0, (int)fileStream.Length);
            }

            Texture2D texture = new Texture2D(width, height);
            texture.LoadImage(bytes);

            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            if (isCurrent && SourcePairs.Count > 0)
                CurrentImage = sprite;
            else
                SourcePairs.Add(new ImagePair() { SpriteName = path, SpriteSource = sprite });
        }

        #endregion

    }
}
