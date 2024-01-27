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
        //public ConeAllegationItem coneAllegationItem;

        public Button ClickButton;

        public MainSceneLoader ChoosingPageLoader;

        public static int MaxIndex = 0;
        public int MyIndex;

        private void Start()
        {
            MainCamera = TemplateSceneManager.SceneComponents.FindLayer("Camera Layer").GetComponent<CameraCore>();
            ChoosingPageLoader = TemplateSceneManager.SceneComponents.FindLayerChilds("System Layer")
                .First(T => T.TryGetComponent<MainSceneLoader>(out var result)).GetComponent<MainSceneLoader>();
            //coneAllegationItem.Allegation = MainCamera.Core.gameObject.GetComponent<ConeAllegation>();
            //coneAllegationItem.OnEnCone.AddListener(FaceAt);
            //coneAllegationItem.OnQuCone.AddListener(SetupPosition);
            //coneAllegationItem.Allegation.Items.Add(coneAllegationItem);
            //SetupPosition();
            ClickButton.AddListener(OnClick);
            ClickButton.SetTitle(MyData.Name);
        }

        public void FaceAt()
        {
            transform.FaceAt(MainCamera.transform);
        }

        private void SetupPosition()
        {
            transform.position = GetPos();
        }

        public static bool IsCanUpdateMove = true;

        //private void Update()
        //{
        //    if(IsCanUpdateMove)
        //    {
        //        transform.position = Vector3.Lerp(transform.position, GetPos(), 0.5f);
        //        FaceAt();
        //    }
        //}

        private Vector3 GetPos()
        {
            var t = Mathf.PI * 2 * MyIndex / (float)MaxIndex - Time.time * 0.03f;
            float z = Mathf.Cos(t) * 30, x = Mathf.Sin(t) * 30;
            return new Vector3(x, 0, z);
        }

        private void OnClick()
        {
            MyData.Load();
            App.instance.RegisterModel(MyData);
            ChoosingPageLoader.Load("ChoosingPage");
            ChoosingPageLoader.sceneLoadAssets.SubBlocks["ChoosingPage"].As<ChoosingPageSubManager>().Setup(MyData);
        }

        public DataAssets MyData = new();
    }
}
