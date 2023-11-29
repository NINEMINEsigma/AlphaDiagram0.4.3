using System.Collections.Generic;
using AD.BASE;
using AD.UI;
using UnityEngine;
using UnityEngine.Events;

namespace AD.Experimental.GameEditor
{
    public class MulVectorField : ADUI
    {
        [SerializeField] private List<AD.UI.InputField> SingleFields = new();
        public int FieldCount => SingleFields.Count;

        public ADEvent<List<string>> CallBack = new();
        private List<string> CurrentResult = new();

        public void AddListener(UnityAction<List<string>> action)
        {
            CallBack.AddListener(action);
        }

        public void RemoveListener(UnityAction<List<string>> action)
        {
            CallBack.RemoveListener(action);
        }

        public void Refresh()
        {
            while (CurrentResult.Count < FieldCount)
            {
                CurrentResult.Add("");
            }
            for (int i = 0; i < CurrentResult.Count; i++)
            {
                CurrentResult[i] = SingleFields[i].text;
            }
        }

        public void RefreshThenCallBack()
        {
            Refresh();
            CallBack.Invoke(CurrentResult);
        }

        public void ClearAndRefresh()
        {
            CurrentResult.Clear();
            Refresh();
        }

        public void Init()
        {
            CallBack.RemoveAllListeners();
            foreach (var field in SingleFields)
            {
                field.source.onEndEdit.RemoveAllListeners();
                field.source.onEndEdit.AddListener(T => RefreshThenCallBack());
            }
        }

        public void Set(List<string> source)
        {
            for (int i = 0, e = Mathf.Min(SingleFields.Count, source.Count); i < e; i++)
            {
                string current_str = source[i];
                var currentField = SingleFields[i];
                currentField.SetText(current_str);
            }
        }

        public void Set(params string[] source)
        {
            for (int i = 0, e = Mathf.Min(SingleFields.Count, source.Length); i < e; i++)
            {
                string current_str = source[i];
                var currentField = SingleFields[i];
                currentField.SetText(current_str);
            }
        }
    }
}
