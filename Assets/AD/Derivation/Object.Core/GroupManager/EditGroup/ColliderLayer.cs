using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AD.Utility.Object
{
    public class ColliderLayer : MonoBehaviour
    {
        public EditGroup ParentGroup;

        public Collider BaseCollider;
        public ADSerializableDictionary<string, Collider> OverlayerCollider = new();
    }
}
