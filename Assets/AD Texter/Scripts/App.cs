using System;
using System.Collections;
using System.Collections.Generic;
using AD.BASE;
using AD.Experimental.GameEditor;
using AD.Experimental.Performance;
using AD.Sample.Texter.Internal;
using AD.Sample.Texter.Project;
using AD.UI;
using AD.Utility;
using AD.Utility.Object;
using UnityEngine;

namespace AD.Sample.Texter
{
    namespace Internal
    {
        public class ProjectItemBlock : ISerializePropertiesEditor
        {
            public ProjectItemBlock(IProjectItem target)
            {
                _that = target;
            }

            public PropertiesItem MatchItem { get; set; }

            public IProjectItem _that;

            public ICanSerializeOnCustomEditor MatchTarget => _that;

            public bool IsDirty { get; set; }
            public int SerializeIndex { get; set; }

            public void OnSerialize()
            {
                PropertiesLayout.SetUpPropertiesLayout(this);
                HowSerialize();
                PropertiesLayout.ApplyPropertiesLayout();
            }

            protected virtual void HowSerialize()
            {
            }

            public UI.InputField DisplayProjectID(ProjectItemData data)
            {
                PropertiesLayout.BeginHorizontal();
                PropertiesLayout.Label("Project ID");
                UI.InputField inputI = PropertiesLayout.InputField(data.ProjectItemID, "Project ID");
                inputI.AddListener(T =>
                {
                    data.ProjectItemID = T;
                    inputI.text = data.ProjectItemID;
                    foreach (var item in this.MatchTarget.MatchHierarchyEditor.MatchItems)
                    {
                        this.MatchTarget.MatchHierarchyEditor.OnSerialize(item);
                    }
                });
                PropertiesLayout.EndHorizontal();
                return inputI;
            }
        }

        [Serializable]
        public class GenerateTargetEntry
        {
            public delegate IProjectItem ProjectItemGenerateDelegate(IProjectItem parentItem);

            public ProjectItemGenerateDelegate delegates;
            public string buttonText, message;
            public bool isModernButton;

            public GenerateTargetEntry(ProjectItemGenerateDelegate delegates, string buttonText, bool isModernButton = false)
                : this(delegates, buttonText, buttonText, isModernButton) { }

            public GenerateTargetEntry(ProjectItemGenerateDelegate delegates, string buttonText, string message, bool isModernButton = false)
            {
                this.delegates = delegates;
                this.buttonText = buttonText;
                this.message = message;
                this.isModernButton = isModernButton;
            }
        }

        public class ProjectItemGeneraterBlock : ProjectItemBlock
        {
            public ProjectItemGeneraterBlock(IProjectItem target, GenerateTargetEntry[] generateTargetEntries, ADEvent<IProjectItem> OnEvent) : base(target)
            {
                this.generateTargetEntries = generateTargetEntries;
                this.OnEvent = OnEvent;
            }

            public GenerateTargetEntry[] generateTargetEntries;

            public ADEvent<IProjectItem> OnEvent = new();

            protected override void HowSerialize()
            {
                if (generateTargetEntries == null || generateTargetEntries.Length == 0)
                {
                    this.MatchItem.SetTitle("Generate(Empty)");
                }
                else
                {
                    this.MatchItem.SetTitle("Generate");
                    foreach (var entry in generateTargetEntries)
                    {
                        PropertiesLayout.Button(entry.buttonText, entry.isModernButton, entry.message, () =>
                        {
                            OnEvent?.Invoke(entry.delegates.Invoke(_that));
                        });
                    }
                }
            }
        }

        public static class InternalUtility
        {
            public static void SetParent(this IProjectItem item, IProjectItem parent, bool JustSetParent = false)
            {
                (item as ICanSerializeOnCustomEditor).SetParent(parent, JustSetParent);
                if (parent != null)
                    App.instance.AddMessage(item.MyEditGroup.name + " is set " + parent.MyEditGroup.name + " parent");
            }

            public static GameObject InternalObtainGameObject(EditGroup target, string Key)
            {
                return target.ViewLayer.GameObjects[Key];
            }

            public static Vector3 AnchoringRightTop(EditGroup target)
            {
                return InternalObtainGameObject(target, "Anchoring Right Top").transform.position;
            }

            public static Vector3 AnchoringRightButtom(EditGroup target)
            {
                return InternalObtainGameObject(target, "Anchoring Right Buttom").transform.position;
            }

            public static Vector3 AnchoringLeftTop(EditGroup target)
            {
                return InternalObtainGameObject(target, "Anchoring Left Top").transform.position;
            }

            public static Vector3 AnchoringLeftButtom(EditGroup target)
            {
                return InternalObtainGameObject(target, "Anchoring Left Buttom").transform.position;
            }

            public static Vector3 OutPoint(EditGroup target)
            {
                return InternalObtainGameObject(target, "Out Point").transform.position;
            }

            public static GameObject InPoints(EditGroup target)
            {
                return InternalObtainGameObject(target, "In Points");
            }

            public static void SetInPointCount(EditGroup target, int count)
            {
                GameObject inPoint = InternalObtainGameObject(target, "In Points");
                int e = inPoint.transform.childCount;
                for (int i = 0; i < e; i++)
                {
                    var cat = inPoint.transform.GetChild(i);
                    cat.gameObject.SetActive(i < count);
                    cat.name = i.ToString();
                }
                GameObject prefab = inPoint.transform.GetChild(0).gameObject;
                for (int i = e; i < count; i++)
                {
                    var cat = prefab.PrefabInstantiate();
                    cat.transform.SetParent(inPoint.transform, false);
                    cat.SetActive(i < count);
                    cat.name = i.ToString();
                }
            }

            public static int GetInPointCount(EditGroup target)
            {
                int result = 0;
                GameObject inPoint = InternalObtainGameObject(target, "In Points");
                int e = inPoint.transform.childCount;
                for (int i = 0; i < e; i++)
                {
                    var cat = inPoint.transform.GetChild(i);
                    if (cat.gameObject.activeSelf) result++;
                }
                return result;
            }

            public static Vector3 InPoint(EditGroup target, int index)
            {
                int result = 0;
                GameObject inPoint = InternalObtainGameObject(target, "In Points");
                int e = inPoint.transform.childCount;
                for (int i = 0; i < e; i++)
                {
                    var cat = inPoint.transform.GetChild(i);
                    if (cat.gameObject.activeSelf)
                    {
                        if (result == index)
                        {
                            return cat.position;
                        }
                        result++;
                    }
                }
                return inPoint.transform.GetChild(0).position;
            }

            public static Vector3 InPoint(EditGroup target, int index, out GameObject InPointResult)
            {
                int result = 0;
                GameObject inPoint = InternalObtainGameObject(target, "In Points");
                int e = inPoint.transform.childCount;
                for (int i = 0; i < e; i++)
                {
                    var cat = inPoint.transform.GetChild(i);
                    if (cat.gameObject.activeSelf)
                    {
                        if (result == index)
                        {
                            InPointResult = cat.gameObject;
                            return cat.position;
                        }
                        result++;
                    }
                }
                InPointResult = inPoint.transform.GetChild(0).gameObject;
                return inPoint.transform.GetChild(0).position;
            }

            public static Vector3 CalculateCubicBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
            {
                float u = 1 - t;
                float tt = t * t;
                float uu = u * u;
                float uuu = uu * u;
                float ttt = tt * t;

                Vector3 p = uuu * p0;
                p += 3 * uu * t * p1;
                p += 3 * u * tt * p2;
                p += ttt * p3;

                return p;
            }
        }
    }

    public class App : ADArchitecture<App>
    {
        public static GenerateTargetEntry[] Get(List<GameObject> sources, bool isModern)
        {
            return sources.GetSubList<GenerateTargetEntry, Type, GameObject>(
                T =>
                {
                    Type TargetCT = null;
                    var temp = T.GetComponents<IProjectItem>();
                    if (temp != null && temp.Length == 1)
                    {
                        TargetCT = temp[0].GetType();
                        return (true, TargetCT);
                    }
                    else return (false, TargetCT);
                }, (T, TargetCT) =>
                {
                    return new GenerateTargetEntry(
                        _Parent =>
                        {
                            var newChild = T.PrefabInstantiate().GetComponent(TargetCT) as IProjectItem;
                            _Parent.MatchHierarchyEditor.IsOpenListView = false;
                            newChild.As<MonoBehaviour>().transform.SetParent(instance.GetController<ProjectManager>().ProjectTransformRoot, false);
                            newChild.MyEditGroup.StartCoroutine(WaitForSetParent(newChild, _Parent));
                            return newChild;
                        },
                        T.name,
                        TargetCT.FullName,
                        isModern
                        );
                }
                ).ToArray();
        }

        public ADEvent<ProjectItemData> OnGenerate = new();

        public static Vector3 GetOriginPosition(Vector2 vec)
        {
            return new Vector3(vec.x, 0, vec.y);
        }

        public static IEnumerator WaitForInit(IProjectItemWhereNeedInitData item)
        {
            yield return null;
            yield return null;
            yield return null;
            item.Init();
        }

        private static IEnumerator WaitForSetParent(IProjectItem newChild, IProjectItem _Parent)
        {
            yield return null;
            yield return null;
            yield return null;
            newChild.SetParent(_Parent);
        }

        private T InternalGetPrefabInstance<T>() where T : Component, IProjectItem
        {
            return App.instance.GetModel<PrefabModel>().Prefabs[nameof(T)].PrefabInstantiate<T, EditGroup>();
        }

        public override void Init()
        {
            OnGenerate.AddListener(T =>
            {
                IProjectItemWhereNeedInitData item = null;
                if (T is ProjectTextData data)
                {
                    var cat = InternalGetPrefabInstance<ProjectTextField>();
                    cat.IsSetupProjectTextSourceData = true;
                    item = cat;
                    data.MatchProjectItem = cat;
                }
                else if(T is ProjectScriptSlicerData slicerData)
                {
                    var cat = InternalGetPrefabInstance<ProjectScriptSlicer>();
                    cat.IsSetupProjectScriptSlicingSourceData = true;
                    item = cat;
                    slicerData.MatchProjectItem = cat;
                }

                if (item != null)
                {
                    item.SourceData = T;
                    item.MyEditGroup.name = item.SourceData.ProjectItemID;
                    item.MyEditGroup.transform.SetParent(instance.GetController<ProjectManager>().ProjectTransformRoot, false);
                }
            });

            GameEditorApp.instance.RegisterCommand<RefreshHierarchyPanel>();
            GameEditorApp.instance.RegisterCommand<RefreshPropertiesPanel>();
            this.RegisterCommand<RefreshHierarchyPanel>();
            this.RegisterCommand<RefreshPropertiesPanel>();
        }

        public override bool FromMap(IBaseMap from)
        {
            throw new System.NotImplementedException();
        }

        public override IBaseMap ToMap()
        {
            throw new System.NotImplementedException();
        }

        public ProjectItemData CurrentProjectItemData;
    }

    public class SubSceneManager : SubSceneLoader
    {
        protected virtual void Start()
        {
            BackSceneButton.AddListener(() => App.instance.GetController<MainSceneLoader>().Unload(this.Scene.name));
            SetupProjectItemData(App.instance.CurrentProjectItemData);
        }

        protected virtual void SetupProjectItemData(ProjectItemData data)
        {

        }

        [Header("Main Assets")]
        public Canvas mainCanvas;
        public virtual IButton BackSceneButton { get; }
        public Text mainTitle;
    }
}
