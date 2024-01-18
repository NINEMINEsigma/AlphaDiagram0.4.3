using System.IO;
using System.Linq;
using AD.BASE;
using AD.Experimental.Performance;
using AD.UI;
using AD.Utility;
using AD.Utility.Object;
using UnityEngine;

namespace AD.Simple.Texter
{
    public class EntryItem : MonoBehaviour
    {
        public CameraCore MainCamera;
        public ConeAllegationItem coneAllegationItem;

        public Button ClickButton;

        public MainSceneLoader ChoosingPageLoader;

        private void Start()
        {
            MainCamera = TemplateSceneManager.SceneComponents.FindLayer("Camera Layer").GetComponent<CameraCore>();
            ChoosingPageLoader = TemplateSceneManager.SceneComponents.FindLayerChilds("System Layer")
                .First(T => T.TryGetComponent<MainSceneLoader>(out var result)).GetComponent<MainSceneLoader>();
            coneAllegationItem.Allegation = MainCamera.Core.gameObject.GetComponent<ConeAllegation>();
            coneAllegationItem.OnEnCone.AddListener(FaceAt);
            coneAllegationItem.OnQuCone.AddListener(SetupPosition);
            coneAllegationItem.Allegation.Items.Add(coneAllegationItem);
            SetupPosition();
            ClickButton.AddListener(OnClick);
            ClickButton.SetTitle(MyData.Name);
        }

        public void FaceAt()
        {
            transform.FaceAt(MainCamera.transform);
        }

        private void SetupPosition()
        {
            transform.position = new Vector3(Random.value - 0.5f, Random.value - 0.5f, 0) * 10 + new Vector3(0, 0, 25);
        }

        public static bool IsCanUpdateMove = true;

        private void Update()
        {
            if(IsCanUpdateMove)
            {
                transform.position.AddX(Time.deltaTime * (Random.value - 0.5f)).AddY(Time.deltaTime * (Random.value - 0.5f));
            }
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
