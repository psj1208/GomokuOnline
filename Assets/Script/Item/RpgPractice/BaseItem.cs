using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Item/BaseItem")]
public class BaseItem : ScriptableObject
{
    [Header("기본 정보")]
    [SerializeField] private int itemId;
    [SerializeField] private Sprite itemImage;
    [SerializeField] private string itemName;
    [TextArea][SerializeField] private string itemDescription;

    [Header("스택 관련")]
    [SerializeField] private bool canStack;
    [SerializeField] private int maxStack = 1;
    public int ItemId => itemId;
    public Sprite ItemImage => itemImage;
    public string ItemName => itemName;
    public string ItemDescription => itemDescription;
    public bool CanStack => canStack;
    public int MaxStack => maxStack;
}
