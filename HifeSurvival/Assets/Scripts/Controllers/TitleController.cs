using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using UniRx.Async;
using UnityEngine.UI;



public class TitleController : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] Button BTN_Google;


    void Start()
    {
        Init().Forget();
    }

    public async UniTaskVoid Init()
    {
         // 1. 서버 스태틱 데이터 로드
        ServerRequestManager.Instance.Test();

        // 2. 로그인 초기화
        await FirebaseAuthManager.Instance.Init(isSuccess=>
        {
            if(isSuccess)
            {
                GoToLobby();
            }
        });
    }


    async void GoToLobby()
    {
        await Task.Delay(3000);

        await SceneManager.Instance.ChangeScene(SceneManager.SCENE_NAME_LOBBY);
    }


    public void OnButtonEvent(Button inButton)
    {
        if(inButton == BTN_Google)
        {
            FirebaseAuthManager.Instance.SignInWithGoogle();
        }
    }
}
