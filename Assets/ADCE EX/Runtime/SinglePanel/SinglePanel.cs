using AD.BASE;
using System.Collections.Generic;
using AD.UI;
using AD.Utility;
using UnityEngine;

namespace AD.Experimental.GameEditor
{
    public class SinglePanel : CustomWindowElement
    {
        protected override bool isSubPageUsingOtherSetting => true;

        public override CustomWindowElement Init()
        {
            base.Init();
            isCanRefresh = false;
            return this;
        }
    }

    public class SinglePanelGenerator : CustomWindowGenerator<SinglePanelGenerator>
    {
        public CustomWindowElement Current;

        public GameObject SinglePanelLinePerfab;

        public override CustomWindowElement ObtainElement()
        {
            if (Current != null) Current.BackPool();
            Current = base.ObtainElement();
            Current.OnEsc.AddListener(() => Current = null);
            return Current;
        }

        public CustomWindowElement OnMenuInit(Dictionary<int, Dictionary<string, ADEvent>> Menu)
        {
            CustomWindowElement singlePanel = ObtainElement(new Vector2(200, 500)).SetTitle("Menu".Translate());

            int iiiindex = 0;
            foreach (var layer in Menu)
            {
                foreach (var item in layer.Value)
                {
                    singlePanel.GenerateButton(iiiindex.ToString() + "_" + item.Key, new Vector2(200, 23)).SetTitle(item.Key).AddListener(item.Value.Invoke);
                }
                singlePanel.SetItemOnWindow(iiiindex++.ToString() + "Layer", GameObject.Instantiate(SinglePanelLinePerfab.gameObject), new Vector2(200, 5));
            }

            return singlePanel;
        }

        public CustomWindowElement OnMenuInitWithRect(Dictionary<int, Dictionary<string, ADEvent>> Menu,RectTransform TargetUI)
        {
            var singlePanel = OnMenuInit(Menu);
            var rects = singlePanel.rectTransform.GetRect();
            var targetRects = TargetUI.GetRect();
            Vector3 lt = rects[0], rb = rects[2];
            singlePanel.rectTransform.position = new Vector3(targetRects[0].x + (rb.x - lt.x) * 0.5f, targetRects[0].y + (lt.y - rb.y) * 0.5f, TargetUI.position.z - 0.01f);
            return singlePanel;
        }
    }
}