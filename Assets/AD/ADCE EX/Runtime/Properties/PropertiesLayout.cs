using System;
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
        private static bool IsApply = true;

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
        public static IADUI GUIField(string text, string style, string message = "", bool IsHorizontal = true)
        {
            GameObject root = ObtainGUIStyleInstance(style, out IADUI targeADUI);
            int extensionalSpaceLine = ((int)root.transform.As<RectTransform>().sizeDelta.y / (int)PropertiesItem.DefaultRectHightLevelSize) - 1;
            GUIContent content = new(root, targeADUI, GUIContentType.Default, extensionalSpaceLine, message);
            if (extensionalSpaceLine > 0)
            {
                EndHorizontal();
            }
            if (IsHorizontal) BeginHorizontal();
            GameObject labelRoot = GUI.skin.FindStyle("Label(UI)").Prefab.PrefabInstantiate();
            DoGUILine(new GUIContent(labelRoot, labelRoot.GetComponentInChildren<AD.UI.Text>().SetText(text.Translate()), GUIContentType.Default, 0, message));
            if (extensionalSpaceLine > 0)
            {
                EndHorizontal();
            }
            DoGUILine(content);
            if (IsHorizontal) EndHorizontal();
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
            if (!IsApply)
            {
                foreach (var GUILine in GUILayoutLineList)
                {
                    foreach (var content in GUILine)
                    {
                        GameObject.Destroy(content.RootObject);
                    }
                }
            }
            IsApply = false;
        }

        public static void ApplyPropertiesLayout()
        {
            try
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
            }
            catch (Exception ex)
            {
                GameEditorApp.instance.GetController<Information>().Error("ApplyPropertiesLayout Failed : " + ex.Message);
                foreach (var items in GUILayoutLineList)
                {
                    foreach (GUIContent item in items)
                    {
                        GameObject.Destroy(item.RootObject);
                    }
                }
            }
            finally
            {
                CurrentEditorThat = null;
                GUILayoutLineList.Clear();
                EndHorizontal();
                IsApply = true;
            }
        }

        //public static T GenerateElement<T>() where T : ADUI
        //{
        //    T element = ADGlobalSystem.FinalCheck<T>(ADGlobalSystem.GenerateElement<T>(), "On PropertiesLayout , you try to obtain a null object with some error");
        //}

        public static IADUI ObjectField(string text, string style, string message = "", bool IsHorizontal = true) => GUIField(text, style, message, IsHorizontal);
        public static IADUI ObjectField(string style, string message = "") => GUIField(style, message);

        public static T CField<T>(string text, string message = "", bool IsHorizontal = true) where T : IADUI
        {
            return (T)GUIField(text, typeof(T).Name, message, IsHorizontal);
        }
        public static T CField<T>(string message = "") where T : IADUI
        {
            return (T)GUIField(typeof(T).Name, message);
        }


        #region L

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
        public static Text Label(string text)
        {
            return Text(text, TextType.Label, text);
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
            return cat as InputField;
        }
        public static InputField InputField(string text, string message)
        {
            var cat = GUIField("InputField(UI)", message).As<InputField>().SetText(text);
            return cat as InputField;
        }

        public static IBoolButton BoolButton(string label, bool isModernUI, bool initBool, string message, UnityAction<bool> action)
        {
            return isModernUI ? ModernUISwitch(label, initBool, message, action) : Toggle(label, initBool, message, action);
        }
        public static ModernUISwitch ModernUISwitch(string label, bool initBool, string message, UnityAction<bool> action)
        {
            var cat = GUIField(label, "Switch(ModernUI)", message) as ModernUISwitch;
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
        public static Dropdown Dropdown(string[] options, string initSelect, string message, UnityAction<string> action)
        {
            Dropdown cat = GUIField("Dropdown(UI)", message).As<Dropdown>();
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
        public static ModernUIDropdown ModernUIDropdown(string[] options, string[] initSelects, string message, UnityAction<string> action)
        {
            ModernUIDropdown cat = GUIField("Dropdown(ModernUI)", message).As<ModernUIDropdown>();
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

        #endregion

        //Extension by 12.12

        public static Vector2UI Vector2(string label, Vector2 initVec, string message, UnityAction<Vector2> action)
        {
            var cat = GUIField(label, "Vector2(UI)", message, false).As<VectorBaseUI>().InitValue(initVec.x, initVec.y) as Vector2UI;
            cat.action = action;
            return cat;
        }
        public static Vector3UI Vector3(string label, Vector3 initVec, string message, UnityAction<Vector3> action)
        {
            var cat = GUIField(label, "Vector3(UI)", message, false).As<VectorBaseUI>().InitValue(initVec.x, initVec.y, initVec.z) as Vector3UI;
            cat.action = action;
            return cat;
        }
        public static Vector4UI Vector4(string label, Vector4 initVec, string message, UnityAction<Vector4> action)
        {
            var cat = GUIField(label, "Vector3(UI)", message, false).As<VectorBaseUI>().InitValue(initVec.x, initVec.y, initVec.z, initVec.w) as Vector4UI;
            cat.action = action;
            return cat;
        }

        public static Vector2UI Vector2(Vector2 initVec, string message, UnityAction<Vector2> action)
        {
            var cat = GUIField("Vector2(UI)", message).As<VectorBaseUI>().InitValue(initVec.x, initVec.y) as Vector2UI;
            cat.action = action;
            return cat;
        }
        public static Vector3UI Vector3(Vector3 initVec, string message, UnityAction<Vector3> action)
        {
            var cat = GUIField("Vector3(UI)", message).As<VectorBaseUI>().InitValue(initVec.x, initVec.y, initVec.z) as Vector3UI;
            cat.action = action;
            return cat;
        }
        public static Vector4UI Vector4(Vector4 initVec, string message, UnityAction<Vector4> action)
        {
            var cat = GUIField("Vector3(UI)", message).As<VectorBaseUI>().InitValue(initVec.x, initVec.y, initVec.z, initVec.w) as Vector4UI;
            cat.action = action;
            return cat;
        }

        public static void Transform(Transform transform)
        {
            EndHorizontal();
            Title("Local", "Local Value");
            var localPosition = Vector3("LocalPosition", transform.localPosition, "LocalPosition", T => transform.localPosition = T);
            localPosition.xyz = () => transform.localPosition;
            var localEulerAngles = Vector3("LocalEulerAngles", transform.localEulerAngles, "LocalEulerAngles", T => transform.localEulerAngles = T);
            localEulerAngles.xyz = () => transform.localEulerAngles;
            var localScale = Vector3("LocalScale", transform.localEulerAngles, "LocalScale", T => transform.localScale = T);
            localScale.xyz = () => transform.localScale;

            Title("World", "World Value");
            var Position = Vector3("Position", transform.position, "Position", T => transform.position = T);
            Position.xyz = () => transform.position;
            var EulerAngles = Vector3("EulerAngles", transform.eulerAngles, "EulerAngles", T => transform.eulerAngles = T);
            EulerAngles.xyz = () => transform.eulerAngles;
        }

        public static Dropdown Enum<T>(string label, int initEnumValue, string message, UnityAction<string> action)
        {
            Type TargetEnum = typeof(T);
            if (!TargetEnum.IsEnum) throw new ADException("No Enum");
            var ops = TargetEnum.GetEnumNames();
            if (!TargetEnum.IsEnumDefined(initEnumValue)) throw new ADException("Not Defined On This Enum");
            return Dropdown(label, ops, TargetEnum.GetEnumName(initEnumValue), message, action);
        }

        public static ModernUIDropdown EnumByModern<T>(string label, int[] initEnumValue, string message, UnityAction<string> action)
        {
            Type TargetEnum = typeof(T);
            if (!TargetEnum.IsEnum) throw new ADException("No Enum");
            var ops = TargetEnum.GetEnumNames();
            if (!TargetEnum.IsEnumDefined(initEnumValue)) throw new ADException("Not Defined On This Enum");
            List<string> iniops = new();
            foreach (var op in initEnumValue)
            {
                iniops.Add(TargetEnum.GetEnumName(op));
            }
            return ModernUIDropdown(label, ops, iniops.ToArray(), message, action);
        }

        public static ModernUIInputField ModernUIInputField(string text, string message)
        {
            var cat = GUIField("InputField(ModernUI)", message).As<ModernUIInputField>().SetText(text);
            return cat as ModernUIInputField;
        }

        //ListView

        //Tie Value

        public static InputField FloatField(string label, float initValue, string message, UnityAction<float> action)
        {
            EndHorizontal();
            BeginHorizontal();
            Label(label, message);
            var input = InputField(initValue.ToString(), label, message);
            input.AddListener(T =>
            {
                if (float.TryParse(T, out var value))
                    action.Invoke(value);
                else
                {
                    input.SetTextWithoutNotify("0");
                    action.Invoke(0);
                }
            });
            EndHorizontal();
            return input;
        }

        public static InputField IntField(string label, int initValue, string message, UnityAction<int> action)
        {
            Label(label, message);
            var input = InputField(initValue.ToString(), label, message);
            input.AddListener(T =>
            {
                if (int.TryParse(T, out var value))
                    action.Invoke(value);
                else
                {
                    input.SetTextWithoutNotify("0");
                    action.Invoke(0);
                }
            });
            return input;
        }
    }
}
