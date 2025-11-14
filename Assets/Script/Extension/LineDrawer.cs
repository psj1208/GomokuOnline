using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineDrawer
{
    static List<GameObject> lines = new List<GameObject>();
    static MonoBehaviour coroutineRunner;

    // 내부용 코루틴 실행 헬퍼
    private static MonoBehaviour GetRunner()
    {
        if (coroutineRunner == null)
        {
            GameObject runnerObj = new GameObject("LineDrawer_Runner");
            UnityEngine.Object.DontDestroyOnLoad(runnerObj);
            coroutineRunner = runnerObj.AddComponent<MonoBehaviourRunner>();
        }
        return coroutineRunner;
    }
    public static void SetLine(Vector3 start, Vector3 end, Transform parent = null, Color color = default, float width = 0.02f)
    {
        //새로운 게임 오브젝트를 생성해 라인 렌더러를 붙이고 해당 속성을 구성.
        GameObject obj = new GameObject("Line" + lines.Count.ToString());
        lines.Add(obj);
        LineRenderer lr = obj.AddComponent<LineRenderer>();

        Material mat = new Material(Shader.Find("Sprites/Default"));
        lr.material = mat;

        lr.startColor = color == default ? Color.black : color;
        lr.endColor = color == default ? Color.black : color;
        lr.startWidth = width;
        lr.endWidth = width;
        lr.useWorldSpace = true;
        lr.positionCount = 2;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);

        if (parent != null)
            obj.transform.SetParent(parent.transform);
    }

    public static void SetAnimatedLine(Vector3 start, Vector3 end, float duration, Action action = null, Transform parent = null, Color color = default, float width = 0.02f)
    {
        MonoBehaviour runner = GetRunner();
        runner.StartCoroutine(DrawLineOverTimeCourt(start, end, duration, action, parent, color, width));
    }

    private static IEnumerator DrawLineOverTimeCourt(Vector3 start,Vector3 end, float duration, Action action = null, Transform parent = null, Color color = default, float width = 0.02f)
    {
        GameObject obj = new GameObject("AnimatedLine" + lines.Count.ToString());
        lines.Add(obj);
        LineRenderer lr = obj.AddComponent<LineRenderer>();

        Material mat = new Material(Shader.Find("Sprites/Default"));
        lr.material = mat;

        lr.startColor = color == default ? Color.black : color;
        lr.endColor = color == default ? Color.black : color;
        lr.startWidth = width;
        lr.endWidth = width;
        lr.useWorldSpace = true;
        lr.positionCount = 2;
        lr.sortingOrder = 10;
        lr.SetPosition(0, start);
        lr.SetPosition(1, start);

        if (parent != null)
            obj.transform.SetParent(parent.transform);

        float curTime = 0f;
        while (curTime < duration)
        {
            curTime += Time.deltaTime;
            float t = Mathf.Clamp01(curTime / duration);
            Vector3 currentPos = Vector3.Lerp(start, end, t);
            lr.SetPosition(1, currentPos);
            yield return null;
        }

        lr.SetPosition(1, end);

        yield return new WaitForSeconds(1.0f);

        action.Invoke();
    }

    private class MonoBehaviourRunner : MonoBehaviour { }
}
