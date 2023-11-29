using System.Collections;
using System.Collections.Generic;
using AD.UI;
using UnityEngine;

namespace AD.Experimental.GameEditor
{
    public class TaskViewItem : ListViewItem
    {
        [SerializeField] AD.UI.Text Title;
        [SerializeField] AD.UI.ModernUIFillBar Percent;

        public TaskInfo taskInfo;

        public override int SortIndex { get => taskInfo.TaskIndex; set { } }

        public override ListViewItem Init()
        {
            Title.SetText("");
            Percent.currentPercent = 1;
            Percent.minValue = 0;
            Percent.maxValue = 1;
            return this;
        }

        public void Refresh()
        {
            Title.SetText(taskInfo.TaskName);
            Percent.currentPercent = taskInfo.TaskPercent;
            Percent.minValue = taskInfo.Range.x;
            Percent.maxValue = taskInfo.Range.y;
        }

    }
}
