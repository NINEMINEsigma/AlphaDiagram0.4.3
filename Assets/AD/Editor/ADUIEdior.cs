using AD.BASE;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public abstract class AbstractCustomADEditor : Editor
{
    protected int currentTab;

    /// <summary>
    /// Get your custom GUI Skin
    /// </summary>
    /// <returns></returns>
    protected virtual GUISkin GetGUISkin()
    {
        return EditorGUIUtility.isProSkin == true
            ? (GUISkin)Resources.Load("Editor\\MUI Skin Dark")
            : (GUISkin)Resources.Load("Editor\\MUI Skin Light");
    }

    protected virtual string TopHeader => "CM Top Header";

    protected virtual void OnEnable()
    {

    }

    public virtual void OnADUIInspectorGUI()
    {
        /*if (Application.isPlaying)
        {
            if (target.As<AD.UI.IADUI>(out var adui))
            {
                GUI.enabled = false;

                GUILayout.BeginHorizontal(EditorStyles.helpBox);
                EditorGUILayout.IntSlider("SerialNumber", adui.SerialNumber, 0, AD.UI.ADUI.TotalSerialNumber - 1);
                EditorGUILayout.TextField("ElementName", adui.ElementName);
                GUILayout.EndHorizontal();

                GUI.enabled = true;
            }
        }
        else HelpBox("ADUI Element Detail Will SerializeField When Playing Mode", MessageType.Info);*/
    }

    public void OnNotChangeGUI(UnityAction action)
    {
        GUI.enabled = false;
        action();
        GUI.enabled = true;
    }

    public void HelpBox(string message, MessageType messageType)
    {
        EditorGUILayout.HelpBox(message, messageType);
    }

    public void HorizontalBlock(UnityAction action)
    {
        GUILayout.BeginHorizontal();
        action();
        GUILayout.EndHorizontal();
    }

    public void HorizontalBlockWithBox(UnityAction action)
    {
        GUILayout.BeginHorizontal(EditorStyles.helpBox);
        action();
        GUILayout.EndHorizontal();
    }

    public void VerticalBlockWithBox(UnityAction action)
    {
        GUILayout.BeginVertical(EditorStyles.helpBox);
        action();
        GUILayout.EndVertical();
    }

    public abstract void OnContentGUI();
    public abstract void OnResourcesGUI();
    public abstract void OnSettingsGUI();

    protected GUISkin customSkin;
    protected Color defaultColor;

    public void Toggle(SerializedProperty enableTrigger, string label)
    {
        GUILayout.BeginHorizontal(EditorStyles.helpBox);

        enableTrigger.boolValue = GUILayout.Toggle(enableTrigger.boolValue, new GUIContent(label), customSkin.FindStyle("Toggle"));
        enableTrigger.boolValue = GUILayout.Toggle(enableTrigger.boolValue, new GUIContent(""), customSkin.FindStyle("Toggle Helper"));

        GUILayout.EndHorizontal();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        defaultColor = GUI.color;

        if (EditorGUIUtility.isProSkin == true)
            customSkin = (GUISkin)Resources.Load("Editor\\ADUI Skin Dark");
        else
            customSkin = (GUISkin)Resources.Load("Editor\\ADUI Skin Light");

        GUILayout.BeginHorizontal();
        GUI.backgroundColor = defaultColor;

        GUILayout.Box(new GUIContent(""), customSkin.FindStyle(TopHeader));

        GUILayout.EndHorizontal();
        GUILayout.Space(-42);

        GUIContent[] toolbarTabs = new GUIContent[3];
        toolbarTabs[0] = new GUIContent("Content");
        toolbarTabs[1] = new GUIContent("Resources");
        toolbarTabs[2] = new GUIContent("Settings");

        GUILayout.BeginHorizontal();
        GUILayout.Space(17);

        currentTab = GUILayout.Toolbar(currentTab, toolbarTabs, customSkin.FindStyle("Tab Indicator"));

        GUILayout.EndHorizontal();
        GUILayout.Space(-40);
        GUILayout.BeginHorizontal();
        GUILayout.Space(17);

        if (GUILayout.Button(new GUIContent("Content", "Content"), customSkin.FindStyle("Tab Content")))
            currentTab = 0;
        if (GUILayout.Button(new GUIContent("Resources", "Resources"), customSkin.FindStyle("Tab Resources")))
            currentTab = 1;
        if (GUILayout.Button(new GUIContent("Settings", "Settings"), customSkin.FindStyle("Tab Settings")))
            currentTab = 2;

        GUILayout.EndHorizontal();

        switch (currentTab)
        {
            case 0:
                {
                    OnADUIInspectorGUI();
                    OnContentGUI();
                }
                break;

            case 1:
                {
                    OnResourcesGUI();
                }
                break;

            case 2:
                {
                    OnSettingsGUI();
                }
                break;
        }

        serializedObject.ApplyModifiedProperties();
    }

    public void OnDefaultInspectorGUI()
    {
        base.OnInspectorGUI();
    }

    public void MakeUpNumericManager(string thatNumericManagerName)
    {
        SerializedProperty property = serializedObject.FindProperty(thatNumericManagerName);
        VerticalBlockWithBox(() =>
        {
            if (property.stringValue.StartsWith("Default"))
                HelpBox("Numeric Manager Is Idle", MessageType.Info);
            else
                HelpBox("You Can Name It Start With Default To Make It Idle", MessageType.Info);
            EditorGUILayout.PropertyField(property);
        });
    }
}

public abstract class IADUIEditor : AbstractCustomADEditor
{
    private AD.UI.IADUI __target = null;

    protected override void OnEnable()
    {
        base.OnEnable();
        this.__target = (AD.UI.IADUI)target;
    }

    public override void OnADUIInspectorGUI()
    {
        GUI.enabled = false;

        if (Application.isPlaying)
        {
            EditorGUILayout.IntSlider("SerialNumber", __target.SerialNumber, 0, AD.UI.ADUI.TotalSerialNumber - 1);
            EditorGUILayout.TextField("ElementName", __target.ElementName);
            //EditorGUILayout.TextField("ElementArea", that.ElementArea);
        }

        GUI.enabled = true;
    }
}

public abstract class ADUIEditor : IADUIEditor
{
    private AD.UI.IADUI _target = null;

    protected override void OnEnable()
    {
        base.OnEnable();
        _target = (AD.UI.IADUI)target;
    }

    /// <summary>
    /// Make ADUI's default InspectorGUI part
    /// </summary>
    public override void OnADUIInspectorGUI()
    {
        if (Application.isPlaying)
        {
            GUI.enabled = false;

            GUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUILayout.IntSlider("SerialNumber", _target.SerialNumber, 0, AD.UI.ADUI.TotalSerialNumber - 1);
            EditorGUILayout.TextField("ElementName", _target.ElementName);
            GUILayout.EndHorizontal();
            if (_target.As<AD.UI.ADUI>(out var adui))
            {
                GUILayout.BeginHorizontal(EditorStyles.helpBox);
                EditorGUILayout.TextField("ElementArea", adui.ElementArea);

                GUI.enabled = true;

                EditorGUILayout.Toggle("IsSelect", adui.Selected, customSkin.GetStyle("Toggle"));
                GUILayout.EndHorizontal();
            }
            else if(_target.As<AD.UI.ViewController>(out var viewC))
            {
                GUILayout.BeginHorizontal(EditorStyles.helpBox);
                EditorGUILayout.TextField("ElementArea", viewC.ElementArea);

                GUI.enabled = true;

                EditorGUILayout.Toggle("IsSelect", viewC.Selected, customSkin.GetStyle("Toggle"));
                GUILayout.EndHorizontal();
            }
        }
        else HelpBox("ADUI Element Detail Will SerializeField When Playing Mode", MessageType.Info);
    }
}

public abstract class ADEditorWindow : EditorWindow
{
    /*
     [MenuItem("MyWindow/Window")]
    static void window()
    {
        Mybianyi mybianyi = GetWindow<Mybianyi>();
        mybianyi.Show();
    }
    private void OnGUI()
     */
}
