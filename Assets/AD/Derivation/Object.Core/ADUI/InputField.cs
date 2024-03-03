using System;
using AD.BASE;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace AD.UI
{
    [Serializable, RequireComponent(typeof(TMP_InputField))]
    [AddComponentMenu("UI/AD/InputField", 100)]
    public class InputField : AD.UI.ADUI, IInputField
    {
        public override bool IsNeedContext => false;

        public InputField()
        {
            ElementArea = "InputField";
        }

        protected void Start()
        {
            AD.UI.ADUI.Initialize(this);
            TextProperty = new(this);
        }

        protected void OnDestroy()
        {
            AD.UI.ADUI.DestroyADUI(this);
        }

        public static AD.UI.InputField Generate(string name, string defaultText = "", string placeholderText = "Enter Text", Transform parent = null)
        {
            AD.UI.InputField inputField = GameObject.Instantiate(ADGlobalSystem.instance._InputField);
            inputField.name = name;
            inputField.transform.SetParent(parent, false);
            inputField.transform.localPosition = Vector3.zero;
            inputField.SetText(defaultText);
            inputField.SetPlaceholderText(placeholderText);
            return inputField;
        }

        private TMP_InputField _source;
        public TMP_InputField source
        {
            get
            {
                if (_source == null) _source = GetComponent<TMP_InputField>();
                return _source;
            }
        }

        public TMP_Text Placeholder;

        public BindPropertyJustSet<string, BindInputFieldAsset> Input
        {
            get
            {
                return TextProperty.BindJustSet();
            }
        }
        public BindPropertyJustGet<string, BindInputFieldAsset> Output
        {
            get
            {
                return TextProperty.BindJustGet();
            }
        }
        public InputFieldValueProperty ValueProperty
        {
            get
            {
                return new InputFieldValueProperty(this);
            }
        }
        public InputFieldProperty TextProperty { get; private set; }
        public string text
        {
            get { return source.text; }
            set { source.text = value; }
        }

        private BindProperty<string> _m_Property = null;

        public void Bind(BindProperty<string> property)
        {
            source.onEndEdit.RemoveListener(WhenTextChange);
            if (_m_Property != null)
            {
                source.onEndEdit.RemoveListener(WhenTextChange);
                source.onEndEdit.AddListener(WhenTextChange);
                _m_Property.RemoveListenerOnSet(WhenBindSet);
                property.AddListenerOnSet(WhenBindSet);
            }
            _m_Property = property;
            source.text = _m_Property.Get();
        }

        private void WhenBindSet(string text)
        {
            source.text = text;
        }

        private void WhenTextChange(string text)
        {
            _m_Property.Set(text);
        }

        public IInputField SetText(string text)
        {
            this.text = text;
            return this;
        }

        public IInputField SetTextWithoutNotify(string text)
        {
            this.source.SetTextWithoutNotify(text);
            return this;
        }

        public void AddListener(UnityEngine.Events.UnityAction<string> action, PressType type = PressType.OnEnd)
        {
            if (type == PressType.OnSelect) source.onSelect.AddListener(action);
            else if (type == PressType.OnEnd) source.onEndEdit.AddListener(action);
            else AD.ADGlobalSystem.AddMessage("You try to add worry listener");
        }

        public void RemoveListener(UnityEngine.Events.UnityAction<string> action, PressType type = PressType.OnEnd)
        {
            if (type == PressType.OnSelect) source.onSelect.RemoveListener(action);
            else if (type == PressType.OnEnd) source.onEndEdit.RemoveListener(action);
            else AD.ADGlobalSystem.AddMessage("You try to remove worry listener");
        }

        public void RemoveAllListener(PressType type = PressType.OnEnd)
        {
            if (type == PressType.OnSelect) source.onSelect.RemoveAllListeners();
            else if (type == PressType.OnEnd) source.onEndEdit.RemoveAllListeners();
        }

        public void SetPlaceholderText(string text)
        {
            Placeholder.text = text;
        }

    }

    public class BindInputFieldAsset : PropertyAsset<string>
    {
        public BindInputFieldAsset() { }
        public BindInputFieldAsset(InputField source)
        {
            this.source = source;
        }

        InputField source;

        public override string value { get => source.text; set => source.text = value; }
    }

    public class BindInputFieldValueAsset : PropertyAsset<float>
    {
        public BindInputFieldValueAsset() { }
        public BindInputFieldValueAsset(InputField source)
        {
            this.source = source;
        }

        InputField source;

        public override float value { get => float.Parse(source.text); set => source.text = value.ToString(); }
    }

    public class InputFieldProperty : AD.BASE.BindProperty<string, BindInputFieldAsset>
    {
        public InputFieldProperty(InputField source)
        {
            SetPropertyAsset(new BindInputFieldAsset(source));
        }
    }

    public class InputFieldValueProperty : AD.BASE.BindProperty<float, BindInputFieldValueAsset>
    {
        public InputFieldValueProperty(InputField source)
        {
            SetPropertyAsset(new BindInputFieldValueAsset(source));
        }
    }
}
