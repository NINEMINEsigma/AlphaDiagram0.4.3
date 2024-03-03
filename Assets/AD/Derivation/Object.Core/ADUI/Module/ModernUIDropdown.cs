using System.Collections.Generic;
using System.Linq;
using AD.BASE;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AD.UI
{
    public class ModernUIDropdown : PropertyModule, IDropdown
    {
        public ModernUIDropdown()
        {
            this.ElementArea = nameof(ModernUIDropdown);
        }

        // Resources
        public GameObject triggerObject;
        public Transform itemParent;
        public GameObject itemObject;
        public GameObject scrollbar;
        private VerticalLayoutGroup itemList;
        private Transform currentListParent;
        public Transform listParent;
        private Animator dropdownAnimator;
        public TextMeshProUGUI setItemText;
        public TextMeshProUGUI title;
        public Image icon;

        // Settings
        public bool enableIcon = true;
        public bool enableTrigger = true;
        public bool enableScrollbar = true;
        public bool setHighPriorty = true;
        public bool outOnPointerExit = false;
        public bool isListItem = false;
        public int maxSelect = 1;
        public AnimationType animationType;
        public ADEvent<string> OnSelect = new();

        // Items
        public List<Item> dropdownItems = new();

        // Other variables
        string textHelper;
        string newItemTitle;
        bool isOn;
        public int iHelper = 0;
        public int siblingIndex = 0;

        [System.Serializable]
        public class ToggleEvent : ADEvent<bool> { }

        public enum AnimationType
        {
            FADING,
            SLIDING,
            STYLISH
        }

        [System.Serializable]
        public class Item
        {
            public string itemName = "Dropdown Item";
            [HideInInspector] public UnityEngine.UI.Toggle ToggleItem;
            public int selectOrder = 0;
            public ToggleEvent toggleEvents = new();
        }

        protected override void Start()
        {
            base.Start();

            try
            {
                dropdownAnimator = this.GetComponent<Animator>();
                itemList = itemParent.GetComponent<VerticalLayoutGroup>();
                itemList = itemParent.GetComponent<VerticalLayoutGroup>();
                SetupDropdown();
                currentListParent = transform.parent;
            }

            catch
            {
                Debug.LogError("Dropdown - Cannot initalize the object due to missing resources.", this);
            }

            if (enableScrollbar == true)
            {
                itemList.padding.right = 25;
                scrollbar.SetActive(true);
            }

            else
            {
                itemList.padding.right = 8;
                Destroy(scrollbar);
            }

            if (setHighPriorty == true)
                transform.SetAsLastSibling();
        }

        public void SetupDropdown()
        {
            foreach (Transform child in itemParent)
                GameObject.Destroy(child.gameObject);

            for (int i = 0; i < dropdownItems.Count; ++i)
            {
                Item current = dropdownItems[i];

                GameObject go = Instantiate(itemObject, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
                go.transform.SetParent(itemParent, false);

                setItemText = go.GetComponentInChildren<TextMeshProUGUI>();
                textHelper = current.itemName;
                setItemText.text = textHelper;

                UnityEngine.UI.Toggle itemToggle = go.GetComponent<UnityEngine.UI.Toggle>();

                iHelper = i;

                current.ToggleItem = itemToggle;

                if (current.selectOrder > 0)
                    itemToggle.isOn = true;
                else
                    itemToggle.isOn = false;

                if (current.toggleEvents != null)
                    itemToggle.onValueChanged.AddListener(current.toggleEvents.Invoke);
                itemToggle.onValueChanged.AddListener(T =>
                {
                    UpdateItemSelectIndex(current, T);
                });
            }

            currentListParent = transform.parent;
        }

        private void UpdateItemSelectIndex(Item target, bool isOn)
        {
            if (isOn)
            {
                for (int i = 0; i < dropdownItems.Count; ++i)
                {
                    Item current = dropdownItems[i];
                    if (current != target)
                    {
                        --current.selectOrder;
                        if (current.selectOrder <= 0)
                        {
                            current.ToggleItem.SetIsOnWithoutNotify(false);
                            current.toggleEvents.Invoke(false);
                            current.ToggleItem.graphic.CrossFadeAlpha(0, 0, true);
                        }
                    }
                    else
                    {
                        current.selectOrder = maxSelect;
                        OnSelect.Invoke(current.itemName);
                    }
                }
            }
            else
            {
                for (int i = 0; i < dropdownItems.Count; ++i)
                {
                    if (dropdownItems[i] != target && dropdownItems[i].selectOrder > 0)
                        dropdownItems[i].selectOrder = Mathf.Clamp(dropdownItems[i].selectOrder + 1, 0, maxSelect);
                    else dropdownItems[i].selectOrder = 0;
                }
            }
        }

        public void Animate()
        {
            if (isOn == false && animationType == AnimationType.FADING)
            {
                dropdownAnimator.Play("Fading In");
                isOn = true;

                if (isListItem == true)
                {
                    siblingIndex = transform.GetSiblingIndex();
                    gameObject.transform.SetParent(listParent, true);
                }
            }

            else if (isOn == true && animationType == AnimationType.FADING)
            {
                dropdownAnimator.Play("Fading Out");
                isOn = false;

                if (isListItem == true)
                {
                    gameObject.transform.SetParent(currentListParent, true);
                    gameObject.transform.SetSiblingIndex(siblingIndex);
                }
            }

            else if (isOn == false && animationType == AnimationType.SLIDING)
            {
                dropdownAnimator.Play("Sliding In");
                isOn = true;

                if (isListItem == true)
                {
                    siblingIndex = transform.GetSiblingIndex();
                    gameObject.transform.SetParent(listParent, true);
                }
            }

            else if (isOn == true && animationType == AnimationType.SLIDING)
            {
                dropdownAnimator.Play("Sliding Out");
                isOn = false;

                if (isListItem == true)
                {
                    gameObject.transform.SetParent(currentListParent, true);
                    gameObject.transform.SetSiblingIndex(siblingIndex);
                }
            }

            else if (isOn == false && animationType == AnimationType.STYLISH)
            {
                dropdownAnimator.Play("Stylish In");
                isOn = true;

                if (isListItem == true)
                {
                    siblingIndex = transform.GetSiblingIndex();
                    gameObject.transform.SetParent(listParent, true);
                }
            }

            else if (isOn == true && animationType == AnimationType.STYLISH)
            {
                dropdownAnimator.Play("Stylish Out");
                isOn = false;
                if (isListItem == true)
                {
                    gameObject.transform.SetParent(currentListParent, true);
                    gameObject.transform.SetSiblingIndex(siblingIndex);
                }
            }

            if (enableTrigger == true && isOn == false)
                triggerObject.SetActive(false);

            else if (enableTrigger == true && isOn == true)
                triggerObject.SetActive(true);

            if (outOnPointerExit == true)
                triggerObject.SetActive(false);

            if (setHighPriorty == true)
                transform.SetAsLastSibling();
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);
            if (outOnPointerExit == true)
            {
                if (isOn == true)
                {
                    Animate();
                    isOn = false;
                }

                if (isListItem == true)
                    gameObject.transform.SetParent(currentListParent, true);
            }
        }

        public void UpdateValues()
        {
            if (enableScrollbar == true)
            {
                itemList.padding.right = 25;
                scrollbar.SetActive(true);
            }

            else
            {
                itemList.padding.right = 8;
                scrollbar.SetActive(false);
            }
        }

        public void CreateNewItem()
        {
            Item item = new();
            item.itemName = newItemTitle;
            dropdownItems.Add(item);
            SetupDropdown();
        }

        public void SetItemTitle(string title)
        {
            newItemTitle = title;
        }

        public void AddNewItem()
        {
            Item item = new Item();
            dropdownItems.Add(item);
        }

        public void SetTitle(string title)
        {
            this.title.text = title;
        }

        public void SetIcon(Sprite sprite)
        {
            icon.sprite = sprite;
        }

        ToggleEvent SetItemEvent(int index)
        {
            if (index >= 0 && index < dropdownItems.Count)
            {
                return dropdownItems[index].toggleEvents;
            }
            else return null;
        }

        public void AddOption(params string[] texts)
        {
            foreach (var text in texts)
            {
                Item item = new()
                {
                    itemName = text
                };
                dropdownItems.Add(item);
            }
            SetupDropdown();
        }
        public void AddOption(params Item[] items)
        {
            foreach (var item in items)
            {
                dropdownItems.Add(item);
            }
            SetupDropdown();
        }

        public void RemoveOption(params string[] texts)
        {
            foreach (var text in texts)
            {
                dropdownItems.RemoveAll(T=>T.itemName == text);
            }
            SetupDropdown();
        }
        public void RemoveOption(params Item[] items)
        {
            foreach (var item in items)
            {
                dropdownItems.RemoveAll(T => T == item);
            }
            SetupDropdown();
        }

        public void ClearOptions()
        {
            dropdownItems.Clear();
            SetupDropdown();
        }

        public void Select(string option)
        {
            var target = dropdownItems.FirstOrDefault(T => T.itemName == option);
            if(target!=default)
            {
                target.ToggleItem.isOn = true;
            }
        }

        public void AddListener(UnityAction<string> action)
        {
            OnSelect.AddListener(action);
        }
        public void RemoveListener(UnityAction<string> action)
        {
            OnSelect.RemoveListener(action);
        }
        public void RemoveAllListeners()
        {
            OnSelect.RemoveAllListeners();
        }
    }
}
