using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;
using System;

using MonsterAnim = Assets.FantasyMonsters.Scripts.Monster;
using MonsterState = Assets.FantasyMonsters.Scripts.MonsterState;


public class MonsterAnimator : MonoBehaviour
{
    [System.Serializable]
    public class MonsterAnimData
    {
        public int id;
        public MonsterAnim anim;
        public Transform pivotUI;
    }


    [SerializeField] private MonsterAnimData[] _animDataArr;

    MonsterAnim _targetAnim;
    Vector3     _targetPivotUIPos;

    public void SetTargetAnim(int inMosterId)
    {
        var animData = _animDataArr.FirstOrDefault(x => x.id == inMosterId);

        if (animData == null)
        {
            Debug.LogError($"[{nameof(SetTargetAnim)}] targetAnim is null or empty! : monsterId : {inMosterId}");
            return;
        }

        _targetAnim = animData.anim;
        _targetAnim.ResetAlphaParts();
        _targetAnim.gameObject.SetActive(true);

        _targetPivotUIPos = animData.pivotUI.position;
    }

    public Vector3 GetPosPivotUI()
    {
        return _targetPivotUIPos;
    }

    public void AddEventDeathCompleted(Action inDeathCallback)
    {
        _targetAnim.OnDeathCompletedHandler = inDeathCallback;
    }

    public void OnIdle()
    {
        _targetAnim.SetState(MonsterState.Idle);
    }

    public void OnAttack(in Vector3 dir)
    {
        SetDir(dir);
        _targetAnim.Attack();
    }

    public void OnDead()
    {
        _targetAnim.Die();
    }

    public void OnWalk(in Vector3 dir)
    {
        SetDir(dir);
        _targetAnim.SetState(MonsterState.Walk);
    }

    public void OnDamaged()
    {
        // Make sure to kill any previous tweens to avoid overlap
        DOTween.Kill(this);

        // Shake object along x-axis
        transform.DOShakePosition(0.2f, new Vector3(1f, 0f, 0f), 2, 90, false, true).SetId(this);

        _targetAnim.Damage();
    }

    public void SetDir(in Vector3 dir)
    {
        var scaleX = dir.x > 0 ? -1 : 1;
        transform.localScale = new Vector3()
        {
            x = scaleX * transform.localScale.x,
            y = transform.localScale.y,
            z = transform.localScale.z,
        };
    }
}
