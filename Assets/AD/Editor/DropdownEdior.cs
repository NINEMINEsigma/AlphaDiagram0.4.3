using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AD.UI.Dropdown)), CanEditMultipleObjects]
public class DropdownEdior : ADUIEditor
{
    private AD.UI.Dropdown that = null;

    SerializedProperty OnSelect;
    SerializedProperty options;

    string currentKey = "";

    public override void OnContentGUI()
    {
        this.HorizontalBlockWithBox(() => this.OnNotChangeGUI(() =>
            {
                EditorGUILayout.TextField("Current Select", that.CurrentSelectOption);
            }));
        this.OnNotChangeGUI(() => EditorGUILayout.PropertyField(options));
        this.HorizontalBlockWithBox(() =>
        {
            currentKey = EditorGUILayout.TextField("New Option", currentKey);
        });
        this.HorizontalBlock(() =>
        {
            if (GUILayout.Button("Add")) that.AddOption(currentKey);
            if (GUILayout.Button("Remove")) that.RemoveOption(currentKey);
            if (GUILayout.Button("Clear")) that.ClearOptions();
        });
    }

    public override void OnResourcesGUI()
    {

    }

    public override void OnSettingsGUI()
    {
        EditorGUILayout.PropertyField(OnSelect);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        that = target as AD.UI.Dropdown;
        OnSelect = serializedObject.FindProperty(nameof(OnSelect));
        options = serializedObject.FindProperty(nameof(options));
    }
}
