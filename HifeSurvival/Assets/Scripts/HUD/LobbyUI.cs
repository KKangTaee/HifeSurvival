using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UniRx.Async;
using UnityEngine.Networking;
using System.IO;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] Button BTN_gameStart;
    [SerializeField] Image IMG_profile;
    [SerializeField] TMP_Text TMP_profileName;


    private void Awake()
    {
        GetComponent<Canvas>().worldCamera = Camera.main;
    }

    public void OnButtonEvent(Button inButton)
    {
        if (inButton == BTN_gameStart)
        {
           JoinGame();
        }
    }

    private async void JoinGame()
    {
        var delayTime = 300;

        SimpleLoading.Expose("네트워크 연결중입니다...");

        var isSuccess = await NetworkManager.Instance.ConnectAsync();
        await UniTask.Delay(delayTime);

        if(isSuccess == false)
        {
            Debug.LogError("네트워크 접속안됨");
            SimpleLoading.Hide();
            return;
        }

        SimpleLoading.ChangeDesc("잠시만 기다려주세요");
        
        isSuccess = await GameMode.Instance.JoinAsync();
        await UniTask.Delay(delayTime);

        if(isSuccess == false)
        {
            Debug.Log("룸에 접속된 유저의 정보를 가지고 오지 못함");
            NetworkManager.Instance.Disconnect();
            SimpleLoading.Hide();
            return;
        }

        SimpleLoading.Hide();

        Debug.Log($"룸 접속 완료 : {GameMode.Instance.RoomId}");

        PopupManager.Instance.Show<PopupSelectHeros>(
            inCreateCallback : popup =>
            {
                var currPlayerEntitys = GameMode.Instance.PlayerEntitysDic;

                foreach(var entity in currPlayerEntitys.Values)
                    popup.AddPlayerView(entity);

                popup.SetPlayerIdSelf(GameMode.Instance.EntitySelf.playerId);

                GameMode.Instance.AddEvent(
                    inRecvJoin  : entity =>  popup.OnRecvJoin(entity),
                    inRecvLeave : playerId => popup.OnLeave(playerId),
                    inRecvSelect : entity => popup.OnRecvSelectHero(entity),
                    inRecvReady : entity => popup.OnRecvReadyToGame(entity)
                );

                popup.AddEvent(
                    inOnSendSelectHero : (playerId, heroId) => GameMode.Instance.OnSendSelectHero(playerId,heroId),
                    inOnSendReadyToGame : ()=> GameMode.Instance.OnSendReadyToGame(),
                    inDisconnect : ()=> NetworkManager.Instance.Disconnect(GameMode.Instance.Leave)
                );

                NetworkManager.Instance.AddEvent(
                    inDisconnect : () => popup.Close( _ => {GameMode.Instance.Leave(); })      
                );
            });
    }


    

    public async UniTask SetProfile()
    {
        var userData = ServerData.Instance.UserData;

        if(userData == null)
            return;

        // 닉네임
        TMP_profileName.text = userData.nickname != null ? userData.nickname
                                                         : "일반인";


        // 프로필이미지.
        if(userData.photo_url != null)
        {
            Texture2D tex = LoadTexture2DFromLocal(userData.photo_url);

            if(tex == null)
            {
                tex = await LoadTexture2DFromWebAsync(userData.photo_url);

                if(tex != null)
                   SaveTexture2DToLocal(tex, $"{userData.photo_url}.png");
            }

            IMG_profile.sprite = Sprite.Create(tex, new Rect(0,0,tex.width, tex.height), new Vector2(0.5f,0.5f));
        }
        else
        {
            //TODO@taeho.kang 디폴트 프로필 이미지로 처리하기.
        }
    }

    public void SaveTexture2DToLocal(Texture2D texture, string inFileName)
    {
        byte[] pngData = texture.EncodeToPNG();

        string filePath = Path.Combine(Application.persistentDataPath, $"{inFileName}.png");

        File.WriteAllBytes(filePath, pngData);

        Debug.Log("Texture2D saved to: " + filePath);
    }

    public Texture2D LoadTexture2DFromLocal(string inFileName)
    {
        string filePath = Path.Combine(Application.persistentDataPath, $"{inFileName}.png");

        if (!File.Exists(filePath))
        {
            Debug.LogError("File not found: " + filePath);
            return null;
        }

        byte[] fileData = File.ReadAllBytes(filePath);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(fileData);

        return texture;
    }

    public async UniTask<Texture2D> LoadTexture2DFromWebAsync(string inFileName)
    {
        Texture2D tex = null;

        using (var webRequest = UnityWebRequestTexture.GetTexture(inFileName))
        {
            await webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[{nameof(SetProfile)}] {webRequest.error}");
            }
            else
            {
                tex = DownloadHandlerTexture.GetContent(webRequest);
            }
        }

        return tex;
    }

}
