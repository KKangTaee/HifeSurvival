using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx.Async;

public class IngameMain : MonoBehaviour
{
    [SerializeField] WorldMap _worldMap;

    private void Awake()
    {
        Init().Forget();       
    }

    public async UniTask Init()
    {
        // 월드맵
        _worldMap.Init();

        // 컨트롤러
        await ControllerManager.Instance.InitAsync();

        // 플레이어 로드
        ControllerManager.Instance.GetController<PlayerController>().LoadPlayer(_worldMap);

        // 몬스터 로드
    }
}
