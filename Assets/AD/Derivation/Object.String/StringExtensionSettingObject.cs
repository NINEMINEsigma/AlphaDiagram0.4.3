using UnityEngine;

namespace AD.Utility.Object
{
    public class StringExtensionSettingObject : MonoBehaviour
    {
        public AD.Utility.Object.StringTranslater setting;

        private void Awake()
        {
            setting.Set();
        }

        private void Start()
        {
            gameObject.SetActive(false);
        }
    }
}
