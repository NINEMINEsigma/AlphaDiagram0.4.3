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
    public class ProjectRoot : MonoBehaviour, IProjectItemRoot, ICatchCameraRayUpdate, ISetupSingleMenuOnClickRight
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
                PropertiesLayout.ModernUIButton("Build Offline File", "", () =>
                {
                    App.instance.GetController<ProjectManager>().CreateOfflineFile();
                });
                PropertiesLayout.ModernUIButton("Build From Offline", "", () =>
                {
                    FileC.SelectFileOnSystem("")
                });
            }
        }

        public ISerializeHierarchyEditor MatchHierarchyEditor { get; set; }
        public List<ISerializePropertiesEditor> MatchPropertiesEditors { get; set; }
        public ICanSerializeOnCustomEditor ParentTarget { get; set; }

        public int SerializeIndex => 0;

        public List<GameObject> SubProjectItemPrefab => App.instance.GetModel<PrefabModel>().SubProjectItemPrefabs[nameof(ProjectRoot)];

        private void Start()
        {
            MatchHierarchyEditor = new HierarchyBlock<ProjectRoot>(this, nameof(ProjectRoot).Translate());
            MatchPropertiesEditors = new List<ISerializePropertiesEditor>()
            {
                new ProjectRootBlock(this),
                new ProjectItemGeneraterBlock(this,App.Get(SubProjectItemPrefab,false),new(SetupChild))
            };
            GetComponent<EditGroup>().OnEnter.AddListener(EnterMe);
            OnMenu = new()
            {
                [0] = new Dictionary<string, ADEvent>()
                {
                    {"Clear",new(()=>
                        {
                            GameEditorApp.instance.GetController<Hierarchy>().ReplaceTop(new List<ISerializeHierarchyEditor>(){ this.MatchHierarchyEditor});
                            GameEditorApp.instance.GetSystem<SinglePanelGenerator>().Current.BackPool();
                        })
                    }
                }
            };
            App.instance.AddMessage("Project Root Setup");
        }
        public Dictionary<int, Dictionary<string, ADEvent>> OnMenu { get; set; }
        public string OnMenuTitle => "Root Menu";

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
                if (sEditor.MatchTarget.GetType() == typeof(ProjectRoot) || IsAbleDisplayedOnHierarchyAtTheSameTime(sEditor.MatchTarget.GetType()))
                    newList.Add(sEditor);
            }
            GameEditorApp.instance.GetController<Hierarchy>().ReplaceTop(newList);
        }

        public void OnRayCatching()
        {
            foreach (var item in Childs.GetSubList<ICanSerializeOnCustomEditor, IUpdateOnChange>())
            {
                item.OnChange();
            }
        }
    }
}
