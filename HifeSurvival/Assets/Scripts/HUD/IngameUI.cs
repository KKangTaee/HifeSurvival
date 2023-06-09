using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using DG.Tweening;
using TMPro;
using UniRx;
using System;
using UniRx.Triggers;

public class IngameUI : MonoBehaviour, IUpdateInvenItemSingle, IUpdatePlayerCurrencySingle, IUpdateStatBroadcast
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


    [SerializeField] RectTransform RT_respawnPanel;
    [SerializeField] TMP_Text TMP_respawnCount;
    [SerializeField] KDView[] _kdViewArr;
    [SerializeField] ItemSlotList _itemSlotList;

    public const int BUTTON_CLICK_INTERVAL_MILLISECONDS = 250;

    private IDisposable _respawnTimer;

    private EStatType _onClickButtonType = EStatType.NONE;
    private long _onClickButtonTime;

    public void Init()
    {
        GetComponent<Canvas>().worldCamera = ControllerManager.Instance.GetController<CameraController>().MainCamera;

        var eventHandler = GameMode.Instance.GetEventHandler<IngamePacketEventHandler>();

        eventHandler.RegisterClient<S_Dead>(OnRecvDead);
        eventHandler.RegisterClient<PickRewardResponse>(OnRecvPickReward);
        eventHandler.RegisterClient<UpdateInvenItem>(OnUpdateInvenItemSingle);
        eventHandler.RegisterClient<UpdatePlayerCurrency>(OnUpdatePlayerCurrencySingle);
        eventHandler.RegisterClient<S_Respawn>(OnRecvRespawn);
        eventHandler.RegisterClient<IncreaseStatResponse>(OnRecvIncreaseStat);
        eventHandler.RegisterClient<UpdateStatBroadcast>(OnUpdateStatBroadcast);

        SetKDView();

        SetButton();
    }

    public void SetStat(EntityStat stat)
    {
        TMP_str.text = stat.str.ToString();

        TMP_def.text = stat.def.ToString();

        TMP_hp.text = stat.hp.ToString();
    }

    public void SetGold(int gold)
    {
        TMP_gold.text = gold.ToString();
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
                view?.SetInfo(entity.id, 0, 0);

                // 나 자신이라면..?
                if (entity.userId == ServerData.Instance.UserData.user_id)
                    SetStat(entity.stat);
            }
        }
    }

    private void SetButton()
    {
        // 버튼을 누르고 있을 때 true로 바꿉니다.
        BTN_addStr.OnPointerDownAsObservable()
            .Where(_ => _onClickButtonType == EStatType.NONE)
            .Subscribe(_ => _onClickButtonType = EStatType.STR)
            .AddTo(this);

        BTN_addStr.OnPointerUpAsObservable()
            .Merge(BTN_addStr.OnPointerExitAsObservable())
            .Subscribe(_ => _onClickButtonType = EStatType.NONE)
            .AddTo(this);

        BTN_addDef.OnPointerDownAsObservable()
            .Where(_ => _onClickButtonType == EStatType.NONE)
            .Subscribe(_ => _onClickButtonType = EStatType.DEF)
            .AddTo(this);

        BTN_addDef.OnPointerUpAsObservable()
            .Merge(BTN_addDef.OnPointerExitAsObservable())
            .Subscribe(_ => _onClickButtonType = EStatType.NONE)
            .AddTo(this);

        BTN_addHp.OnPointerDownAsObservable()
           .Where(_ => _onClickButtonType == EStatType.NONE)
           .Subscribe(_ => _onClickButtonType = EStatType.HP)
           .AddTo(this);

        BTN_addHp.OnPointerUpAsObservable()
            .Merge(BTN_addHp.OnPointerExitAsObservable())
            .Subscribe(_ => _onClickButtonType = EStatType.NONE)
            .AddTo(this);

        this.UpdateAsObservable()
            .Where(_ => _onClickButtonType != EStatType.NONE)
            .Subscribe(_ =>
            {
                var utcNow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                if (utcNow - _onClickButtonTime > BUTTON_CLICK_INTERVAL_MILLISECONDS)
                {
                    IncreaseStat(_onClickButtonType);
                    _onClickButtonTime = utcNow;
                }
            })
            .AddTo(this);
    }

    private void IncreaseStat(EStatType statType)
    {
        PlayAnimIncreaseStat(statType);
        GameMode.Instance.OnSendIncreaseStat((int)statType, 1);
    }

    public void OnRecvDead(S_Dead inPacket)
    {
        if (Entity.GetEntityType(inPacket.id) == Entity.EEntityType.PLAYER)
        {
            var dead = _kdViewArr.FirstOrDefault(x => x.targetId == inPacket.id);
            dead.AddDead(1);

            // 내가 사망했다면...? 리스폰 화면 띄워라
            if (inPacket.id == GameMode.Instance.EntitySelf.id)
                ShowRespawnTimer(inPacket.respawnTime);
        }

        if (Entity.GetEntityType(inPacket.id) == Entity.EEntityType.MOSNTER)
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

    public void PlayAnimIncreaseStat(EStatType type)
    {
        TMP_Text targetUI = null;

        switch (type)
        {
            case EStatType.STR:
                targetUI = TMP_str;
                break;

            case EStatType.DEF:
                targetUI = TMP_def;
                break;

            case EStatType.HP:
                targetUI = TMP_hp;
                break;
        }

        if (targetUI == null)
        {
            Debug.LogError($"[{nameof(PlayAnimIncreaseStat)}] targetUI is null or empty! type : {type}");
            return;
        }

        // Sequence 생성
        Sequence sequence = DOTween.Sequence();

        // 0.1초 동안 scale 1.3으로 확대
        sequence.Append(targetUI.transform.DOScale(1.5f, 0.1f));

        // 0.2초 동안 scale 1로 축소
        sequence.Append(targetUI.transform.DOScale(1f, 0.2f));
    }


    //---------------
    // Server
    //---------------

    public void OnRecvRespawn(S_Respawn packet)
    {
        HideRespawnTimer();
    }


    public void OnRecvPickReward(PickRewardResponse packet)
    {
        switch ((ERewardType)packet.rewardType)
        {
            case ERewardType.GOLD:
                break;

            case ERewardType.ITEM:
                break;
        }
    }

    public void OnRecvIncreaseStat(IncreaseStatResponse packet)
    {
        var entity = GameMode.Instance.EntitySelf;
        // SetStat(entity.stat);
    }


    public void OnUpdateInvenItemSingle(UpdateInvenItem packet)
    {
        var entity = GameMode.Instance.EntitySelf;
        _itemSlotList.EquipItem(entity.itemSlot[packet.invenItem.slot]);
    }


    public void OnUpdatePlayerCurrencySingle(UpdatePlayerCurrency packet)
    {
        var entity = GameMode.Instance.EntitySelf;
        SetGold(entity.gold);
    }


    public void OnUpdateStatBroadcast(UpdateStatBroadcast packet)
    {
        if(GameMode.Instance.IsSelf(packet.id) == false)
           return;

        var entity = GameMode.Instance.EntitySelf;
        SetStat(entity.stat);
    }
}