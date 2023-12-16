using System;
using System.Collections;
using System.Collections.Generic;
using AD.BASE;
using AD.Experimental.GameEditor;
using UnityEngine;
using UnityEngine.Events;

namespace AD.Experimental.HosterSystem.Diagram
{
    public class AddDiagramKey : IHosterTag { }

    public class DiagramOption
    {
        public UnityAction<IMainHoster> Creater;
        public Type KeyType;
        public string KeyName;
        public bool IsJustOnly = false, IsOnly = false;

        private static Dictionary<string, DiagramOption> Values = new();

        public static void RegisterDiagramOption(UnityAction<IMainHoster> action, Type KeyType, string KeyName, bool IsJustOnly = false)
        {
            DiagramOption option = new();
            option.Creater = action;
            option.KeyType = KeyType;
            option.KeyName = KeyName;
            option.IsJustOnly = IsJustOnly;
            option.IsOnly = IsJustOnly;
            Values[KeyName] = option;
        }

        public static DiagramOption ObtainDiagramOption(string Key)
        {
            Values.TryGetValue(Key, out DiagramOption result);
            DiagramOption CopyR = new();
            CopyR.Creater = result.Creater;
            CopyR.KeyType = result.KeyType;
            CopyR.KeyName = result.KeyName;
            CopyR.IsJustOnly = result.IsJustOnly;
            CopyR.IsOnly = result.IsOnly;
            return result;
        }
    }

    public class AddDiagram : BaseDiagram
    {
        private List<DiagramOption> ThatDiagram = new();

        public override bool IsDirty { get => false; set { } }
        public override int SerializeIndex { get => 100031; set => throw new ADException("Not Support"); }

        public override void OnSerialize()
        {
            PropertiesLayout.SetUpPropertiesLayout(this);
            MatchItem.SetTitle("Add Component");
            foreach (var diagram in ThatDiagram)
            {
                if (!diagram.IsJustOnly || diagram.IsOnly)
                    PropertiesLayout.ModernUIButton(diagram.KeyName, diagram.KeyType.FullName, () =>
                    {
                        diagram.Creater(Parent);
                        if (diagram.IsJustOnly) diagram.IsOnly = false;
                        GameEditorApp.instance.GetController<Properties>().RefreshPanel();
                    });
            }
            PropertiesLayout.ApplyPropertiesLayout();
        }

        #region Command

        public AddDiagram RegisterDiagram(string key)
        {
            ThatDiagram.Add(DiagramOption.ObtainDiagramOption(key));
            return this;
        }

        public AddDiagram RegisterDiagram(params string[] keys)
        {
            foreach (var key in keys)
            {
                RegisterDiagram(key);
            }
            return this;
        }

        #endregion
    }
}
