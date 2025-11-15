using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LoadingUI : BaseUI
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] float timePerAnimation = 0.5f;
    string originalText;
    float curTime = 0;
    int count = 0;


    public void Init(string t)
    {
        text.text = t;
        originalText = t;
    }

    private void Update()
    {
        curTime += Time.deltaTime;

        if (curTime >= timePerAnimation)
        {
            curTime = 0;
            if (count >= 3)
            {
                text.text = originalText;
                count = 0;
            }
            else
            {
                text.text += ".";
                count++;
            }
        }
    }
}
