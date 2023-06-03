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
        public const string KEY_DAMANGED = "Damanged";
    }

    public  class ParamKey
    {
        public const string KEY_IS_WALK     = "isWalk";
        public const string KEY_DIR         = "dir";
        public const string KEY_IS_DAMANGED = "isDamaged";
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

    public void OnAttack(in Vector3 inDir)
    {
        _anim.Play(AnimKey.KEY_ATTACK);
        _anim.SetFloat(ParamKey.KEY_DIR, inDir.y);
        _anim.SetBool(ParamKey.KEY_IS_WALK, false);
    }

    public void OnWalk(in Vector2 inDir)
    {
        _anim.SetBool(ParamKey.KEY_IS_WALK, true);
        _anim.SetFloat(ParamKey.KEY_DIR, inDir.y);

        SetDir(inDir.x);
    }

    public void SetDir(float inDir)
    {
        var scaleX = inDir > 0 ? -1 : 1;
         transform.localScale = new Vector3(scaleX, 1,1);
    }

    public void OnDead()
    {   
        _anim.Play(AnimKey.KEY_DEAD);
    }
 
    public void OnIdle(in Vector2 inDir = default)
    {
        if(inDir != default)
            _anim.SetFloat(ParamKey.KEY_DIR, inDir.y);

        _anim.SetBool(ParamKey.KEY_IS_WALK, false);
        _anim.Play(AnimKey.KEY_IDLE);
    }

    public void OnDamaged()
    {
        _anim.SetTrigger(ParamKey.KEY_IS_DAMANGED);
    }
}
