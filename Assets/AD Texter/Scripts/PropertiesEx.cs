using AD.BASE;
using AD.UI;
using UnityEngine;

namespace AD.Experimental.GameEditor
{
    public class PropertiesEx : ADController
    {
        private bool isRegister = false;

        [SerializeField] private CustomWindowElement _CustomWindowElement;
        public CustomWindowElement Window => _CustomWindowElement;

        private void Awake()
        {
            if (!isRegister)
            {
                GameEditorApp.instance.RegisterController(this);
            }

            Window.OnEsc.AddListener(CallBackWindow);
        }

        public override void Init()
        {
            Window.BaseDefaultIsSubPageUsingOtherSetting = true;
            Window.isCanBackPool = false;
            Window.Init();
        }

        public void CallWindow(string title,GameObject Prefab)
        {
            Window.SetTitle(title);
            Window.gameObject.SetActive(true);
            Window.SetItemOnWindow(title, Prefab.PrefabInstantiate());
        }

        public void CallBackWindow()
        {
            Window.Init();
            Window.gameObject.SetActive(false);
        }
    }
}
