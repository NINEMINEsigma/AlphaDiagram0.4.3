using System.Collections;
using System.Collections.Generic;
using AD.BASE;
using AD.UI;
using AD.Utility;
using UnityEngine;

namespace AD.Experimental.GameEditor
{
    public class PropertiesItem : ListViewItem
    {
        public const float DefaultHight = 30;
        public const float DefaultRectHightLevelSize = 30;

        [SerializeField] AD.UI.Toggle Lock_Tilie_Toggle;
        public RectTransform SubPage;
        [SerializeField] RectTransform SubLinePerfab;

        public bool IsLock = false;

        public ISerializePropertiesEditor MatchEditor;

        [SerializeField] private List<GameObject> Lines = new();

        public override int SortIndex { get => MatchEditor.SerializeIndex; set { } }

        public override ListViewItem Init()
        {
            SwitchLock(false);
            InitToggle();
            ClearRectHightLevel();
            foreach (var obj in Lines)
            {
                GameObject.Destroy(obj);
            }
            return this;
        }

        public void SetTitle(string title)
        {
            Lock_Tilie_Toggle.SetTitle(title.Translate());
        }

        private void InitToggle()
        {
            Lock_Tilie_Toggle.RemoveListener(SwitchLock);
            Lock_Tilie_Toggle.AddListener(SwitchLock);
            Lock_Tilie_Toggle.SetTitle("[ P R O P E R T Y ]");
        }

        [SerializeField] private GameObject Mask;
        private void SwitchLock(bool boolen)
        {
            IsLock = boolen;
            Mask.SetActive(boolen);
        }

        public void ClearRectHightLevel()
        {
            Vector2 temp = this.transform.As<RectTransform>().sizeDelta;
            this.transform.As<RectTransform>().sizeDelta = new Vector2(temp.x, DefaultHight);
        }

        public void AddRectHightLevel(int level = 1)
        {
            Vector2 temp = this.transform.As<RectTransform>().sizeDelta;
            this.transform.As<RectTransform>().sizeDelta = new Vector2(temp.x, temp.y + DefaultRectHightLevelSize * level);
        }

        public RectTransform AddNewLevelLine(bool isNewLine, int line)
        {
            AddRectHightLevel(line);
            if (isNewLine)
            {
                Lines.Add(GameObject.Instantiate(SubLinePerfab.gameObject, SubPage));
                RectTransform temp = Lines[^1].GetComponent<RectTransform>();
                temp.sizeDelta= new Vector2(temp.sizeDelta.x, 0);
            }
            GameObject obj = Lines[^1];
            RectTransform result = obj.GetComponent<RectTransform>();
            result.sizeDelta = new Vector2(result.sizeDelta.x, result.sizeDelta.y + DefaultRectHightLevelSize * line);
            return result;
        }
    }
}
