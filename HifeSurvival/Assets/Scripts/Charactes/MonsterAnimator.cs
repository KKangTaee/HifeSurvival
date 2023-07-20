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
    [SerializeField] MonsterAnim _anim;

    public void SetAnim()
    {
        _anim.ResetAlphaParts();
        _anim.gameObject.SetActive(true);
    }

    public void AddEventDeathCompleted(Action inDeathCallback)
    {
        _anim.OnDeathCompletedHandler = inDeathCallback;
    }

    public void OnIdle()
    {
        _anim.SetState(MonsterState.Idle);
    }

    public void OnAttack(in Vector3 dir)
    {
        SetDir(dir);
        _anim.Attack();
    }

    public void OnDead()
    {
        _anim.Die();
    }

    public void OnWalk(in Vector3 dir)
    {
        SetDir(dir);
        _anim.SetState(MonsterState.Walk);
    }

    public void OnDamaged()
    {
        // Make sure to kill any previous tweens to avoid overlap
        DOTween.Kill(this);

        // Shake object along x-axis
        transform.DOShakePosition(0.2f, new Vector3(1f, 0f, 0f), 2, 90, false, true).SetId(this);

        _anim.Damage();
    }

    public void SetDir(in Vector3 dir)
    {
        var scaleX = dir.x > 0 ? -1 : 1;
        transform.localScale = new Vector3()
        {
            x = scaleX * Mathf.Abs(transform.localScale.x),
            y = transform.localScale.y,
            z = transform.localScale.z,
        };
    }
}
