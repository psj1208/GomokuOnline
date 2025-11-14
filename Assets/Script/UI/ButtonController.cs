using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Address")]
    [SerializeField] bool EnterSoundKeyServer = false;
    [SerializeField] private string EnterSoundKey;
    [SerializeField] bool ClickSoundKeyServer = false;
    [SerializeField] private string ClickSoundKey;
    [SerializeField] bool ExitSoundKeyServer = false;
    [SerializeField] private string ExitSoundKey;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!EnterSoundKey.Equals(""))
        {
            if (EnterSoundKeyServer)
                AudioManager.Instance.PlayNetwork2DSound(EnterSoundKey, 1, false, EAudioType.SFX);
            else
                AudioManager.Instance.Audio2DPlay(EnterSoundKey, 1, false, EAudioType.SFX);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!ClickSoundKey.Equals(""))
        {
            if (ClickSoundKeyServer)
                AudioManager.Instance.PlayNetwork2DSound(ClickSoundKey, 1, false, EAudioType.SFX);
            else
                AudioManager.Instance.Audio2DPlay(ClickSoundKey, 1, false, EAudioType.SFX);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!ExitSoundKey.Equals(""))
        {
            if (ExitSoundKeyServer)
                AudioManager.Instance.PlayNetwork2DSound(ExitSoundKey, 1, false, EAudioType.SFX);
            else
                AudioManager.Instance.Audio2DPlay(ExitSoundKey, 1, false, EAudioType.SFX);
        }
    }
}
