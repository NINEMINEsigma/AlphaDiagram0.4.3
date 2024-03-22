using System;
using System.Collections.Generic;
using AD.BASE;
using AD.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AD.Experimental.GameEditor
{
    [Serializable]
    public class PropertiesEditorAssets
    {
        public PropertiesItem PropertiesItemPrefab;
        public ListView PropertiesListView;
        public BehaviourContext behaviourContext;
        public GUISkin CustomSkin;
    }

    public class Properties : ADController
    {
        public PropertiesEditorAssets EditorAssets;

        public ICanSerializeOnCustomEditor MatchTarget;
        public List<ISerializePropertiesEditor> CurrentPropertiesEditors => MatchTarget.MatchPropertiesEditors;

        private void Start()
        {
            GameEditorApp.instance.RegisterController(this);
        }

        public override void Init()
        {
            //EditorAssets.behaviourContext.OnPointerEnterEvent = ADUI.InitializeContextSingleEvent(EditorAssets.behaviourContext.OnPointerEnterEvent, RefreshPanel);
            //EditorAssets.behaviourContext.OnPointerExitEvent = ADUI.InitializeContextSingleEvent(EditorAssets.behaviourContext.OnPointerExitEvent, RefreshPanel);

            GUI.skin = EditorAssets.CustomSkin;
        }

        public ISerializePropertiesEditor this[int index]
        {
            get
            {
                if (index < 0 || index > CurrentPropertiesEditors.Count)
                {
                    Debug.LogError("Over Bound");
                    return null;
                }
                return CurrentPropertiesEditors[index];
            }
            set
            {
                if (index < 0 || index >= CurrentPropertiesEditors.Count)
                {
                    Debug.LogError("Over Bound");
                    return;
                }
                else CurrentPropertiesEditors[index] = value;
            }
        }

        private PropertiesItem RegisterPropertiesItem(ISerializePropertiesEditor target)
        {
            PropertiesItem propertiesItem = EditorAssets.PropertiesListView.GenerateItem() as PropertiesItem;
            target.MatchItem = propertiesItem;
            propertiesItem.MatchEditor = target;
            target.OnSerialize();
            propertiesItem.name = propertiesItem.SortIndex.ToString();
            return propertiesItem;
        }

        public void ClearAndRefresh()
        {
            EditorAssets.PropertiesListView.Clear();
            if (MatchTarget == null) return;
            try
            {
                foreach (var target in CurrentPropertiesEditors)
                {
                    RegisterPropertiesItem(target);
                }
            }
            catch (Exception ex)
            {
                if (MatchTarget is UnityEngine.Object obj)
                    Debug.LogException(ex, obj);
                else
                    Debug.LogException(new ADException(MatchTarget.GetType().FullName + " : an instance is cannt Register", ex));
            }
            EditorAssets.PropertiesListView.SortChilds();
        }

        public void RefreshPanel(PointerEventData axisEventData = null)
        {
            if (MatchTarget == null) return;
            foreach (var target in CurrentPropertiesEditors)
            {
                if (target.IsDirty)
                {
                    target.MatchItem.Init();
                    target.OnSerialize();
                    target.IsDirty = false;
                }
            }
            EditorAssets.PropertiesListView.SortChilds();
        }
    }
}
