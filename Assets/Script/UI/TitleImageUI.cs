using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleImageUI : BaseUI
{
    [SerializeField] private float time = 3f;
    [SerializeField] private AnimationCurve curve;
    Vector3 startPos;
    Vector3 endPos;

    protected override void Start()
    {
        base.Start();

        startPos = transform.position;
        endPos = startPos + new Vector3(0, -500f, 0);
        StartCoroutine(UIAction());
    }

    IEnumerator UIAction()
    {
        float curTime = 0f;
        while (curTime < time)
        {
            float t = curTime / time;

            float eval = curve.Evaluate(t);

            transform.position = Vector3.Lerp(startPos, endPos, eval);

            curTime += Time.deltaTime;
            yield return null;
        }

        transform.position = endPos;
    }
}
