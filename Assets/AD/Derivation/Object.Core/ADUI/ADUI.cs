using System;
using System.Collections.Generic;
using AD.BASE;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace AD.UI
{
    public interface ICanInitializeBehaviourContext
    {
        void InitializeContext();
    }

    public interface IADUI : ICanInitializeBehaviourContext
    {
        IADUI Obtain(int serialNumber);
        IADUI Obtain(string elementName);
        string ElementName { get; set; }
        int SerialNumber { get; set; }
        bool IsNeedContext { get; }
        BehaviourContext Context { get; }
    }

    /// <summary>
    /// 所有ADUI组件禁止在Awake使用ADUI业务逻辑
    /// </summary>
    [Serializable]
    public abstract class ADUI : MonoBehaviour, IADUI
    {
        public static ADUI CurrentSelect { get; internal set; }

        private BehaviourContext _Context;
        public virtual bool IsNeedContext => true;
        public BehaviourContext Context
        {
            get
            {
                if (!IsNeedContext) return null;
                _Context ??= this.GetOrAddComponent<BehaviourContext>();
                return _Context;
            }
        }

        public bool Selected = false;

        public static List<IADUI> Items { get; private set; } = new List<IADUI>();
        public static int TotalSerialNumber { get; private set; } = 0;
        public static string UIArea { get; internal set; } = "null";

        public string ElementName { get; set; } = "null";
        public int SerialNumber { get; set; } = 0;
        public string ElementArea = "null";

        public const string DefaultNumericManagerName= "Default";

        public virtual void TurnsActive(GameObject target) => target.gameObject.SetActive(!target.gameObject.activeSelf);

        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            Selected = true;
            CurrentSelect = this;
            UIArea = ElementArea;
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {
            Selected = false;
            UIArea = "null";
        }

        public virtual IADUI Obtain(int serialNumber)
        {
            return Items.Find((P) => P.SerialNumber == serialNumber);
        }

        public virtual IADUI Obtain(string elementName)
        {
            return Items.Find((P) => P.ElementName == elementName);
        }

        public virtual IADUI TryObtain(int serialNumber)
        {
            foreach (var item in Items)
                if (item.SerialNumber == serialNumber) return item;
            return null;
        }

        public virtual IADUI TryObtain(string elementName)
        {
            foreach (var item in Items)
                if (item.ElementName == elementName) return item;
            return null;
        }

        public virtual List<IADUI> ObtainAll(Predicate<IADUI> _Right)
        {
            List<IADUI> result = new List<IADUI>();
            foreach (var item in Items)
                if (_Right(item)) result.Add(item);
            return (result.Count > 0) ? result : null;
        }

        public static void Initialize(IADUI obj)
        {
            if (obj.IsNeedContext)
                obj.InitializeContext();
            obj.SerialNumber = TotalSerialNumber++;
            Items.Add(obj);
        }

        public static void Initialize(IADUI obj, string numericManagerName)
        {
            if (obj.IsNeedContext)
                obj.InitializeContext();
            obj.As<INumericManager>().SetupByNumericManager(numericManagerName);
            obj.SerialNumber = TotalSerialNumber++;
            Items.Add(obj);
        }

        public static void DestroyADUI(IADUI obj)
        {
            Items.Remove(obj);
        }

        public virtual void InitializeContext()
        {
            Context.OnPointerEnterEvent = InitializeContextSingleEvent(Context.OnPointerEnterEvent, OnPointerEnter);
            Context.OnPointerExitEvent = InitializeContextSingleEvent(Context.OnPointerExitEvent, OnPointerExit);
        }

        public static ADOrderlyEvent<PointerEventData> InitializeContextSingleEvent(ADOrderlyEvent<PointerEventData> Event, params UnityAction<PointerEventData>[] calls)
        {
            Event ??= new();
            foreach (var call in calls)
                Event.RemoveListener(call);
            foreach (var call in calls)
                Event.AddListener(call);
            return Event;
        }

        public static ADOrderlyEvent<BaseEventData> InitializeContextSingleEvent(ADOrderlyEvent<BaseEventData> Event, params UnityAction<BaseEventData>[] calls)
        {
            Event ??= new();
            foreach (var call in calls)
                Event.RemoveListener(call);
            foreach (var call in calls)
                Event.AddListener(call);
            return Event;
        }

        public static ADOrderlyEvent<AxisEventData> InitializeContextSingleEvent(ADOrderlyEvent<AxisEventData> Event, params UnityAction<AxisEventData>[] calls)
        {
            Event ??= new();
            foreach (var call in calls)
                Event.RemoveListener(call);
            foreach (var call in calls)
                Event.AddListener(call);
            return Event;
        }

        public static void SetValue_NumericManagerName(string NumericManagerName, float value)
        {
            ADGlobalSystem.instance.SetFloatValue(NumericManagerName, value);
        }
        public static void SetValue_NumericManagerName(string NumericManagerName, int value)
        {
            ADGlobalSystem.instance.SetIntValue(NumericManagerName, value);
        }
        public static void SetValue_NumericManagerName(string NumericManagerName, string value)
        {
            ADGlobalSystem.instance.SetStringValue(NumericManagerName, value);
        }

        public static bool GetValue_NumericManagerName(string NumericManagerName, out float value)
        {
            return ADGlobalSystem.instance.FloatValues.TryGetValue(NumericManagerName, out value);
        }
        public static bool GetValue_NumericManagerName(string NumericManagerName, out int value)
        {
            return ADGlobalSystem.instance.IntValues.TryGetValue(NumericManagerName, out value);
        }
        public static bool GetValue_NumericManagerName(string NumericManagerName, out string value)
        {
            return ADGlobalSystem.instance.StringValues.TryGetValue(NumericManagerName, out value);
        }

        public void SetupByNumericManager(string numericManagerName)
        {
            if (!string.IsNullOrEmpty(numericManagerName) && !numericManagerName.StartsWith("Default"))
                HowSetupByNumericManager();
        }
        protected virtual void HowSetupByNumericManager()
        {

        }
    }

    public interface IButton : IADUI
    {
        IButton SetTitle(string title);
        IButton AddListener(UnityAction action);
        IButton RemoveListener(UnityAction action);
        IButton RemoveAllListeners();
    }

    public interface IBoolButton : IADUI
    {
        bool isOn { get; set; }
        IBoolButton AddListener(UnityAction<bool> action);
        IBoolButton RemoveListener(UnityAction<bool> action);
    }

    public interface IDropdown : IADUI
    {
        void AddOption(params string[] texts);
        void RemoveOption(params string[] texts);
        void ClearOptions();
        void Select(string option);
        void AddListener(UnityAction<string> action);
        void RemoveListener(UnityAction<string> action);
    }

    public enum PressType
    {
        OnSelect,
        OnEnd
    }

    public interface IInputField : IADUI
    {
        TMP_InputField source { get; }
        string text { get; set; }
        InputFieldValueProperty ValueProperty { get; }
        InputFieldProperty TextProperty { get; }
        BindPropertyJustSet<string, BindInputFieldAsset> Input { get; }
        BindPropertyJustGet<string, BindInputFieldAsset> Output { get; }

        void AddListener(UnityAction<string> action, PressType type = PressType.OnEnd);
        void RemoveAllListener(PressType type = PressType.OnEnd);
        void RemoveListener(UnityAction<string> action, PressType type = PressType.OnEnd);
        void SetPlaceholderText(string text);
        IInputField SetText(string text);
        IInputField SetTextWithoutNotify(string text);
    }

    public interface INumericManager
    {
        void SetupByNumericManager(string numericManagerName);
    }

    public interface INumericManager<T> : INumericManager
    {
        void NumericManager(T value);
    }
}
