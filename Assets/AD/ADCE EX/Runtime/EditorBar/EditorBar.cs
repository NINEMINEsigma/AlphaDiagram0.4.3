using System;
using System.Collections.Generic;
using AD.BASE;
using AD.UI;
using UnityEngine;

namespace AD.Experimental.GameEditor
{
    [Serializable]
    public class EditorBarItem
    {
        public EditorBarItem(ADEvent callbacks, Sprite icon)
        {
            this.Callbacks = callbacks;
            this.icon = icon;
        }

        public ADEvent Callbacks;
        public Sprite icon;
    }

    public class EditorBar : ADController
    {
        private void Start()
        {
            GameEditorApp.instance.RegisterController(this);
        }

        public override void Init()
        {
            Clear();
            GameEditorApp.instance.RegisterSystem<GameEditorWindowGenerator>();
            GameEditorApp.instance.GetSystem<GameEditorWindowGenerator>().Parent = transform.parent as RectTransform;
            GameEditorApp.instance.GetSystem<GameEditorWindowGenerator>().WindowPerfab = CustomWindowElementPerfab;
        }

        [SerializeField] List<EditorBarItem> StaticItem = new();
        List<EditorBarItem> Items = new();
        [SerializeField] Transform SubPage;
        public AD.UI.Button ButtonPerfab;

        public CustomWindowElement CustomWindowElementPerfab;

        List<AD.UI.Button> CurrentButtons = new();

        public void AddItem(EditorBarItem item)
        {
            Items.Add(item);
            Refresh();
        }

        public void RemoveItem(EditorBarItem item)
        {
            Items.Remove(item);
            Refresh();
        }

        public void Refresh()
        {
            for (int i = 0; i < StaticItem.Count; i++)
            {
                EditorBarItem item = StaticItem[i];
                ObtainButton(i, item);
            }
            for (int i = StaticItem.Count; i < Items.Count+ StaticItem.Count; i++)
            {
                EditorBarItem item = Items[i - StaticItem.Count];
                ObtainButton(i, item);
            }
            if (Items.Count + StaticItem.Count < CurrentButtons.Count)
            {
                for (int i = Items.Count + StaticItem.Count; i < CurrentButtons.Count; i++)
                {
                    GameObject.Destroy(CurrentButtons[i].gameObject);
                }
                CurrentButtons.RemoveRange(Items.Count + StaticItem.Count, CurrentButtons.Count - (Items.Count + StaticItem.Count));
            }
        }

        private void ObtainButton(int i, EditorBarItem item)
        {
            AD.UI.Button CurrentButton;
            if (i < CurrentButtons.Count)
                CurrentButton = CurrentButtons[i];
            else
            {
                CurrentButtons.Add(GameObject.Instantiate(ButtonPerfab, SubPage));
                CurrentButton = CurrentButtons[^1];
            }
            CurrentButton.AddListener(() => item.Callbacks.Invoke());
            CurrentButton.SetView(item.icon);
        }

        public void Clear()
        {
            Items.Clear();
            Refresh();
        }

        public void OpenScene(string sceneName)
        {
            ADGlobalSystem.instance.TargetSceneName = sceneName;
            ADGlobalSystem.instance.OnEnd();
        }
    }
}
