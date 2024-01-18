using AD.UI;
using UnityEngine;

public class AIAnswer : MonoBehaviour
{
    public ModernUIInputField InputField;

    public string answer;

    public void AddPage(int value)
    {
        InputField.Source.source.textComponent.pageToDisplay = (int)Mathf.Clamp(InputField.Source.source.textComponent.pageToDisplay + value, 0, Mathf.Infinity);
    }

    public void SetAnswerView(string answer)
    {
        InputField.SetText(answer);
        this.answer = answer;
        InputField.Source.source.textComponent.pageToDisplay = 0;
    }
}
