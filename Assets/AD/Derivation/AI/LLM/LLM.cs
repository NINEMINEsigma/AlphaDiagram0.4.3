using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using AD.BASE;
using UnityEngine;

namespace AD.Experimental.LLM
{
    public class LLM : MonoSystem
    {
        /// <summary>
        /// api地址
        /// </summary>
        public string url;
        /// <summary>
        /// 提示词，与消息一起发送
        /// </summary>
        [Header("发送的提示词设定")]
        public string Prompt = string.Empty;
        [Header("是否使用默认的提示词模式")]
        public bool IsUseDefaultPromptFormat = true;
        /// <summary>
        /// 语言
        /// </summary
        [Header("设置回复的语言")]
        public string lan = "中文";
        /// <summary>
        /// 上下文保留条数
        /// </summary>
        [Header("上下文保留条数")]
        public int m_HistoryKeepCount = 15;
        /// <summary>
        /// 缓存对话
        /// </summary>
        public List<SendData> m_DataList = new();
        [Header("Assets")]
        /// <summary>
        /// 计算方法调用的时间
        /// </summary>
        public Stopwatch stopwatch = new();

        /// <summary>
        /// 发送消息
        /// </summary>
        public virtual void PostMessage(string _msg, Action<string> _callback)
        {
            //上下文条数设置
            CheckHistory();
            //提示词处理
            string message =
                IsUseDefaultPromptFormat ? ("当前为角色的人物设定：" + Prompt +
                " 回答的语言：" + lan +
                " 接下来是我的提问：" + _msg) : _msg;

            //缓存发送的信息列表
            m_DataList.Add(new SendData("user", message));

            StartCoroutine(Request(message, _callback));
        }

        public virtual IEnumerator Request(string _postWord, System.Action<string> _callback)
        {
            yield return new WaitForEndOfFrame();

        }

        /// <summary>
        /// 设置保留的上下文条数，防止太长
        /// </summary>
        public virtual void CheckHistory()
        {
            if (m_DataList.Count > m_HistoryKeepCount)
            {
                m_DataList.RemoveAt(0);
            }
        }

        public override void Init()
        {
            m_DataList = new List<SendData>();
        }

        [Serializable]
        public class SendData
        {
            [SerializeField] public string role;
            [SerializeField] public string content;
            public SendData() { }
            public SendData(string _role, string _content)
            {
                role = _role;
                content = _content;
            }

        }

        public virtual VariantSetting GetSetting()
        {
            return new VariantSetting()
            {
                DataList = m_DataList,
                HistoryKeepCount = m_HistoryKeepCount,
                lan = lan,
                url = url,
                Prompt = Prompt,
                IsUseDefaultPromptFormat = IsUseDefaultPromptFormat,
            };
        }

        public virtual void InitVariant(VariantSetting setting)
        {
            this.url=setting.url;
            this.lan = setting.lan;
            this.m_HistoryKeepCount = setting.HistoryKeepCount;
            this.m_DataList = setting.DataList;
            this.Prompt = setting.Prompt;
            this.IsUseDefaultPromptFormat = setting.IsUseDefaultPromptFormat;
        }

        [Serializable]
        public class VariantSetting
        {
            public string url;
            public string lan;
            public int HistoryKeepCount;
            public string Prompt;
            public bool IsUseDefaultPromptFormat;
            public List<SendData> DataList;
            public Dictionary<string, string> Settings;
        }
    }
}
