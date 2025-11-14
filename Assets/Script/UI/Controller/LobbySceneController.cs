using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbySceneController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var _ = SceneLoadManager.Instance;
        UIManager.Instance.show<AudioControlUI>();
        AudioManager.Instance.AudioBGMPlay("LobbyBackGround", true, 1, EAudioType.BGM);
    }
}
