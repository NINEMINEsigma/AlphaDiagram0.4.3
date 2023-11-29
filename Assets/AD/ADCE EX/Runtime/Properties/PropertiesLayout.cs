using System.Collections.Generic;
using System.Linq;
using AD.BASE;
using AD.UI;
using AD.Utility;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
using static AD.Experimental.GameEditor.GUIContent;

namespace AD.Experimental.GameEditor
{
    public class GUI
    {
        public static GUISkin skin;
    }

    public class GUIContent
    {
        public GameObject RootObject;
        public IADUI TargetItem;
        public GUIContentType ContentType = GUIContentType.Default;
        public int ExtensionalSpaceLine = 0;
        public string Message;

        public enum GUIContentType
        {
            Space,
            Default
        }

        public GUIContent(GameObject root, IADUI targetItem, GUIContentType contentType = GUIContentType.Default, int extensionalSpaceLine = 0, string message = "")
        {
            RootObject = root;
            TargetItem = targetItem;
            ContentType = contentType;
            ExtensionalSpaceLine = extensionalSpaceLine;
            Message = message;
        }
    }

    public static class PropertiesLayout
    {
        private static ISerializePropertiesEditor _CurrentEditorThat;
        public static ISerializePropertiesEditor CurrentEditorThat
        {
            get
            {
                return _CurrentEditorThat;
            }
            private set
            {
                _CurrentEditorThat = value;
            }
        }

        private static List<List<GUIContent>> GUILayoutLineList = new();
        private static bool IsNeedMulLine = true;
        private static void DoGUILine(GUIContent content)
        {
            if (!IsNeedMulLine)
            {
                GUILayoutLineList[^1].Add(content);
            }
            else
            {
                GUILayoutLineList.Add(new() { content });
            }
        }
        public static IADUI GUIField(string text, string style, string message = "")
        {
            GameObject root = ObtainGUIStyleInstance(style, out IADUI targeADUI);
            int extensionalSpaceLine = ((int)root.transform.As<RectTransform>().sizeDelta.y / (int)PropertiesItem.DefaultRectHightLevelSize) - 1;
            GUIContent content = new(root, targeADUI, GUIContentType.Default, extensionalSpaceLine, message);
            if (extensionalSpaceLine > 0)
            {
                EndHorizontal();
            }
            BeginHorizontal();
            GameObject labelRoot = GUI.skin.FindStyle("Label(UI)").Prefab.PrefabInstantiate();
            DoGUILine(new GUIContent(labelRoot, labelRoot.GetComponentInChildren<AD.UI.Text>().SetText(text.Translate()), GUIContentType.Default, 0, message));
            if (extensionalSpaceLine > 0)
            {
                EndHorizontal();
            }
            DoGUILine(content);
            EndHorizontal();
            return targeADUI;
        }
        public static IADUI GUIField(string style, string message = "")
        {
            GameObject root = ObtainGUIStyleInstance(style, out IADUI targeADUI);
            int extensionalSpaceLine = ((int)root.transform.As<RectTransform>().sizeDelta.y / (int)PropertiesItem.DefaultRectHightLevelSize) - 1;
            GUIContent content = new(root, targeADUI, GUIContentType.Default, extensionalSpaceLine, message);
            DoGUILine(content);
            return targeADUI;
        }
        public static void Space(int line)
        {
            GUIContent content = new(null, null, GUIContentType.Space, line, "");
            DoGUILine(content);
        }
        private static GameObject ObtainGUIStyleInstance(string style, out IADUI targetUI)
        {
            GUIStyle targetStyle = ADGlobalSystem.FinalCheckWithThrow(GUI.skin.FindStyle(style), "cannt find this GUIStyle");
            GameObject cat = targetStyle.Prefab.PrefabInstantiate();
            var targetADUIs = cat.GetComponents<IADUI>();
            if (targetADUIs == null || targetADUIs.Length == 0) targetADUIs = cat.GetComponentsInChildren<IADUI>();
            if (targetADUIs == null || targetADUIs.Length == 0)
            {
                GameEditorApp.instance.AddMessage("PropertiesLayout.ObtainGUIStyleInstance Error");
                targetUI = null;
                return null;
            }
            targetUI = targetADUIs.FirstOrDefault(T => T.GetType().Name == targetStyle.TypeName);
            return cat;
        }

        public static void SetUpPropertiesLayout(ISerializePropertiesEditor target)
        {
            CurrentEditorThat = ADGlobalSystem.FinalCheck(target);
            CurrentEditorThat.MatchItem.Init();
            IsNeedMulLine = true;
        }

        public static void ApplyPropertiesLayout()
        {
            foreach (var line in GUILayoutLineList)
            {
                var rect = CurrentEditorThat.MatchItem.AddNewLevelLine(true, 1);
                rect.GetComponent<AreaDetecter>().Message = line[0].Message;
                int LineItemCount = line.Count;
                int extensionalSpaceLine = 0;
                foreach (var content in line)
                {
                    switch (content.ContentType)
                    {
                        case GUIContent.GUIContentType.Space:
                            {
                                CurrentEditorThat.MatchItem.AddNewLevelLine(false, content.ExtensionalSpaceLine);
                            }
                            break;
                        default:
                            {
                                if (content.ExtensionalSpaceLine > extensionalSpaceLine) extensionalSpaceLine = content.ExtensionalSpaceLine;
                                content.RootObject.transform.SetParent(rect, false);
                                var contentRect = content.RootObject.transform.As<UnityEngine.RectTransform>();
                                contentRect.sizeDelta = new UnityEngine.Vector2(rect.sizeDelta.x / (float)LineItemCount, contentRect.sizeDelta.y);
                            }
                            break;
                    }
                }
                if (extensionalSpaceLine > 0)
                    CurrentEditorThat.MatchItem.AddNewLevelLine(false, extensionalSpaceLine);
            }
            CurrentEditorThat = null;
            GUILayoutLineList.Clear();
            EndHorizontal();
        }

        //public static T GenerateElement<T>() where T : ADUI
        //{
        //    T element = ADGlobalSystem.FinalCheck<T>(ADGlobalSystem.GenerateElement<T>(), "On PropertiesLayout , you try to obtain a null object with some error");
        //}

        public static IButton Button(string buttonText, bool isModernUI, string message, UnityAction action)
        {
            return GUIField(isModernUI ? "Button(ModernUI)" : "Button(UI)", message).As<IButton>().SetTitle(buttonText).AddListener(action);
        }
        public static Button Button(string buttonText, string message, UnityAction action)
        {
            return Button(buttonText, false, message, action) as AD.UI.Button;
        }
        public static ModernUIButton ModernUIButton(string buttonText, string message, UnityAction action)
        {
            return Button(buttonText, true, message, action) as AD.UI.ModernUIButton;
        }

        public enum TextType
        {
            Text, Title, Label
        }
        public static Text Text(string text, TextType type, string message)
        {
            var cat = GUIField(type.ToString() + "(UI)", message).As<Text>().SetText(text);
            return cat;
        }
        public static Text Label(string text, string message)
        {
            return Text(text, TextType.Label, message);
        }
        public static Text Title(string text, string message)
        {
            return Text(text, TextType.Title, message);
        }
        public static Text Text(string text, string message)
        {
            return Text(text, TextType.Text, message);
        }

        public static InputField InputField(string text, string placeholderText, string message)
        {
            var cat = GUIField("InputField(UI)", message).As<InputField>().SetText(text);
            cat.SetPlaceholderText(placeholderText);
            return cat;
        }
        public static InputField InputField(string text, string message)
        {
            var cat = GUIField("InputField(UI)", message).As<InputField>().SetText(text);
            return cat;
        }

        public static IBoolButton BoolButton(string label, bool isModernUI, bool initBool, string message, UnityAction<bool> action)
        {
            return isModernUI ? ModernUISwitch(label, initBool, message, action) : Toggle(label, initBool, message, action);
        }
        public static ModernUISwitch ModernUISwitch(string label,  bool initBool, string message, UnityAction<bool> action)
        {
            var cat = GUIField(label,"Switch(ModernUI)", message) as ModernUISwitch;
            cat.isOn = initBool;
            cat.AddListener(action);
            return cat;
        }
        public static Toggle Toggle(string label, bool initBool, string message, UnityAction<bool> action)
        {
            var cat = GUIField("Toggle(UI)", message) as Toggle;
            cat.isOn = initBool;
            cat.AddListener(action);
            cat.SetTitle(label);
            return cat;
        }

        public static Dropdown Dropdown(string label, string[] options, string initSelect, string message, UnityAction<string> action)
        {
            Dropdown cat = GUIField(label, "Dropdown(UI)", message).As<Dropdown>();
            cat.AddOption(options);
            cat.AddListener(action);
            cat.Select(initSelect);
            return cat;
        }
        public static ModernUIDropdown ModernUIDropdown(string label, string[] options, string[] initSelects, string message, UnityAction<string> action)
        {
            ModernUIDropdown cat = GUIField(label, "Dropdown(ModernUI)", message).As<ModernUIDropdown>();
            cat.AddOption(options);
            cat.AddListener(action);
            cat.maxSelect = initSelects.Length;
            foreach (var initSelect in initSelects)
            {
                cat.Select(initSelect);
            }
            return cat;
        }

        public static RawImage RawImage(Texture texture, string message)
        {
            var cat = GUIField("RawImage(UI)", message) as RawImage;
            cat.source.texture = texture;
            return cat;
        }

        public static Slider Slider(string label, float min, float max, float initValue, bool IsNormalized, string message, UnityAction<float> action)
        {
            BeginHorizontal();
            Label(label, message);
            var cat = GUIField("Slider(UI)", message) as Slider;
            cat.source.minValue = min;
            cat.source.maxValue = max;
            if (IsNormalized) cat.source.value = initValue;
            else cat.source.normalizedValue = initValue;
            cat.source.onValueChanged.AddListener(action);
            EndHorizontal();
            return cat;
        }
        public static Slider Slider(float min, float max, float initValue, bool IsNormalized, string message, UnityAction<float> action)
        {
            var cat = GUIField("Slider(UI)", message) as Slider;
            cat.source.minValue = min;
            cat.source.maxValue = max;
            if (IsNormalized) cat.source.value = initValue;
            else cat.source.normalizedValue = initValue;
            cat.source.onValueChanged.AddListener(action);
            return cat;
        }
        public static ModernUIFillBar ModernUIFillBar(string label, float min, float max, float initValue, string message, UnityAction<float> action)
        {
            BeginHorizontal();
            Label(label, message);
            var cat = GUIField("FillBar(ModernUI)", message) as ModernUIFillBar;
            cat.Set(initValue, min, max);
            cat.OnValueChange.AddListener(action);
            EndHorizontal();
            return cat;
        }
        public static ModernUIFillBar ModernUIFillBar(float min, float max, float initValue, string message, UnityAction<float> action)
        {
            var cat = GUIField("FillBar(ModernUI)", message) as ModernUIFillBar;
            cat.Set(initValue, min, max);
            cat.OnValueChange.AddListener(action);
            return cat;
        }

        public static ColorManager ColorPanel(string label, Color initColor, string message, UnityAction<Color> action)
        {
            EndHorizontal();
            Label(label, message);
            var cat = GUIField("ColorPanel(UI)", message) as ColorManager;
            cat.ColorValue = initColor;
            cat.ColorProperty.AddListenerOnSet(action);
            return cat;
        }
        public static ColorManager ColorPanel(Color initColor, string message, UnityAction<Color> action)
        {
            var cat = GUIField("ColorPanel(UI)", message) as ColorManager;
            cat.ColorValue = initColor;
            cat.ColorProperty.AddListenerOnSet(action);
            return cat;
        }

        public static ViewController Image(string label, string message)
        {
            BeginHorizontal();
            Label(label, message);
            var cat = GUIField("Image(UI)", message) as ViewController;
            EndHorizontal();
            return cat;
        }
        public static ViewController Image(string message)
        {
            var cat = GUIField("Image(UI)", message) as ViewController;
            return cat;
        }

        public static void BeginHorizontal()
        {
            if (IsNeedMulLine)
            {
                IsNeedMulLine = false;
                if (GUILayoutLineList.Count == 0 || GUILayoutLineList[^1].Count > 0)
                    GUILayoutLineList.Add(new());
            }
            else
            {
                IsNeedMulLine = false;
                if (GUILayoutLineList.Count == 0)
                    GUILayoutLineList.Add(new());
            }
        }
        public static void EndHorizontal()
        {
            IsNeedMulLine = true;
        }
    }
}
