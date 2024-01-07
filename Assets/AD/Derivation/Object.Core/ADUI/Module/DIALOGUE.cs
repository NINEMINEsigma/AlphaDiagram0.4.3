using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AD.BASE;
using AD.Experimental.LLM;
using UnityEngine;

namespace AD.UI
{
    [Serializable]
    public class DIALOGUE_Source
    {
        public string Speaker;
        public string Chart;
        public float HoldTime;

        public DIALOGUE_Source()
        {
        }

        public DIALOGUE_Source(string speaker, string chart, float holdTime)
        {
            Speaker = speaker;
            Chart = chart;
            HoldTime = holdTime;
        }
    }

    public class DIALOGUE : ListView
    {
        [SerializeField] private bool IsAutoPlayChart = true;
        private bool IsAutoPlayChartStart = false;
        [SerializeField] private List<DIALOGUE_Source> SourcePairs = new();
        [SerializeField] private int Index = 0;
        private float HoldTime = 0;

        public void StartAutoPlay()
        {
            IsAutoPlayChartStart = true;
            Index = 0;
            if (SourcePairs.Count > 0) HoldTime = SourcePairs[Index].HoldTime;
            this.Clear();
            if (SourcePairs.Count > 0) this.GenerateItem().As<DIALOGUE_Item>().Set(SourcePairs[Index].Speaker, SourcePairs[Index].Chart);
        }

        protected override void Start()
        {
            base.Start();
            if(IsUsingLLM)
            {
                SourcePairs.Add(new DIALOGUE_Source(LLMName, "初始提示词是：" + TargetLLM.Prompt, 2));
                HoldTime = SourcePairs[Index].HoldTime;
            }
        }

        private void Update()
        {
            if (Index >= SourcePairs.Count || SourcePairs.Count == 0) return;
            if (IsAutoPlayChart && IsAutoPlayChartStart)
            {
                if (HoldTime <= 0)
                {
                    this.GenerateItem().As<DIALOGUE_Item>().Set(SourcePairs[Index].Speaker, SourcePairs[Index].Chart);
                    Index++;
                    if(Index < SourcePairs.Count) HoldTime = SourcePairs[Index].HoldTime;
                }
                else HoldTime -= Time.deltaTime;
            }
        }

        #region Exten

        public bool IsUsingLLM = false;
        public LLM TargetLLM;
        public string SenderName, LLMName;

        public AD.UI.ModernUIInputField InputWord;

        /// <summary>
        /// 发送信息
        /// </summary>
        public virtual void SendData()
        {
            if (InputWord.text.Equals(""))
                return;

            //添加记录聊天
            SourcePairs.Add(new(SenderName, InputWord.text, 0.5f));
            //提示词
            string _msg = InputWord.text;

            //发送数据
            TargetLLM.PostMessage(_msg, CallBack);

            InputWord.text = "";
        }
        /// <summary>
        /// 带文字发送
        /// </summary>
        /// <param name="_postWord"></param>
        public virtual void SendData(string _postWord)
        {
            if (_postWord.Equals(""))
                return;

            //添加记录聊天
            SourcePairs.Add(new(SenderName, _postWord, 2));
            //提示词
            string _msg = _postWord;

            //发送数据
            TargetLLM.PostMessage(_msg, CallBack);

            InputWord.text = "";
        }

        /// <summary>
        /// AI回复的信息的回调
        /// </summary>
        /// <param name="_response"></param>
        protected virtual void CallBack(string _response)
        {
            List<string> strs = _response.Trim()[1..^1].Split("\\n").ToList();
            Debug.Log("收到回复：" + _response);
            float ht = Mathf.Clamp(strs.Count / 2.0f, 0.5f, 2);
            int lengthCount = 80;
            foreach (string str in strs)
            {
                int tc = str.Length / lengthCount;
                for (int i = 0; i <= tc; i++)
                {
                    int start = i * lengthCount, end = Mathf.Min((i + 1) * lengthCount, str.Length);
                    if (start == end) break;
                    string sub = str[start..end];
                    Debug.Log("生成：" + sub);
                    SourcePairs.Add(new(LLMName, sub, ht));
                }
            }
        }

        #endregion

    }
}
