using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.UI;
using DG.Tweening;
public class AudioControlUI : BaseUI
{
    [SerializeField] private Button button;
    [SerializeField] private GameObject panel;
    bool isOpened;
    [SerializeField] private Slider totalVolume;
    [SerializeField] private Slider sfxVolume;
    [SerializeField] private Slider bgmVolume;
    [SerializeField] private float openDuration = 0.5f;
    [SerializeField] private float closeDuration = 0.5f;
    private Tween curTween;
    private RectTransform rect;
    private float startValue;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        rect = panel.GetComponent<RectTransform>();
        startValue = rect.offsetMax.y;

        AudioManager.Instance.LoadAudioSettingNotPrefs(EAudioType.Master, totalVolume);
        AudioManager.Instance.LoadAudioSettingNotPrefs(EAudioType.SFX, sfxVolume);
        AudioManager.Instance.LoadAudioSettingNotPrefs(EAudioType.BGM, bgmVolume);
        button.onClick.AddListener(TogglePanel);
        totalVolume.onValueChanged.AddListener(value => AudioManager.Instance.SetLevel(value, EAudioType.Master));
        sfxVolume.onValueChanged.AddListener(value => AudioManager.Instance.SetLevel(value, EAudioType.SFX));
        bgmVolume.onValueChanged.AddListener(value => AudioManager.Instance.SetLevel(value, EAudioType.BGM));
    }

    void TogglePanel()
    {
        curTween?.Kill();

        Vector2 to = isOpened ? new Vector2(rect.offsetMax.x, startValue) : new Vector2(rect.offsetMax.x, 0f);
        float duration = isOpened ? closeDuration : openDuration;

        curTween = DOTween.To(() => rect.offsetMax, x => rect.offsetMax = x, to, duration)
                         .SetEase(Ease.OutCubic);

        isOpened = !isOpened;
    }
}
