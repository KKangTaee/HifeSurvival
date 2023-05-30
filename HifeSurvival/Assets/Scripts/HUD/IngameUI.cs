using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using DG.Tweening;


public class IngameUI : MonoBehaviour
{
    [SerializeField] RectTransform RT_kdList;

    [SerializeField] KDView[] _kdViewArr;

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
                view?.SetInfo(entity.playerId, 0, 0);
            }
        }
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

        // ����� ��ȭ�� �ִٸ�..? 
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

        // ���� �����Ŷ��..?
        if(inPacket.toId == GameMode.Instance.EntitySelf.playerId)
        {
            // ��ũ���� ���ó���Ѵ�.
        }
    }
}
