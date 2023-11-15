using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AD.UI
{
    public class CustomWindowTester : MonoBehaviour
    {
        public CustomWindowElement CustomWindowElement;

        public void Add()
        {
            CustomWindowElement.GenerateButton(CustomWindowElement.Childs.Count.ToString(), new Vector2(Random.Range(50, 200), Random.Range(50, 200)));
        }

        public void Init()
        {
            CustomWindowElement.RefreshAllChild();
        }

    }
}
