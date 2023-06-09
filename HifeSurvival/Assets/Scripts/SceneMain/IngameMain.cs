using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx.Async;


public class IngameMain : MonoBehaviour
{
    [SerializeField] WorldMap   _worldMap;
    [SerializeField] IngameUI   _ingameUI;

    private void Awake()
    {
        Init().Forget();       
    }

    public async UniTask Init()
    {
        // 컨트롤러
        await ControllerManager.Instance.InitAsync();

        // 월드맵
        _worldMap.Init();

        // UI
        _ingameUI.Init();

        // TODO@taeho.kang 임시
        GameMode.Instance.OnSendPlayStart();
    }
}
