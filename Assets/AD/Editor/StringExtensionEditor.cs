using UnityEditor;
using UnityEngine;

namespace AD.Utility.Object
{
    [CustomEditor(typeof(AD.Utility.Object.StringTranslater))]
    public class StringExtensionEditor : Editor
    {
        private AD.Utility.Object.StringTranslater that = null;

        private void OnEnable()
        {
            that = target as AD.Utility.Object.StringTranslater;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            if (GUILayout.Button("Sort"))
            {
                foreach (var item in that.Packages)
                {
                    item.values.Sort((T, P) => T.key.CompareTo(P.key));
                }
            }

            if (GUILayout.Button("GenerateObject"))
            {
                StringExtensionSettingObject obj = new GameObject("StringExtensionSetting").AddComponent<StringExtensionSettingObject>();
                obj.setting = that;
            }

            serializedObject.ApplyModifiedProperties();

        }

    }
}
