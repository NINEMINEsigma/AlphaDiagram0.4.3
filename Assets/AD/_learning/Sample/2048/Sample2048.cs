using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AD.BASE;
using AD.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AD.Learn
{
    [Serializable]
    public class HistoryStack : ADModel
    {
        public List<List<int>> KeyHistorys;

        public override void Init()
        {
            KeyHistorys = new()
            {
                new List<int>(){0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0}
            };

            Architecture.SendCommand<GenerateRandomKey>();
        }
    }

    public class App : ADArchitecture<App>
    {
        public override void Init()
        {
            //There are strict requirements for the order of registration
            this
                .RegisterCommand<RightMove>()
                .RegisterCommand<LeftMove>()
                .RegisterCommand<UpMove>()
                .RegisterCommand<DownMove>()

                .RegisterCommand<GenerateRandomKey>()
                .RegisterModel<HistoryStack>()
                ;
        }
    }

    /// <summary>
    /// Create a new key based on the current
    /// </summary>
    public class GenerateRandomKey : ADCommand
    {
        public override void OnExecute()
        {
            var keyB = Architecture.GetModel<HistoryStack>().KeyHistorys.Last();
            int index = GetNumber();
            HashSet<int> iindexs = new();
            int timeC = 0;
            while (iindexs.Count < 16 && timeC < 5000)
            {
                while (iindexs.Contains(index)) index = GetNumber();
                if (keyB[index] == 0) break;
                else iindexs.Add(index);
                timeC++;
            }
            if (iindexs.Count < 16)
                keyB[index] = (int)Mathf.Pow(2, GetNumber() / 4);
        }

        private int GetNumber()
        {
            return (int)(UnityEngine.Random.value % 16);
        }
    }

    /// <summary>
    /// Create a new keyboard at history last after the move
    /// </summary>
    public abstract class MoveKeys : ADCommand
    {
        public override void OnExecute()
        {
            OnMoveExecute();
            Architecture
                .SendCommand<GenerateRandomKey>()
                .GetController<Sample2048>()
                .RefreshPanel();
        }

        protected abstract void OnMoveExecute();
    }

    public class UpMove : MoveKeys
    {
        protected override void OnMoveExecute()
        {
            var history = Architecture.GetModel<HistoryStack>();
            var cur = history.KeyHistorys.Last();
            List<int> result = new();
            result.AddRange(cur);
            history.KeyHistorys.Add(result);

            bool isMerge = true;
            while (isMerge)
            {
                isMerge = false;
                for (int horizontal = 0; horizontal < 4; horizontal++)
                {
                    for (int vertical = 0; vertical < 4; vertical++)
                    {
                        int current = result[vertical * 4 + horizontal];
                        if (current == 0) continue;
                        for (int next = vertical - 1; next >= 0; next--)
                        {
                            int nextKey = result[next * 4 + horizontal];
                            if (nextKey == 0)
                            {
                                continue;
                            }
                            else if (nextKey == current)
                            {
                                result[next * 4 + horizontal] = nextKey + current;
                                isMerge = true;
                            }
                            else
                            {
                                result[vertical * 4 + horizontal] = 0;
                                result[(next + 1) * 4 + horizontal] = current;
                            }
                        }
                    }
                }
            }
        }
    }

    public class DownMove : MoveKeys
    {
        protected override void OnMoveExecute()
        {
            var history = Architecture.GetModel<HistoryStack>();
            var cur = history.KeyHistorys.Last();
            List<int> result = new();
            result.AddRange(cur);
            history.KeyHistorys.Add(result);

            bool isMerge = true;
            while (isMerge)
            {
                isMerge = false;
                for (int horizontal = 0; horizontal < 4; horizontal++)
                {
                    for (int vertical = 4; vertical >= 0; vertical--)
                    {
                        int current = result[vertical * 4 + horizontal];
                        if (current == 0) continue;
                        for (int next = vertical + 1; next < 4; next++)
                        {
                            int nextKey = result[next * 4 + horizontal];
                            if (nextKey == 0)
                            {
                                continue;
                            }
                            else if (nextKey == current)
                            {
                                result[next * 4 + horizontal] = nextKey + current;
                                isMerge = true;
                            }
                            else
                            {
                                result[vertical * 4 + horizontal] = 0;
                                result[(next - 1) * 4 + horizontal] = current;
                            }
                        }
                    }
                }
            }
        }
    }

    public class RightMove : MoveKeys
    {
        protected override void OnMoveExecute()
        {
            var history = Architecture.GetModel<HistoryStack>();
            var cur = history.KeyHistorys.Last();
            List<int> result = new();
            result.AddRange(cur);
            history.KeyHistorys.Add(result);

            bool isMerge = true;
            while (isMerge)
            {
                isMerge = false;
                for (int vertical = 0; vertical < 4; vertical++)
                {
                    for (int horizontal = 4; horizontal >= 0; horizontal--)
                    {
                        int current = result[vertical * 4 + horizontal];
                        if (current == 0) continue;
                        for (int next = horizontal + 1; next < 4; next++)
                        {
                            int nextKey = result[vertical * 4 + next];
                            if (nextKey == 0)
                            {
                                continue;
                            }
                            else if (nextKey == current)
                            {
                                result[next * 4 + horizontal] = nextKey + current;
                                isMerge = true;
                            }
                            else
                            {
                                result[vertical * 4 + horizontal] = 0;
                                result[vertical * 4 + (next - 1)] = current;
                            }
                        }
                    }
                }
            }
        }
    }

    public class LeftMove : MoveKeys
    {
        protected override void OnMoveExecute()
        {
            var history = Architecture.GetModel<HistoryStack>();
            var cur = history.KeyHistorys.Last();
            List<int> result = new();
            result.AddRange(cur);
            history.KeyHistorys.Add(result);

            bool isMerge = true;
            while (isMerge)
            {
                isMerge = false;
                for (int vertical = 0; vertical < 4; vertical++)
                {
                    for (int horizontal = 0; horizontal < 4; horizontal++)
                    {
                        int current = result[vertical * 4 + horizontal];
                        if (current == 0) continue;
                        for (int next = horizontal - 1; next >= 0; next--)
                        {
                            int nextKey = result[vertical * 4 + next];
                            if (nextKey == 0)
                            {
                                continue;
                            }
                            else if (nextKey == current)
                            {
                                result[next * 4 + horizontal] = nextKey + current;
                                isMerge = true;
                            }
                            else
                            {
                                result[vertical * 4 + horizontal] = 0;
                                result[vertical * 4 + (next + 1)] = current;
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// This section will teach you how to use the AD content to quickly build a 2048 game
    /// </summary>
    public class Sample2048 : ADController
    {
        public TouchPanel m_TouchPanel;

        public ModernUIButton[] ButtonKeys;
        public List<int> stack0, stack1;

        private void Start()
        {
            App.instance.Init();
            App.instance.RegisterController(this);
        }

        /// <summary>
        /// Receive the registrar obtained from the ADG and keep it present to avoid its self-deregistration after its destruction
        /// </summary>
        private RegisterInfo onClick, onRel;

        /// <summary>
        /// AddListeners to the input UI components
        /// </summary>
        public override void Init()
        {
            InitVecCounter();
            //Input listening is done through the component itself and the ADG mounted on it
            m_TouchPanel.OnEvent.AddListener(CatchPointerDrag);
            onClick = ADGlobalSystem.AddListener(Mouse.current.leftButton, InitVecCounter, PressType.ThisFramePressed);
            onRel = ADGlobalSystem.AddListener(Mouse.current.leftButton, CounterEndStatus, PressType.ThisFrameReleased);

            RefreshPanel();
        }
        /// <summary>
        /// Releasing registered listeners upon destruction
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();
            m_TouchPanel.OnEvent?.RemoveListener(CatchPointerDrag);
            onClick?.UnRegister();
            onRel?.UnRegister();
        }

        private void InitVecCounter()
        {
            TimeCounter = 0;
            Delta = Vector2.zero;
        }

        private void CounterEndStatus()
        {
            if (TimeCounter > 0.01f && Delta.magnitude > 3)
            {
                if (Mathf.Abs(Delta.x) > Mathf.Abs(Delta.y))
                {
                    if (Delta.x > 0)
                    {
                        this.SendCommand<RightMove>();
                    }
                    else
                    {
                        this.SendCommand<LeftMove>();
                    }
                }
                else
                {
                    if (Delta.y > 0)
                    {
                        this.SendCommand<UpMove>();
                    }
                    else
                    {
                        this.SendCommand<DownMove>();
                    }
                }
            }
            InitVecCounter();
        }

        public float TimeCounter = 0;
        public Vector2 Delta = Vector2.zero;
        public int isZooming = 0;

        public void CatchPointerDrag(Vector2 vec)
        {
            if (isZooming > 0) return;
            if (Mathf.Abs(Mathf.Abs(vec.x) - Mathf.Abs(vec.y)) >= 2)
            {
                TimeCounter += Time.deltaTime;
                Delta += vec;
            }
            else
            {
                // InitVecCounter();
            }
        }

        private IEnumerator ZoomingText(int index, float start, float end)
        {
            isZooming++;
            float tc = 1;
            while (tc > 0)
            {
                ButtonKeys[index].SetTitle(Mathf.Lerp(end, start, tc).ToString("F2"));
                yield return null;
                tc -= Time.deltaTime;
            }
            ButtonKeys[index].SetTitle(((int)end).ToString());
            isZooming--;
        }

        public void RefreshPanel()
        {
            var hi = Architecture.GetModel<HistoryStack>().KeyHistorys;
            if (hi.Count == 1)
            {
                for (int i = 0; i < 4; i++)
                    for (int j = 0; j < 4; j++)
                    {
                        int index = i * 4 + j;
                        ButtonKeys[index].SetTitle(hi[^1][index].ToString());
                    }
            }
            else
            {
                List<int> current = hi[^1], last = hi[^2];
                stack0 = current;
                stack1 = last;
                for (int i = 0; i < 4; i++)
                    for (int j = 0; j < 4; j++)
                    {
                        int index = i * 4 + j;
                        if (current[index] != last[index])
                            StartCoroutine(ZoomingText(index, last[index], current[index]));
                    }
            }
        }
    }
}
