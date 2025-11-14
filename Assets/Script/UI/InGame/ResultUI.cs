using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ResultUI : AnimationUI
{
    [SerializeField] private RectMask2D rectMask2D;
    [SerializeField] private TextMeshProUGUI teamText;
    [SerializeField] private TextMeshProUGUI userNameText;
    [SerializeField] private float startDuration = 1.5f;
    [SerializeField] private float animationDuration = 1.5f;
    [SerializeField] private float endDuration = 1.5f;
    [SerializeField] private Button exitButton;
    [Space]
    [SerializeField] private Color blackColor;
    [SerializeField] private Color whiteColor;
    CanvasGroup group;
    Vector4 padding;
    float startPaddingRight;

    protected override void Start()
    {
        base.Start();
        group = GetComponent<CanvasGroup>();
        padding = rectMask2D.padding;
        startPaddingRight = GetComponent<RectTransform>().rect.width;
        padding.z = startPaddingRight;
        rectMask2D.padding = padding;
        group.alpha = 0f;
        exitButton.onClick.AddListener(EndAnmation);
        exitButton.gameObject.SetActive(false);
        StartAnmation();
    }

    public void Init(TeamType team, string userName)
    {
        teamText.color = team == TeamType.Black ? blackColor : whiteColor;
        teamText.text = team.ToString();
        userNameText.text = userName;
    }

    protected override IEnumerator StartAnimationCourt()
    {
        AudioManager.Instance.Audio2DPlay("GameEndSound",1, false, EAudioType.SFX);
        float elapsed = 0f;
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float ratio = Mathf.Clamp01(elapsed / animationDuration);

            group.alpha = ratio;

            yield return null;
        }
        group.alpha = 1f;

        elapsed = 0f;
        float endPaddingRight = 0f;

        while(elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float ratio = Mathf.Clamp01(elapsed / animationDuration);

            padding.z = Mathf.Lerp(startPaddingRight, endPaddingRight, ratio);
            rectMask2D.padding = padding;

            yield return null;
        }
        padding.z = endPaddingRight;
        rectMask2D.padding = padding;
        exitButton.gameObject.SetActive(true);
    }

    protected override IEnumerator EndAnimationCourt()
    {
        float elapsed = 0f;
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float ratio = Mathf.Clamp01(elapsed / animationDuration);

            group.alpha = 1 - ratio;

            yield return null;
        }
        group.alpha = 0;
        Destroy(gameObject);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        SceneLoadManager.Instance.PrivateSceneMove("PSJLobbyScene");
    }
}
