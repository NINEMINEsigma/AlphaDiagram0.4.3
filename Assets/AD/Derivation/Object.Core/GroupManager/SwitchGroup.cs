using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AD.Utility;
using AD.UI;

namespace AD.Utility.Object
{
    public class SwitchGroup : MonoBehaviour
    {
        void Start()
        {
            List<IBoolButton> buttons = this.GetChilds().GetSubList(T => T.GetComponents<IBoolButton>().Length > 0, T => T.GetComponents<IBoolButton>()[0]);
            for (int i = 0, e = buttons.Count; i < e; i++)
            {
                //闭包传递序号值
                //否则会因lambda表达式的捕获性质使得i为buttons.Count
                int index = i;
                buttons[i].AddListener(T =>
                {
                    if (!T) return;
                    foreach (var button in buttons.GetSubList(T => T != buttons[index])) button.isOn = false;
                });
            }
        }
    }
}
