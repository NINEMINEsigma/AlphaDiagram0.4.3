using System.Collections;
using System.Collections.Generic;
using AD.BASE;
using UnityEngine;

namespace AD.Experimental.GameEditor
{
    public class HierarchyEx : ADController
    {
        private bool isRegister = false;
        public float minWidth, MaxWidth;
        public RectTransform HRectTransform;

        private void Awake()
        {
            if(!isRegister)
            {
                GameEditorApp.instance.RegisterController(this);
            }
        }

        public override void Init()
        {

        }

        public void SetWidth(float value)
        {
            HRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Lerp(minWidth, MaxWidth, value));
        }
    }
}
