using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using UniRx.Async;
using UnityEngine.UI;
using System;


public class TitleController : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] Button BTN_Google;
    [SerializeField] Image IMG_progress;

    List<(string desc, Func<Task<bool>> action)> processSchedulers;


    void Awake()
    {
        processSchedulers = new List<(string desc, Func<Task<bool>> action)>()
        {
             // 스태틱 로드
            ("스태틱 데이터를 불러옵니다", PROC_LOAD_STATIC_DATA),

            // 로그인
            ("로그인을 시도합니다", PROC_LOGIN), 

            // 서버
            ("서버DB에 접근합니다.", PROC_LOAD_SERVER_DATA),

            // 타이틀씬 완료
            ("전장으로 이동합니다", PROC_TITLE_COMPLETE),
        };
    }

    void Start()
    {
        Init().Forget();
    }


    public async UniTaskVoid Init()
    {
        for(int i =0; i<processSchedulers?.Count; i++)
        {
            var process = processSchedulers[i];

            // 여기서 로딩바 액션 처리
            SetProgress(i /(float)(processSchedulers.Count - 1));
            if(await process.action?.Invoke())
            {
                // 성공처리
            }
            else
            {
                break;
            }
        }
    }


    private async void GoToLobby()
    {
        await Task.Delay(3000);

        await SceneManager.Instance.ChangeScene(SceneManager.SCENE_NAME_LOBBY);
    }


    public void OnButtonEvent(Button inButton)
    {
        if (inButton == BTN_Google)
        {
            FirebaseAuthManager.Instance.SignInWithGoogle();
        }
    }

    public void SetProgress(float inRatio)
    {
        IMG_progress.fillAmount = inRatio;
    }



    private async Task<bool> PROC_LOAD_STATIC_DATA()
    {
        // 1. 서버 스태틱 데이터 로드(테스트)
        ServerRequestManager.Instance.Test();
        return true;
    }

    public async Task<bool> PROC_LOGIN()
    {
        // 2. 로그인 초기화
        await FirebaseAuthManager.Instance.Init();
        return true;    
    }

    public async Task<bool> PROC_LOAD_SERVER_DATA()
    {
        await FirestoreManager.Instance.Init();
        return true;
    }

    public async Task<bool> PROC_TITLE_COMPLETE()
    {
        // 로비씬으로 이동
        GoToLobby();
        return true;
    }
}
