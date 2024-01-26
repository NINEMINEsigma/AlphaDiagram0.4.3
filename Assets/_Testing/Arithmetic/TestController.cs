using System.Collections;
using System.Collections.Generic;
using AD.UI;
using AD.Utility;
using UnityEngine;

public class TestController : MonoBehaviour
{
    public ModernUIInputField input, output;

    public AD.Utility.ArithmeticInfo CurrentResult;

    private void Start()
    {
        input.AddListener(InputParse);
    }

    public void InputParse(string str)
    {
        if (AD.Utility.ArithmeticExtension.TryParse(str, out AD.Utility.ArithmeticInfo result))
        {
            output.SetText(result.ToString());
        }
        else output.SetText(str + " is error");
        CurrentResult = result;
    }

}
