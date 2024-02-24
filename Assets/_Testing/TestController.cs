using UnityEngine;
using AD.Utility;
using AD;

namespace TTT
{

    public class TestController : MonoBehaviour
    {
        public Vector3 vec;

        private void Start()
        {
            var finger = ADGlobalSystem.RegisterFinger();
            finger.OnTouch.AddListener(T =>
            {
                vec = T.position;
            });
        }
    }
}
