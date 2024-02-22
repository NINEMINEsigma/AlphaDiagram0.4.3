using AD.BASE;
using UnityEngine.EventSystems;

namespace AD.UI
{
    public class OneRouletteItem : RouletteItem
    {
        public ADEvent SureMyArea = new();

        public bool IsEnter = false;

        protected override void OnEnter(PointerEventData pointerEnterEvent)
        {
            base.OnEnter(pointerEnterEvent);
            IsEnter = true;
        }

        protected override void OnExit(PointerEventData pointerEnterEvent)
        {
            base.OnExit(pointerEnterEvent);
            IsEnter = false;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            IsEnter = false;
        }

        protected override void OnDisable()
        {
            if(IsEnter)
            {
                SureMyArea.Invoke();
            }
        }
    }
}
