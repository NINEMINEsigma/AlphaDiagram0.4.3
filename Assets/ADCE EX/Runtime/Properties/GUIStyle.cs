using AD.UI;
using UnityEngine;

namespace AD.Experimental.GameEditor
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "New GUIStyle", menuName = "AD/GUIStyle", order = 11)]
    public class GUIStyle : AD.Experimental.EditorAsset.Cache.AbstractScriptableObject
    {
        public UnityEngine.GameObject Prefab;
        public string TypeName;

        public void OnValidate()
        {
            if (this.Prefab != null)
            {
                BindKey = Prefab.name;
                var targetADUIs = Prefab.GetComponents<IADUI>();
                if (targetADUIs == null || targetADUIs.Length == 0) targetADUIs = Prefab.GetComponentsInChildren<IADUI>();
                TypeName = targetADUIs[0].GetType().Name;
            }
        }
    }
}
