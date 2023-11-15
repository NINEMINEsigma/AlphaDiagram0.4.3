using System;
using System.Collections.Generic;
using AD.BASE;
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

    [Serializable]
    public abstract class ADUI : MonoBehaviour, IADUI
    {
        public static ADUI CurrentSelect { get; private set; }

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
        public static string UIArea { get; private set; } = "null";

        public string ElementName { get; set; } = "null";
        public int SerialNumber { get; set; } = 0;
        public string ElementArea = "null";

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

        public static void Destory(IADUI obj)
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
}
