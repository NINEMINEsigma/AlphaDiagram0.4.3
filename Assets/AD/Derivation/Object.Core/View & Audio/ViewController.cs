using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using AD.BASE;
using AD.Utility;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace AD.UI
{
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
    [AddComponentMenu("UI/AD/ViewController", 100)]
    public sealed class ViewController : BaseMeshEffect, IADUI, IADController
    {
        #region ADUI

        public bool Selected = false;
        private BehaviourContext _Context;
        public bool IsNeedContext => true;
        public BehaviourContext Context
        {
            get
            {
                if (!IsNeedContext) return null;
                _Context ??= this.GetOrAddComponent<BehaviourContext>();
                return _Context;
            }
        }
        public string ElementName { get; set; } = "null";
        public int SerialNumber { get; set; } = 0;
        public string ElementArea = "null";
        public IADUI Obtain(int serialNumber)
        {
            return ADUI.Items.Find((P) => P.SerialNumber == serialNumber);
        }
        public IADUI Obtain(string elementName)
        {
            return ADUI.Items.Find((P) => P.ElementName == elementName);
        }
        public void InitializeContext()
        {
            Context.OnPointerEnterEvent = ADUI.InitializeContextSingleEvent(Context.OnPointerEnterEvent, OnPointerEnter);
            Context.OnPointerExitEvent = ADUI.InitializeContextSingleEvent(Context.OnPointerExitEvent, OnPointerExit);
        }
        public void OnPointerEnter(PointerEventData eventData)
        {
            Selected = true;
            ADUI.CurrentSelect = null;
            ADUI.UIArea = ElementArea;
        }
        public void OnPointerExit(PointerEventData eventData)
        {
            Selected = false;
            ADUI.UIArea = "null";
        }

        #endregion

        #region Attribute 

        [SerializeField] private Image _ViewImage = null;
        public bool HasImage = true;
        public Image ViewImage
        {
            get
            {
                if (_ViewImage == null)
                {
                    if (TryGetComponent<Image>(out var image))
                    {
                        _ViewImage = image;
                    }
                    else HasImage = false;
                }
                return _ViewImage;
            }
        }

        public List<ImagePair> SourcePairs = new();
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

        [HideInInspector] public ICanTransformSprite canTransformSprite = null;

        //会对性能产生不低的影响
        public bool IsKeepCoverParent = false;
        //更好的选择
        public bool IsSetUpCoverParentAtStart = false;

        [SerializeField]
        Type _gradientType;

        [SerializeField]
        Blend _blendMode = Blend.Multiply;

        [SerializeField]
        bool _modifyVertices = true;

        [SerializeField]
        [Range(-1, 1)]
        float _offset = 0f;

        [SerializeField]
        [Range(0.1f, 10)]
        float _zoom = 1f;

        [SerializeField]
        UnityEngine.Gradient _effectGradient = new Gradient()
        { colorKeys = new GradientColorKey[] { new GradientColorKey(Color.white, 1), new GradientColorKey(Color.white, 1) } };

        public Blend BlendMode
        {
            get { return _blendMode; }
            set
            {
                _blendMode = value;
                graphic.SetVerticesDirty();
            }
        }

        public UnityEngine.Gradient EffectGradient
        {
            get { return _effectGradient; }
            set
            {
                _effectGradient = value;
                graphic.SetVerticesDirty();
            }
        }

        public Type GradientType
        {
            get { return _gradientType; }
            set
            {
                _gradientType = value;
                graphic.SetVerticesDirty();
            }
        }

        public bool ModifyVertices
        {
            get { return _modifyVertices; }
            set
            {
                _modifyVertices = value;
                graphic.SetVerticesDirty();
            }
        }

        public float Offset
        {
            get { return _offset; }
            set
            {
                _offset = Mathf.Clamp(value, -1f, 1f);
                graphic.SetVerticesDirty();
            }
        }

        public float Zoom
        {
            get { return _zoom; }
            set
            {
                _zoom = Mathf.Clamp(value, 0.1f, 10f);
                graphic.SetVerticesDirty();
            }
        }

        public enum Type
        {
            Horizontal,
            Vertical,
            Diamond
        }

        public enum Blend
        {
            Override,
            Add,
            Multiply
        }

        #endregion

        #region MonoFunction

        public ViewController()
        {
            ElementArea = "Image";
        }

        protected override void Start()
        {
            base.Start();

            if (!Application.isPlaying) return;
            if (ViewImage != null)
            {
                AD.UI.ADUI.Initialize(this);
                ViewImage.sprite = CurrentImage;
            }
            if (IsSetUpCoverParentAtStart)
            {
                SetupCoverParent();
            }
        }
#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            if (IsKeepCoverParent)
            {
                SetupCoverParent();
            }
            if (ViewImage != null) ViewImage.sprite = CurrentImage;
        }
#endif
        private void Update()
        {
            if (IsKeepCoverParent)
            {
                SetupCoverParent();
            }
        }
        protected override void OnEnable()
        {
            if (ViewImage != null) ViewImage.sprite = CurrentImage;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (ViewImage != null)
            {
                AD.UI.ADUI.DestroyADUI(this);
            }
        }

        #endregion

        #region Image - View

        public static ViewController Generate(string name = "New Image", Transform parent = null, params System.Type[] components)
        {
            ViewController source = new GameObject(name, components).AddComponent<ViewController>();
            source.transform.SetParent(parent, false);

            return source;
        }

        public IADArchitecture ADinstance()
        {
            return Architecture;
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
        public ViewController SetPair(string key)
        {
            if (SourcePairs.FindIndex(T => T.Name == key).Share(out var result) != -1) SetPair(result);
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

        public void SetupCoverParent()
        {
            if (SourcePairs.Count == 0 && this.transform.parent == null) return;
            if (CurrentImage == null)
            {
                this.transform.localPosition = Vector3.zero;
                this.transform.As<RectTransform>().sizeDelta = this.transform.parent.transform.As<RectTransform>().sizeDelta;
            }
            else
            {
                float ratio = (float)CurrentImage.texture.height / CurrentImage.texture.width;
                var p_rectt = this.transform.parent.transform.As<RectTransform>();
                var p_temp_rect = p_rectt.GetRect();
                float parentRatio = Mathf.Abs(p_temp_rect[0].y - p_temp_rect[1].y) / (float)Mathf.Abs(p_temp_rect[2].x - p_temp_rect[1].x);
                this.transform.localPosition = Vector3.zero;
                if (ratio > parentRatio)
                {
                    this.transform.As<RectTransform>().sizeDelta = new Vector2(p_rectt.sizeDelta.x, p_rectt.sizeDelta.x * ratio);
                }
                else
                {
                    this.transform.As<RectTransform>().sizeDelta = new Vector2(p_rectt.sizeDelta.y / ratio, p_rectt.sizeDelta.y);
                }
            }
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

        public IADArchitecture Architecture { get; set; } = null;

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
            if (string.IsNullOrEmpty(source)) return;
            string finalPath = Application.dataPath + "/Resources/" + source;
            ADGlobalSystem.OpenCoroutine(LoadTexture2D(finalPath, isCurrent));
        }

        public void LoadOnUrl(string url, bool isCurrent = true)
        {
            if (string.IsNullOrEmpty(url)) return;
            ADGlobalSystem.OpenCoroutine(LoadTexture2D(url, isCurrent));
        }

        public void SyncLoadOnResource(string source, bool isCurrent = true)
        {
            string path = Application.dataPath + "/Resources/" + source;
            UnityWebRequest request = UnityWebRequestTexture.GetTexture(path);
            request.SendWebRequest().MarkCompleted(() =>
            {
                if (request.result == UnityWebRequest.Result.Success)
                {
                    var texture = DownloadHandlerTexture.GetContent(request);
                    Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                    if (isCurrent && SourcePairs.Count > 0)
                        CurrentImage = sprite;
                    else
                        SourcePairs.Add(new ImagePair() { SpriteName = path, Name = path, SpriteSource = sprite });
                    Refresh();
                }
                else
                    Debug.LogError("Failed To Load " + request.result.ToString());
            });
        }

        public void SyncLoadOnUrl(string url, bool isCurrent = true)
        {
            string path = url;
            UnityWebRequest request = UnityWebRequestTexture.GetTexture(path);
            request.SendWebRequest().MarkCompleted(() =>
            {
                if (request.result == UnityWebRequest.Result.Success)
                {
                    var texture = DownloadHandlerTexture.GetContent(request);
                    Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                    if (isCurrent && SourcePairs.Count > 0)
                        CurrentImage = sprite;
                    else
                        SourcePairs.Add(new ImagePair() { SpriteName = path, Name = path, SpriteSource = sprite });
                    Refresh();
                }
                else
                    Debug.LogError("Failed To Load " + request.result.ToString());
            });

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
                    SourcePairs.Add(new ImagePair() { SpriteName = path, Name = path, SpriteSource = sprite });
                Refresh();
            }
            else
                Debug.LogError("Failed To Load " + request.result.ToString());
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
                SourcePairs.Add(new ImagePair() { SpriteName = path, Name = path, SpriteSource = sprite });
        }

        #endregion

        #region Gradient

        public override void ModifyMesh(VertexHelper helper)
        {
            if (!IsActive() || helper.currentVertCount == 0)
                return;

            List<UIVertex> _vertexList = new List<UIVertex>();
            helper.GetUIVertexStream(_vertexList);
            int nCount = _vertexList.Count;

            switch (GradientType)
            {
                case Type.Horizontal:
                case Type.Vertical:
                    {
                        Rect bounds = GetBounds(_vertexList);
                        float min = bounds.xMin;
                        float w = bounds.width;
                        Func<UIVertex, float> GetPosition = v => v.position.x;

                        if (GradientType == Type.Vertical)
                        {
                            min = bounds.yMin;
                            w = bounds.height;
                            GetPosition = v => v.position.y;
                        }

                        float width = w == 0f ? 0f : 1f / w / Zoom;
                        float zoomOffset = (1 - (1 / Zoom)) * 0.5f;
                        float offset = (Offset * (1 - zoomOffset)) - zoomOffset;

                        if (ModifyVertices)
                            SplitTrianglesAtGradientStops(_vertexList, bounds, zoomOffset, helper);

                        UIVertex vertex = new UIVertex();
                        for (int i = 0; i < helper.currentVertCount; i++)
                        {
                            helper.PopulateUIVertex(ref vertex, i);
                            vertex.color = BlendColor(vertex.color, EffectGradient.Evaluate((GetPosition(vertex) - min) * width - offset));
                            helper.SetUIVertex(vertex, i);
                        }
                    }
                    break;

                case Type.Diamond:
                    {
                        Rect bounds = GetBounds(_vertexList);
                        float height = bounds.height == 0f ? 0f : 1f / bounds.height / Zoom;
                        float radius = bounds.center.y / 2f;
                        Vector3 center = (Vector3.right + Vector3.up) * radius + Vector3.forward * _vertexList[0].position.z;

                        if (ModifyVertices)
                        {
                            helper.Clear();
                            for (int i = 0; i < nCount; i++) helper.AddVert(_vertexList[i]);

                            UIVertex centralVertex = new UIVertex();
                            centralVertex.position = center;
                            centralVertex.normal = _vertexList[0].normal;
                            centralVertex.uv0 = new Vector2(0.5f, 0.5f);
                            centralVertex.color = Color.white;
                            helper.AddVert(centralVertex);

                            for (int i = 1; i < nCount; i++) helper.AddTriangle(i - 1, i, nCount);
                            helper.AddTriangle(0, nCount - 1, nCount);
                        }

                        UIVertex vertex = new UIVertex();

                        for (int i = 0; i < helper.currentVertCount; i++)
                        {
                            helper.PopulateUIVertex(ref vertex, i);
                            vertex.color = BlendColor(vertex.color, EffectGradient.Evaluate(
                                Vector3.Distance(vertex.position, center) * height - Offset));
                            helper.SetUIVertex(vertex, i);
                        }
                    }
                    break;
            }
        }

        Rect GetBounds(List<UIVertex> vertices)
        {
            float left = vertices[0].position.x;
            float right = left;
            float bottom = vertices[0].position.y;
            float top = bottom;

            for (int i = vertices.Count - 1; i >= 1; --i)
            {
                float x = vertices[i].position.x;
                float y = vertices[i].position.y;

                if (x > right)
                    right = x;
                else if (x < left)
                    left = x;

                if (y > top)
                    top = y;
                else if (y < bottom)
                    bottom = y;
            }

            return new Rect(left, bottom, right - left, top - bottom);
        }

        void SplitTrianglesAtGradientStops(List<UIVertex> _vertexList, Rect bounds, float zoomOffset, VertexHelper helper)
        {
            List<float> stops = FindStops(zoomOffset, bounds);
            if (stops.Count > 0)
            {
                helper.Clear();
                int nCount = _vertexList.Count;

                for (int i = 0; i < nCount; i += 3)
                {
                    float[] positions = GetPositions(_vertexList, i);
                    List<int> originIndices = new List<int>(3);
                    List<UIVertex> starts = new List<UIVertex>(3);
                    List<UIVertex> ends = new List<UIVertex>(2);

                    for (int s = 0; s < stops.Count; s++)
                    {
                        int initialCount = helper.currentVertCount;
                        bool hadEnds = ends.Count > 0;
                        bool earlyStart = false;

                        for (int p = 0; p < 3; p++)
                        {
                            if (!originIndices.Contains(p) && positions[p] < stops[s])
                            {
                                int p1 = (p + 1) % 3;
                                var start = _vertexList[p + i];

                                if (positions[p1] > stops[s])
                                {
                                    originIndices.Insert(0, p);
                                    starts.Insert(0, start);
                                    earlyStart = true;
                                }

                                else
                                {
                                    originIndices.Add(p);
                                    starts.Add(start);
                                }
                            }
                        }

                        if (originIndices.Count == 0)
                            continue;
                        if (originIndices.Count == 3)
                            break;

                        foreach (var start in starts)
                            helper.AddVert(start);

                        ends.Clear();
                        foreach (int index in originIndices)
                        {
                            int oppositeIndex = (index + 1) % 3;

                            if (positions[oppositeIndex] < stops[s])
                                oppositeIndex = (oppositeIndex + 1) % 3;
                            ends.Add(CreateSplitVertex(_vertexList[index + i], _vertexList[oppositeIndex + i], stops[s]));
                        }

                        if (ends.Count == 1)
                        {
                            int oppositeIndex = (originIndices[0] + 2) % 3;
                            ends.Add(CreateSplitVertex(_vertexList[originIndices[0] + i], _vertexList[oppositeIndex + i], stops[s]));
                        }

                        foreach (var end in ends)
                            helper.AddVert(end);

                        if (hadEnds)
                        {
                            helper.AddTriangle(initialCount - 2, initialCount, initialCount + 1);
                            helper.AddTriangle(initialCount - 2, initialCount + 1, initialCount - 1);

                            if (starts.Count > 0)
                            {
                                if (earlyStart)
                                    helper.AddTriangle(initialCount - 2, initialCount + 3, initialCount);
                                else
                                    helper.AddTriangle(initialCount + 1, initialCount + 3, initialCount - 1);
                            }
                        }

                        else
                        {
                            int vertexCount = helper.currentVertCount;
                            helper.AddTriangle(initialCount, vertexCount - 2, vertexCount - 1);

                            if (starts.Count > 1)
                                helper.AddTriangle(initialCount, vertexCount - 1, initialCount + 1);
                        }

                        starts.Clear();
                    }

                    if (ends.Count > 0)
                    {
                        if (starts.Count == 0)
                        {
                            for (int p = 0; p < 3; p++)
                            {
                                if (!originIndices.Contains(p) && positions[p] > stops[stops.Count - 1])
                                {
                                    int p1 = (p + 1) % 3;
                                    UIVertex end = _vertexList[p + i];

                                    if (positions[p1] > stops[stops.Count - 1])
                                        starts.Insert(0, end);
                                    else
                                        starts.Add(end);
                                }
                            }
                        }

                        foreach (var start in starts)
                            helper.AddVert(start);

                        int vertexCount = helper.currentVertCount;

                        if (starts.Count > 1)
                        {
                            helper.AddTriangle(vertexCount - 4, vertexCount - 2, vertexCount - 1);
                            helper.AddTriangle(vertexCount - 4, vertexCount - 1, vertexCount - 3);
                        }

                        else if (starts.Count > 0)
                            helper.AddTriangle(vertexCount - 3, vertexCount - 1, vertexCount - 2);
                    }

                    else
                    {
                        helper.AddVert(_vertexList[i]);
                        helper.AddVert(_vertexList[i + 1]);
                        helper.AddVert(_vertexList[i + 2]);
                        int vertexCount = helper.currentVertCount;
                        helper.AddTriangle(vertexCount - 3, vertexCount - 2, vertexCount - 1);
                    }
                }
            }
        }

        float[] GetPositions(List<UIVertex> _vertexList, int index)
        {
            float[] positions = new float[3];

            if (GradientType == Type.Horizontal)
            {
                positions[0] = _vertexList[index].position.x;
                positions[1] = _vertexList[index + 1].position.x;
                positions[2] = _vertexList[index + 2].position.x;
            }

            else
            {
                positions[0] = _vertexList[index].position.y;
                positions[1] = _vertexList[index + 1].position.y;
                positions[2] = _vertexList[index + 2].position.y;
            }

            return positions;
        }

        List<float> FindStops(float zoomOffset, Rect bounds)
        {
            List<float> stops = new List<float>();
            var offset = Offset * (1 - zoomOffset);
            var startBoundary = zoomOffset - offset;
            var endBoundary = (1 - zoomOffset) - offset;

            foreach (var color in EffectGradient.colorKeys)
            {
                if (color.time >= endBoundary)
                    break;

                if (color.time > startBoundary)
                    stops.Add((color.time - startBoundary) * Zoom);
            }

            foreach (var alpha in EffectGradient.alphaKeys)
            {
                if (alpha.time >= endBoundary)
                    break;

                if (alpha.time > startBoundary)
                    stops.Add((alpha.time - startBoundary) * Zoom);
            }

            float min = bounds.xMin;
            float size = bounds.width;

            if (GradientType == Type.Vertical)
            {
                min = bounds.yMin;
                size = bounds.height;
            }

            stops.Sort();

            for (int i = 0; i < stops.Count; i++)
            {
                stops[i] = (stops[i] * size) + min;

                if (i > 0 && Math.Abs(stops[i] - stops[i - 1]) < 2)
                {
                    stops.RemoveAt(i);
                    --i;
                }
            }

            return stops;
        }

        UIVertex CreateSplitVertex(UIVertex vertex1, UIVertex vertex2, float stop)
        {
            if (GradientType == Type.Horizontal)
            {
                float sx = vertex1.position.x - stop;
                float dx = vertex1.position.x - vertex2.position.x;
                float dy = vertex1.position.y - vertex2.position.y;
                float uvx = vertex1.uv0.x - vertex2.uv0.x;
                float uvy = vertex1.uv0.y - vertex2.uv0.y;
                float ratio = sx / dx;
                float splitY = vertex1.position.y - (dy * ratio);

                UIVertex splitVertex = new UIVertex();
                splitVertex.position = new Vector3(stop, splitY, vertex1.position.z);
                splitVertex.normal = vertex1.normal;
                splitVertex.uv0 = new Vector2(vertex1.uv0.x - (uvx * ratio), vertex1.uv0.y - (uvy * ratio));
                splitVertex.color = Color.white;
                return splitVertex;
            }

            else
            {
                float sy = vertex1.position.y - stop;
                float dy = vertex1.position.y - vertex2.position.y;
                float dx = vertex1.position.x - vertex2.position.x;
                float uvx = vertex1.uv0.x - vertex2.uv0.x;
                float uvy = vertex1.uv0.y - vertex2.uv0.y;
                float ratio = sy / dy;
                float splitX = vertex1.position.x - (dx * ratio);

                UIVertex splitVertex = new UIVertex();
                splitVertex.position = new Vector3(splitX, stop, vertex1.position.z);
                splitVertex.normal = vertex1.normal;
                splitVertex.uv0 = new Vector2(vertex1.uv0.x - (uvx * ratio), vertex1.uv0.y - (uvy * ratio));
                splitVertex.color = Color.white;
                return splitVertex;
            }
        }

        Color BlendColor(Color colorA, Color colorB)
        {
            switch (BlendMode)
            {
                default: return colorB;
                case Blend.Add: return colorA + colorB;
                case Blend.Multiply: return colorA * colorB;
            }
        }

        #endregion

    }
}
