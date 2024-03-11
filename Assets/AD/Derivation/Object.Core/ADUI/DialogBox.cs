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
        public string text;
        public ADEvent OnNext = new();
        [Header("Setting")]
        public bool IsZoom = true;
        public bool IsTwoSpeed = false;
        public bool IsAuto = false;

        private bool IsEnd = true;
        public void EndImmediately()
        {
            if(IsZoom&&!IsEnd)
            {
                IsEnd = true;
            }
            else
            {
                DoImmediatelyNext();
            }
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
        public void SendingText(float duration)
        {
            IsEnd = false;
            if (OnCoroutine != null) StopCoroutine(OnCoroutine);
            OnCoroutine = StartCoroutine(DoSendingText(duration));
        }
        private IEnumerator DoSendingText(float duration)
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
            }
            else
                yield return new WaitForSeconds(duration);
            MainText.SetText(text);
            if (!IsEnd)
            {
                OnNext.Invoke();
            }
            IsEnd = true;
            OnCoroutine = null;
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
        }
    }
}
