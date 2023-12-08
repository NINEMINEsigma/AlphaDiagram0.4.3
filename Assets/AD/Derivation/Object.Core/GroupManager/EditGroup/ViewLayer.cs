using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AD.Utility.Object
{
    public class ViewLayer : MonoBehaviour
    {
        public Canvas UILayerCanvas;
        public GameObject GameObjectLayer;

        public ADSerializableDictionary<string, GameObject> UIs = new();
        public ADSerializableDictionary<string, GameObject> GameObjects = new();
    }
}
