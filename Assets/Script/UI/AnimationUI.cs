using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AnimationUI : BaseUI
{
    private Coroutine curCourtine;

    protected virtual void StartAnmation() { StartCoroutine(StartAnimationCourt()); }
    protected virtual void EndAnmation() {
        if (curCourtine == null)
            curCourtine = StartCoroutine(EndAnimationCourt()); 
    }
    protected virtual IEnumerator StartAnimationCourt() { yield return null; }
    protected virtual IEnumerator EndAnimationCourt() { yield return null; }
}
