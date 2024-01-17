using System.Collections;
using System.Collections.Generic;
using AD.Utility;
using UnityEngine;

public class TemplateSceneManager : MonoBehaviour
{
    public ADSerializableDictionary<string, ADSerializableDictionary<GameObject, List<GameObject>>> SceneComponents = new();

    public void LoadAllKeyLayer()
    {

    }
}
