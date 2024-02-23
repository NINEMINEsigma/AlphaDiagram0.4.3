using System;
using System.Collections.Generic;
using System.Linq;
using AD.Utility;
using AD.BASE;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.InputSystem.Controls;
using System.Collections;

namespace AD.UI
{
    public abstract class RouletteItem : MonoBehaviour
    {
        private BehaviourContext context;
        public BehaviourContext Context
        {
            get
            {
                if (context == null)
                {
                    context = this.GetOrAddComponent<BehaviourContext>();
                }
                return context;
            }
        }

        [SerializeField] private ViewController View;
        [SerializeField] private Image Image;

        public Roulette Parent;

        protected virtual void Start()
        {
            View = null;
            Image = null;
            if (!TryGetComponent(out View))
            {
                TryGetComponent(out Image);
            }
            SetHighLight(0.5f);
        }

        public virtual void InitOrder(int index, int max, Roulette roulette)
        {
            if (context == null)
            {
                context = this.GetOrAddComponent<BehaviourContext>();
            }
            StartCoroutine(WaitForContext(roulette));
        }

        private IEnumerator WaitForContext(Roulette roulette)
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            Context.OnPointerEnterEvent = ADUI.InitializeContextSingleEvent(Context.OnPointerEnterEvent, OnEnter);
            Context.OnPointerExitEvent = ADUI.InitializeContextSingleEvent(Context.OnPointerExitEvent, OnExit);
            Parent = roulette;
        }

        protected virtual void OnEnter(PointerEventData pointerEnterEvent)
        {
            SetHighLight(1);
        }

        protected void SetHighLight(float valuue)
        {
            if (View != null)
            {
                View.SetAlpha(valuue);
            }
            else if (Image != null)
            {
                Image.color = Image.color.Result_A(valuue);
            }
        }

        protected virtual void OnExit(PointerEventData pointerEnterEvent)
        {
            SetHighLight(0.5f);
        }

        protected virtual void OnEnable()
        {
            SetHighLight(0.5f);
        }

        protected virtual void OnDisable()
        {

        }
    }

    /// <summary>
    /// 开始时需要初始化，在生成的第五帧前不要失活，否则可能导致Context失灵
    /// </summary>
    public class Roulette : PropertyModule
    {
        [Serializable]
        public class RouletteEntry
        {
            public string KeyName;
            public int Key;
            public RouletteItem RouletteItemInstance;
        }

        [Header("Resources")]
        [SerializeField] List<RouletteEntry> SourcePairs;
        [SerializeField] Animator animator;

        [Header("Setting")]
        public Key KeyType = Key.None;
        public bool IsPressDetectForLeft = false;
        public bool IsNeedKeepPress = false;
        public float Depth = 10;

        protected override void Start()
        {
            base.Start();
            InitItems();
        }

        protected override void LetChildAdd(GameObject child)
        {
            child.SetActive(true);
        }

        protected override void LetChildDestroy(GameObject child)
        {
            child.SetActive(false);
        }

        public void SetActive(bool active, int key)
        {
            if (active)
            {
                if (!Childs.ContainsKey(key))
                    Add(key, SourcePairs.First(T => T.Key == key).RouletteItemInstance.gameObject);
            }
            else
            {
                Remove(key);
            }
        }

        public void SetActive(bool active, string key)
        {
            var pair = SourcePairs.FirstOrDefault(T => T.KeyName == key);
            if (pair == default) return;
            if (active)
            {
                if (!Childs.ContainsKey(pair.Key))
                    Add(pair.Key, pair.RouletteItemInstance.gameObject);
            }
            else
            {
                Remove(pair.Key);
            }
        }

        public void InitItems()
        {
            for (int i = 0, e = SourcePairs.Count; i < e; i++)
            {
                RouletteEntry pair = SourcePairs[i];
                Add(pair.Key, pair.RouletteItemInstance.gameObject);
                pair.RouletteItemInstance.InitOrder(i, e, this);
                //pair.RouletteItemInstance.gameObject.SetActive(false);
            }
        }

        public void OpenView()
        {
            if (animator != null)
            {
                animator.Play("ZoomIn");
            }
            else
            {
                gameObject.SetActive(true);
            }
            foreach (var item in Childs)
            {
                item.Value.SetActive(true);
            }
            IsActive = true;
        }

        public void HideView()
        {
            if (animator != null)
            {
                animator.Play("ZoomOut");
            }
            else
            {
                gameObject.SetActive(false);
            }
            foreach (var item in Childs)
            {
                item.Value.SetActive(false);
            }
            IsActive = false;
        }

        public void PositionOnTouchPanel(Vector3 position)
        {
            if (KeyType != Key.None)
            {
                foreach (KeyControl item in Keyboard.current.allKeys)
                {
                    if (item.keyCode == KeyType)
                    {
                        if (item.isPressed) break;
                        else return;
                    }
                }
            }
            WorldPointer = position;//.SetZ(Depth);
            OpenView();
            //var rect = transform.As<RectTransform>().rect;
            var pos = Mouse.current.position.ReadValue();
            //transform.As<RectTransform>().rect.Set(pos.x, pos.y, rect.width, rect.height);
            transform.position = pos;
            //IsActive = true;
        }

        public bool IsActive = false;

        private void Update()
        {
            if (IsActive)
            {
                if (IsPressDetectForLeft && Mouse.current.leftButton.wasReleasedThisFrame ||
                    !IsPressDetectForLeft && Mouse.current.rightButton.wasReleasedThisFrame)
                {
                    HideView();
                }
            }
        }

        public Vector3 WorldPointer = Vector3.zero;
    }
}
