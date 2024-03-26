using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AD.BASE;
using AD.UI;
using AD.Utility;
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

#if ARCHITECTURE_VIRTUAL
        public override IADArchitecture SendCommand<_Command>()
        {
            DebugExtension.LogMethodEnabled = true;
            base.SendCommand<_Command>();
            DebugExtension.LogMethodEnabled = false;
            return this;
        }
#endif

        public static string Serialize(List<int> from)
        {
            return "\n" +
                $"{from[0]} {from[1]} {from[2]} {from[3]}\n" +
                $"{from[4]} {from[5]} {from[6]} {from[7]}\n" +
                $"{from[8]} {from[9]} {from[10]} {from[11]}\n" +
                $"{from[12]} {from[13]} {from[14]} {from[15]}";
        }


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

            DebugExtension.LogMethodEnabled = false;
        }
    }

    /// <summary>
    /// Create a new key based on the current
    /// </summary>
    public class GenerateRandomKey : ADCommand
    {
        public GenerateRandomKey()
        {
            UnityEngine.Random.InitState((int)System.DateTime.Today.Ticks);
        }

        public override void OnExecute()
        {
            var keyB = Architecture.GetModel<HistoryStack>().KeyHistorys.Last();
            int index = GetNumber();
            HashSet<int> iindexs = new();
            while (iindexs.Count < 16)
            {
                while (iindexs.Contains(index)) index = ++index % 16;
                if (keyB[index] == 0) break;
                else iindexs.Add(index);
            }
            if (iindexs.Count < 16)
                keyB[index] = (int)Mathf.Pow(2, GetNumber() / 4);
        }

        private int GetNumber()
        {
            return (int)(UnityEngine.Random.value * 16);
        }
    }

    /// <summary>
    /// Create a new keyboard at history last after the move
    /// </summary>
    public abstract class MoveKeys : ADCommand
    {
        public override void OnExecute()
        {
            var history = Architecture.GetModel<HistoryStack>();
            var cur = history.KeyHistorys.Last();
            List<int> result = new();
            result.AddRange(cur);
            history.KeyHistorys.Add(result);
            OnMoveExecute(result);
            Architecture
                .SendCommand<GenerateRandomKey>()
                .GetController<Sample2048>()
                .RefreshPanel();
        }

        protected abstract void OnMoveExecute(List<int> result);
    }

    public class UpMove : MoveKeys
    {
        protected override void OnMoveExecute(List<int> result)
        {

            DebugExtension.LogMessage("Start : " + App.Serialize(result));

            bool isMerge = true;
            while (isMerge)
            {
                isMerge = false;
                //scanning from left to right
                for (int horizontal = 0; horizontal < 4 && horizontal >= 0; horizontal++)
                {
                    //scanning from top to bottom
                    for (int vertical = 0; vertical < 4 && vertical >= 0; vertical++)
                    {
                        if (result[vertical * 4 + horizontal] != 0)
                        {
                            for (int i = vertical - 1; i >= 0; i--)
                            {
                                DebugExtension.LogMessage("OnUp : " + App.Serialize(result));
                                int next = result[i * 4 + horizontal];
                                int current = result[(i + 1) * 4 + horizontal];
                                if (next == 0)
                                {
                                    result[i * 4 + horizontal] = result[(i + 1) * 4 + horizontal];
                                    result[(i + 1) * 4 + horizontal] = 0;
                                }
                                else if (next == current)
                                {
                                    result[i * 4 + horizontal] = result[(i + 1) * 4 + horizontal] * 2;
                                    result[(i + 1) * 4 + horizontal] = 0;
                                }
                                else break;
                            }
                        }
                    }
                }
            }

            DebugExtension.LogMessage("End : " + App.Serialize(result));
        }
    }

    public class DownMove : MoveKeys
    {
        protected override void OnMoveExecute(List<int> result)
        {
            DebugExtension.LogMessage("Start : " + App.Serialize(result));

            bool isMerge = true;
            while (isMerge)
            {
                isMerge = false;
                //scanning from left to right
                for (int horizontal = 0; horizontal < 4 && horizontal >= 0; horizontal++)
                {
                    //scanning from bottom to top
                    for (int vertical = 3; vertical < 4 && vertical >= 0; vertical--)
                    {
                        if (result[vertical * 4 + horizontal] != 0)
                        {
                            //move to bottom
                            for (int i = vertical + 1; i < 4; i++)
                            {
                                DebugExtension.LogMessage("OnDown : " + App.Serialize(result));
                                int next = result[i * 4 + horizontal];
                                int current = result[(i - 1) * 4 + horizontal];
                                if (next == 0)
                                {
                                    result[i * 4 + horizontal] = result[(i - 1) * 4 + horizontal];
                                    result[(i - 1) * 4 + horizontal] = 0;
                                }
                                else if (next == current)
                                {
                                    result[i * 4 + horizontal] = result[(i - 1) * 4 + horizontal] * 2;
                                    result[(i - 1) * 4 + horizontal] = 0;
                                }
                                else break;
                            }
                        }
                    }
                }
            }

            DebugExtension.LogMessage("End : " + App.Serialize(result));
        }
    }

    public class RightMove : MoveKeys
    {
        protected override void OnMoveExecute(List<int> result)
        {
            DebugExtension.LogMessage("Start : " + App.Serialize(result));

            bool isMerge = true;
            while (isMerge)
            {
                isMerge = false;
                //scanning from top to bottom
                for (int vertical = 0; vertical < 4 && vertical >= 0; vertical++)
                {
                    //scanning from right to left
                    for (int horizontal = 3; horizontal < 4 && horizontal >= 0; horizontal--)
                    {
                        //if not null
                        if (result[vertical * 4 + horizontal] != 0)
                        {
                            //move to right
                            for (int i = horizontal + 1; i < 4 ; i++)
                            {
                                DebugExtension.LogMessage("OnRight : " + App.Serialize(result));
                                int next = result[i  + vertical * 4];
                                int current = result[(i - 1)  + vertical * 4];
                                if (next == 0)
                                {
                                    result[i  + vertical * 4] = result[(i - 1) + vertical * 4];
                                    result[(i - 1)  + vertical * 4] = 0;
                                }
                                else if (next == current)
                                {
                                    result[i  + vertical * 4] = result[(i - 1) + vertical * 4] * 2;
                                    result[(i - 1)  + vertical * 4] = 0;
                                }
                                else break;
                            }
                        }
                    }
                }
            }

            DebugExtension.LogMessage("End : " + App.Serialize(result));
        }
    }

    public class LeftMove : MoveKeys
    {
        protected override void OnMoveExecute(List<int> result)
        {
            DebugExtension.LogMessage("Start : " + App.Serialize(result));

            bool isMerge = true;
            while (isMerge)
            {
                isMerge = false;
                //scanning from top to bottom
                for (int vertical = 0; vertical < 4 && vertical >= 0; vertical++)
                {
                    //scanning from left to right
                    for (int horizontal = 0; horizontal < 4 && horizontal >= 0; horizontal++)
                    {
                        //if not null
                        if (result[vertical * 4 + horizontal] != 0)
                        {
                            //move to left
                            for (int i = horizontal - 1; i >= 0; i--)
                            {
                                DebugExtension.LogMessage("OnLeft : " + App.Serialize(result));
                                int next = result[i + vertical * 4];
                                int current = result[(i + 1) + vertical * 4];
                                if (next == 0)
                                {
                                    result[i + vertical * 4] = result[(i + 1) + vertical * 4];
                                    result[(i + 1) + vertical * 4] = 0;
                                    isMerge = true;
                                }
                                else if (next == current)
                                {
                                    result[i + vertical * 4] = result[(i + 1) + vertical* 4 ] * 2;
                                    result[(i + 1) + vertical * 4] = 0;
                                    isMerge = true;
                                }
                                else break;
                            }
                        }
                    }
                }
            }

            DebugExtension.LogMessage("End : " + App.Serialize(result));
        }
    }

    /// <summary>
    /// This section will teach you how to use the AD content to quickly build a 2048 game
    /// </summary>
    public class Sample2048 : ADController
    {
        public TouchPanel m_TouchPanel;

        public RectTransform ButtonPaneTransform;

        public ModernUIButton[] ButtonKeys;

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
                StartCoroutine(ReleasePanelEuler());
            }
            InitVecCounter();
        }

        public float TimeCounter = 0;
        public Vector2 Delta = Vector2.zero;
        public int isZooming = 0;

        private IEnumerator ReleasePanelEuler()
        {
            isZooming++;
            float tc = 1;
            Vector3 origin = ButtonPaneTransform.localEulerAngles;
            while (tc > 0)
            {
                yield return null;
                tc -= Time.deltaTime * 3;
                float t = EaseCurve.InQuad(0, 1, tc);
                ButtonPaneTransform.localEulerAngles = new Vector3(Mathf.LerpAngle(0, origin.x, t), Mathf.LerpAngle(0, origin.y, t), Mathf.LerpAngle(0, origin.z, t));
            }
            ButtonPaneTransform.localEulerAngles = Vector3.zero;
            isZooming--;
        }

        public void CatchPointerDrag(Vector2 vec)
        {
            if (isZooming > 0) return;
            if (Mathf.Abs(Mathf.Abs(vec.x) - Mathf.Abs(vec.y)) >= 2)
            {
                TimeCounter += Time.deltaTime;
                Delta += vec;
                var v = -Delta.normalized * TimeCounter * 125;
                ButtonPaneTransform.localEulerAngles = new Vector3(Mathf.Clamp(v.y, -7, 7), Mathf.Clamp(v.x, -7, 7),0);
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
            Color targetColor = start > end ? Color.red : Color.green;
            Color endCOLOR = start == 0 ? Color.yellow : Color.white;
            ButtonKeys[index].SetTitle(((int)end).ToString());
            while (tc > 0)
            {
                float t = EaseCurve.OutQuad(end, start, tc);
                //ButtonKeys[index].SetTitle(t.ToString("F1"));
                ButtonKeys[index].SetNormalColor(Color.Lerp(endCOLOR, targetColor, t));
                yield return null;
                tc -= Time.deltaTime * 3;
            }
            ButtonKeys[index].SetNormalColor(endCOLOR);
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
