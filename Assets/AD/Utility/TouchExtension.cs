using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AD.Utility
{
    public static class TouchExtension
    {
        public static Touch[] touches => Input.touches;

        public struct TouchData
        {
            public TouchData(Touch touch)
            {
                this.touch = touch;
                isNull = false;
            }

            private static TouchData NullOne;
            public static TouchData Null()
            {
                NullOne.isNull = true;
                return NullOne;
            }

            private bool isNull;
            public Touch touch;
            public Vector2 position => touch.position;
            public Vector2 delta => touch.deltaPosition;
            public TouchPhase phase => touch.phase;
            public AD.PressType press
            {
                get
                {
                    return phase switch
                    {
                        TouchPhase.Began => AD.PressType.ThisFramePressed,
                        TouchPhase.Moved or TouchPhase.Stationary => AD.PressType.JustPressed,
                        TouchPhase.Ended => AD.PressType.ThisFrameReleased,
                        _ => AD.PressType.None,
                    };
                }
            }

            public static implicit operator bool(TouchData data) => !data.isNull;
        }

        public static TouchData[] Build()
        {
            TouchData[] result = new TouchData[Input.touchCount];
            for (int i = 0, e = Input.touchCount; i < e; i++)
            {
                result[i] = new TouchData(Input.GetTouch(i));
            }
            return result;
        }
    }
}
