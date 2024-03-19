using AD.BASE;
using UnityEngine;
using UnityEngine.Events;
using Unity.VisualScripting;
using AD.Utility;

namespace AD.UI
{
    [RequireComponent(typeof(ViewController))]
    public class CustomWindowElement : MonoBehaviour
    {
        public ADSerializableDictionary<string, RectTransform> Childs = new();

        public RectTransform this[int index]
        {
            get
            {
                int i = 0;
                foreach (var child in Childs) 
                    if (i++ == index) return child.Value; 
                return null;
            }
        }

        public RectTransform this[string key]
        {
            get
            {
                return Childs[key];
            }
        }

        private RectTransform _rectTransform;
        public RectTransform rectTransform
        {
            get
            {
                _rectTransform ??= GetComponent<RectTransform>();
                return _rectTransform;
            }
        }
        private AD.UI.ViewController _background;
        public AD.UI.ViewController background
        {
            get
            {
                _background ??= GetComponent<AD.UI.ViewController>();
                return _background;
            }
        }

        public Vector2 capacity => new Vector2(rectTransform.sizeDelta.x, rectTransform.sizeDelta.y - TopLine.sizeDelta.y);
        private Vector2 size;
        [SerializeField] private Vector4 _Padding = Vector4.zero;
        public Vector4 Padding
        {
            get
            {
                return _Padding;
            }
            set
            {
                _Padding = value;
                RefreshAllChild();
            }
        }

        [HideInInspector] public bool isCanBackPool = true;
        public bool isCanRefresh = true;
        public bool BaseDefaultIsSubPageUsingOtherSetting = false;
        protected virtual bool isSubPageUsingOtherSetting => BaseDefaultIsSubPageUsingOtherSetting;

        public bool isCanDrag
        {
            get => _DragBehaviour.isCanDrag;
            set => _DragBehaviour.isCanDrag = value;
        }

        [SerializeField] private RectTransform SubPage, TopLine;
        [SerializeField] private AD.UI.Text Title;

        public ADEvent OnEsc = new();

        public virtual CustomWindowElement Init()
        {
            foreach (var item in Childs)
            {
                Destroy(item.Value.gameObject);
            }
            Childs.Clear();
            rectTransform.localPosition = Vector3.zero;
            if (!(isSubPageUsingOtherSetting || BaseDefaultIsSubPageUsingOtherSetting))
                SetRect(new Vector2(300, 0));
            size = Vector2.zero;
            Padding = Vector4.zero;
            isCanBackPool = true;
            Title.text = "";
            MaxHightInThisLine = 0;
            _DragBehaviour = this.GetOrAddComponent<DragBehaviour>();
            _DragBehaviour.Init(rectTransform);
            return this;
        }

        public void Remove(string key)
        {
            if (Childs.TryGetValue(key, out var obj))
            {
                Childs.Remove(key);
                HowDestroyChild(obj);
            }
        }

        protected virtual void HowDestroyChild(RectTransform rectTransform)
        {
            GameObject.Destroy(rectTransform.gameObject);
        }

        public void BackPool()
        {
            OnEsc.Invoke();
            if (!isCanBackPool) return;
            HowBackPool.Invoke(this);
        }

        public ADInvokableCall<CustomWindowElement> HowBackPool;

        public CustomWindowElement MoveTo(Vector3 pos)
        {
            if (isCanRefresh)
                rectTransform.localPosition = pos;
            return this;
        }

        public CustomWindowElement MoveWithRectControl(Vector4 args)
        {
            if (!isCanRefresh) return this;
            rectTransform.rect.Set(args[0], args[1], args[2], args[3]);
            return this;
        }

        public CustomWindowElement SetRect(Vector2 Rect)
        {
            if (!isCanRefresh) return this;
            try
            {
                rectTransform.sizeDelta = Rect + new Vector2(0, TopLine.sizeDelta.y);
            }
            catch(System.Exception ex)
            {
                Debug.LogAssertion(ex);
            }
            return this;
        }

        public CustomWindowElement SetRectIgnoreTopLine(Vector2 Rect)
        {
            if (!isCanRefresh) return this;
            try
            {
                rectTransform.sizeDelta = Rect;
            }
            catch (System.Exception ex)
            {
                Debug.LogAssertion(ex);
            }
            return this;
        }

        public RectTransform GetChild(string keyName)
        {
            if (Childs.ContainsKey(keyName)) return Childs[keyName];
            else return null;
        }

        public CustomWindowElement SetTitle(string title)
        {
            Title.text = title;
            return this;
        }

        #region Refresh

        private float MaxHightInThisLine = 0;

        public void RefreshAllChild()
        {
            if (isCanRefresh)
            {
                size = Vector2.zero;
                foreach (var item in Childs)
                {
                    RefreshWithNeedSpace(item.Value);
                }
            }
        }

        private void RefreshWithNeedSpace(float x, float y, RectTransform rect)
        {
            if (!isCanRefresh) return;
            HowRefreshWithNeedSpace(x, y, rect);
        }

        private void HowRefreshWithNeedSpace(float x, float y, RectTransform rect)
        {
            if (IsNeedExpandCapacityX(x)) WhenCapacityXIsAdequate(x, y, rect);
            else WhenNeedExpandCapacityX(x, y, rect);
        }

        protected virtual bool IsNeedExpandCapacityX(float x)
        {
            return capacity.x >= x + Padding[0] + Padding[2];
        }

        protected virtual void WhenNeedExpandCapacityX(float x, float y, RectTransform rect)
        {
            SetRect(new Vector2(x + Padding[0] + Padding[2], size.y));
            RefreshAllChild();
            RefreshWithNeedSpace(x, y, rect);
        }

        private void WhenCapacityXIsAdequate(float x, float y, RectTransform rect)
        {
            float MaxX = size.x + x + Padding[0] + Padding[2], MaxY = size.y + y + Padding[1] + Padding[3];
            if (capacity.x >= MaxX)
                WhenCapacityXEnoughToPushNewChildOnThisLine(x, y, rect, MaxY);
            else
                WhenCapacityXNotEnoughToPushNewChildOnThisLine(x, y, rect, MaxY);
        }

        protected virtual void WhenCapacityXNotEnoughToPushNewChildOnThisLine(float x, float y, RectTransform rect, float MaxY)
        { 
            size = new Vector2(x, size.y + MaxHightInThisLine);
            rect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, Padding[0], rect.sizeDelta.x);
            rect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, size.y + Padding[1], rect.sizeDelta.y);
            MaxHightInThisLine = y;
            if (capacity.y <= size.y + MaxHightInThisLine + Padding[1] + Padding[3])
                SetRect(new Vector2(capacity.x, size.y + MaxHightInThisLine + Padding[1] + Padding[3]));
        }

        protected virtual void WhenCapacityXEnoughToPushNewChildOnThisLine(float x,float y, RectTransform rect, float MaxY)
        {
            rect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, size.x + Padding[0], rect.sizeDelta.x);
            rect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, size.y + Padding[1], rect.sizeDelta.y);
            size = new Vector2(size.x + x, size.y);
            if (y > MaxHightInThisLine) MaxHightInThisLine = y;
            if (capacity.y <= MaxY)
                SetRect(new Vector2(capacity.x, MaxY));
        }

        private void RefreshWithNeedSpace(RectTransform rect)
        {
            if (!isCanRefresh) return;
            float x = rect.sizeDelta.x, y = rect.sizeDelta.y;
            RefreshWithNeedSpace(x, y, rect);
        }

        #endregion

        #region Generate

        public T SetADUIOnWindow<T>(string keyName, ADUI item) where T : ADUI, new()
        {
            return SetADUIOnWindow<T>(keyName, item, item.GetComponent<RectTransform>().sizeDelta);
        }

        public bool SetItemOnWindow(string keyName, GameObject item)
        {
            return SetItemOnWindow(keyName, item, item.GetComponent<RectTransform>().sizeDelta);
        }

        public T SetADUIOnWindow<T>(string keyName, ADUI item, Vector2 Rect) where T : ADUI, new()
        {
            if (Childs.ContainsKey(keyName)) return null; 
            if (SetItemOnWindow(keyName, item.gameObject, Rect))
                return item.GetComponent<T>();
            else return null;
        }

        public bool SetItemOnWindow(string keyName, GameObject prefab, Vector2 Rect)
        {
            if (Childs.ContainsKey(keyName) || !prefab.TryGetComponent<RectTransform>(out var target)) return false;
            target.SetParent(SubPage, false);
            target.sizeDelta = Rect;
            Childs.Add(keyName, target);
            RefreshWithNeedSpace(target);
            return true;
        }

        public CustomWindowElement GenerateSubWindow(string keyName, Vector2 rect,string title)
        { 
            var subWindow = GameObject.Instantiate(gameObject).GetComponent<CustomWindowElement>();
            if (SetItemOnWindow(keyName, subWindow.gameObject, rect))
                return subWindow.SetTitle(title);
            else return null;
        }

        #region Button

        public AD.UI.Button GenerateButton(string keyName)
        {
            return SetADUIOnWindow<AD.UI.Button>(keyName, AD.UI.Button.Generate(keyName)).SetTitle(keyName);
        }

        public AD.UI.Button GenerateButton(string keyName, Vector2 rect)
        {
            return SetADUIOnWindow<AD.UI.Button>(keyName, AD.UI.Button.Generate(keyName), rect).SetTitle(keyName);
        }

        #endregion

        #region Slider

        public AD.UI.Slider GenerateSlider(string keyName)
        {
            return SetADUIOnWindow<AD.UI.Slider>(keyName, AD.UI.Slider.Generate(keyName));
        }

        public AD.UI.Slider GenerateSlider(string keyName, Vector2 rect)
        {
            return SetADUIOnWindow<AD.UI.Slider>(keyName, AD.UI.Slider.Generate(keyName), rect);
        }

        #endregion

        #region Text

        public AD.UI.Text GenerateText(string keyName)
        {
            return SetADUIOnWindow<AD.UI.Text>(keyName, AD.UI.Text.Generate(keyName));
        }

        public AD.UI.Text GenerateText(string keyName, string defaultText)
        {
            return SetADUIOnWindow<AD.UI.Text>(keyName, AD.UI.Text.Generate(keyName, defaultText));
        }

        public AD.UI.Text GenerateText(string keyName, Vector2 rect)
        {
            return SetADUIOnWindow<AD.UI.Text>(keyName, AD.UI.Text.Generate(keyName), rect);
        }

        public AD.UI.Text GenerateText(string keyName, string defaultText, Vector2 rect)
        {
            return SetADUIOnWindow<AD.UI.Text>(keyName, AD.UI.Text.Generate(keyName, defaultText), rect);
        }

        #endregion

        #region Toggle

        public AD.UI.Toggle GenerateToggle(string keyName)
        {
            return SetADUIOnWindow<AD.UI.Toggle>(keyName, AD.UI.Toggle.Generate(keyName));
        }

        public AD.UI.Toggle GenerateToggle(string keyName, string defaultText)
        {
            return SetADUIOnWindow<AD.UI.Toggle>(keyName, AD.UI.Toggle.Generate(keyName)).SetTitle(defaultText);
        }

        public AD.UI.Toggle GenerateToggle(string keyName, Vector2 rect)
        {
            return SetADUIOnWindow<AD.UI.Toggle>(keyName, AD.UI.Toggle.Generate(keyName), rect);
        }

        public AD.UI.Toggle GenerateToggle(string keyName, string defaultText, Vector2 rect)
        {
            return SetADUIOnWindow<AD.UI.Toggle>(keyName, AD.UI.Toggle.Generate(keyName), rect).SetTitle(defaultText);
        }

        #endregion

        #region InputField

        public AD.UI.InputField GenerateInputField(string keyName)
        {
            return SetADUIOnWindow<AD.UI.InputField>(keyName, AD.UI.InputField.Generate(keyName));
        }

        public AD.UI.InputField GenerateInputField(string keyName, Vector2 Rect)
        {
            return SetADUIOnWindow<AD.UI.InputField>(keyName, AD.UI.InputField.Generate(keyName), Rect);
        }

        #endregion

        #region RawImage

        public AD.UI.RawImage GenerateRawImage(string keyName)
        {
            return SetADUIOnWindow<AD.UI.RawImage>(keyName, AD.UI.RawImage.Generate(keyName));
        }

        public AD.UI.RawImage GenerateRawImage(string keyName, Vector2 Rect)
        {
            return SetADUIOnWindow<AD.UI.RawImage>(keyName, AD.UI.RawImage.Generate(keyName), Rect);
        }

        #endregion

        #endregion

        #region Drag

        public DragBehaviour _DragBehaviour;

        #endregion

    }
}
