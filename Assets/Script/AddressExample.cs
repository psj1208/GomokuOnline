using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class AddressExample : MonoBehaviour
{
    Button but;
    AudioSource source;

    void Start()
    {
        but = GetComponent<Button>();
        source = GetComponent<AudioSource>();
        but.onClick.AddListener(()=>
        {
            AddressManager.Instance.LoadAssetAsync<AudioClip>("Assets/Audio/ButtonClick.wav",(clip)=>
            {
                source.clip = clip;
                source.Play();
            });
        });
    }
}
