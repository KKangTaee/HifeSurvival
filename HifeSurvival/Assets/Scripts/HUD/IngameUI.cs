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

    [SerializeField] KDView[] _kdViewArr;

    IDisposable _respawnTimer;


    public void Start()
    {
        GameMode.Instance.OnRecvDeadCB += OnRecvDead;
    }

    public void Init()
    {
        var playerEntitys = GameMode.Instance.PlayerEntitysDic.Values;

        var iter = _kdViewArr.GetEnumerator();

        foreach (var entity in playerEntitys)
        {
            if (iter.MoveNext())
            {
                KDView view = iter.Current as KDView;
                view?.gameObject.SetActive(true);
                view?.SetInfo(entity.targetId, 0, 0);

                // 나 자신이라면..?
                if(entity.userId == ServerData.Instance.UserData.user_id)
                    SetStatUI(entity.stat, 1000);
            }
        }

        GetComponent<Canvas>().worldCamera = ControllerManager.Instance.GetController<CameraController>().MainCamera;
    }

    public void SetStatUI(EntityStat inStat, int inGold)
    {
        TMP_str.text  = inStat.str.ToString();

        TMP_def.text  = inStat.def.ToString();

        TMP_hp.text   = inStat.hp.ToString();

        TMP_gold.text = inGold.ToString();
    }

    public void OnRecvDead(S_Dead inPacket)
    {
        var kill = _kdViewArr.FirstOrDefault(x => x.PlayerId == inPacket.fromId);
        var dead = _kdViewArr.FirstOrDefault(x => x.PlayerId == inPacket.toId);

        if (kill == null || dead == null)
        {
            Debug.LogError("KDView is null or empty!");
            return;
        }

        kill.AddKill(1);
        dead.AddDead(1);

        float _animationDuration = 0.5f;

        _kdViewArr = _kdViewArr.OrderByDescending(x => x.KillCount).ToArray();

        // Apply animation
        for (int i = 0; i < _kdViewArr.Length; i++)
        {
            RectTransform rectTransform = _kdViewArr[i].GetComponent<RectTransform>();

            // Calculate the new position of this KDView
            Vector3 newPosition = new Vector3(rectTransform.localPosition.x, -i * rectTransform.rect.height, rectTransform.localPosition.z);

            // If this player is the one who killed, increase the scale
            if (_kdViewArr[i].PlayerId == inPacket.fromId)
            {
                _kdViewArr[i].transform.DOScale(1.2f, _animationDuration).OnComplete(() =>
                {
                    // After increasing the scale, move the KDView to the new position and then restore the scale
                    rectTransform.DOLocalMove(newPosition, _animationDuration).OnComplete(() =>
                    {
                        _kdViewArr[i].transform.DOScale(1f, _animationDuration);
                    });
                });
            }
            else
            {
                // Just move the KDView to the new position
                rectTransform.DOLocalMove(newPosition, _animationDuration);
            }
        }

        if(inPacket.toId == GameMode.Instance.EntitySelf.targetId)
            PlayRespawnTimer(inPacket.respawnTime);
    }


    public void PlayRespawnTimer(int totalTime)
    {
        RT_respawnPanel?.gameObject.SetActive(true);

        int elapsedSeconds = 0; // 지나간 시간

        _respawnTimer = Observable.Interval(TimeSpan.FromSeconds(1))
            .Subscribe(_ =>
            {
                elapsedSeconds++;
                Debug.Log($"Elapsed Time: {elapsedSeconds}s");

                // 알파값을 0.5에서 1로 바꾸는 동안 0.5초의 애니메이션을 수행합니다.
                TMP_respawnCount.DOFade(1f, 0.5f);
                
                // 알파값을 1에서 0.5로 바꾸는 동안 0.5초의 애니메이션을 수행합니다.
                TMP_respawnCount.DOFade(0.5f, 0.5f).SetDelay(0.5f);

                if (elapsedSeconds >= totalTime)
                {
                    _respawnTimer?.Dispose(); // 타이머를 정지합니다.
                }
            }).AddTo(this);
    }
}
