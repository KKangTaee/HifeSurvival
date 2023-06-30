using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using System;


[ObjectPool(PATH_IN_RESOURCES_FOLDER = "Prefabs/Characters/ActionDisplayUI",
            IN_RESOURCES_FORLDER = true)]
public class ActionDisplayUI : MonoBehaviour
{
    public enum ESpawnType
    {
        ATTACK,
        TAKE_DAMAGE,
        GET_GOLD,
    }


    [SerializeField] private TMP_Text TMP_attack;  // Text Component
    [SerializeField] private TMP_Text TMP_takeDamage;
    [SerializeField] private TMP_Text TMP_getGold;

    public Vector3 startPos;    // Starting Position
    public Vector3 endPos;      // Ending Position
    public float height;        // Height of the bounce
    public float duration;      // Duration of the animation

    public void PlayAttack(int attackVal, Action doneCallback)
    {
        TMP_attack.text = attackVal.ToString();

        // Create the waypoints for the animation
        Vector3[] waypoints = new Vector3[]
        {
            transform.position + startPos,
            transform.position + new Vector3(startPos.x + ((endPos.x - startPos.x) / 2), startPos.y + height, startPos.z),
            transform.position + endPos
        };

        Sequence sequence = DOTween.Sequence();

        TMP_attack.transform.localPosition = Vector3.zero;
        TMP_attack.DOFade(1, 0);

        sequence.Append(TMP_attack.transform.DOScaleX(2f, 0.1f));
        sequence.Append(TMP_attack.transform.DOScaleY(0.8f, 0.1f));
        sequence.Append(TMP_attack.transform.DOScale(1f, 0.05f));

        sequence.Insert(0, TMP_attack.transform.DOMove(waypoints[1], duration * 0.25f).SetEase(Ease.OutQuad));
        sequence.Insert(duration * 0.25f, TMP_attack.transform.DOMove(waypoints[2], duration * 0.6f).SetEase(Ease.InCubic));

        sequence.OnComplete(() =>
        {
            TMP_attack.DOFade(0, 0);
            doneCallback?.Invoke();
        });

        sequence.Play();
    }


    public void PlayDamaged(int damageVal, Action doneCallback)
    {
        TMP_takeDamage.text = damageVal.ToString(); // Show the damaged value
        TMP_takeDamage.color = new Color(TMP_takeDamage.color.r, TMP_takeDamage.color.g, TMP_takeDamage.color.b, 0); // Set the alpha to 0

        Sequence s = DOTween.Sequence();
        s.Append(TMP_takeDamage.DOFade(1, 0)); // Instantly set the alpha to 1
        s.Append(TMP_takeDamage.transform.DOMoveY(TMP_takeDamage.transform.position.y + 1.8f, 0.8f)); // Move the position by 2 on y axis over 0.5 second
        s.Append(TMP_takeDamage.DOFade(0, 0.3f)) // Fade out the alpha over 0.3 second
          .OnComplete(() =>
            {
                TMP_takeDamage.transform.localPosition = Vector3.zero; // Reset position
                doneCallback?.Invoke(); // Execute the done callback function
            });
        s.Play();
    }


    public void PlayGetGold(int gold, Action doneCallback)
    {
        float duration      = 1f; 
        float moveY         = 2f; 
        float fadeDuration  = 0.5f;

        TMP_getGold.text = $"+{gold}";
        var canvasGroup = TMP_getGold.GetComponent<CanvasGroup>();

        // 처음 상태를 초기화
        TMP_getGold.transform.localPosition = Vector3.zero;
        canvasGroup.alpha = 1;

        // 애니메이션을 순차적으로 진행
        Sequence seq = DOTween.Sequence();
        
        // 이동 애니메이션 추가
        seq.Append(TMP_getGold.transform.DOLocalMoveY(moveY, duration));

        // 페이드 아웃 애니메이션 추가
        seq.Insert(duration - fadeDuration, canvasGroup.DOFade(0, fadeDuration));

        // 모든 애니메이션이 끝나면 콜백 함수 호출
        seq.OnComplete(() => doneCallback?.Invoke());
    }


    //---------------
    // static
    //---------------

    public static void Show(ESpawnType type, in int val, in Vector3 pos)
    {
        var objectPool = ControllerManager.Instance.GetController<ObjectPoolController>();
        var inst = objectPool.SpawnFromPool<ActionDisplayUI>();

        switch (type)
        {
            case ESpawnType.ATTACK:
                inst.transform.position = pos;
                inst.PlayAttack(val, () => objectPool.StoreToPool(inst));
                break;

            case ESpawnType.TAKE_DAMAGE:
                int offsetX = UnityEngine.Random.Range(-5, 5);
                // int offsetY = UnityEngine.Random.Range(-3, 3);
                inst.transform.position = pos + new Vector3(offsetX * 0.1f, 0, 0);
                inst.PlayDamaged(val, () => objectPool.StoreToPool(inst));
                break;

            case ESpawnType.GET_GOLD:
                
                break;
        }
    }
}