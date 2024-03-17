using System;
using System.Collections.Generic;
using AD.BASE;
using AD.Experimental.GameEditor;
using AD.Experimental.Performance;
using AD.Sample.Texter.Data;
using AD.Sample.Texter.Internal;
using AD.Sample.Texter.Project;
using AD.Sample.Texter.Scene;
using AD.UI;
using AD.Utility;
using AD.Utility.Object;
using UnityEngine;

namespace AD.Sample.Texter
{
    [Serializable]
    public class LLMCoreData : ProjectItemData
    {

        public LLMCoreData(LLMCore projectItem) : base(projectItem)
        {
        }

        public override bool FromMap(ProjectData_BaseMap from)
        {
            if (from is LLMCore_BaseMap data)
            {
                return base.FromMap(from);
            }
            else return false;
        }

        public override void ToMap(out ProjectData_BaseMap BM)
        {
            BM = new LLMCore_BaseMap();
            BM.FromObject(this);
        }
    }

    namespace Data
    {
        [EaseSave3]
        [Serializable]
        public class LLMCore_BaseMap : ProjectData_BaseMap
        {
            public LLMCore_BaseMap()
            {
            }

            public override bool FromObject(ProjectItemData from)
            {
                if (from is LLMCoreData data)
                {
                    return base.FromObject(from);
                }
                else return false;
            }

            public override void ToObject(out ProjectItemData obj)
            {
                obj = new LLMCoreData(null);
                obj.FromMap(this);
            }
        }
    }
}

namespace AD.Sample.Texter.Project
{
    public class LLMCore : MonoBehaviour, IProjectItemWhereNeedInitData, IUpdateOnChange, ICatchCameraRayUpdate, ISetupSingleMenuOnClickRight
    {
        public class LLMCoreBlock : ProjectItemBlock
        {
            public LLMCoreBlock(LLMCore target) : base(target)
            {
                that = target;
            }

            public LLMCore that;

            protected override void HowSerialize()
            {
                var data = that.ProjectLLMSourceData;
                this.MatchItem.SetTitle("LLM Core");

                DisplayProjectID(data);

            }
        }

        public bool IsSetupProjectLLMSourceData = false;
        public LLMCoreData ProjectLLMSourceData;
        public ProjectItemData SourceData { get => ProjectLLMSourceData; set => ProjectLLMSourceData = ADGlobalSystem.FinalCheck(value as LLMCoreData); }
        public ISerializeHierarchyEditor MatchHierarchyEditor { get; set; }
        public List<ISerializePropertiesEditor> MatchPropertiesEditors { get; set; }
        public ICanSerializeOnCustomEditor ParentTarget { get; set; }

        public int SerializeIndex { get; set; }

        public List<GameObject> SubProjectItemPrefab => App.instance.GetModel<PrefabModel>().SubProjectItemPrefabs[nameof(LLMCore)];

        private void Start()
        {
            StartCoroutine(App.WaitForInit(this));
        }

        public void Init()
        {
            MyEditGroup.OnEnter.AddListener(EnterMe);
            if (IsSetupProjectLLMSourceData)
            {
                transform.localPosition = App.GetOriginPosition(ProjectLLMSourceData.ProjectItemPosition);
                this.SetParent(ADGlobalSystem.FinalCheckWithThrow(ProjectItemData.GetParent(ProjectLLMSourceData.ParentItemID)));
            }
            else
            {
                ProjectLLMSourceData = new(this);
            }
            MatchHierarchyEditor = new HierarchyBlock<LLMCore>(this, () => this.SourceData.ProjectItemID);
            MatchPropertiesEditors = new List<ISerializePropertiesEditor>()
            {
                new LLMCoreBlock(this),
                new ProjectItemGeneraterBlock(this,App.Get(SubProjectItemPrefab,false),new(SetupChild))
            };
            App.instance.AddMessage($"Project Item(LLM Core) {ProjectLLMSourceData.ProjectItemID} Setup");
            OnMenu = new()
            {
                [0] = new Dictionary<string, ADEvent>()
                {
                    { "Delete", new(() =>
                        {
                            GameObject.Destroy(gameObject);
                            GameEditorApp.instance.GetSystem<SinglePanelGenerator>().Current.BackPool();
                        })
                    }
                },
                [1] = new Dictionary<string, ADEvent>()
                {
                    { "Initialize", new(() =>
                        {
                            InitCurrentLLM();
                            GameEditorApp.instance.GetSystem<SinglePanelGenerator>().Current.BackPool();
                        })
                    }
                }
            };
        }

        public void InitCurrentLLM()
        {

        }

        private void OnEnable()
        {
            OnChange();
        }

        private void OnDestroy()
        {
            if (ADGlobalSystem.instance == null) return;
            App.instance.GetController<ProjectManager>().CurrentProjectData.Remove(new(ProjectItemBindKey));
            GameEditorApp.instance.GetController<Hierarchy>().RemoveOnTop(this.MatchHierarchyEditor);
            this.ProjectLLMSourceData = null;
            this.MatchHierarchyEditor = null;
            this.MatchPropertiesEditors.Clear();
            this.SetParent(null);
        }

        public void SetupChild(IProjectItem child)
        {
            child.As<MonoBehaviour>().transform.position = this.transform.position + new Vector3(3, 0, 0);
        }

        public string ProjectItemBindKey => this.SourceData.ProjectItemID;

        public void ClickOnLeft()
        {

        }

        public Dictionary<int, Dictionary<string, ADEvent>> OnMenu { get; set; }
        public string OnMenuTitle => "LLM Menu";

        public void ClickOnRight()
        {

        }

        public List<ICanSerializeOnCustomEditor> Childs = new();

        public List<ICanSerializeOnCustomEditor> GetChilds() => Childs;

        public bool IsAbleDisplayedOnHierarchyAtTheSameTime(Type type)
        {
            return type == typeof(LLMCore) || type == typeof(ProjectRoot);
        }

        public bool SaveProjectSourceData()
        {
            try
            {
                var currentData = App.instance.GetController<ProjectManager>().CurrentProjectData;
                ProjectLLMSourceData.ProjectItemPosition = new Vector2(transform.position.x, transform.position.z);
                if (currentData.TryGetValue(new(ProjectItemBindKey), out ProjectItemDataCache data))
                {
                    WasRegisteredOnProjectItemData(data);
                }
                else
                {
                    RegisterOnProjectItemData(currentData);
                }
                ADGlobalSystem.AddMessage(nameof(LLMCore) + " " + SourceData.ProjectItemID.ToString(), "Save");
                App.instance.AddMessage($"Save LLM Core {SourceData.ProjectItemID} Data Successful");
                return true;
            }
            catch (Exception ex)
            {
                ADGlobalSystem.AddError(nameof(LLMCore) + " " + SourceData.ProjectItemID.ToString(), ex);
                App.instance.AddMessage($"Save LLM Core {SourceData.ProjectItemID} Data Failed");
                return false;
            }
        }

        private void RegisterOnProjectItemData(ProjectData currentData)
        {
            InitParentRef();
            this.SourceData.ToMap(out ProjectData_BaseMap bm);
            currentData.Add(new(ProjectItemBindKey), new(ProjectItemBindKey, SourceData, bm));
        }

        private void WasRegisteredOnProjectItemData(ProjectItemDataCache data)
        {
            InitParentRef();
            this.name = SourceData.ProjectItemID;
            data.MatchElement.Set(this.SourceData);
            this.SourceData.ToMap(out ProjectData_BaseMap bm);
            data.MatchElementBM.Set(bm);
        }

        private void InitParentRef()
        {
            //父物体是根或者父物体不存在（未知错误）
            if (this.ParentTarget is ProjectRoot)
                this.SourceData.ParentItemID = ProjectItemData.ProjectRootID;
            //当父物体具有数据类时
            else if (this.ParentTarget is IProjectItemWhereNeedInitData pad)
                this.SourceData.ParentItemID = pad.SourceData.ProjectItemID;
            //未知情况
            else
            {
                ADGlobalSystem.ThrowLogicError("Bad Parent");
            }
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

        public LineRenderer MyLineRenderer;
        public const int PointCount = 100;
        public const float PointCountM = 1 / (float)PointCount;

        private void EnterMe()
        {
            InternalUtility.InternalDefault_EnterMe(this);
        }

        public void ReDrawLine()
        {
            InternalUtility.InternalDefault_ReDrawLine(this, MyLineRenderer, PointCount, PointCountM, this.ParentTarget as IProjectItem);
        }

        public void OnChange()
        {
            if (this.ParentTarget == null)
            {
                MyLineRenderer.positionCount = 0;
            }
            else
            {
                ReDrawLine();
            }

            foreach (var item in Childs.GetSubList<ICanSerializeOnCustomEditor, IUpdateOnChange>())
            {
                item.As<IProjectItem>().ReDrawLine();
            }
        }

        public void OnRayCatching()
        {
            OnChange();
        }
    }
}
