using System.Collections;
using AD.BASE;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace AD.UI
{
    public class Ripple : MonoBehaviour
    {
        public bool unscaledTime = false;
        public float speed;
        public float maxSize;
        public Color startColor;
        public Color transitionColor;
        UnityEngine.UI.Image colorImg;

        void Start()
        {
            transform.localScale = new Vector3(0f, 0f, 0f);
            colorImg = GetComponent<UnityEngine.UI.Image>();
            colorImg.color = new Color(startColor.r, startColor.g, startColor.b, startColor.a);
        }

        void Update()
        {
            if (unscaledTime == false)
            {
                transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(maxSize, maxSize, maxSize), Time.deltaTime * speed);
                colorImg.color = Color.Lerp(colorImg.color, new Color(transitionColor.r, transitionColor.g, transitionColor.b, transitionColor.a), Time.deltaTime * speed);

                if (transform.localScale.x >= maxSize * 0.998)
                {
                    if (transform.parent.childCount == 1)
                        transform.parent.gameObject.SetActive(false);

                    Destroy(gameObject);
                }
            }

            else
            {
                transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(maxSize, maxSize, maxSize), Time.unscaledDeltaTime * speed);
                colorImg.color = Color.Lerp(colorImg.color, new Color(transitionColor.r, transitionColor.g, transitionColor.b, transitionColor.a), Time.unscaledDeltaTime * speed);

                if (transform.localScale.x >= maxSize * 0.998)
                {
                    if (transform.parent.childCount == 1)
                        transform.parent.gameObject.SetActive(false);

                    Destroy(gameObject);
                }
            }
        }
    }

    public class ModernUIButton : PropertyModule, IButton
    {
        //[Header("Context")]
        public string buttonText = "Button";
        public ADEvent clickEvent;
        public ADEvent hoverEvent;
        public AudioClip hoverSound;
        public AudioClip clickSound;
        [SerializeField] private UnityEngine.UI.Button _buttonVar;
        public UnityEngine.UI.Button buttonVar
        {
            get
            {
                if (_buttonVar == null) _buttonVar = GetComponent<UnityEngine.UI.Button>();
                return _buttonVar;
            }
        }
        //[Header("Resources")]
        public TextMeshProUGUI normalText;
        public TextMeshProUGUI highlightedText;
        public AudioSource soundSource;
        public GameObject rippleParent;
        //[Header("Settings")]
        public AnimationSolution animationSolution = AnimationSolution.SCRIPT;
        [Range(0.25f, 15)] public float fadingMultiplier = 8;
        public bool useCustomContent = false;
        public bool enableButtonSounds = false;
        public bool useHoverSound = true;
        public bool useClickSound = true;
        public bool useRipple = true;
        //[Header("Ripple")]
        public RippleUpdateMode rippleUpdateMode = RippleUpdateMode.UNSCALED_TIME;
        public Sprite rippleShape;
        [Range(0.1f, 5)] public float speed = 1f;
        [Range(0.5f, 25)] public float maxSize = 4f;
        public Color startColor = new Color(1f, 1f, 1f, 1f);
        public Color transitionColor = new Color(1f, 1f, 1f, 1f);
        public bool renderOnTop = false;
        public bool centered = false;
        bool isPointerOn;
        //[Header("Others")]
        CanvasGroup normalCG;
        CanvasGroup highlightedCG;
        float currentNormalValue;
        float currenthighlightedValue;

        public ModernUIButton()
        {
            ElementArea = "ModernUIButton";
        }

        protected override void Start()
        {
            base.Start();

            if (animationSolution == AnimationSolution.SCRIPT)
            {
                normalCG = transform.Find("Normal").GetComponent<CanvasGroup>();
                highlightedCG = transform.Find("Highlighted").GetComponent<CanvasGroup>();

                Animator tempAnimator = this.GetComponent<Animator>();
                Destroy(tempAnimator);
            }

            buttonVar.onClick.AddListener(delegate { clickEvent.Invoke(); });

            if (enableButtonSounds == true && useClickSound == true)
                buttonVar.onClick.AddListener(delegate { soundSource.PlayOneShot(clickSound); });

            if (useCustomContent == false)
                UpdateUI();

            if (useRipple == true && rippleParent != null)
                rippleParent.SetActive(false);
            else if (useRipple == false && rippleParent != null)
                Destroy(rippleParent);
        }

        public override void InitializeContext()
        {
            base.InitializeContext();
            Context.OnPointerDownEvent = InitializeContextSingleEvent(Context.OnPointerClickEvent, OnPointerDown);
            Context.OnPointerEnterEvent = InitializeContextSingleEvent(Context.OnPointerEnterEvent, OnPointerEnter);
            Context.OnPointerExitEvent = InitializeContextSingleEvent(Context.OnPointerExitEvent, OnPointerExit);
        }

        public ModernUIButton SetTitle(string title)
        {
            buttonText=title;
            UpdateUI();
            return this;
        }

        IButton IButton.SetTitle(string title)
        {
            buttonText = title;
            UpdateUI();
            return this;
        }

        public enum AnimationSolution
        {
            ANIMATOR,
            SCRIPT
        }

        public enum RippleUpdateMode
        {
            NORMAL,
            UNSCALED_TIME
        }

        void OnEnable()
        {
            if (normalCG == null && highlightedCG == null)
                return;

            normalCG.alpha = 1;
            highlightedCG.alpha = 0;
        }

        public void UpdateUI()
        {
            normalText.text = buttonText;
            highlightedText.text = buttonText;
        }

        public void CreateRipple(Vector2 pos)
        {
            if (rippleParent != null)
            {
                GameObject rippleObj = new GameObject();
                rippleObj.AddComponent<UnityEngine.UI.Image>();
                rippleObj.GetComponent<UnityEngine.UI.Image>().sprite = rippleShape;
                rippleObj.name = "Ripple";
                rippleParent.SetActive(true);
                rippleObj.transform.SetParent(rippleParent.transform);

                if (renderOnTop == true)
                    rippleParent.transform.SetAsLastSibling();
                else
                    rippleParent.transform.SetAsFirstSibling();

                if (centered == true)
                    rippleObj.transform.localPosition = new Vector2(0f, 0f);
                else
                    rippleObj.transform.position = pos;

                rippleObj.AddComponent<Ripple>();
                Ripple tempRipple = rippleObj.GetComponent<Ripple>();
                tempRipple.speed = speed;
                tempRipple.maxSize = maxSize;
                tempRipple.startColor = startColor;
                tempRipple.transitionColor = transitionColor;

                if (rippleUpdateMode == RippleUpdateMode.NORMAL)
                    tempRipple.unscaledTime = false;
                else
                    tempRipple.unscaledTime = true;
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (useRipple == true && isPointerOn == true)
#if ENABLE_LEGACY_INPUT_MANAGER
                CreateRipple(Input.mousePosition);
#elif ENABLE_INPUT_SYSTEM && ENABLE_LEGACY_INPUT_MANAGER
                CreateRipple(Input.mousePosition);
#elif ENABLE_INPUT_SYSTEM
                CreateRipple(Mouse.current.position.ReadValue());
#endif
            else if (useRipple == false)
                this.enabled = false;
        }

        public override void OnPointerEnter(PointerEventData eventData)
        { 
            base.OnPointerEnter(eventData);

            if (enableButtonSounds == true && useHoverSound == true && buttonVar.interactable == true)
                soundSource.PlayOneShot(hoverSound);

            hoverEvent.Invoke();
            isPointerOn = true;

            if (animationSolution == AnimationSolution.SCRIPT && buttonVar.interactable == true)
                StartCoroutine(nameof(FadeIn));
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);

            isPointerOn = false;

            if (animationSolution == AnimationSolution.SCRIPT && buttonVar.interactable == true)
                StartCoroutine(nameof(FadeOut));
        }

        IEnumerator FadeIn()
        {
            StopCoroutine("FadeOut");
            currentNormalValue = normalCG.alpha;
            currenthighlightedValue = highlightedCG.alpha;

            while (currenthighlightedValue <= 1)
            {
                currentNormalValue -= Time.deltaTime * fadingMultiplier;
                normalCG.alpha = currentNormalValue;

                currenthighlightedValue += Time.deltaTime * fadingMultiplier;
                highlightedCG.alpha = currenthighlightedValue;

                if (normalCG.alpha >= 1)
                    StopCoroutine("FadeIn");

                yield return null;
            }
        }

        IEnumerator FadeOut()
        {
            StopCoroutine("FadeIn");
            currentNormalValue = normalCG.alpha;
            currenthighlightedValue = highlightedCG.alpha;

            while (currentNormalValue >= 0)
            {
                currentNormalValue += Time.deltaTime * fadingMultiplier;
                normalCG.alpha = currentNormalValue;

                currenthighlightedValue -= Time.deltaTime * fadingMultiplier;
                highlightedCG.alpha = currenthighlightedValue;

                if (highlightedCG.alpha <= 0)
                    StopCoroutine("FadeOut");

                yield return null;
            }
        }

        public IButton AddListener(UnityAction action)
        {
            clickEvent.AddListener(action);
            return this;
        }

        public IButton RemoveListener(UnityAction action)
        {
            clickEvent.RemoveListener(action);
            return this;
        }

        public IButton RemoveAllListeners()
        {
            clickEvent.RemoveAllListeners();
            return this;
        }
    }
}
