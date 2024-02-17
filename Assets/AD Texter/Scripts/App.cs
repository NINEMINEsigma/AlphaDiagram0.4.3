using System;
using System.Collections.Generic;
using AD.BASE;
using AD.Experimental.GameEditor;
using AD.Sample.Texter.Internal;
using AD.Sample.Texter.Project;
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

            public void DisplayProjectID(ProjectItemData data)
            {
                PropertiesLayout.BeginHorizontal();
                PropertiesLayout.Label("Project ID");
                var inputI = PropertiesLayout.InputField(data.ProjectItemID, "Project ID");
                inputI.AddListener(T =>
                {
                    data.ProjectItemID = T;
                    inputI.text = data.ProjectItemID;
                });
                PropertiesLayout.EndHorizontal();
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
                            if (OnEvent != null)
                                OnEvent.Invoke(entry.delegates.Invoke(_that));
                        });
                    }
                }
            }
        }

        public static class InternalUtility
        {
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
                return InternalObtainGameObject(target, "InPoints");
            }

            public static void SetInPointCount(EditGroup target, int count)
            {
                GameObject inPoint = InternalObtainGameObject(target, "InPoints");
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
                GameObject inPoint = InternalObtainGameObject(target, "InPoints");
                int e = inPoint.transform.childCount;
                for (int i = 0; i < e; i++)
                {
                    var cat = inPoint.transform.GetChild(i);
                    if (cat.gameObject.activeSelf) result++;
                }
                return result;
            }

            public static Vector3 InPoint(EditGroup target,int index)
            {
                int result = 0;
                GameObject inPoint = InternalObtainGameObject(target, "InPoints");
                int e = inPoint.transform.childCount;
                for (int i = 0; i < e; i++)
                {
                    var cat = inPoint.transform.GetChild(i);
                    if (cat.gameObject.activeSelf)
                    {
                        if(result==index)
                        {
                            return cat.position;
                        }
                        result++;
                    }
                }
                return inPoint.transform.GetChild(0).position;
            }
        }
    }

    public class App : ADArchitecture<App>
    {
        public static GenerateTargetEntry[] Get(List<GameObject> sources,bool isModern)
        {
            Type TargetCT = null;
            return sources.GetSubList(
                T =>
                {
                    var temp = T.GetComponents<IProjectItem>();
                    if (temp != null && temp.Length == 1)
                    {
                        TargetCT = temp[0].GetType();
                        return true;
                    }
                    else return false;
                }, T =>
                {
                    return new GenerateTargetEntry(
                        _Parent =>
                        {
                            var newChild = T.PrefabInstantiate().GetComponent(TargetCT) as IProjectItem;
                            _Parent.MatchHierarchyEditor.IsOpenListView = false;
                            newChild.SetParent(_Parent);
                            newChild.As<MonoBehaviour>().transform.SetParent(instance.GetController<ProjectManager>().ProjectTransformRoot, false);
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

        public override void Init()
        {
            OnGenerate.AddListener(T =>
            {
                if (T is ProjectTextData data)
                {
                    var cat = App.instance.GetModel<PrefabModel>().Prefabs[nameof(ProjectTextField)].PrefabInstantiate<ProjectTextField, EditGroup>();
                    cat.transform.SetParent(instance.GetController<ProjectManager>().ProjectTransformRoot, false);
                    cat.ProjectTextSourceData = data;
                    cat.name = cat.SourceData.ProjectItemID;
                    cat.IsSetupProjectTextSourceData = true;
                    cat.SetParent(ProjectTextData.GetParent(data.ParentItemID));
                }
            });
        }

        public override bool FromMap(IBaseMap from)
        {
            throw new System.NotImplementedException();
        }

        public override IBaseMap ToMap()
        {
            throw new System.NotImplementedException();
        }
    }
}
