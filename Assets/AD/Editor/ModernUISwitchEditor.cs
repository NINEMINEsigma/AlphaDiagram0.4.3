using AD.UI;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ModernUISwitch))]
public class ModernUISwitchEditor : ADUIEditor
{
    ModernUISwitch that;

    SerializedProperty OnEvents;
    SerializedProperty OffEvents;
    SerializedProperty SwitchEvents;
    SerializedProperty enableSwitchSounds;
    SerializedProperty useHoverSound;
    SerializedProperty useClickSound;
    SerializedProperty soundSource;
    SerializedProperty hoverSound;
    SerializedProperty clickSound;

    protected override void OnEnable()
    {
        base.OnEnable();
        that = (ModernUISwitch)target;

        OnEvents = serializedObject.FindProperty("OnEvents");
        OffEvents = serializedObject.FindProperty("OffEvents");
        enableSwitchSounds = serializedObject.FindProperty("enableSwitchSounds");
        useHoverSound = serializedObject.FindProperty("useHoverSound");
        useClickSound = serializedObject.FindProperty("useClickSound");
        soundSource = serializedObject.FindProperty("soundSource");
        hoverSound = serializedObject.FindProperty("hoverSound");
        clickSound = serializedObject.FindProperty("clickSound");
        SwitchEvents = serializedObject.FindProperty("SwitchEvents");
    }

    public override void OnContentGUI()
    {
        MakeUpNumericManager(nameof(that.NumericManagerName));
        EditorGUILayout.PropertyField(OnEvents, new GUIContent("On Events"), true);
        EditorGUILayout.PropertyField(OffEvents, new GUIContent("Off Events"), true);
        EditorGUILayout.PropertyField(SwitchEvents, new GUIContent("Switch Events"), true);

    }

    public override void OnResourcesGUI()
    {
        if (enableSwitchSounds.boolValue == true)
        {
            if (enableSwitchSounds.boolValue == true && useHoverSound.boolValue == true)
            {
                GUILayout.BeginHorizontal(EditorStyles.helpBox);

                EditorGUILayout.LabelField(new GUIContent("Hover Sound"), customSkin.FindStyle("Text"), GUILayout.Width(120));
                EditorGUILayout.PropertyField(hoverSound, new GUIContent(""));

                GUILayout.EndHorizontal();
            }

            if (enableSwitchSounds.boolValue == true && useClickSound.boolValue == true)
            {
                GUILayout.BeginHorizontal(EditorStyles.helpBox);

                EditorGUILayout.LabelField(new GUIContent("Click Sound"), customSkin.FindStyle("Text"), GUILayout.Width(120));
                EditorGUILayout.PropertyField(clickSound, new GUIContent(""));

                GUILayout.EndHorizontal();
            }
        }
        else
        {
            this.HelpBox("You Can Set SwitchSounds Enable To Open Those Item", MessageType.Info);
        }
    }

    public override void OnSettingsGUI()
    {
        GUILayout.BeginHorizontal(EditorStyles.helpBox);

        that.isOn = GUILayout.Toggle(that.isOn, new GUIContent("Is On"), customSkin.FindStyle("Toggle"));
        that.isOn = GUILayout.Toggle(that.isOn, new GUIContent(""), customSkin.FindStyle("Toggle Helper"));

        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal(EditorStyles.helpBox);

        enableSwitchSounds.boolValue = GUILayout.Toggle(enableSwitchSounds.boolValue, new GUIContent("Enable Switch Sounds"), customSkin.FindStyle("Toggle"));
        enableSwitchSounds.boolValue = GUILayout.Toggle(enableSwitchSounds.boolValue, new GUIContent(""), customSkin.FindStyle("Toggle Helper"));

        GUILayout.EndHorizontal();

        if (enableSwitchSounds.boolValue == true)
        {
            GUILayout.BeginHorizontal(EditorStyles.helpBox);

            useHoverSound.boolValue = GUILayout.Toggle(useHoverSound.boolValue, new GUIContent("Enable Hover Sound"), customSkin.FindStyle("Toggle"));
            useHoverSound.boolValue = GUILayout.Toggle(useHoverSound.boolValue, new GUIContent(""), customSkin.FindStyle("Toggle Helper"));

            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal(EditorStyles.helpBox);

            useClickSound.boolValue = GUILayout.Toggle(useClickSound.boolValue, new GUIContent("Enable Click Sound"), customSkin.FindStyle("Toggle"));
            useClickSound.boolValue = GUILayout.Toggle(useClickSound.boolValue, new GUIContent(""), customSkin.FindStyle("Toggle Helper"));

            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal(EditorStyles.helpBox);

            EditorGUILayout.LabelField(new GUIContent("Sound Source"), customSkin.FindStyle("Text"), GUILayout.Width(120));
            EditorGUILayout.PropertyField(soundSource, new GUIContent(""));

            GUILayout.EndHorizontal();
        }
    }
}