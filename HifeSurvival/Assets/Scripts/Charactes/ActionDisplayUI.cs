using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using System;


[ObjectPool(PATH_IN_RESOURCES_FOLDER = "Prefabs/Popups/PopupSelectHeros/PopupSelectHeros",
            IN_RESOURCES_FORLDER = true)]
public class ActionDisplayUI : MonoBehaviour
{
    public enum ESpawnType
    {
        ATTACK_SELF,
        ATTACK_OTHER,
    }


    [SerializeField] private TMP_Text TMP_attackSelf;  // Text Component
    [SerializeField] private TMP_Text TMP_attackOther;

    public Vector3 startPos;    // Starting Position
    public Vector3 endPos;      // Ending Position
    public float height;        // Height of the bounce
    public float duration;      // Duration of the animation

    public void PlayAttackSelf(int inAttackVal, Action doneCallback)
    {
        TMP_attackSelf.text = inAttackVal.ToString();

        // Create the waypoints for the animation
        Vector3[] waypoints = new Vector3[]
        {
            startPos,
            new Vector3(startPos.x + ((endPos.x - startPos.x) / 2), startPos.y + height, startPos.z),
            endPos
        };

        Sequence sequence = DOTween.Sequence();

        TMP_attackSelf.transform.position = Vector3.zero;
        TMP_attackSelf.DOFade(1, 0);

        sequence.Append(TMP_attackSelf.transform.DOScaleX(2f, 0.1f));
        sequence.Append(TMP_attackSelf.transform.DOScaleY(0.8f, 0.1f));
        sequence.Append(TMP_attackSelf.transform.DOScale(1f, 0.05f));

        sequence.Insert(0, TMP_attackSelf.transform.DOMove(waypoints[1], duration * 0.25f).SetEase(Ease.OutQuad));
        sequence.Insert(duration * 0.25f, TMP_attackSelf.transform.DOMove(waypoints[2], duration * 0.6f).SetEase(Ease.InCubic));

        sequence.OnComplete(() =>
        {
            TMP_attackSelf.DOFade(0, 0);
            doneCallback?.Invoke();
        });

        sequence.Play();
    }



    //---------------
    // static
    //---------------

    public static void Show(ESpawnType inType, in int inVal, in Vector3 inPos)
    {
        var objectPool = ControllerManager.Instance.GetController<ObjectPoolController>();
        var inst = objectPool.SpawnFromPool<ActionDisplayUI>();

        inst.transform.position = inPos;

        switch(inType)
        {
            case ESpawnType.ATTACK_SELF:
                inst.PlayAttackSelf(inVal, () => objectPool.RestoreToSpawn(inst));
            break;

            case ESpawnType.ATTACK_OTHER:
            
            break;
        }
    }
}