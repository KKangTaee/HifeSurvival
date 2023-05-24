using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroAnimator : MonoBehaviour
{
    [SerializeField] Animator _anim;

    [SerializeField] RuntimeAnimatorController[] _heroAnimSetArr;

    private void Awake()
    {
        SetAnimatorController(0);
    }

    public class AnimKey
    {
        public const string KEY_IDLE = "Idle";
        public const string KEY_WALK = "Walk";
        public const string KEY_DEAD = "Dead";
        public const string KEY_ATTACK = "Attack";
    }

    public  class ParamKey
    {
        public const string KEY_IS_WALK = "isWalk";
        public const string KEY_DIR     = "dir";
    }

    public void SetAnimatorController(int inIndex)
    {
        if (inIndex < 0 || _heroAnimSetArr.Length <= inIndex)
            return;

        _anim.runtimeAnimatorController = _heroAnimSetArr[inIndex];
        
        OnIdle();
    }

    public void PlayAnimation(string inKey)
    {
        _anim.Play(inKey);
    }

    public void OnAttack()
    {
        _anim.Play(AnimKey.KEY_ATTACK);
    }

    public void OnWalk(float inDir)
    {
        _anim.SetFloat(ParamKey.KEY_DIR, inDir);
    }

    public void OnDead()
    {

    }

    public void OnIdle()
    {
        _anim.Play(AnimKey.KEY_IDLE);
    }
}
