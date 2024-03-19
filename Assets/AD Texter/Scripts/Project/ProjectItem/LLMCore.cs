using System;
using System.Collections.Generic;
using System.Linq;
using AD.BASE;
using AD.Experimental.GameEditor;
using AD.Experimental.LLM;
using AD.Sample.Texter.Data;
using AD.Sample.Texter.Internal;
using AD.Sample.Texter.Project;
using AD.Utility;
using AD.Utility.Object;
using UnityEngine;
using static AD.Experimental.LLM.LLM;

namespace AD.Sample.Texter
{
    [Serializable]
    public class LLMCoreData : ProjectItemData
    {
        public List<SendData> m_DataList = new();
        public Dictionary<string, VariantSetting> VariantSettingPairs = new();
        public string Target;

        public LLMCoreData(LLMCore projectItem, List<SendData> dataList, Dictionary<string, VariantSetting> variantSettingPairs,string target) : base(projectItem)
        {
            this.m_DataList = dataList;
            this.VariantSettingPairs = variantSettingPairs;
            Target = target;
        }

        public override bool FromMap(ProjectData_BaseMap from)
        {
            if (from is LLMCore_BaseMap data)
            {
                this.m_DataList = data.m_DataList;
                this.VariantSettingPairs = data.VariantSettingPairs;
                this.Target = data.Target;
                return base.FromMap(from);
            }
            else return false;
        }

        public override void ToMap(out ProjectData_BaseMap BM)
        {
            BM = new LLMCore_BaseMap(m_DataList, VariantSettingPairs,Target);
            BM.FromObject(this);
        }
    }

    namespace Data
    {
        [EaseSave3]
        [Serializable]
        public class LLMCore_BaseMap : ProjectData_BaseMap
        {
            public List<SendData> m_DataList = new();
            public Dictionary<string, VariantSetting> VariantSettingPairs = new();
            public string Target;

            public LLMCore_BaseMap(List<SendData> dataList, Dictionary<string, VariantSetting> variantSettingPairs, string target)
            {
                this.m_DataList = dataList;
                this.VariantSettingPairs = variantSettingPairs;
                Target = target;
            }

            public override bool FromObject(ProjectItemData from)
            {
                if (from is LLMCoreData data)
                {
                    this.m_DataList = data.m_DataList;
                    this.VariantSettingPairs = data.VariantSettingPairs;
                    this.Target = data.Target;
                    return base.FromObject(from);
                }
                else return false;
            }

            public override void ToObject(out ProjectItemData obj)
            {
                obj = new LLMCoreData(null, m_DataList, VariantSettingPairs, Target);
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

                PropertiesLayout.Title("目标大模型配置");
                PropertiesLayout.Label("API URL");
                PropertiesLayout.InputField(that.MatchLLM.url, "api url").AddListener(T => that.MatchLLM.url = T);
                PropertiesLayout.Label("Language Label");
                PropertiesLayout.InputField(that.MatchLLM.lan, "lan").AddListener(T => that.MatchLLM.lan = T);
                PropertiesLayout.Label("Prompt");
                PropertiesLayout.InputField(that.MatchLLM.Prompt, "Prompt words").AddListener(T => that.MatchLLM.Prompt = T);

                PropertiesLayout.Toggle("Prompt Mode", that.MatchLLM.IsUseDefaultPromptFormat, "Is Use Default Prompt Format", T => that.MatchLLM.IsUseDefaultPromptFormat = T);

                PropertiesLayout.Label("Context Max", "Max History Context, Memoray");
                PropertiesLayout.InputField(that.MatchLLM.m_HistoryKeepCount.ToString(), "Context Max", "Context Max(Max History Context)").Share(out var CMaxInput)
                    .AddListener(T =>
                {
                    if (ArithmeticExtension.TryParse(T, out var result))
                    {
                        that.MatchLLM.m_HistoryKeepCount = (int)result.ReadValue();
                    }
                    else CMaxInput.SetTextWithoutNotify(that.MatchLLM.m_HistoryKeepCount.ToString());
                });

                PropertiesLayout.Label("Select LLM");
                PropertiesLayout.Dropdown(GetALLMatchLLM(that.MyEditGroup).GetSubList<string, LLM>(T => T != null, T => T.gameObject.name).ToArray()
                   , that.MatchLLMMonoName, "The Current Working LLM", T => { }).Share(out var SeLLM).SetTitle(that.MatchLLMMonoName);
                SeLLM.AddListener(T =>
                    {
                        var cat = GetMatchLLM(that.MyEditGroup, T);
                        if (cat != null)
                        {
                            if(that.ProjectLLMSourceData.VariantSettingPairs.ContainsKey(that.m_MatchLLM.name))
                            {
                                that.ProjectLLMSourceData.VariantSettingPairs[that.m_MatchLLM.name] = that.m_MatchLLM.GetSetting();
                                that.ProjectLLMSourceData.m_DataList = that.m_MatchLLM.m_DataList;
                            }
                            else
                            {
                                that.ProjectLLMSourceData.VariantSettingPairs.Add(that.m_MatchLLM.name, that.m_MatchLLM.GetSetting());
                            }
                            that.m_MatchLLM = cat;
                            if(that.ProjectLLMSourceData.VariantSettingPairs.TryGetValue(cat.name,out var vSetting))
                            {
                                cat.InitVariant(vSetting);
                                cat.m_DataList = that.ProjectLLMSourceData.m_DataList;
                            }
                            SeLLM.SetTitle(that.MatchLLMMonoName);
                            GameEditorApp.instance.GetController<Properties>().ClearAndRefresh();
                        }
                    });
            }
        }

        public class LLMCoreVariantBlock : ProjectItemBlock
        {
            public LLMCoreVariantBlock(LLMCore target) : base(target)
            {
                that = target;
            }

            public LLMCore that;

            protected override void HowSerialize()
            {
                var data = that.ProjectLLMSourceData;
                this.MatchItem.SetTitle("LLM Setting");

                if (that.m_MatchLLM.GetType() == typeof(ChatSpark))
                {
                    SerializeChatSpark();
                }
            }

            private void SerializeChatSpark()
            {
                ChatSpark chat = that.m_MatchLLM as ChatSpark;
                PropertiesLayout.Label("API Key");
                PropertiesLayout.InputField(chat.m_XunfeiSettings.m_APIKey, "api key").AddListener(T => chat.m_XunfeiSettings.m_APIKey = T);
                PropertiesLayout.Label("API Secret");
                PropertiesLayout.InputField(chat.m_XunfeiSettings.m_APISecret, "api Secret").AddListener(T => chat.m_XunfeiSettings.m_APISecret = T);
                PropertiesLayout.Label("API AppID");
                PropertiesLayout.InputField(chat.m_XunfeiSettings.m_AppID, "api AppID").AddListener(T => chat.m_XunfeiSettings.m_AppID = T);
                PropertiesLayout.Enum<ChatSpark.ModelType>("Model Level", (int)chat.m_SparkModel, "Model Level 1.0 - 3.5", T =>
                {
                    chat.m_SparkModel = T switch
                    {
                        "ModelV15" => ChatSpark.ModelType.ModelV15,
                        "ModelV20" => ChatSpark.ModelType.ModelV20,
                        "ModelV30" => ChatSpark.ModelType.ModelV30,
                        "ModelV35" => ChatSpark.ModelType.ModelV35,
                        _ => ChatSpark.ModelType.ModelV35
                    };
                    //Enum.TryParse<ChatSpark.ModelType>(T, out var result) ? result : ChatSpark.ModelType.ModelV30;
                }).Select(chat.m_SparkModel.ToString());
                Debug.Log(chat.m_SparkModel.ToString());

                PropertiesLayout.Button("Open", "打开使用窗口", () =>
                {
                    var target = GameEditorApp.instance.GetController<PropertiesEx>().Share(out var propertiesEx).CallWindow("LLMCore", that.PropertiesExWindowPrefab);
                    propertiesEx.Window.SetTitle("LLM Window");
                    var window = target.SeekComponent<LLMCorePExWindow>();
                    window.Init();
                    window.SetTitle(that.MatchLLMMonoName);
                    window.isCanBackPool = false;
                });
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
                foreach (var single in GetALLMatchLLM(MyEditGroup))
                {
                    if (ProjectLLMSourceData.VariantSettingPairs.TryGetValue(single.name, out var setting))
                        single.InitVariant(setting);
                    single.m_DataList = ProjectLLMSourceData.m_DataList;
                }
                this.MatchLLM = GetMatchLLM(MyEditGroup, ProjectLLMSourceData.Target);
            }
            else
            {
                ProjectLLMSourceData = new(this, new(), new(), this.MatchLLMMonoName);
            }
            MatchHierarchyEditor = new HierarchyBlock<LLMCore>(this, () => this.SourceData.ProjectItemID);
            MatchPropertiesEditors = new List<ISerializePropertiesEditor>()
            {
                new LLMCoreBlock(this),
                new LLMCoreVariantBlock(this),
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
            this.m_MatchLLM.InitVariant(new());
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
                this.ProjectLLMSourceData.VariantSettingPairs.Clear();
                foreach (var single in GetALLMatchLLM(MyEditGroup))
                {
                    this.ProjectLLMSourceData.VariantSettingPairs.Add(single.name, single.GetSetting());
                }

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

        public static LLM GetMatchLLM(EditGroup editGroup)
        {
            return ADGlobalSystem.FinalCheckCanntNull(editGroup.ViewLayer.GameObjects.FirstOrDefault(T => T.Key.StartsWith("Chat")).Value.SeekComponent<LLM>());
        }

        public static LLM GetMatchLLM(EditGroup editGroup,string Key)
        {
            return editGroup.ViewLayer.GameObjects.FirstOrDefault(T => T.Key== Key).Value.SeekComponent<LLM>();
        }

        public static LLM[] GetALLMatchLLM(EditGroup editGroup)
        {
            return editGroup.ViewLayer.GameObjects.GetSubList<LLM, KeyValuePair<string,GameObject>>(T => T.Key.StartsWith("Chat"), T => T.Value.SeekComponent<LLM>()).ToArray();
        }

        [SerializeField] private LLM m_MatchLLM;
        public LLM MatchLLM
        { 
            get
            {
                if (m_MatchLLM == null) m_MatchLLM = GetMatchLLM(MyEditGroup);
                return m_MatchLLM;
            }
            set => m_MatchLLM = value;
        }
        public string MatchLLMMonoName => MatchLLM.name;
        public string MatchLLMComponentName => MatchLLM.GetType().Name;

        [Header("P EX Prefab")]
        public GameObject PropertiesExWindowPrefab;
    }
}
