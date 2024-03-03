using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using AD.BASE;
using TMPro;
using AD.Utility;
using System;

namespace AD.UI
{
    public class ColorManager : ADUI
    {
        [SerializeField] private ColorRGB CRGB;
        [SerializeField] private ColorPanel CP;
        [SerializeField] private ColorCircle CC;

        public UnityEngine.UI.Slider sliderCRGB;
        public UnityEngine.UI.Slider sliderCA;
        public Image colorShow;

        public TMP_Text title;

        public AD.UI.InputField RGBAText;

        private BindProperty<Color> _ColorProperty = new();
        public BindProperty<Color> ColorProperty
        {
            get => _ColorProperty;
            set
            {
                _ColorProperty = value;
                if (value == null)
                {
                    InitColorPropertyAndShow(Color.white);
                    return;
                }
                InitColorPropertyAndShow(value.Get());
                CP.SetColorPanel(value.Get());
                sliderCA.value = value.Get().a;
            }
        }
        public Color ColorValue
        {
            get => _ColorProperty.Get();
            set
            {
                _ColorProperty.Set(value);
                InitColorPropertyAndShow(value);
                CP.SetColorPanel(value);
                sliderCA.value = value.a;
            }
        }

        void OnDisable()
        {
            CC.getPos -= CC_getPos;
        }

        private void CC_getPos(Vector2 pos)
        {
            Color getColor = CP.GetColorByPosition(pos);
            getColor.a = sliderCA == null ? 1 : sliderCA.value;
            InitColorPropertyAndShow(getColor);
        }

        private void InitColorPropertyAndShow(Color getColor)
        {
            colorShow.color = getColor;
            ColorProperty?.Set(getColor);
            string R = Convert.ToString((int)(256 * getColor.r), 16);
            string G = Convert.ToString((int)(256 * getColor.g), 16);
            string B = Convert.ToString((int)(256 * getColor.b), 16);
            string A = Convert.ToString((int)(256 * getColor.a), 16);
            if (R == "100") R = "ff";
            if (G == "100") G = "ff";
            if (B == "100") B = "ff";
            if (A == "100") A = "ff";
            if (RGBAText != null) RGBAText.SetText(R + G + B + A);
        }

        void Start()
        {
            sliderCRGB.onValueChanged.AddListener(OnCRGBValueChanged);
            sliderCA.onValueChanged.AddListener(OnCAValueChanged);

            CC.getPos += CC_getPos;

            RGBAText?.AddListener(T =>
            {
                string R = T[0..2], G = T[2..4], B = T[4..6], A = T[6..];
                float r = Convert.ToInt32(R, 16) / 256.0f;
                float g = Convert.ToInt32(G, 16) / 256.0f;
                float b = Convert.ToInt32(B, 16) / 256.0f;
                float a = Convert.ToInt32(A, 16) / 256.0f;
                InitColorPropertyAndShow(new Color(r, g, b, a));
            });

            ADUI.Initialize(this);
        }

        private void OnDestroy()
        {
            ADUI.DestroyADUI(this);
        }

        public ColorManager()
        {
            ElementArea = "ColorManager";
        }

        void OnCRGBValueChanged(float value)
        {
            Color endColor = CRGB.GetColorBySliderValue(value);
            CP.SetColorPanel(endColor);
            CC.setShowColor();
        }

        void OnCAValueChanged(float value)
        {
            CC.setShowColor();
        }
    }
}
