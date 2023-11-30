using System;
using System.Collections;
using System.Collections.Generic;
using AD.BASE;
using UnityEngine;

namespace AD.UI
{
    [Serializable]
    public class DIALOGUE_Source
    {
        public string Speaker;
        public string Chart;
        public float HoldTime;
    }

    public class DIALOGUE : ListView
    {
        [SerializeField] private bool IsAutoPlayChart = true;
        private bool IsAutoPlayChartStart = false;
        [SerializeField] private List<DIALOGUE_Source> SourcePairs = new();
        private int Index = 0;
        private float HoldTime = 0;

        public void StartAutoPlay()
        {
            IsAutoPlayChartStart = true;
            Index = 0;
            HoldTime = SourcePairs[Index].HoldTime;
            this.Clear();
            this.GenerateItem().As<DIALOGUE_Item>().Set(SourcePairs[Index].Speaker, SourcePairs[Index].Chart);
        }

        private void Update()
        {
            if (Index >= SourcePairs.Count) return;
            if(IsAutoPlayChart&& IsAutoPlayChartStart)
            {
                HoldTime -= Time.deltaTime;
                if (HoldTime < 0)
                {
                    Index++;
                    if (Index >= SourcePairs.Count) return;
                    HoldTime = SourcePairs[Index].HoldTime;
                    this.GenerateItem().As<DIALOGUE_Item>().Set(SourcePairs[Index].Speaker, SourcePairs[Index].Chart);
                }
            }
        }
    }
}
