using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

// NOTE@taeho.kang 구매한 에셋에 대한 정의
using ItemAnimator = Assets.FantasyMonsters.Scripts.Monster;

[ObjectPool(PATH_IN_RESOURCES_FOLDER = "Prefabs/Characters/ActionDisplayUI",
            IN_RESOURCES_FORLDER = true)]
public class WorldItem : WorldObjectBase
{
    [SerializeField] private Transform _pivot;

    [SerializeField] private ItemAnimator[] _animArr;

    private ItemAnimator _targetAnim;

    private float jumpHeight = 2f;
    private float jumpDuration = 0.5f;

    public int ItemId { get; private set; }

    public void SetInfo(int inDropId, ItemData inItemIds)
    {
        ItemId = inDropId;
        SetTargetAnim(inItemIds.itemType);
    }

    public void SetTargetAnim(int inItemType)
    {
        for (int i = 0; i < _animArr?.Length; i++)
        {
            if (i == inItemType)
            {
                _targetAnim = _animArr[i];
                _targetAnim.gameObject.SetActive(true);
            }
            else
            {
                _animArr[i].gameObject.SetActive(false);
            }
        }
    }

    public void PlayDropItem(Action doneCallback = null)
    {
        Vector3 originalPosition = _pivot.position;
        Vector3 originalScale    = _pivot.localScale;

        Tweener rotationTween = _pivot.DORotate(new Vector3(0, 0, 360), 0.2f, RotateMode.FastBeyond360)
            .SetLoops(-1, LoopType.Yoyo);

        _pivot.DOJump(originalPosition + new Vector3(0, jumpHeight, 0), jumpHeight, 1, jumpDuration)
            .OnComplete(() =>
            {
                rotationTween.Kill(); // Stop the rotation when the object lands.

                // After jumping, shake the object to simulate the impact of landing.
                _pivot.DOShakeScale(0.5f).OnComplete(() => 
                {
                    // Make sure to return the object to its original scale after the shake.
                    _pivot.DOScale(originalScale, 0);
                    doneCallback?.Invoke();
                });
            });
    }

    public void PlayGetItem(Action doneCallback = null)
    {
        SpriteRenderer spriteRenderer = _targetAnim.GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            spriteRenderer.material.DOFade(0, jumpDuration);
        }

        // Move the _pivot object upwards.
        _pivot.DOMoveY(_pivot.position.y + jumpHeight, jumpDuration).OnComplete(() =>
        {
            // 획득 후 풀에 넣는다.
            ControllerManager.Instance.GetController<ObjectPoolController>().StoreToPool(this); 
        });
    }
}


public struct ItemData
{

    //--------------
    // enums
    //--------------

    public enum EItemType
    {
        GOLD,

        ITEM,
    }

    public int itemType;
    public int subType;
    public int count;

    public static ItemData[] Parse(string inItemIds)
    {
        var itemIdsSet = inItemIds.Split(',');

        var itemDataArr = new ItemData[itemIdsSet.Length];

        for (int i = 0; i < itemIdsSet.Length; i++)
        {
            var split = itemIdsSet[i].Split(':');

            if (split?.Length != 3)
            {
                Debug.LogError($"itemData is wrong! : {itemIdsSet[i]}");
                return null;
            }

            if (int.TryParse(split[0], out var item_type) == false ||
               int.TryParse(split[1], out var sub_type) == false ||
               int.TryParse(split[2], out var count) == false)
            {
                Debug.LogError($"itemData is wrong! : {itemIdsSet[i]}");
                return null;
            }

            itemDataArr[i] = new ItemData()
            {
                itemType = item_type,
                subType = sub_type,
                count = count,
            };
        }

        return itemDataArr;
    }
}

