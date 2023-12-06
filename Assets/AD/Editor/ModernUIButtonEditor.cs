using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AD.UI.ModernUIButton)), CanEditMultipleObjects]
public class ModernUIButtonEditor : ADUIEditor
{
    private AD.UI.ModernUIButton that;

    SerializedProperty buttonText;
    SerializedProperty hoverSound;
    SerializedProperty clickSound;
    SerializedProperty clickEvent;
    SerializedProperty hoverEvent;
    SerializedProperty normalText;
    SerializedProperty highlightedText;
    SerializedProperty soundSource;
    SerializedProperty useCustomContent;
    SerializedProperty enableButtonSounds;
    SerializedProperty useHoverSound;
    SerializedProperty useClickSound;
    SerializedProperty rippleParent;
    SerializedProperty useRipple;
    SerializedProperty renderOnTop;
    SerializedProperty centered;
    SerializedProperty rippleShape;
    SerializedProperty speed;
    SerializedProperty maxSize;
    SerializedProperty startColor;
    SerializedProperty transitionColor;
    SerializedProperty animationSolution;
    SerializedProperty fadingMultiplier;
    SerializedProperty rippleUpdateMode;

    protected override string TopHeader => "Button Top Header";

    protected override void OnEnable()
    {
        base.OnEnable();
        that = target as AD.UI.ModernUIButton;

        buttonText = serializedObject.FindProperty("buttonText");
        hoverSound = serializedObject.FindProperty("hoverSound");
        clickSound = serializedObject.FindProperty("clickSound");
        clickEvent = serializedObject.FindProperty("clickEvent");
        hoverEvent = serializedObject.FindProperty("hoverEvent");
        normalText = serializedObject.FindProperty("normalText");
        highlightedText = serializedObject.FindProperty("highlightedText");
        soundSource = serializedObject.FindProperty("soundSource");
        useCustomContent = serializedObject.FindProperty("useCustomContent");
        enableButtonSounds = serializedObject.FindProperty("enableButtonSounds");
        useHoverSound = serializedObject.FindProperty("useHoverSound");
        useClickSound = serializedObject.FindProperty("useClickSound");
        rippleParent = serializedObject.FindProperty("rippleParent");
        useRipple = serializedObject.FindProperty("useRipple");
        renderOnTop = serializedObject.FindProperty("renderOnTop");
        centered = serializedObject.FindProperty("centered");
        rippleShape = serializedObject.FindProperty("rippleShape");
        speed = serializedObject.FindProperty("speed");
        maxSize = serializedObject.FindProperty("maxSize");
        startColor = serializedObject.FindProperty("startColor");
        transitionColor = serializedObject.FindProperty("transitionColor");
        animationSolution = serializedObject.FindProperty("animationSolution");
        fadingMultiplier = serializedObject.FindProperty("fadingMultiplier");
        rippleUpdateMode = serializedObject.FindProperty("rippleUpdateMode");
    }

    public override void OnContentGUI()
    {
        GUILayout.BeginHorizontal(EditorStyles.helpBox);

        EditorGUILayout.LabelField(new GUIContent("Button Text"), customSkin.FindStyle("Text"), GUILayout.Width(120));
        EditorGUILayout.PropertyField(buttonText, new GUIContent(""));

        GUILayout.EndHorizontal();
        
        if (useCustomContent.boolValue == false && that.normalText == null)
        {
            GUILayout.Space(2);
            EditorGUILayout.HelpBox("'Text Object' is not assigned. Go to Resources tab and assign the correct variable.", MessageType.Error);
        }

        if (enableButtonSounds.boolValue == true && useHoverSound.boolValue == true)
        {
            GUILayout.BeginHorizontal(EditorStyles.helpBox);

            EditorGUILayout.LabelField(new GUIContent("Hover Sound"), customSkin.FindStyle("Text"), GUILayout.Width(120));
            EditorGUILayout.PropertyField(hoverSound, new GUIContent(""));

            GUILayout.EndHorizontal();
        }

        if (enableButtonSounds.boolValue == true && useClickSound.boolValue == true)
        {
            GUILayout.BeginHorizontal(EditorStyles.helpBox);

            EditorGUILayout.LabelField(new GUIContent("Click Sound"), customSkin.FindStyle("Text"), GUILayout.Width(120));
            EditorGUILayout.PropertyField(clickSound, new GUIContent(""));

            GUILayout.EndHorizontal();
        }

        GUILayout.Space(4);
        EditorGUILayout.PropertyField(clickEvent, new GUIContent("On Click Event"), true);
        EditorGUILayout.PropertyField(hoverEvent, new GUIContent("On Hover Event"), true);
    }

    public override void OnResourcesGUI()
    {
        GUILayout.BeginHorizontal(EditorStyles.helpBox);

        EditorGUILayout.LabelField(new GUIContent("Normal Text"), customSkin.FindStyle("Text"), GUILayout.Width(120));
        EditorGUILayout.PropertyField(normalText, new GUIContent(""));

        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal(EditorStyles.helpBox);

        EditorGUILayout.LabelField(new GUIContent("Highlighted Text"), customSkin.FindStyle("Text"), GUILayout.Width(120));
        EditorGUILayout.PropertyField(highlightedText, new GUIContent(""));

        GUILayout.EndHorizontal();

        if (enableButtonSounds.boolValue == true)
        {
            GUILayout.BeginHorizontal(EditorStyles.helpBox);

            EditorGUILayout.LabelField(new GUIContent("Sound Source"), customSkin.FindStyle("Text"), GUILayout.Width(120));
            EditorGUILayout.PropertyField(soundSource, new GUIContent(""));

            if (that.soundSource == null)
            {
                EditorGUILayout.HelpBox("'Sound Source' is not assigned. Go to Resources tab or click the button to create a new audio source.", MessageType.Warning);

                if (GUILayout.Button("Create a new one", customSkin.button))
                {
                    that.soundSource = that.gameObject.AddComponent(typeof(AudioSource)) as AudioSource;
                    currentTab = 1;
                }
            }

            GUILayout.EndHorizontal();
        }

        if (useRipple.boolValue == true)
        {
            GUILayout.BeginHorizontal(EditorStyles.helpBox);

            EditorGUILayout.LabelField(new GUIContent("Ripple Parent"), customSkin.FindStyle("Text"), GUILayout.Width(120));
            EditorGUILayout.PropertyField(rippleParent, new GUIContent(""));

            GUILayout.EndHorizontal();
        }
    }

    public override void OnSettingsGUI()
    {
        GUILayout.BeginHorizontal(EditorStyles.helpBox);

        EditorGUILayout.LabelField(new GUIContent("Animation Solution"), customSkin.FindStyle("Text"), GUILayout.Width(120));
        EditorGUILayout.PropertyField(animationSolution, new GUIContent(""));

        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal(EditorStyles.helpBox);

        EditorGUILayout.LabelField(new GUIContent("Fading Multiplier"), customSkin.FindStyle("Text"), GUILayout.Width(120));
        EditorGUILayout.PropertyField(fadingMultiplier, new GUIContent(""));

        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal(EditorStyles.helpBox);

        useCustomContent.boolValue = GUILayout.Toggle(useCustomContent.boolValue, new GUIContent("Use Custom Content"), customSkin.FindStyle("Toggle"));
        useCustomContent.boolValue = GUILayout.Toggle(useCustomContent.boolValue, new GUIContent(""), customSkin.FindStyle("Toggle Helper"));

        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal(EditorStyles.helpBox);

        enableButtonSounds.boolValue = GUILayout.Toggle(enableButtonSounds.boolValue, new GUIContent("Enable Button Sounds"), customSkin.FindStyle("Toggle"));
        enableButtonSounds.boolValue = GUILayout.Toggle(enableButtonSounds.boolValue, new GUIContent(""), customSkin.FindStyle("Toggle Helper"));

        GUILayout.EndHorizontal();

        if (enableButtonSounds.boolValue == true)
        {
            GUILayout.BeginHorizontal(EditorStyles.helpBox);

            EditorGUILayout.LabelField(new GUIContent("Sound Source"), customSkin.FindStyle("Text"), GUILayout.Width(120));
            EditorGUILayout.PropertyField(soundSource, new GUIContent(""));

            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal(EditorStyles.helpBox);

            useHoverSound.boolValue = GUILayout.Toggle(useHoverSound.boolValue, new GUIContent("Enable Hover Sound"), customSkin.FindStyle("Toggle"));
            useHoverSound.boolValue = GUILayout.Toggle(useHoverSound.boolValue, new GUIContent(""), customSkin.FindStyle("Toggle Helper"));

            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal(EditorStyles.helpBox);

            useClickSound.boolValue = GUILayout.Toggle(useClickSound.boolValue, new GUIContent("Enable Click Sound"), customSkin.FindStyle("Toggle"));
            useClickSound.boolValue = GUILayout.Toggle(useClickSound.boolValue, new GUIContent(""), customSkin.FindStyle("Toggle Helper"));

            GUILayout.EndHorizontal();

            if (that.soundSource == null)
            {
                EditorGUILayout.HelpBox("'Sound Source' is not assigned. Go to Resources tab or click the button to create a new audio source.", MessageType.Warning);

                if (GUILayout.Button("Create a new one", customSkin.button))
                {
                    that.soundSource = that.gameObject.AddComponent(typeof(AudioSource)) as AudioSource;
                    currentTab = 2;
                }
            }
        }

        GUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.Space(-2);
        GUILayout.BeginHorizontal();

        useRipple.boolValue = GUILayout.Toggle(useRipple.boolValue, new GUIContent("Use Ripple"), customSkin.FindStyle("Toggle"));
        useRipple.boolValue = GUILayout.Toggle(useRipple.boolValue, new GUIContent(""), customSkin.FindStyle("Toggle Helper"));

        GUILayout.EndHorizontal();
        GUILayout.Space(4);

        if (useRipple.boolValue == true)
        {
            GUILayout.BeginHorizontal(EditorStyles.helpBox);

            renderOnTop.boolValue = GUILayout.Toggle(renderOnTop.boolValue, new GUIContent("Render On Top"), customSkin.FindStyle("Toggle"));
            renderOnTop.boolValue = GUILayout.Toggle(renderOnTop.boolValue, new GUIContent(""), customSkin.FindStyle("Toggle Helper"));

            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal(EditorStyles.helpBox);

            centered.boolValue = GUILayout.Toggle(centered.boolValue, new GUIContent("Centered"), customSkin.FindStyle("Toggle"));
            centered.boolValue = GUILayout.Toggle(centered.boolValue, new GUIContent(""), customSkin.FindStyle("Toggle Helper"));

            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal(EditorStyles.helpBox);

            EditorGUILayout.LabelField(new GUIContent("Update Mode"), customSkin.FindStyle("Text"), GUILayout.Width(120));
            EditorGUILayout.PropertyField(rippleUpdateMode, new GUIContent(""));

            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal(EditorStyles.helpBox);

            EditorGUILayout.LabelField(new GUIContent("Shape"), customSkin.FindStyle("Text"), GUILayout.Width(120));
            EditorGUILayout.PropertyField(rippleShape, new GUIContent(""));

            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal(EditorStyles.helpBox);

            EditorGUILayout.LabelField(new GUIContent("Speed"), customSkin.FindStyle("Text"), GUILayout.Width(120));
            EditorGUILayout.PropertyField(speed, new GUIContent(""));

            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal(EditorStyles.helpBox);

            EditorGUILayout.LabelField(new GUIContent("Max Size"), customSkin.FindStyle("Text"), GUILayout.Width(120));
            EditorGUILayout.PropertyField(maxSize, new GUIContent(""));

            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal(EditorStyles.helpBox);

            EditorGUILayout.LabelField(new GUIContent("Start Color"), customSkin.FindStyle("Text"), GUILayout.Width(120));
            EditorGUILayout.PropertyField(startColor, new GUIContent(""));

            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal(EditorStyles.helpBox);

            EditorGUILayout.LabelField(new GUIContent("Transition Color"), customSkin.FindStyle("Text"), GUILayout.Width(120));
            EditorGUILayout.PropertyField(transitionColor, new GUIContent(""));

            GUILayout.EndHorizontal();
        }

        GUILayout.EndVertical();
    }
}
