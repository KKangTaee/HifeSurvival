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
    // [SerializeField] Image IMG_profile;
    [SerializeField] TMP_Text TMP_userName;
    [SerializeField] TMP_Text TMP_chapterName;
    [SerializeField] TMP_Text TMP_chapterNum;


    private void Awake()
    {
        GetComponent<Canvas>().worldCamera = Camera.main;

        HeroRTCapture.GetInstance().GetCaptureTexture();
        HeroRTCapture.GetInstance().GetAnimator().SetAnimatorController(0);
    }


    public void OnButtonEvent(Button inButton)
    {
        if (inButton == BTN_gameStart)
        {
            PopupManager.Instance.Show<PopupInputIPAddress>();
        }
    }

    public async UniTask Init()
    {
        await SetProfile();

        SetChapter();
    }
    

    public async UniTask SetProfile()
    {
        var userData = ServerData.Instance.UserData;

        if(userData == null)
            return;

        // 닉네임
        TMP_userName.text = userData.nickname != null ? userData.nickname
                                                         : "일반인";

        // 프로필이미지.
        // if(userData.photo_url != null)
        // {
        //     Texture2D tex = LoadTexture2DFromLocal(userData.photo_url);

        //     if(tex == null)
        //     {
        //         tex = await LoadTexture2DFromWebAsync(userData.photo_url);

        //         if(tex != null)
        //            SaveTexture2DToLocal(tex, $"{userData.photo_url}.png");
        //     }

        //     IMG_profile.sprite = Sprite.Create(tex, new Rect(0,0,tex.width, tex.height), new Vector2(0.5f,0.5f));
        // }
        // else
        // {
        //     //TODO@taeho.kang 디폴트 프로필 이미지로 처리하기.
        // }
    }

    public void SetChapter()
    {
        var staticData = StaticData.Instance.ChapaterDataDict["1"];

        TMP_chapterName.text = staticData.name;
        TMP_chapterNum.text  = $"Chapter {1}";
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
