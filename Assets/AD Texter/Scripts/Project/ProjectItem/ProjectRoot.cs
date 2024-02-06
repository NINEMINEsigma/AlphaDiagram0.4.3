using System;
using System.Collections.Generic;
using UnityEngine;
using AD.BASE;
using AD.Experimental.GameEditor;
using AD.Sample.Texter.Internal;
using AD.UI;
using AD.Utility;
using AD.Utility.Object;

namespace AD.Sample.Texter.Project
{
    public class ProjectRoot : MonoBehaviour, IProjectItemRoot
    {
        public class ProjectRootBlock : ProjectItemBlock
        {
            public ProjectRootBlock(ProjectRoot target) : base(target)
            {
                that = target;
            }

            public ProjectRoot that;

            protected override void HowSerialize()
            {
                var data = that.CurrentDataAssets;
                this.MatchItem.SetTitle("Project Assets");
                PropertiesLayout.Label(data.CreaterName, "Project Creater");
                PropertiesLayout.Label(data.DateTime, "Project Created Time");
                var inputD = PropertiesLayout.InputField(data.Description, "Project Description");
                PropertiesLayout.Label(data.AssetsName, "Project Assets Time");
                PropertiesLayout.Button("Description", "Enter Description On A Bigger Field", () =>
                {
                    var temp = GameEditorApp.instance.GetSystem<GameEditorWindowGenerator>()
                    .ObtainElement(new Vector2(800, 600));
                    var mif = ADGlobalSystem.GenerateElement<ModernUIInputField>().PrefabInstantiate();
                    mif.transform.As<RectTransform>().sizeDelta = new Vector2(800, 600);
                    temp
                    .SetADUIOnWindow<ModernUIInputField>("Field", mif)
                    .SetText(data.Description)
                    .AddListener(T =>
                    {
                        data.Description = T;
                        inputD.text = T;
                        temp.BackPool();
                    });
                });
                PropertiesLayout.Button("Save", "Save Project Assets", () => data.Save());
            }
        }

        public ISerializeHierarchyEditor MatchHierarchyEditor { get; set; }
        public List<ISerializePropertiesEditor> MatchPropertiesEditors { get; set; }
        public ICanSerializeOnCustomEditor ParentTarget { get; set; }

        public int SerializeIndex => 0;

        [Header("Prefab")] public List<GameObject> SubProjectItemPrefab = new();

        private void Start()
        {
            MatchHierarchyEditor = new HierarchyBlock<ProjectRoot>(this, nameof(ProjectRoot).Translate());
            MatchPropertiesEditors = new List<ISerializePropertiesEditor>()
            {
                new ProjectRootBlock(this),
                new ProjectItemGeneraterBlock(this,App.Get(SubProjectItemPrefab,false),new(SetupChild))
            };
            GetComponent<EditGroup>().OnEnter.AddListener(EnterMe);
        }

        private void Update()
        {
            /*Vector3 mlt = InternalUtility.AnchoringLeftTop(MyEditGroup), mlb = InternalUtility.AnchoringLeftButtom(MyEditGroup);
            Vector3 mrt = InternalUtility.AnchoringRightTop(MyEditGroup), mrb = InternalUtility.AnchoringRightButtom(MyEditGroup);
            foreach (var child in Childs)
            {
                if (child is MonoBehaviour mono)
                {
                    var eg = mono.GetComponent<EditGroup>();
                    Vector3 lt= InternalUtility.AnchoringLeftTop(eg),lb= InternalUtility.AnchoringLeftButtom(eg);
                    Vector3 rt = InternalUtility.AnchoringRightTop(eg), rb = InternalUtility.AnchoringRightButtom(eg);
                }
            }*/
        }

        public void SetupChild(IProjectItem child)
        {
            child.As<MonoBehaviour>().transform.position = this.transform.position + new Vector3(5, 0, 0);
        }

        public void ClickOnLeft()
        {

        }

        public void ClickOnRight()
        {

        }

        public List<ICanSerializeOnCustomEditor> Childs = new();
        public List<ICanSerializeOnCustomEditor> GetChilds() => Childs;

        public bool IsAbleDisplayedOnHierarchyAtTheSameTime(Type type)
        {
            return true;
        }

        public void SaveProjectSourceData_ReturnVoid()
        {
            SaveProjectSourceData();
        }

        public bool SaveProjectSourceData()
        {
            try
            {
                CurrentDataAssets.Save();
                ADGlobalSystem.AddMessage(nameof(ProjectRoot), "Save");
                App.instance.AddMessage("Save Project Source Data Successful");
                return true;
            }
            catch (Exception ex)
            {
                ADGlobalSystem.AddError(nameof(ProjectRoot), ex);
                App.instance.AddMessage("Save Project Source Data Failed");
                return false;
            }
        }

        public DataAssets CurrentDataAssets => App.instance.GetModel<DataAssets>();

        [Header("Bad Items")] public List<GameObject> badSaveItemsObjects;

        public void SaveDataForStaticEditorBar()
        {
            if (!SaveData(out var badSaveItems))
                badSaveItemsObjects = badSaveItems.GetSubList<GameObject, IProjectItem>(T => T is MonoBehaviour, T => T.As<MonoBehaviour>().gameObject);
            else
                App.instance.GetController<ProjectManager>().SaveProjectData();
        }

        public bool SaveData(out List<IProjectItem> badSaveItems)
        {
            badSaveItems = null;
            if (SaveProjectSourceData())
            {
                bool result = true;
                foreach (var child in Childs)
                {
                    result = child.As<IProjectItem>().SaveData(out List<IProjectItem> temp) && result;
                    if (temp != null)
                    {
                        if (badSaveItems == null) badSaveItems = temp;
                        else badSaveItems.AddRange(temp);
                    }
                }
                return result;
            }
            else
            {
                badSaveItems = new() { this };
                return false;
            }
        }

        //EditGroup

        [SerializeField, Header("EditGroup")] private EditGroup m_EditGroup;
        public EditGroup MyEditGroup => m_EditGroup;

        private void EnterMe()
        {
            if (GameEditorApp.instance.GetController<Hierarchy>().TargetTopObjectEditors.Contains(this.MatchHierarchyEditor)) return;

            List<ISerializeHierarchyEditor> newList = new() { this.MatchHierarchyEditor };
            foreach (ISerializeHierarchyEditor sEditor in GameEditorApp.instance.GetController<Hierarchy>().TargetTopObjectEditors)
            {
                if (IsAbleDisplayedOnHierarchyAtTheSameTime(sEditor.MatchTarget.GetType()))
                    newList.Add(sEditor);
            }
            GameEditorApp.instance.GetController<Hierarchy>().ReplaceTop(newList);
        }

    }
}
