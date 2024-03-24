using System.Collections.Generic;
using AD.BASE;
using UnityEditor;
using UnityEngine;
using AD.UI;
using System.Linq;
using UnityEngine.Playables;

namespace AD
{
    [CustomEditor(typeof(ADGlobalSystem))]
    public class GlobalSystemEditor : AbstractCustomADEditor
    {
        ADGlobalSystem that = null;

        List<string> buttons = new();

        SerializedProperty IsNeedExcepion;
        SerializedProperty MaxRecordItemCount;

        SerializedProperty _Button;
        SerializedProperty _DropDown;
        SerializedProperty _InputField;
        SerializedProperty _RawImage;
        SerializedProperty _Slider;
        SerializedProperty _Text;
        SerializedProperty _Toggle;

        SerializedProperty _ModernButton;
        SerializedProperty _ModernUIDropdown;
        SerializedProperty _ModernUIFillBar;
        SerializedProperty _ModernUIInputField;
        SerializedProperty _ModernUISwitch;

        SerializedProperty _Image;
        SerializedProperty _ColorManager;
        SerializedProperty _AudioSource;
        SerializedProperty _CustomWindowElement;
        SerializedProperty _ListView;
        SerializedProperty _TouchPanel;

        SerializedProperty RecordPath;

        //SceneBaseController

        SerializedProperty OnSceneStart;
        SerializedProperty OnSceneEnd;

        SerializedProperty mCanvasInitializer;

        SerializedProperty IsEnableSceneController;
        SerializedProperty TargetSceneName;
        SerializedProperty isAsyncToLoadNextScene;
        SerializedProperty WaitTime;

        //NumericManager

        SerializedProperty IntValues;
        SerializedProperty FloatValues;
        SerializedProperty StringValues;

        SerializedProperty CastListeners;

        protected override void OnEnable()
        {
            base.OnEnable();
            that = target as ADGlobalSystem;

            buttons = new List<string>();
            foreach (var key in that.multipleInputController)
            {
                foreach (var button in key.Key)
                {
                    buttons.Add(button.ToString() + "  ");
                }
            }
            that._IsOnValidate = false;

            IsNeedExcepion = serializedObject.FindProperty(nameof(IsNeedExcepion));
            MaxRecordItemCount = serializedObject.FindProperty(nameof(MaxRecordItemCount));
            _Button = serializedObject.FindProperty(nameof(_Button));
            _DropDown = serializedObject.FindProperty(nameof(_DropDown));
            _InputField = serializedObject.FindProperty(nameof(_InputField));
            _RawImage = serializedObject.FindProperty(nameof(_RawImage));
            _Slider = serializedObject.FindProperty(nameof(_Slider));
            _Text = serializedObject.FindProperty(nameof(_Text));
            _Toggle = serializedObject.FindProperty(nameof(_Toggle));
            _ModernButton = serializedObject.FindProperty(nameof(_ModernButton));
            _ModernUIDropdown = serializedObject.FindProperty(nameof(_ModernUIDropdown));
            _ModernUIFillBar = serializedObject.FindProperty(nameof(_ModernUIFillBar));
            _ModernUIInputField = serializedObject.FindProperty(nameof(_ModernUIInputField));
            _ModernUISwitch = serializedObject.FindProperty(nameof(_ModernUISwitch));
            _Image = serializedObject.FindProperty(nameof(_Image));
            _ColorManager = serializedObject.FindProperty(nameof(_ColorManager));
            _AudioSource = serializedObject.FindProperty(nameof(_AudioSource));
            _CustomWindowElement = serializedObject.FindProperty(nameof(_CustomWindowElement));
            _ListView = serializedObject.FindProperty(nameof(_ListView));
            _TouchPanel = serializedObject.FindProperty(nameof(_TouchPanel));
            RecordPath = serializedObject.FindProperty(nameof(RecordPath));
            OnSceneStart = serializedObject.FindProperty(nameof(OnSceneStart));
            OnSceneEnd = serializedObject.FindProperty(nameof(OnSceneEnd));
            mCanvasInitializer = serializedObject.FindProperty(nameof(mCanvasInitializer));
            IsEnableSceneController = serializedObject.FindProperty(nameof(IsEnableSceneController));
            TargetSceneName = serializedObject.FindProperty(nameof(TargetSceneName));
            isAsyncToLoadNextScene = serializedObject.FindProperty(nameof(isAsyncToLoadNextScene));
            WaitTime = serializedObject.FindProperty(nameof(WaitTime));
            IntValues = serializedObject.FindProperty(nameof(IntValues));
            FloatValues = serializedObject.FindProperty(nameof(FloatValues));
            StringValues = serializedObject.FindProperty(nameof(StringValues));
            CastListeners = serializedObject.FindProperty(nameof(CastListeners));
        }

        public override void OnContentGUI()
        {
            if (IsEnableSceneController.boolValue)
            {
                HorizontalBlockWithBox(() =>
                {
                    EditorGUILayout.LabelField(new GUIContent("TargetSceneName"), customSkin.FindStyle("Text"), GUILayout.Width(120));
                    EditorGUILayout.PropertyField(TargetSceneName, new GUIContent(""));
                    if(TargetSceneName.stringValue.Equals(ADGlobalSystem._BackSceneTargetSceneName))
                    {
                        this.HelpBox("Now Next Scene Will Be The Scene Which Is Previous Scene", MessageType.Info);
                    }
                    else if(TargetSceneName.stringValue.Contains("back",System.StringComparison.InvariantCultureIgnoreCase))
                    {
                        this.HelpBox($"Do You Want To Set The Target To Go Back To The Previous Scene , You Can Enter \"{ADGlobalSystem._BackSceneTargetSceneName}\"", MessageType.Warning);
                    }
                });
                GUILayout.BeginHorizontal(EditorStyles.helpBox);

                isAsyncToLoadNextScene.boolValue = GUILayout.Toggle(isAsyncToLoadNextScene.boolValue, new GUIContent("Is AsyncLoad"), customSkin.FindStyle("Toggle"));
                isAsyncToLoadNextScene.boolValue = GUILayout.Toggle(isAsyncToLoadNextScene.boolValue, new GUIContent(""), customSkin.FindStyle("Toggle Helper"));

                EditorGUILayout.PropertyField(WaitTime, new GUIContent("Waiting Of Load"));

                GUILayout.EndHorizontal();

                GUILayout.BeginVertical(EditorStyles.helpBox);

                GUILayout.BeginHorizontal(EditorStyles.helpBox);
                EditorGUILayout.LabelField(new GUIContent("On Scene Event"), customSkin.FindStyle("Text"), GUILayout.Width(120));
                GUILayout.EndHorizontal();

                GUILayout.Space(2);
                EditorGUILayout.PropertyField(OnSceneStart, true);
                EditorGUILayout.PropertyField(OnSceneEnd, true);
                GUILayout.Space(2);
                GUILayout.EndVertical();
            }

            if (that._IsOnValidate)
            {
                buttons = new List<string>();
                foreach (var key in that.multipleInputController)
                {
                    foreach (var button in key.Key)
                    {
                        buttons.Add(button.ToString() + "  ");
                    }
                }
                that._IsOnValidate = false;
            }

            if (GUILayout.Button("Init All ADUI"))
            {
                foreach (var item in ADUI.Items)
                {
                    if (item.Is<ListView>(out var listView)) listView.Init();
                    else if (item.Is<Toggle>(out var toggle)) toggle.Init();
                }
            }
            if (Application.isPlaying)
            {

                EditorGUILayout.Space(25);

                if (buttons.Count == 0) EditorGUILayout.TextArea("No Event was register");
                else foreach (var key in buttons) EditorGUILayout.TextArea(key);

                if (GUILayout.Button("SaveRecord"))
                {
                    that.SaveRecord();
                }
            }

            this.VerticalBlockWithBox(() =>
            {
                EditorGUILayout.Space(5);
                if (that.IsPermittedBroadcast)
                {
                    EditorGUILayout.PropertyField(CastListeners);
                    if (GUILayout.Button("-- Delete Broadcast Listeners -- "))
                    {
                        that.CastListeners = null;
                        that.IsPermittedBroadcast = false;
                    }
                    this.HorizontalBlock(() =>
                    {
                        if (GUILayout.Button("-- Add -- "))
                        {
                            that.CastListeners.TryAdd("New Key", new());
                        }
                        if (GUILayout.Button("-- Remove -- "))
                        {
                            that.CastListeners.RemoveNullValues();
                            if (that.CastListeners.Count > 0) that.CastListeners.Remove(that.CastListeners.Last().Key);
                        }
                    });
                }
                else
                {
                    if (GUILayout.Button("-- Create Broadcast Listeners -- "))
                    {
                        that.CastListeners = new();
                        that.IsPermittedBroadcast = true;
                    }
                }
                EditorGUILayout.Space(5);
            });
        }

        public override void OnResourcesGUI()
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);

            GUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUILayout.LabelField(new GUIContent("AD UI"), customSkin.FindStyle("Text"), GUILayout.Width(120));
            GUILayout.EndHorizontal();

            GUILayout.Space(2);
            EditorGUILayout.PropertyField(_Button);
            EditorGUILayout.PropertyField(_DropDown);
            EditorGUILayout.PropertyField(_InputField);
            EditorGUILayout.PropertyField(_RawImage);
            EditorGUILayout.PropertyField(_Slider);
            EditorGUILayout.PropertyField(_Text);
            EditorGUILayout.PropertyField(_Toggle);
            GUILayout.Space(2);
            GUILayout.EndVertical();

            GUILayout.BeginVertical(EditorStyles.helpBox);

            GUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUILayout.LabelField(new GUIContent("AD ModernUI"), customSkin.FindStyle("Text"), GUILayout.Width(120));
            GUILayout.EndHorizontal();

            GUILayout.Space(2);
            EditorGUILayout.PropertyField(_ModernButton);
            EditorGUILayout.PropertyField(_ModernUIDropdown);
            EditorGUILayout.PropertyField(_ModernUIFillBar);
            EditorGUILayout.PropertyField(_ModernUIInputField);
            EditorGUILayout.PropertyField(_ModernUISwitch);
            GUILayout.Space(2);
            GUILayout.EndVertical();

            GUILayout.BeginVertical(EditorStyles.helpBox);

            GUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUILayout.LabelField(new GUIContent("AD CoreObject"), customSkin.FindStyle("Text"), GUILayout.Width(120));
            GUILayout.EndHorizontal();

            GUILayout.Space(2);
            EditorGUILayout.PropertyField(_Image);
            EditorGUILayout.PropertyField(_ColorManager);
            EditorGUILayout.PropertyField(_AudioSource);
            EditorGUILayout.PropertyField(_CustomWindowElement);
            EditorGUILayout.PropertyField(_ListView);
            EditorGUILayout.PropertyField(_TouchPanel);
            GUILayout.Space(2);
            GUILayout.EndVertical();
        }

        public override void OnSettingsGUI()
        {
            GUILayout.BeginHorizontal(EditorStyles.helpBox);
            this.HelpBox(ADGlobalSystem.Version, MessageType.Info);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(EditorStyles.helpBox);

            this.HelpBox("Is set this system \nas SceneController", MessageType.Info);

            IsEnableSceneController.boolValue = GUILayout.Toggle(IsEnableSceneController.boolValue, new GUIContent("SC Enable"), customSkin.FindStyle("Toggle"));
            IsEnableSceneController.boolValue = GUILayout.Toggle(IsEnableSceneController.boolValue, new GUIContent(""), customSkin.FindStyle("Toggle Helper"));

            GUILayout.EndHorizontal();

            HorizontalBlockWithBox(() =>
            {
                IsNeedExcepion.boolValue = GUILayout.Toggle(IsNeedExcepion.boolValue, new GUIContent("Is Need Throw Excepion"), customSkin.FindStyle("Toggle"));
                IsNeedExcepion.boolValue = GUILayout.Toggle(IsNeedExcepion.boolValue, new GUIContent(""), customSkin.FindStyle("Toggle Helper"));
            });
            HorizontalBlockWithBox(() =>
            {
                EditorGUILayout.PropertyField(RecordPath);
            });
            HorizontalBlockWithBox(() =>
            {
                EditorGUILayout.PropertyField(MaxRecordItemCount);
            });

            GUILayout.BeginVertical(EditorStyles.helpBox);

            GUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUILayout.LabelField(new GUIContent("Init Canvas"), customSkin.FindStyle("Text"), GUILayout.Width(120));
            GUILayout.EndHorizontal();

            GUILayout.Space(2);
            EditorGUILayout.PropertyField(mCanvasInitializer, true);
            GUILayout.Space(2);
            GUILayout.EndVertical();

            HorizontalBlockWithBox(() =>
            {
                UnityEngine.Object @object = null;
                EditorGUI.BeginChangeCheck();
                ADGlobalSystem temp_cat = EditorGUILayout.ObjectField("Instance", ADGlobalSystem._m_instance, typeof(ADGlobalSystem), @object) as ADGlobalSystem;
                if (EditorGUI.EndChangeCheck()) ADGlobalSystem._m_instance = temp_cat;
            });
            this.OnNotChangeGUI(() =>
            {
                HorizontalBlockWithBox(() =>
                {
                    UnityEngine.Object @object = null;
                    EditorGUILayout.ObjectField("CurrentADUI", ADUI.CurrentSelect, typeof(ADUI), @object);
                });
            });

            GUILayout.BeginVertical(EditorStyles.helpBox);
            if (GUILayout.Button("  -- Save NumericManager Values --")) that.SaveNumericManager();
            GUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.PropertyField(IntValues);
            EditorGUILayout.PropertyField(FloatValues);
            EditorGUILayout.PropertyField(StringValues);
            GUILayout.EndVertical();
            GUILayout.EndVertical();
        }
    }

    public class MenuIniter
    {
        [MenuItem("GameObject/AD/AudioSource", false, 30)]
        private static void AudioSource(UnityEditor.MenuCommand menuCommand)
        {
            var target = GameObject.Instantiate(ADGlobalSystem.instance._AudioSource);
            target.name = "AudioSource";
            GameObjectUtility.SetParentAndAlign(target.gameObject, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(target.gameObject, "Create " + target.name);
            Selection.activeObject = target.gameObject;
        }
        [MenuItem("GameObject/AD/Image", false, 30)]
        private static void Image(UnityEditor.MenuCommand menuCommand)
        {
            var target = GameObject.Instantiate(ADGlobalSystem.instance._Image);
            target.name = "Image";
            GameObjectUtility.SetParentAndAlign(target.gameObject, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(target.gameObject, "Create " + target.name);
            Selection.activeObject = target.gameObject;
        }
        [MenuItem("GameObject/AD/ColorManager", false, 30)]
        private static void ColorManager(UnityEditor.MenuCommand menuCommand)
        {
            var target = GameObject.Instantiate(ADGlobalSystem.instance._ColorManager);
            target.name = "ColorManager";
            GameObjectUtility.SetParentAndAlign(target.gameObject, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(target.gameObject, "Create " + target.name);
            Selection.activeObject = target.gameObject;
        }
        [MenuItem("GameObject/AD/CustomWindowElement", false, 30)]
        private static void CustomWindowElement(UnityEditor.MenuCommand menuCommand)
        {
            var target = GameObject.Instantiate(ADGlobalSystem.instance._CustomWindowElement);
            target.name = "CustomWindowElement";
            GameObjectUtility.SetParentAndAlign(target.gameObject, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(target.gameObject, "Create " + target.name);
            Selection.activeObject = target.gameObject;
        }
        [MenuItem("GameObject/AD/ListView", false, 30)]
        private static void ListView(UnityEditor.MenuCommand menuCommand)
        {
            var target = GameObject.Instantiate(ADGlobalSystem.instance._ListView);
            target.name = "ListView";
            GameObjectUtility.SetParentAndAlign(target.gameObject, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(target.gameObject, "Create " + target.name);
            Selection.activeObject = target.gameObject;
        }
        [MenuItem("GameObject/AD/TouchPanel", false, 30)]
        private static void TouchPanel(UnityEditor.MenuCommand menuCommand)
        {
            var target = GameObject.Instantiate(ADGlobalSystem.instance._TouchPanel);
            target.name = "TouchPanel";
            GameObjectUtility.SetParentAndAlign(target.gameObject, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(target.gameObject, "Create " + target.name);
            Selection.activeObject = target.gameObject;
        }

        [MenuItem("GameObject/AD/TimeLine", false, 50)]
        private static void TimeLine(UnityEditor.MenuCommand menuCommand)
        {
            var target = new GameObject("Timeline");
            target.AddComponent<PlayableDirector>();
            GameObjectUtility.SetParentAndAlign(target.gameObject, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(target.gameObject, "Create " + target.name);
            Selection.activeObject = target.gameObject;
        }

        [MenuItem("GameObject/AD/TemplateSceneManager", false, 70)]
        private static void TemplateSceneManager(UnityEditor.MenuCommand menuCommand)
        {
            var target = new GameObject("TemplateSceneManager");
            GameObjectUtility.SetParentAndAlign(target.gameObject, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(target.gameObject, "Create " + target.name);
            Selection.activeObject = target.gameObject;
            target.AddComponent<TemplateSceneManager>().LoadAllKeyLayer();
        }

        [MenuItem("GameObject/AD/ADUI/Slider", false, 10)]
        private static void Slider(UnityEditor.MenuCommand menuCommand)
        {
            var target = GameObject.Instantiate(ADGlobalSystem.instance._Slider);
            target.name = "Slider";
            GameObjectUtility.SetParentAndAlign(target.gameObject, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(target.gameObject, "Create " + target.name);
            Selection.activeObject = target.gameObject;
        }
        [MenuItem("GameObject/AD/ADUI/Button", false, 10)]
        private static void Button(UnityEditor.MenuCommand menuCommand)
        {
            var target = GameObject.Instantiate(ADGlobalSystem.instance._Button);
            target.name = "Button";
            GameObjectUtility.SetParentAndAlign(target.gameObject, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(target.gameObject, "Create " + target.name);
            Selection.activeObject = target.gameObject;
        }
        [MenuItem("GameObject/AD/ADUI/DropDown", false, 10)]
        private static void DropDown(UnityEditor.MenuCommand menuCommand)
        {
            var target = GameObject.Instantiate(ADGlobalSystem.instance._DropDown);
            target.name = "DropDown";
            GameObjectUtility.SetParentAndAlign(target.gameObject, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(target.gameObject, "Create " + target.name);
            Selection.activeObject = target.gameObject;
        }
        [MenuItem("GameObject/AD/ADUI/InputField", false, 10)]
        private static void InputField(UnityEditor.MenuCommand menuCommand)
        {
            var target = GameObject.Instantiate(ADGlobalSystem.instance._InputField);
            target.name = "InputField";
            GameObjectUtility.SetParentAndAlign(target.gameObject, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(target.gameObject, "Create " + target.name);
            Selection.activeObject = target.gameObject;
        }
        [MenuItem("GameObject/AD/ADUI/RawImage", false, 10)]
        private static void RawImage(UnityEditor.MenuCommand menuCommand)
        {
            var target = GameObject.Instantiate(ADGlobalSystem.instance._RawImage);
            target.name = "RawImage";
            GameObjectUtility.SetParentAndAlign(target.gameObject, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(target.gameObject, "Create " + target.name);
            Selection.activeObject = target.gameObject;
        }
        [MenuItem("GameObject/AD/ADUI/Text", false, 10)]
        private static void Text(UnityEditor.MenuCommand menuCommand)
        {
            var target = GameObject.Instantiate(ADGlobalSystem.instance._Text);
            target.name = "Text";
            GameObjectUtility.SetParentAndAlign(target.gameObject, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(target.gameObject, "Create " + target.name);
            Selection.activeObject = target.gameObject;
        }
        [MenuItem("GameObject/AD/ADUI/Toggle", false, 10)]
        private static void Toggle(UnityEditor.MenuCommand menuCommand)
        {
            var target = GameObject.Instantiate(ADGlobalSystem.instance._Toggle);
            target.name = "Toggle";
            GameObjectUtility.SetParentAndAlign(target.gameObject, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(target.gameObject, "Create " + target.name);
            Selection.activeObject = target.gameObject;
        }

        [MenuItem("GameObject/AD/ModernUI/Button", false, 11)]
        private static void ModernButton(UnityEditor.MenuCommand menuCommand)
        {
            var target = GameObject.Instantiate(ADGlobalSystem.instance._ModernButton);
            target.name = "Button";
            GameObjectUtility.SetParentAndAlign(target.gameObject, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(target.gameObject, "Create " + target.name);
            Selection.activeObject = target.gameObject;
        }
        [MenuItem("GameObject/AD/ModernUI/Dropdown", false, 11)]
        private static void ModernUIDropdown(UnityEditor.MenuCommand menuCommand)
        {
            var target = GameObject.Instantiate(ADGlobalSystem.instance._ModernUIDropdown);
            target.name = "Dropdown";
            GameObjectUtility.SetParentAndAlign(target.gameObject, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(target.gameObject, "Create " + target.name);
            Selection.activeObject = target.gameObject;
        }
        [MenuItem("GameObject/AD/ModernUI/FillBar", false, 11)]
        private static void ModernUIFillBar(UnityEditor.MenuCommand menuCommand)
        {
            var target = GameObject.Instantiate(ADGlobalSystem.instance._ModernUIFillBar);
            target.name = "FillBar";
            GameObjectUtility.SetParentAndAlign(target.gameObject, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(target.gameObject, "Create " + target.name);
            Selection.activeObject = target.gameObject;
        }
        [MenuItem("GameObject/AD/ModernUI/InputField", false, 11)]
        private static void ModernUIInputField(UnityEditor.MenuCommand menuCommand)
        {
            var target = GameObject.Instantiate(ADGlobalSystem.instance._ModernUIInputField);
            target.name = "InputField";
            GameObjectUtility.SetParentAndAlign(target.gameObject, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(target.gameObject, "Create " + target.name);
            Selection.activeObject = target.gameObject;
        }
        [MenuItem("GameObject/AD/ModernUI/Switch", false, 11)]
        private static void ModernUISwitch(UnityEditor.MenuCommand menuCommand)
        {
            var target = GameObject.Instantiate(ADGlobalSystem.instance._ModernUISwitch);
            target.name = "Switch";
            GameObjectUtility.SetParentAndAlign(target.gameObject, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(target.gameObject, "Create " + target.name);
            Selection.activeObject = target.gameObject;
        }
    }

}
