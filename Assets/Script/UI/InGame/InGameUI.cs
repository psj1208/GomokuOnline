using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InGameUI : BaseUI
{
    [SerializeField] private Button turnChangeButton;

    protected override void Start()
    {
        base.Start();
        turnChangeButton.onClick.AddListener(OnTurnButtonClicked);
    }

    #region 버튼 동작
    private void OnTurnButtonClicked()
    {
        TurnManager.Instance.NextTurn();
    }
    #endregion

    public void SetInteractable(bool value)
    {
        turnChangeButton.gameObject.SetActive(value);
    }
}
