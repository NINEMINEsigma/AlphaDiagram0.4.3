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
    public class ScriptItemEntry
    {
        public string Name = "";
        public string Words = "";
        public const string NoVoice = "No Voice";
        public string SoundAssets = NoVoice;
        public string Command = "";
    }

    [Serializable]
    public class SceneEndingSelectOption
    {
        public string TargetScriptSceneID = "";
        public string OptionName = "";
    }

    [Serializable]
    public class ProjectScriptSlicerData : ProjectItemData
    {
        public List<ScriptItemEntry> Items;
        public List<SceneEndingSelectOption> Options;
        public string BackgroundImage = NoBackgroundImage;
        public const string NoBackgroundImage = "No BackgroundImage";

        public ProjectScriptSlicerData(ProjectScriptSlicer projectItem, List<ScriptItemEntry> items, List<SceneEndingSelectOption> options, string backgroundImage) : base(projectItem)
        {
            Items = items;
            Options = options;
            BackgroundImage = backgroundImage;
        }

        public override bool FromMap(ProjectData_BaseMap from)
        {
            if (from is ProjectScriptSlicer_BaseMap data)
            {
                data.Items = Items;
                data.Options = Options;
                data.BackgroundImage = BackgroundImage;
                return base.FromMap(from);
            }
            else return false;
        }

        public override void ToMap(out ProjectData_BaseMap BM)
        {
            BM = new ProjectScriptSlicer_BaseMap(Items, Options, BackgroundImage);
            BM.FromObject(this);
        }
    }

    namespace Data
    {
        [EaseSave3]
        [Serializable]
        public class ProjectScriptSlicer_BaseMap : ProjectData_BaseMap
        {
            public List<ScriptItemEntry> Items;
            public List<SceneEndingSelectOption> Options;
            public string BackgroundImage;

            public ProjectScriptSlicer_BaseMap(List<ScriptItemEntry> items, List<SceneEndingSelectOption> options, string backgroundImage)
            {
                Items = items;
                Options = options;
                BackgroundImage = backgroundImage;
            }

            public override bool FromObject(ProjectItemData from)
            {
                if (from is ProjectScriptSlicerData data)
                {
                    data.Items = Items;
                    data.Options = Options;
                    data.BackgroundImage = BackgroundImage;
                    return base.FromObject(from);
                }
                else return false;
            }

            public override void ToObject(out ProjectItemData obj)
            {
                obj = new ProjectScriptSlicerData(null, Items, Options, BackgroundImage);
                obj.FromMap(this);
            }
        }
    }
}

namespace AD.Sample.Texter.Project
{
    public class ProjectScriptSlicer : MonoBehaviour, IProjectItemWhereNeedInitData, IUpdateOnChange, ICatchCameraRayUpdate, ISetupSingleMenuOnClickRight
    {
        public class ProjectScriptSlicingBlock : ProjectItemBlock
        {
            public ProjectScriptSlicingBlock(ProjectScriptSlicer target) : base(target)
            {
                that = target;
            }

            public ProjectScriptSlicer that;

            protected override void HowSerialize()
            {
                var data = that.ProjectScriptSlicingSourceData;
                this.MatchItem.SetTitle("Script Slicer");

                DisplayProjectID(data);

                PropertiesLayout.Title("背景");
                PropertiesLayout
                    .InputField(that.ProjectScriptSlicingSourceData.BackgroundImage, "背景图片路径")
                    .AddListener(T => that.ProjectScriptSlicingSourceData.BackgroundImage = T);
                PropertiesLayout.ModernUIButton("背景", "寻找背景图片", () =>
                {
                    FileC.SelectFileOnSystem(T =>
                    {
                        that.ProjectScriptSlicingSourceData.BackgroundImage = T;
                    }, "图片", "", "jpg", "png");
                });

                PropertiesLayout.Title("语句");
                PropertiesLayout.ModernUIButton("添加新的语句", "添加新的语句", () =>
                {
                    that.ProjectScriptSlicingSourceData.Items ??= new();
                    that.ProjectScriptSlicingSourceData.Items.Add(new()
                    {
                        Name = that.ProjectScriptSlicingSourceData.Items.Count > 1 ? that.ProjectScriptSlicingSourceData.Items[^2].Name : "新的角色",
                        Words = "(语句)"
                    });
                    GameEditorApp.instance.GetController<Properties>().ClearAndRefresh();
                });
                if (that.ProjectScriptSlicingSourceData.Items != null)
                {
                    foreach (var item in that.ProjectScriptSlicingSourceData.Items)
                    {
                        SingleLine(item);
                    }
                }

                PropertiesLayout.Title("选项");
                PropertiesLayout.ModernUIButton("添加新的选项", "添加新的选项", () =>
                {
                    that.ProjectScriptSlicingSourceData.Options ??= new();
                    that.ProjectScriptSlicingSourceData.Options.Add(new()
                    {
                        TargetScriptSceneID = that.ProjectScriptSlicingSourceData.Options.Count > 1 ? that.ProjectScriptSlicingSourceData.Options[^1].TargetScriptSceneID : "未知的目标",
                        OptionName = "(选项)"
                    });
                    GameEditorApp.instance.GetController<Properties>().ClearAndRefresh();
                });
                if (that.ProjectScriptSlicingSourceData.Options != null)
                {
                    foreach (var item in that.ProjectScriptSlicingSourceData.Options)
                    {
                        SingleLine(item);
                    }
                }

                PropertiesLayout.Button("Open", "Open Sub Project Scene", () =>
                {
                    App.instance.CurrentProjectItemData = that.ProjectScriptSlicingSourceData;
                    App.instance.GetController<MainSceneLoader>().Load<ScriptSlicerManager>(nameof(ProjectScriptSlicer));
                });
            }

            private void SingleLine(ScriptItemEntry entry)
            {
                PropertiesLayout.BeginHorizontal();
                {
                    PropertiesLayout.Label("角色名");
                    PropertiesLayout.InputField(entry.Name, "角色名").AddListener(T => entry.Name = T);
                }
                PropertiesLayout.EndHorizontal();
                PropertiesLayout.BeginHorizontal();
                {
                    PropertiesLayout.Label("声音");
                    PropertiesLayout.InputField(entry.SoundAssets, "引用的声音资源名称").AddListener(T => entry.SoundAssets = T);
                }
                PropertiesLayout.EndHorizontal();
                PropertiesLayout.ModernUIInputField(entry.Words, "当前对象说的话").AddListener(T => entry.Words = T);
                PropertiesLayout.InputField(entry.Command, "执行到该处时发送的指令").AddListener(T => entry.Command = T);
                PropertiesLayout.Button("删除", "删除", () =>
                {
                    that.ProjectScriptSlicingSourceData.Items.Remove(entry);
                    GameEditorApp.instance.GetController<Properties>().ClearAndRefresh();
                });
            }

            private void SingleLine(SceneEndingSelectOption entry)
            {
                PropertiesLayout.BeginHorizontal();
                {
                    PropertiesLayout.Label("目标场景ID");
                    PropertiesLayout.InputField(entry.TargetScriptSceneID, "目标场景ID");
                }
                PropertiesLayout.EndHorizontal();
                PropertiesLayout.BeginHorizontal();
                {
                    PropertiesLayout.Label("选项文本");
                    PropertiesLayout.InputField(entry.OptionName, "选项文本");
                }
                PropertiesLayout.EndHorizontal();
                PropertiesLayout.Button("删除", "删除", () =>
                {
                    that.ProjectScriptSlicingSourceData.Options.Remove(entry);
                    GameEditorApp.instance.GetController<Properties>().ClearAndRefresh();
                });
            }
        }

        public bool IsSetupProjectScriptSlicingSourceData = false;
        public ProjectScriptSlicerData ProjectScriptSlicingSourceData;
        public ProjectItemData SourceData { get => ProjectScriptSlicingSourceData; set => ProjectScriptSlicingSourceData = ADGlobalSystem.FinalCheck(value as ProjectScriptSlicerData); }
        public ISerializeHierarchyEditor MatchHierarchyEditor { get; set; }
        public List<ISerializePropertiesEditor> MatchPropertiesEditors { get; set; }
        public ICanSerializeOnCustomEditor ParentTarget { get; set; }

        public int SerializeIndex { get; set; }

        public List<GameObject> SubProjectItemPrefab => App.instance.GetModel<PrefabModel>().SubProjectItemPrefabs[nameof(ProjectScriptSlicer)];

        private void Start()
        {
            StartCoroutine(App.WaitForInit(this));
        }

        public void Init()
        {
            MyEditGroup.OnEnter.AddListener(EnterMe);
            if (IsSetupProjectScriptSlicingSourceData)
            {
                transform.localPosition = App.GetOriginPosition(ProjectScriptSlicingSourceData.ProjectItemPosition);
                this.SetParent(ADGlobalSystem.FinalCheckWithThrow(ProjectItemData.GetParent(ProjectScriptSlicingSourceData.ParentItemID)));
            }
            else
            {
                ProjectScriptSlicingSourceData = new(this, new(), null, "");
            }
            MatchHierarchyEditor = new HierarchyBlock<ProjectScriptSlicer>(this, () => this.SourceData.ProjectItemID);
            MatchPropertiesEditors = new List<ISerializePropertiesEditor>()
            {
                new ProjectScriptSlicingBlock(this),
                new ProjectItemGeneraterBlock(this,App.Get(SubProjectItemPrefab,false),new(SetupChild))
            };
            App.instance.AddMessage($"Project Item(Script Slicing) {ProjectScriptSlicingSourceData.ProjectItemID} Setup");
            OnMenu = new()
            {
                [0] = new Dictionary<string, ADEvent>()
                {
                    {"Delete",new(()=>
                        {
                            GameObject.Destroy(gameObject);
                            GameEditorApp.instance.GetSystem<SinglePanelGenerator>().Current.BackPool();
                        })
                    }
                }
            };
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
            this.ProjectScriptSlicingSourceData = null;
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
        public string OnMenuTitle => "Slicer Menu";

        public void ClickOnRight()
        {

        }

        public List<ICanSerializeOnCustomEditor> Childs = new();

        public List<ICanSerializeOnCustomEditor> GetChilds() => Childs;

        public bool IsAbleDisplayedOnHierarchyAtTheSameTime(Type type)
        {
            return type == this.GetType();
        }

        public bool SaveProjectSourceData()
        {
            try
            {
                var currentData = App.instance.GetController<ProjectManager>().CurrentProjectData;
                ProjectScriptSlicingSourceData.ProjectItemPosition = new Vector2(transform.position.x, transform.position.z);
                if (currentData.TryGetValue(new(ProjectItemBindKey), out ProjectItemDataCache data))
                {
                    WasRegisteredOnProjectItemData(data);
                }
                else
                {
                    RegisterOnProjectItemData(currentData);
                }
                ADGlobalSystem.AddMessage(nameof(ProjectScriptSlicer) + " " + SourceData.ProjectItemID.ToString(), "Save");
                App.instance.AddMessage($"Save Slicer {SourceData.ProjectItemID} Data Successful");
                return true;
            }
            catch (Exception ex)
            {
                ADGlobalSystem.AddError(nameof(ProjectScriptSlicer) + " " + SourceData.ProjectItemID.ToString(), ex);
                App.instance.AddMessage($"Save Slicer {SourceData.ProjectItemID} Data Failed");
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
            if (GameEditorApp.instance.GetController<Hierarchy>().TargetTopObjectEditors.Contains(this.MatchHierarchyEditor)) return;

            List<ISerializeHierarchyEditor> newList = new() { this.MatchHierarchyEditor };
            foreach (ISerializeHierarchyEditor sEditor in GameEditorApp.instance.GetController<Hierarchy>().TargetTopObjectEditors)
            {
                if (sEditor.MatchTarget.GetType() == typeof(ProjectRoot) || IsAbleDisplayedOnHierarchyAtTheSameTime(sEditor.MatchTarget.GetType()))
                    newList.Add(sEditor);
            }
            GameEditorApp.instance.GetController<Hierarchy>().ReplaceTop(newList);
        }

        public void OnChange()
        {
            if (this.ParentTarget == null)
            {
                MyLineRenderer.positionCount = 0;
            }
            else
            {
                MyLineRenderer.positionCount = PointCount + 1;
                Vector3 start = InternalUtility.OutPoint(this.ParentTarget.As<IProjectItem>().MyEditGroup), end = InternalUtility.InPoint(this.MyEditGroup, 0);
                Vector3[] positions = new[]
                {
                    start,
                    start+new Vector3(0, -0.1f,-1.5f),
                    end+new Vector3(-1.5f,-0.1f,0),
                    end
                };
                for (int i = 0; i <= PointCount; i++)
                {
                    MyLineRenderer.SetPosition(i, InternalUtility.CalculateCubicBezierPoint(i * PointCountM, positions[0], positions[1], positions[2], positions[3]));
                }
            }

            foreach (var item in Childs.GetSubList<ICanSerializeOnCustomEditor, IUpdateOnChange>())
            {
                item.OnChange();
            }
        }

        public void OnRayCatching()
        {
            OnChange();
        }
    }
}
