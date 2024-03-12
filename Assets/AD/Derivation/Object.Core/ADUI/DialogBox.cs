using System.Collections;
using System.Collections.Generic;
using AD.BASE;
using UnityEngine;

namespace AD.UI
{
    public class DialogStyle
    {
        public string Head;
        public string Tail;

        public DialogStyle()
        {
        }

        public DialogStyle(string head, string tail)
        {
            Head = head;
            Tail = tail;
        }
    }

    public class DialogBox : ADUI
    {
        public DialogBox()
        {
            this.ElementArea = nameof(DialogBox);
        }

        [Header("Objects")]
        public List<GameObject> MyUIs = new();
        public List<ADUI> MyADUIs = new();
        [Header("Assets")]
        public Text MainText;
        public Button ZoomButton;
        public Button TwoSpeedButton;
        public Button AutoButton;
        public string text;
        public ADEvent OnNext = new();
        public float duration = 2;
        [Header("Setting")]
        public bool IsZoom = true;
        public bool IsTwoSpeed = false;
        public bool IsAuto = false;
        public int CharPerSecond = 5;

        public bool IsStop = false;

        private bool IsEnd = true;
        public void EndImmediately()
        {
            if (IsStop) return;
            if (IsAuto)
            {
                IsEnd = false;
                AutoButton.IsClick = false;
                return;
            }
            if (IsEnd)
            {
                SendingText();
                return;
            }
            IsEnd = true;
        }
        private void DoImmediatelyNext()
        {
            OnNext.Invoke();
        }

        public void SetZoom(bool value)
        {
            IsZoom = value;
        }
        public void SetTwoSpeed(bool value)
        {
            IsTwoSpeed = value;
        }
        public void SetAuto(bool value)
        {
            IsAuto = value;
        }

        private Coroutine OnCoroutine;
        public void SendingText()
        {
            if (IsStop) return;
            IsEnd = false;
            if (OnCoroutine != null) StopCoroutine(OnCoroutine);
            OnCoroutine = StartCoroutine(DoSendingText());
        }
        private IEnumerator DoSendingText()
        {
            float current = duration;
            if (IsZoom)
            {
                while (current > 0 && !IsEnd)
                {
                    MainText.SetText(text[..(int)((1 - current / duration) * text.Length)]);
                    yield return null;
                    current -= Time.deltaTime * (IsTwoSpeed ? 2 : 1);
                }
                MainText.SetText(text);
            }
            else
            {
                MainText.SetText(text);
                while (current > 0 && !IsEnd)
                {
                    yield return null;
                    current -= Time.deltaTime * (IsTwoSpeed ? 2 : 1);
                }
            }
            if(!IsEnd)
            {
                OnNext.Invoke();
            }
            IsEnd = true;
            OnCoroutine = null;
        }

        private float update_auto_counter = 0;
        private void Update()
        {
            if (IsAuto && IsEnd && !IsStop)
            {
                if (update_auto_counter < duration && IsEnd)
                {
                    update_auto_counter += Time.deltaTime * (IsTwoSpeed ? 2 : 1);
                }
                else if (IsEnd)
                {
                    update_auto_counter = 0;
                    SendingText();
                }
            }
        }

        public void InitEmpty()
        {
            MainText.SetText("");
            text = "";
        }

        public void Additional(string text)
        {
            this.text += text;
        }
        public void Additional(string text, DialogStyle style)
        {
            this.text = style.Head + text + style.Tail;
        }

        public void SetText(string text)
        {
            this.text = text;
        }
        public void SetDurationn(float value)
        {
            duration = value;
        }

        private void OnDestroy()
        {
            ADUI.Destroy(this);
        }
        private void Start()
        {
            ADUI.Initialize(this);
            for (int i = 0, e = transform.childCount; i < e; i++)
            {
                var child = transform.GetChild(i);
                if(child.SeekComponent<ADUI>().Share(out ADUI targetad) != null)
                {
                    MyADUIs.Add(targetad);
                    MyUIs.Add(child.gameObject);
                }
                else if(child.SeekComponent<IADUI>().Share(out IADUI target)!=null)
                {
                    MyUIs.Add(child.gameObject);
                }
            }
            ZoomButton.AddListener(() => SetZoom(true), AD.PressType.ThisFrameReleased);
            ZoomButton.AddListener(() => SetZoom(false), AD.PressType.ThisFramePressed);
            ZoomButton.IsClick = !IsZoom;
            TwoSpeedButton.AddListener(() => SetTwoSpeed(true), AD.PressType.ThisFrameReleased);
            TwoSpeedButton.AddListener(() => SetTwoSpeed(false), AD.PressType.ThisFramePressed);
            TwoSpeedButton.IsClick = !IsTwoSpeed;
            AutoButton.AddListener(() => SetAuto(false), AD.PressType.ThisFrameReleased);
            AutoButton.AddListener(() => SetAuto(true), AD.PressType.ThisFramePressed);
            AutoButton.IsClick = IsAuto;
            if (!IsStop)
                SendingText();
        }
    }
}
