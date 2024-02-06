using System;
using System.Collections.Generic;
using AD.BASE;
using AD.Experimental.GameEditor;
using AD.Sample.Texter.Internal;
using AD.Sample.Texter.Project;
using AD.Utility;
using AD.Utility.Object;
using CW.Common;
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
            public static Vector3 AnchoringRightTop(EditGroup target)
            {
                return target.ViewLayer.GameObjects["Anchoring Right Top"].transform.position;
            }

            public static Vector3 AnchoringRightButtom(EditGroup target)
            {
                return target.ViewLayer.GameObjects["Anchoring Right Buttom"].transform.position;
            }

            public static Vector3 AnchoringLeftTop(EditGroup target)
            {
                return target.ViewLayer.GameObjects["Anchoring Left Top"].transform.position;
            }

            public static Vector3 AnchoringLeftButtom(EditGroup target)
            {
                return target.ViewLayer.GameObjects["Anchoring Left Buttom"].transform.position;
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
