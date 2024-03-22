using System;
using System.Collections;
using System.Collections.Generic;
using AD.BASE;
using AD.UI;
using UnityEngine;

namespace AD.Experimental.GameEditor
{
    [Serializable]
    public sealed class TaskInfo : IComparable<TaskInfo>
    {
        public string TaskName;
        public int TaskIndex;
        private float _TaskPercent;
        public float TaskPercent
        {
            get => _TaskPercent;
            set
            {
                _TaskPercent = Mathf.Clamp(value, 0, 1.01f);
                GameEditorApp.instance.GetController<Information>()?.Refresh();
            }
        }
        public float TaskValue
        {
            get => TaskPercent * RangeDic + Range.x;
            set
            {
                TaskPercent = (value - Range.x) / RangeDic;
            }
        }
        private Vector2 _Range = new(0, 1);
        public Vector2 Range
        {
            get => _Range;
            set
            {
                _Range = value;
                RangeDic = value.y - value.x;
            }
        }
        private float RangeDic = 1;
        public bool IsInt = false;

        public TaskInfo() { }
        public TaskInfo(string taskName, int taskIndex, float taskPercent, Vector2 range, bool isInt)
        {
            TaskName = taskName;
            TaskIndex = taskIndex;
            TaskPercent = taskPercent;
            Range = range;
            IsInt = isInt;
        }

        public void Wait()
        {
            GameEditorApp.instance.GetModel<TaskList>().Wait(this);
        }

        public void WaitLast()
        {
            GameEditorApp.instance.GetModel<TaskList>().WaitLast(this);
        }

        public int CompareTo(TaskInfo other)
        {
            return this.TaskIndex.CompareTo(other.TaskIndex);
        }

        public void Register()
        {
            GameEditorApp.instance.GetModel<TaskList>().AddTask(this);
        }

        public void UnRegister()
        {
            GameEditorApp.instance.GetModel<TaskList>().RemoveTask(this);
        }
    }

    public sealed class TaskList : ADModel
    {
        public override void Init()
        {
            Current = null;
            Tasks.Clear();
            AddTaskCallBack = new();
            RemoveTaskCallBack = new();
            CompleteTaskCallBack = new();
        }

        public TaskInfo Current { get; private set; }
        public List<TaskInfo> Tasks { get; private set; } = new();

        public IADModel Load(string path)
        {
            throw new NotImplementedException();
        }

        public void Save(string path)
        {
            throw new NotImplementedException();
        }

        public TaskInfo RegisterTask(string taskName, int taskIndex, float taskPercent, Vector2 range, bool isInt)
        {
            TaskInfo task = new(taskName, taskIndex, taskPercent, range, isInt);
            AddTask(task);
            return task;
        }

        public ADEvent<TaskInfo> AddTaskCallBack = new();
        public ADEvent<TaskInfo> RemoveTaskCallBack = new();
        public ADEvent<TaskInfo> CompleteTaskCallBack = new();

        public void AddTask(TaskInfo task)
        {
            if (Current != null)
            {
                if(task.TaskIndex<Current.TaskIndex)
                {
                    Tasks.Add(Current);
                    Current = task;
                }
                else Tasks.Add(task);
                Tasks.Sort();
            }
            else Current = task;
            AddTaskCallBack.Invoke(task);
            Update();
        }

        public void RemoveTask(TaskInfo task)
        {
            if (Current == task)
            {
                if (Tasks.Count > 0)
                {
                    Current = Tasks[0];
                    Tasks.RemoveAt(0);
                }
                else Current = null;
            }
            else
            {
                Tasks.Remove(task);
                Tasks.Sort();
            }
            RemoveTaskCallBack.Invoke(task);
            Update();
        }

        public void Update()
        {
            if (Current == null) return;
            if (Current.TaskPercent >= 1)
            {
                CompleteTaskCallBack.Invoke(Current);
                RemoveTask(Current);
            }
            else Architecture.GetController<Information>().Refresh();
        }

        public void Wait(TaskInfo task)
        {
            if (Current == task)
            {
                if (Tasks.Count == 0) return;
                else
                {
                    Current = Tasks[0];
                    Tasks[0] = task;
                }
            }
            else if (Tasks.Count > 1 && Tasks[^1] != task)
            {
                int targetIndex = Tasks.FindIndex(T => T == task);
                var temp = Tasks[targetIndex + 1];
                Tasks[targetIndex + 1] = task;
                Tasks[targetIndex] = temp;
            }
            Update();
        }

        public void WaitLast(TaskInfo task)
        {
            if (Current == task)
            {
                if (Current == task && Tasks.Count == 0) return;
                else
                {
                    Current = Tasks[^1];
                    Tasks[^1] = task;
                }
            }
            else if (Tasks.Count > 1 && Tasks[^1] != task)
            {
                int targetIndex = Tasks.FindIndex(T => T == task);
                var temp = Tasks[^1];
                Tasks[^1] = task;
                Tasks[targetIndex] = temp;
            }
            Update();
        }
    }

    public class Information : ADController,AD.BASE.ICanMonitorCommand<TaskListRefresh>
    {
        [SerializeField] AD.UI.Text LeftText;
        [SerializeField] AD.UI.Text RightText;
        [SerializeField] Animator TaskPanelAnimator;
        [SerializeField] AD.UI.Text TaskPanelTitle;
        [SerializeField] AD.UI.ListView TaskPanelListView;
        [SerializeField] AD.UI.ModernUIFillBar TaskPanelPercentBar;
        [SerializeField] TaskViewItem TaskViewItemPerfab;
        [SerializeField] SinglePanel SinglePanelPerfab;
        [SerializeField] GameObject SinglePanelLinePerfab;
        [SerializeField] Button EnterMessagePanelTigger;
        [SerializeField] ModernUIInputField MessageInputField;
        [SerializeField] UnityEngine.UI.Button ExitMessagePanelTigger;

        private void Start()
        {
            GameEditorApp.instance.RegisterController(this);
            EnterMessagePanelTigger.AddListener(() =>
            {
                MessageInputField.gameObject.SetActive(true);
                ExitMessagePanelTigger.gameObject.SetActive(true);
            });
            MessageInputField.gameObject.SetActive(false);
            ExitMessagePanelTigger.gameObject.SetActive(false);
            ExitMessagePanelTigger.onClick.AddListener(() =>
            {
                MessageInputField.gameObject.SetActive(false);
                ExitMessagePanelTigger.gameObject.SetActive(false);
            });
            this.SetRight(ADGlobalSystem.Version);
        }

        public override void Init()
        {
            var _m_TaskList = Architecture.GetModel<TaskList>();
            _m_TaskList.AddTaskCallBack.AddListener(T => Refresh());
            _m_TaskList.RemoveTaskCallBack.AddListener(T => Refresh());
            _m_TaskList.CompleteTaskCallBack.AddListener(T => Refresh());
            _m_TaskList.CompleteTaskCallBack.AddListener(CompleteTask);
            TaskPanelListView.SetPrefab(TaskViewItemPerfab);

            Architecture.RegisterSystem<SinglePanelGenerator>();
            Architecture.GetSystem<SinglePanelGenerator>().Parent = transform.parent as RectTransform;
            Architecture.GetSystem<SinglePanelGenerator>().WindowPerfab = SinglePanelPerfab;
            Architecture.GetSystem<SinglePanelGenerator>().SinglePanelLinePerfab = SinglePanelLinePerfab;
        }

        bool IsOpenTaskPanel = false;
        public void ClickTaskProgramBar()
        {
            Refresh();
            IsOpenTaskPanel = !IsOpenTaskPanel;
            TaskPanelAnimator.Play(IsOpenTaskPanel ? "Open" : "Hide");
        }

        public void Log(string message)
        {
            SetLeft(message);
        }

        public void Warning(string message)
        {
            SetLeft(message);
            LeftText.source.color = Color.yellow;
            MessageInputField.text = MessageInputField.text + "\n" + message;
        }

        public void Error(string message)
        {
            SetLeft(message);
            LeftText.source.color = Color.red;
            MessageInputField.text = MessageInputField.text + message + "\n";
        }

        public void Version(string version)
        {
            SetRight(version);
        }

        public void SetLeft(string text)
        {
            try
            {
                LeftText.SetText(text);
                LeftText.source.color = Color.white;
            }
            catch { }
            finally { }
        }

        public void SetRight(string text)
        {
            try
            {
                RightText.SetText(text);
            }
            catch { }
            finally { }
        }

        private TaskViewItem RegisterTaskListItem(TaskInfo task,int index)
        {
            var item = TaskPanelListView.GenerateItem().As<TaskViewItem>();
            item.taskInfo = task;
            item.Refresh();
            task.TaskIndex = index;
            return item;
        }

        public void Refresh()
        {
            var _m_TaskList = Architecture.GetModel<TaskList>();
            //_m_TaskList.Update();
            if (_m_TaskList.Current == null)
            {
                TaskPanelTitle.SetText("");
                TaskPanelListView.Clear();
                TaskPanelPercentBar.Init();
            }
            else
            {
                var current = _m_TaskList.Current;
                TaskPanelPercentBar.Set(current.TaskPercent, current.Range.x, current.Range.y);
            }
            TaskPanelListView.Clear();
            if (_m_TaskList.Current == null) return;
            RegisterTaskListItem(_m_TaskList.Current,0);
            for (int i = 0; i < _m_TaskList.Tasks.Count; i++)
            {
                TaskInfo task = _m_TaskList.Tasks[i];
                RegisterTaskListItem(task, i + 1);
            }
            TaskPanelListView.SortChilds();
        }

        private void CompleteTask(TaskInfo task)
        {
            Architecture.AddMessage(task.TaskName + " is complete");
        }

        public void OnCommandCall(TaskListRefresh c) => Refresh();
    }

    public class TaskListRefresh : AD.BASE.Vibration
    {
        protected TaskListRefresh() { }
    }
}
