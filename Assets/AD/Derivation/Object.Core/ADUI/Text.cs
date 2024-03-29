using System;
using AD.BASE;
using TMPro;
using UnityEngine;

namespace AD.UI
{
    [Serializable]
    [AddComponentMenu("UI/AD/Text", 100)]
    public class Text : ADUI
    {
        public Text()
        {
            ElementArea = "Text";
        }

        protected void Start()
        {
            AD.UI.ADUI.Initialize(this);
            TextProperty = new(this);
            ValueProperty = new(this);
        }
        protected void OnDestory()
        {
            AD.UI.ADUI.DestroyADUI(this);
        }

        private TextMeshProUGUI _source = null;
        public TextMeshProUGUI source
        {
            get
            {
                if (_source == null)
                    _source = GetComponent<TextMeshProUGUI>();
                return _source;
            }
        }
        public string text { get { return GetText(); } set { SetText(value); } }

        public static AD.UI.Text Generate(string name = "New Text", string defaultText = "", Transform parent = null)
        {
            AD.UI.Text text = GameObject.Instantiate(ADGlobalSystem.instance._Text);
            text.name = name;
            text.transform.SetParent(parent, false);
            text.transform.localPosition = Vector3.zero;
            text.SetText(defaultText);
            return text;
        }

        public TextProperty TextProperty { get; private set; }
        public TextValueProperty ValueProperty { get; private set; }

        public Text SetText(string text)
        {
            _SetText(text);
            _m_Property?.RemoveListenerOnSet(_SetText);
            _m_Property = null;
            return this;
        }

        private void _SetText(string text)
        {
            source.text = text;
        }

        private string GetText()
        {
            return source.text;
        }

        public void Bind(AD.BASE.BindProperty<string> property)
        {
            if (property != null)
                property.AddListenerOnSet(_SetText);
            else
                _m_Property.RemoveListenerOnSet(_SetText);
            _m_Property = property;
        }

        private AD.BASE.BindProperty<string> _m_Property = null;

    }

    public class BindTextAsset : PropertyAsset<string>
    {
        public BindTextAsset() { }
        public BindTextAsset(Text source)
        {
            this.source = source;
        }

        Text source;

        public override string value { get => source.text; set => source.text = value; }
    }

    public class BindTextValueAsset : PropertyAsset<float>
    {
        public BindTextValueAsset() { }
        public BindTextValueAsset(Text source)
        {
            this.source = source;
        }

        Text source;

        public override float value
        {
            get
            {
                if (float.TryParse(source.text, out var result))
                    return result;
                else
                {
                    Debug.LogWarning("{" + source.text + "} isnot a value");
                    return 0;
                }
            }
            set
            {
                source.text = value.ToString();
            }
        }
    }

    public class TextProperty : AD.BASE.BindProperty<string, BindTextAsset>
    {
        public TextProperty(Text source)
        {
            SetPropertyAsset(new BindTextAsset(source));
        }
    }

    public class TextValueProperty : AD.BASE.BindProperty<float, BindTextValueAsset>
    {
        public TextValueProperty(Text source)
        {
            SetPropertyAsset(new BindTextValueAsset(source));
        }
    }

}
