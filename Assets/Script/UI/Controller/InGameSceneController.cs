using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameSceneController : MonoBehaviour
{
    void Start()
    {
        UIManager.Instance.show<AudioControlUI>();
    }
}
