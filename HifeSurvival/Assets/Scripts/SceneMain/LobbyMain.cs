using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx.Async;

public class LobbyMain : MonoBehaviour
{
    [SerializeField] LobbyUI _lobbyUI;

    // Start is called before the first frame update
    void Start()
    {
        // _= NetworkManager.Instance.ConnectAsync();
        // –NetworkManager.Instance.Connect();
        InitalizeAsync().Forget();
    }
    

    public async UniTask InitalizeAsync()
    {
        // TopHUD.Instance.Show();

        // 개인 프로필 업데이트
        await _lobbyUI.SetProfile();
    }
}
