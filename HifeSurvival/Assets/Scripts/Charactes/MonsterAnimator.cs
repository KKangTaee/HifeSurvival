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

    public void SetTargetAnim(int inMosterId, Action<Vector3> pivotUICallback)
    {
        var animData = _animDataArr.FirstOrDefault(x => x.id == inMosterId);

        if (animData == null)
        {
            Debug.LogError($"[{nameof(SetTargetAnim)}] targetAnim is null or empty! : monsterId : {inMosterId}");
            return;
        }

        _targetAnim = animData.anim;
        _targetAnim.gameObject.SetActive(true);
        
        pivotUICallback?.Invoke(animData.pivotUI.position);
    }

    public void OnIdle()
    {
        _targetAnim.SetState(MonsterState.Idle);
    }

    public void OnAttack()
    {
        _targetAnim.Attack();
    }

    public void OnDead()
    {
        _targetAnim.SetState(MonsterState.Death);
    }

    public void OnWalk()
    {
        _targetAnim.SetState(MonsterState.Walk);
    }

    public void OnDamaged()
    {
        // Make sure to kill any previous tweens to avoid overlap
        DOTween.Kill(this);

        // Shake object along x-axis
        transform.DOShakePosition(0.2f, new Vector3(1f, 0f, 0f), 2, 90, false, true).SetId(this);

        // Change color to red and then return to original
        _targetAnim.Head.DOColor(Color.red, 0.1f)
            .OnComplete(() => _targetAnim.Head.DOColor(Color.white, 0.2f).SetDelay(0.2f));
    }

    public void SetDir(float inDir)
    {
        var scaleX = inDir > 0 ? -1 : 1;
        transform.localScale = new Vector3()
        {
            x = scaleX * transform.localScale.x,
            y = transform.localScale.y,
            z = transform.localScale.z,
        };
    }
}
