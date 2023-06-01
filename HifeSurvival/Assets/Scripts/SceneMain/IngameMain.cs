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

        _ingameUI.Init();

        // 플레이어 로드
        ControllerManager.Instance.GetController<PlayerController>()?.LoadPlayer(_worldMap);
    }
}
