using AD.BASE;
using UnityEngine;

namespace AD.UI
{
    public class ModernUIInputField : PropertyModule
    {
        public ModernUIInputField()
        {
            this.ElementArea = nameof(ModernUIInputField);
        }

        [Header("Resources")]
        public AD.UI.Text Title;
        [SerializeField]private AD.UI.InputField _Source;
        public AD.UI.InputField Source
        {
            get
            {
                if (_Source == null) GetComponent<AD.UI.InputField>();
                return _Source;
            }
        }
        public AD.UI.ViewController Icon;
        [SerializeField]private Animator _inputFieldAnimator;
        public Animator inputFieldAnimator
        {
            get
            {
                if (_inputFieldAnimator == null) GetComponent<Animator>();
                return _inputFieldAnimator;
            }
        } 

        // Hidden variables
        private readonly string inAnim = "In";
        private readonly string outAnim = "Out";

        protected override void Start()
        {
            base.Start();

            Source.source.onSelect.AddListener(delegate { AnimateIn(); });
            Source.source.onEndEdit.AddListener(delegate { AnimateOut(); });
            UpdateState();
        }

        public ModernUIInputField SetTitle(string title)
        {
            Title.SetText(title);
            return this;
        }

        void OnEnable()
        {
            if (Source == null)
                return;

            Source.source.ForceLabelUpdate();
            UpdateState();
        }

        public void AnimateIn()
        {
            inputFieldAnimator.Play(inAnim);
        }

        public void AnimateOut()
        {
            if (Source.text.Length == 0)
                inputFieldAnimator.Play(outAnim);
        }

        public void UpdateState()
        {
            if (Source.text.Length == 0)
                AnimateOut();
            else
                AnimateIn();
        }

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
                return new InputFieldValueProperty(this.Source);
            }
        }
        public InputFieldProperty TextProperty { get; private set; }
        public string text
        {
            get { return Source.text; }
            set { Source.text = value; }
        }

        public ModernUIInputField SetText(string text)
        {
            this.text = text;
            return this;
        }

        public enum PressType
        {
            OnSelect,
            OnEnd
        }

        public void AddListener(UnityEngine.Events.UnityAction<string> action, PressType type = PressType.OnEnd)
        {
            if (type == PressType.OnSelect) Source.source.onSelect.AddListener(action);
            else if (type == PressType.OnEnd) Source.source.onEndEdit.AddListener(action);
            else AD.ADGlobalSystem.AddMessage("You try to add worry listener");
        }

        public void RemoveListener(UnityEngine.Events.UnityAction<string> action, PressType type = PressType.OnEnd)
        {
            if (type == PressType.OnSelect) Source.source.onSelect.RemoveListener(action);
            else if (type == PressType.OnEnd) Source.source.onEndEdit.RemoveListener(action);
            else AD.ADGlobalSystem.AddMessage("You try to remove worry listener");
        }

        public void RemoveAllListener(PressType type = PressType.OnEnd)
        {
            if (type == PressType.OnSelect) Source.source.onSelect.RemoveAllListeners();
            else if (type == PressType.OnEnd) Source.source.onEndEdit.RemoveAllListeners();
        }

        public ModernUIInputField SetIcon(ImagePair pair)
        {
            Icon.CurrentImagePair = pair;
            Icon.Refresh();
            return this;
        }

        public ModernUIInputField SetIcon(Sprite image)
        {
            Icon.CurrentImage = image;
            Icon.Refresh();
            return this;
        }

    } 
}
