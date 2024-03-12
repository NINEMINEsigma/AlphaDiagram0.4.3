using System.Linq;
using AD.BASE;
using AD.Experimental.Performance;
using AD.UI;
using AD.Utility;
using AD.Utility.Object;
using UnityEngine;

namespace AD.Sample.Texter
{
    public class EntryItem : MonoBehaviour
    {
        public CameraCore MainCamera;

        public Button ClickButton;
        public Text CreaterText;
        public Text AssetsPathText;
        public Text DescriptionText;
        public Text DateTimeText;

        public MainSceneLoader ChoosingPageLoader;

        public static int MaxIndex = 0;
        public int MyIndex;

        private void Start()
        {
            MyData.Load();
            MainCamera = TemplateSceneManager.SceneComponents.FindLayer("Camera Layer").GetComponent<CameraCore>();
            ChoosingPageLoader = TemplateSceneManager.SceneComponents.FindLayerChilds("System Layer")
                .First(T => T.TryGetComponent<MainSceneLoader>(out var result)).GetComponent<MainSceneLoader>();
            ClickButton.AddListener(OnClick);
            ClickButton.SetTitle(MyData.Name);
            CreaterText.SetText(MyData.CreaterName);
            AssetsPathText.SetText(MyData.AssetsName);
            DescriptionText.SetText(MyData.Description);
            DateTimeText.SetText(MyData.DateTime);
        }

        private void OnClick()
        {
            App.instance.RegisterModel(MyData);
            ChoosingPageLoader.Load("ChoosingPage");
            ChoosingPageLoader.sceneLoadAssets.SubBlocks["ChoosingPage"].As<ChoosingPageSubManager>().Setup(MyData);
        }

        public DataAssets MyData = new();
    }
}
