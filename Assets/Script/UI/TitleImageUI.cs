using System.Collections;
using UnityEngine;

public class TitleImageUI : BaseUI
{
    [SerializeField] private float time = 3f;
    [SerializeField] private AnimationCurve curve;

    RectTransform rect;
    Vector2 startPos;
    Vector2 endPos;

    protected override void Start()
    {
        base.Start();

        rect = GetComponent<RectTransform>();

        startPos = rect.anchoredPosition;
        endPos = startPos + new Vector2(0, -500f);

        StartCoroutine(UIAction());
    }

    IEnumerator UIAction()
    {
        float curTime = 0f;

        while (curTime < time)
        {
            float t = curTime / time;
            float eval = curve.Evaluate(t);

            rect.anchoredPosition = Vector2.Lerp(startPos, endPos, eval);

            curTime += Time.deltaTime;
            yield return null;
        }

        rect.anchoredPosition = endPos;
    }
}
