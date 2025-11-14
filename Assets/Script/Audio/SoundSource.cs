using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class SoundSource : MonoBehaviour
{
    ObjectPool<SoundSource> pool;

    public void Init(ObjectPool<SoundSource> pool)
    {
        this.pool = pool;
    }

    public void Play(float lifeTime)
    {
        CancelInvoke();
        Invoke(nameof(ReturnToPool), lifeTime);
    }

    public void ReturnToPool()
    {
        if (pool != null)
            pool.Release(this);
    }
}
