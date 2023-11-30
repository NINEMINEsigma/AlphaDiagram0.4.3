using TMPro;
using UnityEngine;

namespace AD.UI
{
    public class DIALOGUE_Item : ListViewItem
    {
        public TMP_Text Character, Chart;
        public float RootHight, TargetHight, MoveHight;
        public AnimationCurve Curve = AnimationCurve.Linear(0, 0, 1, 1);
        public float FixTime = 0;

        public Animator Animator;

        public override ListViewItem Init()
        {
            Set("", "");
            return this;
        }

        public void Set(string character,string chart)
        {
            Character.text = character;
            Chart.text = chart;
        }
    }
}
