using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TeamInfo : MonoBehaviour
{
    [Header("About UserInfo")]
    [SerializeField] Transform parent;
    public Transform Parent { get { return parent; } }
    [Space]
    [Header("About Ready")]
    [SerializeField] bool isReady;
    public bool IsReady {  get { return isReady; } }
    [SerializeField] TextMeshProUGUI readyText;

    private void Start()
    {
        isReady = false;
        readyText.gameObject.SetActive(IsReady);
    }

    public void ReadyAct(bool value)
    {
        isReady = value;
        readyText.gameObject.SetActive(IsReady);
    }
}
