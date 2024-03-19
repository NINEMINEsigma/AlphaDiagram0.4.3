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
            Window.OnEsc.AddListener(CallBackWindow);

            if (!isRegister)
            {
                GameEditorApp.instance.RegisterController(this);
            }
        }

        public override void Init()
        {
            Window.BaseDefaultIsSubPageUsingOtherSetting = true;
            Window.Init();
            Window.isCanRefresh = false;
            Window.isCanBackPool = false;
        }

        public GameObject CallWindow(string title,GameObject Prefab)
        {
            Window.SetTitle(title);
            Window.gameObject.SetActive(true);
            GameObject temp = Prefab.PrefabInstantiate();
            Window.SetItemOnWindow(title, temp);
            return temp;
        }

        public void CallBackWindow()
        {
            Init();
            Window.gameObject.SetActive(false);
        }
    }
}
