using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using AD.Utility;
using UnityEngine.EventSystems;
using AD.BASE;

namespace AD.UI
{
    public class VirtualJoystick : PropertyModule
    {
        [Header("VirtualJoystick")]
        public Camera TargetCamera;

        public Image MineVirtualJoystick;
        public Image Background;
        public Image Fill;
        public Image JoyStick;

        /// <summary>
        /// z=DateValue
        /// </summary>
        public Vector3 Value = new Vector3();
        [HideInInspector] public Vector3 CatchValue = new Vector3();
        public Vector3 DirectionValue
        {
            get
            {
                return new Vector3(Mathf.Clamp(Value.x / MaxX * 0.5f, -1, 1), Mathf.Clamp(Value.y / MaxX * 0.5f, -1, 1), 0);
            }
        }

        public ADEvent<PointerEventData> OnDragEvent = new ADEvent<PointerEventData>(),
            OnStart = new ADEvent<PointerEventData>(),
            OnEnd = new ADEvent<PointerEventData>();

        protected override void Start()
        {
            base.Start();

            MineVirtualJoystick.SetColor_A(0);
            Background.SetColor_A(0);
            JoyStick.SetColor_A(0);
            Fill.fillAmount = 0;

            var vertexs = MineVirtualJoystick.rectTransform.GetRect();
            MaxX = Vector3.Distance(vertexs[1], vertexs[2]);
        }

        public override void InitializeContext()
        {
            base.InitializeContext();
            Context.OnDragEvent = InitializeContextSingleEvent(Context.OnDragEvent, OnDrag);
            Context.OnPointerDownEvent = InitializeContextSingleEvent(Context.OnPointerClickEvent, OnPointerDown);
            Context.OnPointerUpEvent = InitializeContextSingleEvent(Context.OnPointerUpEvent, OnPointerUp);
        }

        public VirtualJoystick()
        {
            ElementArea = nameof(VirtualJoystick);
        }

        [SerializeField] private bool IsDrag = false;
        private float MaxX;
        [HideInInspector] public float DateValue = 180;
        [HideInInspector] public float GlobalAngle = 0;

        private void Update()
        {
            if (IsDrag)
            {
                GlobalAngle = Mathf.Atan2(JoyStick.transform.position.y - transform.position.y, JoyStick.transform.position.x - transform.position.x) * 180 / Mathf.PI;
                float angle = DateValue / 2.0f - 90 + GlobalAngle;
                Fill.rectTransform.eulerAngles = new Vector3(0, 0, angle);
                while (DateValue > 30)
                {
                    DateValue -= 1.5f;
                    Fill.fillAmount = DateValue / 360.0f;
                    Value.z = DateValue;
                }
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            OnDragEvent.Invoke(eventData);
            var Pos = TargetCamera.ScreenToWorldPoint(new Vector3(eventData.position.x, eventData.position.y, Vector3.Distance(transform.position, TargetCamera.transform.position)));
            CatchValue = Value = Pos - transform.position;
            if (Value.magnitude > MaxX / 2.0f) Pos = transform.position + (Pos - transform.position).normalized * MaxX / 2.0f;
            JoyStick.transform.position = new Vector3(Pos.x, Pos.y, JoyStick.transform.position.z);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            OnStart.Invoke(eventData);
            StopCoroutine(nameof(OnInitializePotentialDrag_Start));
            StartCoroutine(OnInitializePotentialDrag_Start());
            IsDrag = true;
        }
        private IEnumerator OnInitializePotentialDrag_Start()
        {
            StopCoroutine(nameof(OnInitializePotentialDrag_End));
            DateValue = 180;
            Value = new Vector3();
            float end = 15.0f;
            for (int i = 0; i < end; i++)
            {
                MineVirtualJoystick.SetColor_A(i / end * 0.5f);
                Background.SetColor_A(i / end * 0.5f);
                JoyStick.SetColor_A(i / end * 0.75f);
                yield return new WaitForEndOfFrame();
            }
            yield return new WaitForEndOfFrame();
            MineVirtualJoystick.SetColor_A(0.5f);
            Background.SetColor_A(0.5f);
            JoyStick.SetColor_A(0.75f);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            OnEnd.Invoke(eventData);
            StopCoroutine(nameof(OnInitializePotentialDrag_End));
            StartCoroutine(OnInitializePotentialDrag_End());
            IsDrag = false;
        }
        private IEnumerator OnInitializePotentialDrag_End()
        {
            StopCoroutine(nameof(OnInitializePotentialDrag_Start));
            float end = 15.0f;
            DateValue = 0;
            Value = new Vector3();
            Fill.fillAmount = DateValue;
            for (int i = 0; i < end; i++)
            {
                MineVirtualJoystick.SetColor_A((1 - i / end) * 0.5f);
                Background.SetColor_A((1 - i / end) * 0.5f);
                JoyStick.SetColor_A((1 - i / end) * 0.75f);
                yield return new WaitForEndOfFrame();
            }
            yield return new WaitForEndOfFrame();
            MineVirtualJoystick.SetColor_A(0);
            Background.SetColor_A(0);
            JoyStick.SetColor_A(0);
            JoyStick.transform.localPosition = new Vector3();
        }

    }
}
