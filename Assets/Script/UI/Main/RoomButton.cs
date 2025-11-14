using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomButton : MonoBehaviour
{
    [SerializeField] private PSJMainPanel mainPanel;
    [SerializeField] private Image BackGround;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI numberText;
    [SerializeField] private Button button;
    [Space]
    [Header("About Color")]
    [SerializeField] private Color defaultColor = Color.white;
    [SerializeField] private Color selectedColor = Color.green;

    [SerializeField] private string roomName;
    public string RoomName { get { return roomName; } }
    
    public void SetUp(RoomInfo info, PSJMainPanel panel)
    {
        mainPanel = panel;
        roomName = info.Name;
        titleText.text = roomName;
        numberText.text = $"{info.PlayerCount} / {info.MaxPlayers}";
        button.onClick.AddListener(Select);
    }

    public void Select()
    {
        mainPanel.RoomSelect(this);
    }

    public void SetSelected(bool isSelected)
    {
        BackGround.color = isSelected ? selectedColor : defaultColor;
    }
}
