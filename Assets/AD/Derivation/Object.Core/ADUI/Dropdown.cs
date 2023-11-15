using System.Collections.Generic;
using System.Linq;
using AD.BASE;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace AD.UI
{
    public class Dropdown : ADUI, IDropdown
    {
        private TMP_Dropdown _source;
        public TMP_Dropdown source
        {
            get
            {
                if (_source == null) _source = GetComponent<TMP_Dropdown>();
                return _source;
            }
        }

        public Dropdown()
        {
            ElementArea = "Dropdown";
        }

        protected void Start()
        {
            AD.UI.ADUI.Initialize(this);
            source.onValueChanged.AddListener(SetOptionByIndex);
        }

        private void SetOptionByIndex(int T)
        {
            if (options.Count == 0) return;
            if (T > 0 && T < options.Count)
            {
                CurrentSelectOption = options[T];
                OnSelect.Invoke(CurrentSelectOption);
            }
            else
            {
                if (!options.Contains(CurrentSelectOption)) CurrentSelectOption = options[0];
                source.SetValueWithoutNotify(options.IndexOf(CurrentSelectOption));
            }
        }

        protected void OnDestroy()
        {
            AD.UI.ADUI.Destory(this);
        }

        public void Init()
        {
            OnSelect.RemoveAllListeners();
            options.Clear();
            CurrentSelectOption = "Default";
        }

#if UNITY_EDITOR
        [MenuItem("GameObject/AD/Dropdown", false, 10)]
        private static void ADD(UnityEditor.MenuCommand menuCommand)
        {
            AD.UI.Dropdown dropDown = GameObject.Instantiate(ADGlobalSystem.instance._DropDown);
            dropDown.name = "New Dropdown";
            GameObjectUtility.SetParentAndAlign(dropDown.gameObject, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(dropDown.gameObject, "Create " + dropDown.name);
            Selection.activeObject = dropDown.gameObject;
        }
#endif

        public static AD.UI.Dropdown Generate(string name = "New Dropdown", Transform parent = null)
        {
            AD.UI.Dropdown dropDown = GameObject.Instantiate(ADGlobalSystem.instance._DropDown);
            dropDown.transform.SetParent(parent, false);
            dropDown.transform.localPosition = Vector3.zero;
            dropDown.name = name;
            return dropDown;
        }

        public void AddListener(UnityAction<string> action)
        {
            OnSelect.AddListener(action);
        }
        public void RemoveListener(UnityAction<string> action)
        {
            OnSelect.AddListener(action);
        }
        public void RemoveAllListeners()
        {
            OnSelect.RemoveAllListeners();
        }

        public ADEvent<string> OnSelect = new();
        [SerializeField] private List<string> options = new();
        public string CurrentSelectOption { get; private set; }
        public void AddOption(params string[] texts)
        {
            options.AddRange(texts);
            source.AddOptions(texts.ToList());
        }
        public void RemoveOption(params string[] texts)
        {
            foreach (var item in texts)
            {
                options.Remove(item);
            }
            source.ClearOptions();
            source.AddOptions(options);
        }
        public void ClearOptions()
        {
            options.Clear();
            source.ClearOptions();
        }

        public void Select(string option)
        {
            if(options.Contains(option))
            {
                SetOptionByIndex(options.IndexOf(option));
            }
        }
    }
}
