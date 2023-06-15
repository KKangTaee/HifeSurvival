using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using DG.Tweening;
using TMPro;
using UniRx;
using System;


public class IngameUI : MonoBehaviour
{
    [SerializeField] RectTransform RT_kdList;

    [Header("[Top]")]
    [SerializeField] Button BTN_addStr;
    [SerializeField] Button BTN_addDef;
    [SerializeField] Button BTN_addHp;
    [SerializeField] TMP_Text TMP_str;
    [SerializeField] TMP_Text TMP_def;
    [SerializeField] TMP_Text TMP_hp;
    [SerializeField] TMP_Text TMP_gold;


    [SerializeField] RectTransform  RT_respawnPanel;
    [SerializeField] TMP_Text       TMP_respawnCount;
    [SerializeField] KDView []      _kdViewArr;
    [SerializeField] ItemSlotList   _itemSlotList;

    IDisposable _respawnTimer;


    public void Init()
    {
        GetComponent<Canvas>().worldCamera = ControllerManager.Instance.GetController<CameraController>().MainCamera;

        GameMode.Instance.OnRecvDeadHandler    += OnRecvDead;
        GameMode.Instance.OnRecvRespawnHandler += OnRecvRespawn;
        GameMode.Instance.OnRecvGetItemHandler += OnRecvGetItem;

        SetKDView();
    }


    public void SetStatUI(EntityStat inStat, int inGold)
    {
        TMP_str.text  = inStat.str.ToString();

        TMP_def.text  = inStat.def.ToString();

        TMP_hp.text   = inStat.hp.ToString();

        TMP_gold.text = inGold.ToString();
    }


    public void SetKDView()
    {
        var playerEntitys = GameMode.Instance.PlayerEntitysDict.Values;

        var iter = _kdViewArr.GetEnumerator();

        foreach (var entity in playerEntitys)
        {
            if (iter.MoveNext())
            {
                KDView view = iter.Current as KDView;
                view?.SetInfo(entity.targetId, 0, 0);

                // 나 자신이라면..?
                if (entity.userId == ServerData.Instance.UserData.user_id)
                    SetStatUI(entity.stat, entity.gold);
            }
        }
    }


    public void OnRecvDead(S_Dead inPacket)
    {
        if(inPacket.toIsPlayer == true)
        {
            var dead = _kdViewArr.FirstOrDefault(x => x.targetId == inPacket.toId);
            dead.AddDead(1);

            // 내가 사망했다면...? 리스폰 화면 띄워라
            if(inPacket.toId == GameMode.Instance.EntitySelf.targetId)
                ShowRespawnTimer(inPacket.respawnTime);
        }

        if(inPacket.fromIsPlayer == true)
        {
            var kill = _kdViewArr.FirstOrDefault(x => x.targetId == inPacket.fromId);
            kill.AddKill(1);
        }   
    }


    public void ShowRespawnTimer(int totalTime)
    {
        RT_respawnPanel?.gameObject.SetActive(true);
        RT_kdList?.gameObject.SetActive(false);

        int elapsedSeconds = totalTime;

        _respawnTimer = Observable.Interval(TimeSpan.FromSeconds(1))
            .Subscribe(_ =>
            {
                elapsedSeconds--;

                TMP_respawnCount.text = $"부활까지 {elapsedSeconds}초 남았습니다";

                // 알파값을 0.5에서 1로 바꾸는 동안 0.5초의 애니메이션을 수행합니다.
                TMP_respawnCount.DOFade(1f, 0.5f);
                
                // 알파값을 1에서 0.5로 바꾸는 동안 0.5초의 애니메이션을 수행합니다.
                TMP_respawnCount.DOFade(0.5f, 0.5f).SetDelay(0.5f);

                if (elapsedSeconds >= totalTime)
                    _respawnTimer?.Dispose(); // 타이머를 정지합니다.

            }).AddTo(this);
    }


    public void HideRespawnTimer()
    {
        _respawnTimer?.Dispose();
        RT_respawnPanel?.gameObject.SetActive(false);
        RT_kdList?.gameObject.SetActive(true);
    }


    //---------------
    // Server
    //---------------

    public void OnRecvRespawn(Entity inEntity)
    {
        HideRespawnTimer();
    }

    public void OnRecvGetItem(PlayerEntity inEntity)
    {
        
    }

    public void OnRecvGetGold(S_GetGold inPacket)
    {

    }
}
