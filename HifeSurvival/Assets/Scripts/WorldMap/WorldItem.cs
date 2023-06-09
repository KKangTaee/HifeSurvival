﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

// NOTE@taeho.kang 구매한 에셋에 대한 정의
using ItemAnimator = Assets.FantasyMonsters.Scripts.Monster;

[ObjectPool(PATH_IN_RESOURCES_FOLDER = "Prefabs/WorldObjects/WorldItem",
            IN_RESOURCES_FORLDER = true)]
public class WorldItem : WorldObjectBase
{
    [SerializeField] private Transform _center;
    [SerializeField] private CircleCollider2D _col;

    [SerializeField] private ItemAnimator[] _animArr;

    private ItemAnimator _targetAnim;

    private float jumpHeight = 2.5f;
    private float jumpDuration = 0.5f;

    public int WorldId { get; private set; }

    public void SetInfo(int inWorldId, in Vector3 inPos, int inRewardType)
    {
        WorldId = inWorldId;
        transform.position = inPos;
        SetTargetAnim(inRewardType);

        _col.enabled = false;
    }

    public void SetTargetAnim(int inItemType)
    {
        for (int i = 0; i < _animArr?.Length; i++)
        {
            if (i == inItemType - 1)
            {
                _targetAnim = _animArr[i];
                _targetAnim.gameObject.SetActive(true);
            }
            else
            {
                _animArr[i].gameObject.SetActive(false);
            }
        }

        _targetAnim.ResetAlphaParts();
    }


    public void PlayDropItem(Action doneCallback = null)
    {
        Vector3 originalPosition = _center.position;
        Vector3 originalScale = _center.localScale;

        Tweener rotationTween = _center.DORotate(new Vector3(0, 0, 360), 0.2f, RotateMode.FastBeyond360)
            .SetLoops(-1, LoopType.Yoyo);

        _center.DOJump(originalPosition + new Vector3(0, jumpHeight, 0), jumpHeight, 1, jumpDuration)
            .OnComplete(() =>
            {
                rotationTween.Kill();

                _center.rotation = Quaternion.identity;

                _center.DOShakeScale(0.3f).OnComplete(() =>
                {
                    _center.DOScale(originalScale, 0);
                    doneCallback?.Invoke();
                    _col.enabled = true;

                });
            });
    }


    public void PlayGetItem(Action doneCallback = null)
    {
        _col.enabled = false;

        transform.DOMoveY(transform.position.y + jumpHeight, 1.1f).OnComplete(() =>
        {
            // 획득 후 풀에 넣는다.
            doneCallback?.Invoke();
        });

        _targetAnim.Fade(0, 1.1f);
    }
}