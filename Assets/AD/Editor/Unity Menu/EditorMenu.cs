using UnityEditor;

public class EditorMenu
{
    [MenuItem("Tools/AD/InstanceID", priority = 0)]
    private static void ShowInstanceIDToolsWindow()
    {
        var window = (InstanceIDToolsWindow)EditorWindow.GetWindow(typeof(InstanceIDToolsWindow), false, "InstanceID Tools");
        window.Show();
    }

    [MenuItem("Tools/AD/EditorNotification", priority = 20)]
    private static void ShowEditorNotificationToolsWindow()
    {
        var window = (EditorNotificationToolsWindow)EditorWindow.GetWindow(typeof(EditorNotificationToolsWindow), false, "EditorNotification Tools");
        window.Show();
    }
}
