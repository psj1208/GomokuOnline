using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class NameTag : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private TextMeshProUGUI nameText;

    public void SetName(string name)
    {
        nameText.text = name;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            Player player = TeamManager.Instance.GetPlayerByNickName(nameText.text);
            if (player.NickName != PhotonNetwork.LocalPlayer.NickName)
            {
                UIManager.Instance.show<PlayerControlPanel>((panel) =>
                {
                    panel.SetUpAndShow(player, transform.position);
                });
            }
        }
    }
}
