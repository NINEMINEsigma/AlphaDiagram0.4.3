using System.Collections.Generic;
using AD.BASE;
using UnityEngine;

namespace AD.Experimental.Performance
{
    public class ConeAllegationItem : MonoBehaviour
    {
        public ConeAllegation Allegation;
        public virtual List<Vector3> Pointers { get => new() { transform.position }; }

        public bool IsOnCone = true;
        private bool LateIsOnCone = true;

        public ADEvent OnEnCone = new(), OnQuCone = new();

        private void LateUpdate()
        {
            if (IsOnCone != LateIsOnCone)
            {
                if (IsOnCone) OnEnCone.Invoke();
                else OnQuCone.Invoke();
                LateIsOnCone = IsOnCone;
            }
        }

    }
}
