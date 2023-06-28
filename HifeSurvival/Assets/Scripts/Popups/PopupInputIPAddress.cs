using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using UniRx.Async;
using TMPro;

[Popup(PATH_IN_RESOURCES_FOLDER = "Prefabs/Popups/PopupInputIPAddress/PopupInputIPAddress",
       IN_RESOURCES_FORLDER = true)]
public class PopupInputIPAddress : PopupBase
{
    [Header("[PopupInputIPAddress]")]
    [SerializeField] Button BTN_connect;
    [SerializeField] Button BTN_close;
    [SerializeField] TMP_InputField IF_inputAddress;


    //------------------
    // unity events
    //------------------

    protected override void Awake()
    {
        base.Awake();

        IF_inputAddress.onValidateInput += ValidateInput;
        IF_inputAddress.text = PlayerPrefs.GetString("ipAddr");
    }


    //------------------
    // override
    //------------------

    protected override void OnButtonEvent(Button inButton)
    {
        if(inButton == BTN_connect)
        {
            ConnectServer();
        }
        else if(inButton == BTN_close)
        {
            Close();
        }
    }


    //------------------
    // functions
    //------------------
    private char ValidateInput(string text, int charIndex, char addedChar)
    {
        string fullText = text + addedChar;

        // 숫자 및 '.'만 허용하는 정규식
        if (Regex.IsMatch(fullText, @"^[0-9.]*$"))
        {
            return addedChar;
        }
        else
        {
            // 유효하지 않은 문자는 무시
            return '\0';
        }
    }

    private bool IsValidIpAddr(string inIpAddr)
    {
        if(string.IsNullOrEmpty(inIpAddr) == true)
            return false;

        // IP 주소 형식의 정규 표현식
        string pattern = @"^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$";

        // 정규 표현식을 사용하여 IP 주소 형식 확인
        return Regex.IsMatch(inIpAddr, pattern);
    }


    public void ConnectServer()
    {
        var ipAddr = IF_inputAddress.text;

        if(IsValidIpAddr(ipAddr) == false)
        {
            PopupManager.Instance.Show<PopupNotice>(popup=>popup.SetDesc("IP주소가 올바르지 않습니다.\n주소를 다시 확인해주세요"));
            return;
        }

        PlayerPrefs.SetString("ipAddr", ipAddr);

        Close(_=> 
        {
            JoinGame(ipAddr).Forget();
        });
    }

    public async UniTaskVoid JoinGame(string inIpAddr)
    {
        var delayTime = 300;

        SimpleLoading.Show("네트워크 연결중입니다...");

        var isSuccess = await NetworkManager.Instance.ConnectAsync(inIpAddr);
        await UniTask.Delay(delayTime);

        if(isSuccess == false)
        {
            PopupManager.Instance.Show<PopupNotice>(popup=>popup.SetDesc("네트워크에 연결 할 수 없습니다./n네트워크를 확인해주세요"));
            SimpleLoading.Hide();
            return;
        }

        SimpleLoading.ChangeDesc("잠시만 기다려주세요");
        
        isSuccess = await GameMode.Instance.JoinAsync();
        await UniTask.Delay(delayTime);

        if(isSuccess == false)
        {
            PopupManager.Instance.Show<PopupNotice>(popup=>popup.SetDesc("체널에 대한 정보를 가지고 올 수 없습니다"));
            NetworkManager.Instance.Disconnect();
            SimpleLoading.Hide();
            return;
        }

        SimpleLoading.Hide();

        Debug.Log($"룸 접속 완료 : {GameMode.Instance.RoomId}");

        PopupManager.Instance.Show<PopupSelectHeros>(
            inCreateCallback : popup =>
            {
                var currPlayerEntitys = GameMode.Instance.PlayerEntitysDict;

                foreach(var entity in currPlayerEntitys.Values)
                    popup.AddPlayerView(entity);

                popup.SetPlayerIdSelf(GameMode.Instance.EntitySelf.id);

                GameMode.Instance.AddEvent(
                    inRecvJoin  : entity =>  popup.OnRecvJoin(entity),
                    inRecvLeave : playerId => popup.OnLeave(playerId),
                    inRecvSelect : entity => popup.OnRecvSelectHero(entity),
                    inRecvReady : entity => popup.OnRecvReadyToGame(entity),
                    inRecvCountdown : sec => popup.OnRecvCountdown(sec),
                    inRecvGameStart: ()=> popup.OnRecvStartGame()
                );

                popup.AddEvent(
                    inOnSendSelectHero : (heroId) => GameMode.Instance.OnSendSelectHero(heroId),
                    inOnSendReadyToGame : ()=> GameMode.Instance.OnSendReadyToGame(),
                    inDisconnect : ()=> NetworkManager.Instance.Disconnect(GameMode.Instance.Leave)
                );

                NetworkManager.Instance.AddEvent(
                    inDisconnect : () => popup.Close( _ => {GameMode.Instance.Leave(); })      
                );
            });
    }
}
