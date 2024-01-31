using System.Collections;
using System.Collections.Generic;
using AD.BASE;
using AD.UI;
using TMPro;
using UnityEngine;

namespace AD.Utility.Object
{
    public class EditGroup : MonoBehaviour
    {
        private const string TitleKey = "Title";

        #region TopLayer

        public ColliderLayer ColliderLayer;
        public ViewLayer ViewLayer;

        #endregion

        #region Resources

        /// <summary>
        /// 应该在生成实例前配置（既预制体方式设置）
        /// </summary>
        public bool IsHaveTitleTip = false;
        private Text _Title;
        public string TitleText
        {
            get { return (IsHaveTitleTip) ? ViewLayer.UIs[TitleKey].GetComponent<TMP_Text>().text : ""; }
            set
            {
                if (IsHaveTitleTip)
                {
                    _Title.SetText(value);
                }
            }
        }

        #endregion

        private void Start()
        {
            if (IsHaveTitleTip)
            {
                _Title = ViewLayer.UIs[TitleKey].GetComponent<Text>();
                _Title.gameObject.SetActive(false);
            }
        }

        public void DoUpdate(CameraCore core, bool isActive)
        {
            if (IsHaveTitleTip)
            {
                _Title.gameObject.SetActive(isActive);
                if (isActive && !core.Is2D) _Title.gameObject.transform.FaceAt(core.Core.transform);
            }
            if(isActive)
            {
                OnEnter?.Invoke();
            }
            else
            {
                OnExit?.Invoke();
            }
        }

        public ADEvent OnEnter = new(), OnExit = new();
    }
}
