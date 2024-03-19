using System.Collections.Generic;
using System.Linq;
using AD.BASE;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace AD.UI
{
    [AddComponentMenu("UI/AD/Dropdown", 100)]
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
                source.value = T;
            }
            else
            {
                if (!options.Contains(CurrentSelectOption)) source.value = 0;
                //source.SetValueWithoutNotify(options.IndexOf(CurrentSelectOption));
            }
            OnSelect.Invoke(CurrentSelectOption);
        }

        protected void OnDestroy()
        {
            AD.UI.ADUI.DestroyADUI(this);
        }

        public void Init()
        {
            OnSelect.RemoveAllListeners();
            options.Clear();
        }

        public static AD.UI.Dropdown Generate(string name = "New Dropdown", Transform parent = null)
        {
            AD.UI.Dropdown dropDown = GameObject.Instantiate(ADGlobalSystem.instance._DropDown);
            dropDown.transform.SetParent(parent, false);
            dropDown.transform.localPosition = Vector3.zero;
            dropDown.name = name;
            return dropDown;
        }

        public void SetTitle(string title)
        {
            source.captionText.text = title;
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
        public string CurrentSelectOption { get => source.captionText.text; }
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
            if (options.Contains(option))
            {
                SetOptionByIndex(options.IndexOf(option));
            }
            else Debug.LogWarning("Unknown Option : " + option);
        }
    }
}
