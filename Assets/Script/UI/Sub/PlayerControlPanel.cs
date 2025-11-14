using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerControlPanel : BaseUI
{
    [SerializeField] private Button kickButton;
    private Player targetPlayer;
    private RectTransform panelRect;

    private void Awake()
    {
        panelRect = GetComponent<RectTransform>();
    }

    protected override void Start()
    {
        base.Start();
        kickButton.onClick.AddListener(KickAction);
    }

    private void Update()
    {
        CheckOuterClickAndDestroy();
    }

    public void SetUpAndShow(Player player, Vector2 position)
    {
        targetPlayer = player;
        gameObject.SetActive(true);

        transform.position = position;
    }

    private void KickAction()
    {
        TeamManager.Instance.KickPlayer(targetPlayer);
        Destroy(gameObject);
    }
}