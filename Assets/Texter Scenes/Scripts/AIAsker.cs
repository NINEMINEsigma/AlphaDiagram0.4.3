using System.Collections;
using System.Collections.Generic;
using AD.Experimental.LLM;
using AD.UI;
using UnityEngine;

public class AIAsker : MonoBehaviour
{
    public LLM TargetLLM;
    public ModernUIInputField InputField;
    public AIAnswer Answer;

    public InputField DespWordInputField;

    public ModernUIFillBar Bar;
    private float BarValue;
    public AnimationCurve BarValueCurve = AnimationCurve.Linear(0, 0, 1, 1);

    private void Start()
    {
        DespWord = DespWordInputField.text = TargetLLM.Prompt;
        DespWordInputField.AddListener(RegistDespWord);
    }

    public void PostMessage()
    {
        BarValue = 1;
        Bar.SetPerecent(0);
        Bar.Set(0, (TargetLLM.m_DataList.Count + 1) * 10);
        TargetLLM.PostMessage(InputField.text, P =>
        {
            string result = P.Trim().Trim('"', 'бо').Replace("\\n", "\n")[1..^1];
            Answer.SetAnswerView(P);
            Bar.SetPerecent(1);
            BarValue = 1;
        });
        Bar.SetPerecent(0.3f);
        BarValue = 0.3f;
        StartCoroutine(SetBar());
    }

    private IEnumerator SetBar()
    {
        float realP = 0;
        while (realP < 0.9f)
        {
            BarValue += Time.deltaTime * 0.02f;
            realP = (BarValue - 0.3f) / 0.7f;
            Bar.SetPerecent(BarValueCurve.Evaluate(realP));
            yield return null;
        }
    }

    public string DespWord;

    public void ClearHistoryAndResetupDespWord()
    {
        TargetLLM.Init();
        TargetLLM.Prompt = DespWord;
    }

    public void RegistDespWord(string DespWord)
    {
        this.DespWord = DespWord;
    }
}
