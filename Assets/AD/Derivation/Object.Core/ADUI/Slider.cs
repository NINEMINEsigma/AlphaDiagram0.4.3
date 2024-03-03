using System;
using AD.BASE;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AD.UI
{
    [Serializable]
    [AddComponentMenu("UI/AD/Slider", 100)]
    public class Slider : ADUI, IPointerEnterHandler, IPointerExitHandler
    {
        #region Attribute

        public override bool IsNeedContext => false;

        private UnityEngine.UI.Slider _source = null;
        public UnityEngine.UI.Slider source
        {
            get
            {
                _source ??= GetComponent<UnityEngine.UI.Slider>();
                return _source;
            }
        }

        [SerializeField] private UnityEngine.UI.Image background = null;
        [SerializeField] private UnityEngine.UI.Image handle = null;
        [SerializeField] private UnityEngine.UI.Image fill = null;

        public float value { get { return transformer(source.value); } }
        public SliderProperty ValueProperty { get; private set; }

        public Sprite backgroundView 
        { 
            get { if (background == null) return null; else return background.sprite; }
            set { if (background != null) background.sprite = value; } 
        }
        public Sprite handleView
        {
            get { if (handle == null) return null; else return handle.sprite; }
            set { if (handle != null) handle.sprite = value; }
        }
        public Sprite fillView
        {
            get { if (fill == null) return null; else return fill.sprite; }
            set { if (fill != null) fill.sprite = value; }
        }

        public delegate float Transformer(float value);
        public Transformer transformer = (T) => { return T; };

        #endregion

        public Slider()
        {
            ElementArea = "Slider";
            ValueProperty = new(this);
        }

        protected void Start()
        {
            AD.UI.ADUI.Initialize(this); 
        }
        protected void OnDestory()
        {
            AD.UI.ADUI.DestroyADUI(this);
        }

        #region Function 

        public static AD.UI.Slider Generate(string name = "New Slider", Transform parent = null)
        {
            AD.UI.Slider slider = GameObject.Instantiate(ADGlobalSystem.instance._Slider);
            slider.transform.SetParent(parent, false);
            slider.transform.localPosition = Vector3.zero;
            slider.name = name;
            return slider;
        }

        public static AD.UI.Slider Generate(Transform parent = null)
        {
            return Generate("New Slider", parent);
        }

        public Slider AddListener(UnityEngine.Events.UnityAction<float> call)
        {
            source.onValueChanged.AddListener(call);
            return this;
        }

        #endregion

    }

    public class BindSliderAsset : PropertyAsset<float>
    {
        public BindSliderAsset() { }
        public BindSliderAsset(Slider source)
        {
            this.source = source;
        }

        Slider source;

        public override float value { get => source.value; set => source.source.value = value; }
    }

    public class SliderProperty : AD.BASE.BindProperty<float>
    {
        public SliderProperty(Slider source)
        {
            SetPropertyAsset(new BindSliderAsset(source));
        }
    }
}