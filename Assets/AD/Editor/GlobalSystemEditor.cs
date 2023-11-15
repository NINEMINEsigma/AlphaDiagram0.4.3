using System.Collections.Generic;
using AD.BASE;
using UnityEditor;
using UnityEngine;
using AD.UI;
using System;
using Codice.CM.Common;

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

        SerializedProperty RecordPath;

        //SceneBaseController

        SerializedProperty OnSceneStart;
        SerializedProperty OnSceneEnd;

        SerializedProperty mCanvasInitializer;

        SerializedProperty TargetSceneName;

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
            RecordPath = serializedObject.FindProperty(nameof(RecordPath));
            OnSceneStart = serializedObject.FindProperty(nameof(OnSceneStart));
            OnSceneEnd = serializedObject.FindProperty(nameof(OnSceneEnd));
            mCanvasInitializer = serializedObject.FindProperty(nameof(mCanvasInitializer));
            TargetSceneName = serializedObject.FindProperty(nameof(TargetSceneName));
        }

        public override void OnContentGUI()
        {
            HorizontalBlockWithBox(() =>
            {
                EditorGUILayout.LabelField(new GUIContent("TargetSceneName"), customSkin.FindStyle("Text"), GUILayout.Width(120));
                EditorGUILayout.PropertyField(TargetSceneName, new GUIContent(""));
            });

            GUILayout.BeginVertical(EditorStyles.helpBox);

            GUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUILayout.LabelField(new GUIContent("On Scene Event"), customSkin.FindStyle("Text"), GUILayout.Width(120));
            GUILayout.EndHorizontal();

            GUILayout.Space(2);
            EditorGUILayout.PropertyField(OnSceneStart, true);
            EditorGUILayout.PropertyField(OnSceneEnd, true);
            GUILayout.Space(2);
            GUILayout.EndVertical();

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
            GUILayout.Space(2);
            GUILayout.EndVertical();
        }

        public override void OnSettingsGUI()
        {
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
        }
    }

}
