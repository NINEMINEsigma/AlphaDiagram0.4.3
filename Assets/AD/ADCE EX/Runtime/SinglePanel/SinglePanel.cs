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
            //isCanRefresh = false;
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

        public override CustomWindowElement ObtainElement(Vector2 rect)
        {
            return ObtainElement().SetRect(rect);
        }

        public CustomWindowElement OnMenuInit(Dictionary<int, Dictionary<string, ADEvent>> Menu, string title = "Menu")
        {
            int counter = 0;
            foreach (var item in Menu)
            {
                foreach (var single in item.Value)
                {
                    counter++;
                }
                counter++;
            }

            CustomWindowElement singlePanel = ObtainElement(new Vector2(200, Mathf.Min(counter * 23, 800))).SetTitle(title.Translate());

            if (counter * 23 > 800)
            {
                var listView = ADGlobalSystem.GenerateElement<ListView>().PrefabInstantiate();
                singlePanel.SetADUIOnWindow<ListView>("ListView", listView, new Vector2(200, 800));

                int iiiindex = 0;
                int icounter = 0;
                foreach (var layer in Menu)
                {
                    foreach (var item in layer.Value)
                    {
                        AD.UI.Button button = AD.UI.Button.Generate(iiiindex.ToString() + "_" + item.Key).SetTitle(item.Key).AddListener(item.Value.Invoke);
                        button.transform.As<RectTransform>().sizeDelta = new Vector2(200, 23);
                        listView.Add(icounter++, button.gameObject);
                    }
                    GameObject obj = SinglePanelLinePerfab.PrefabInstantiate();
                    obj.name = iiiindex.ToString() + "_Layer";
                    obj.transform.As<RectTransform>().sizeDelta = new Vector2(200, 5);
                    listView.Add(icounter++, obj);
                }
            }
            else
            {
                int iiiindex = 0;
                foreach (var layer in Menu)
                {
                    foreach (var item in layer.Value)
                    {
                        singlePanel.GenerateButton(iiiindex.ToString() + "_" + item.Key, new Vector2(200, 23)).SetTitle(item.Key).AddListener(item.Value.Invoke);
                    }
                    singlePanel.SetItemOnWindow(iiiindex++.ToString() + "Layer", SinglePanelLinePerfab.PrefabInstantiate(), new Vector2(200, 5));
                }

                singlePanel.RefreshAllChild();
            }

            return singlePanel;
        }

        public CustomWindowElement OnMenuInitWithRect(Dictionary<int, Dictionary<string, ADEvent>> Menu, RectTransform TargetUI, string title = "Menu")
        {
            var singlePanel = OnMenuInit(Menu, title);
            var rects = singlePanel.rectTransform.GetRect();
            var targetRects = TargetUI.GetRect();
            Vector3 lt = rects[0], rb = rects[2];
            singlePanel.rectTransform.position = new Vector3(targetRects[0].x + (rb.x - lt.x) * 0.5f, targetRects[0].y + (lt.y - rb.y) * 0.5f, TargetUI.position.z - 0.01f);
            return singlePanel;
        }
    }
}
