using AD.UI;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ModernUIDropdown))]
public class ModernUIDropdownEditor : ADUIEditor
{
    ModernUIDropdown that;

    SerializedProperty dropdownItems;
    SerializedProperty triggerObject;
    SerializedProperty itemParent;
    SerializedProperty itemObject;
    SerializedProperty scrollbar;
    SerializedProperty listParent;
    SerializedProperty enableIcon;
    SerializedProperty enableTrigger;
    SerializedProperty enableScrollbar;
    SerializedProperty setHighPriorty;
    SerializedProperty outOnPointerExit;
    SerializedProperty isListItem;
    SerializedProperty animationType;
    SerializedProperty maxSelect;
    SerializedProperty title;
    SerializedProperty icon;
    SerializedProperty OnSelect;

    protected override void OnEnable()
    {
        base.OnEnable();
        that = (ModernUIDropdown)target;

        dropdownItems = serializedObject.FindProperty("dropdownItems");
        triggerObject = serializedObject.FindProperty("triggerObject");
        itemParent = serializedObject.FindProperty("itemParent");
        itemObject = serializedObject.FindProperty("itemObject");
        scrollbar = serializedObject.FindProperty("scrollbar");
        listParent = serializedObject.FindProperty("listParent");
        enableIcon = serializedObject.FindProperty("enableIcon");
        enableTrigger = serializedObject.FindProperty("enableTrigger");
        enableScrollbar = serializedObject.FindProperty("enableScrollbar");
        setHighPriorty = serializedObject.FindProperty("setHighPriorty");
        outOnPointerExit = serializedObject.FindProperty("outOnPointerExit");
        isListItem = serializedObject.FindProperty("isListItem");
        animationType = serializedObject.FindProperty("animationType");
        maxSelect = serializedObject.FindProperty("maxSelect");
        title = serializedObject.FindProperty("title");
        icon = serializedObject.FindProperty("icon");
        OnSelect = serializedObject.FindProperty("OnSelect");
    }

    public override void OnContentGUI()
    {
        GUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUI.indentLevel = 1;

        EditorGUILayout.PropertyField(dropdownItems, new GUIContent("Dropdown Items"), true);
        dropdownItems.isExpanded = true;

        EditorGUI.indentLevel = 0;

        if (GUILayout.Button("+  Add a new item", customSkin.button))
            that.AddNewItem();

        GUILayout.EndVertical();

        if (GUILayout.Button("---  Refresh  ---", customSkin.button))
            that.SetupDropdown();
    }

    public override void OnResourcesGUI()
    {
        GUILayout.BeginHorizontal(EditorStyles.helpBox);

        EditorGUILayout.LabelField(new GUIContent("Title"), customSkin.FindStyle("Text"), GUILayout.Width(120));
        EditorGUILayout.PropertyField(title, new GUIContent(""));

        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal(EditorStyles.helpBox);

        EditorGUILayout.LabelField(new GUIContent("Icon"), customSkin.FindStyle("Text"), GUILayout.Width(120));
        EditorGUILayout.PropertyField(icon, new GUIContent(""));

        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal(EditorStyles.helpBox);

        EditorGUILayout.LabelField(new GUIContent("Trigger Object"), customSkin.FindStyle("Text"), GUILayout.Width(120));
        EditorGUILayout.PropertyField(triggerObject, new GUIContent(""));

        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal(EditorStyles.helpBox);

        EditorGUILayout.LabelField(new GUIContent("Item Prefab"), customSkin.FindStyle("Text"), GUILayout.Width(120));
        EditorGUILayout.PropertyField(itemObject, new GUIContent(""));

        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal(EditorStyles.helpBox);

        EditorGUILayout.LabelField(new GUIContent("Item Parent"), customSkin.FindStyle("Text"), GUILayout.Width(120));
        EditorGUILayout.PropertyField(itemParent, new GUIContent(""));

        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal(EditorStyles.helpBox);

        EditorGUILayout.LabelField(new GUIContent("Scrollbar"), customSkin.FindStyle("Text"), GUILayout.Width(120));
        EditorGUILayout.PropertyField(scrollbar, new GUIContent(""));

        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal(EditorStyles.helpBox);

        EditorGUILayout.LabelField(new GUIContent("List Parent"), customSkin.FindStyle("Text"), GUILayout.Width(120));
        EditorGUILayout.PropertyField(listParent, new GUIContent(""));

        GUILayout.EndHorizontal();
    }

    public override void OnSettingsGUI()
    {
        GUILayout.BeginHorizontal(EditorStyles.helpBox);

        EditorGUILayout.PropertyField(maxSelect);

        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal(EditorStyles.helpBox);

        enableIcon.boolValue = GUILayout.Toggle(enableIcon.boolValue, new GUIContent("Enable Icon"), customSkin.FindStyle("Toggle"));
        enableIcon.boolValue = GUILayout.Toggle(enableIcon.boolValue, new GUIContent(""), customSkin.FindStyle("Toggle Helper"));

        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal(EditorStyles.helpBox);

        enableTrigger.boolValue = GUILayout.Toggle(enableTrigger.boolValue, new GUIContent("Enable Trigger"), customSkin.FindStyle("Toggle"));
        enableTrigger.boolValue = GUILayout.Toggle(enableTrigger.boolValue, new GUIContent(""), customSkin.FindStyle("Toggle Helper"));

        GUILayout.EndHorizontal();

        if (enableTrigger.boolValue == true && that.triggerObject == null)
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.HelpBox("'Trigger Object' is not assigned. Go to Resources tab and assign the correct variable.", MessageType.Error);
            GUILayout.EndHorizontal();
        }

        GUILayout.BeginHorizontal(EditorStyles.helpBox);

        enableScrollbar.boolValue = GUILayout.Toggle(enableScrollbar.boolValue, new GUIContent("Enable Scrollbar"), customSkin.FindStyle("Toggle"));
        enableScrollbar.boolValue = GUILayout.Toggle(enableScrollbar.boolValue, new GUIContent(""), customSkin.FindStyle("Toggle Helper"));

        GUILayout.EndHorizontal();

        if (that.scrollbar != null)
        {
            if (enableScrollbar.boolValue == true)
                that.scrollbar.SetActive(true);
            else
                that.scrollbar.SetActive(false);
        }

        else
        {
            if (enableScrollbar.boolValue == true)
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.HelpBox("'Scrollbar' is not assigned. Go to Resources tab and assign the correct variable.", MessageType.Error);
                GUILayout.EndHorizontal();
            }
        }

        GUILayout.BeginHorizontal(EditorStyles.helpBox);

        setHighPriorty.boolValue = GUILayout.Toggle(setHighPriorty.boolValue, new GUIContent("Set High Priorty"), customSkin.FindStyle("Toggle"));
        setHighPriorty.boolValue = GUILayout.Toggle(setHighPriorty.boolValue, new GUIContent(""), customSkin.FindStyle("Toggle Helper"));

        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal(EditorStyles.helpBox);

        outOnPointerExit.boolValue = GUILayout.Toggle(outOnPointerExit.boolValue, new GUIContent("Out On Pointer Exit"), customSkin.FindStyle("Toggle"));
        outOnPointerExit.boolValue = GUILayout.Toggle(outOnPointerExit.boolValue, new GUIContent(""), customSkin.FindStyle("Toggle Helper"));

        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal(EditorStyles.helpBox);

        isListItem.boolValue = GUILayout.Toggle(isListItem.boolValue, new GUIContent("Is List Item"), customSkin.FindStyle("Toggle"));
        isListItem.boolValue = GUILayout.Toggle(isListItem.boolValue, new GUIContent(""), customSkin.FindStyle("Toggle Helper"));

        GUILayout.EndHorizontal();

        if (isListItem.boolValue == true && that.listParent == null)
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.HelpBox("'List Parent' is not assigned. Go to Resources tab and assign the correct variable.", MessageType.Error);
            GUILayout.EndHorizontal();
        }

        GUILayout.BeginHorizontal(EditorStyles.helpBox);

        EditorGUILayout.LabelField(new GUIContent("Animation Type"), customSkin.FindStyle("Text"), GUILayout.Width(120));
        EditorGUILayout.PropertyField(animationType, new GUIContent(""));

        GUILayout.EndHorizontal();

        EditorGUILayout.PropertyField(OnSelect);
    }
}
