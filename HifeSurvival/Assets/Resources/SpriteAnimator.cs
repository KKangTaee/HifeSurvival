using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


[RequireComponent(typeof(SpriteRenderer), typeof(Animator))]
public class SpriteAnimator : MonoBehaviour
{
    [SerializeField] private Animator _anim;

    private void Reset()
    {
        _anim = GetComponent<Animator>();
    }

    private void Awake()
    {
        _anim.speed = 1.5f;
    }

    public void PlayAnimation(string inKeyName)
    {
        _anim.Play(inKeyName, 0);
    }


    private void Update()
    {
      
         bool isWalk = false;
        OnKey(KeyCode.W, ()=>
        {
            _anim.SetFloat("dir", 1);
           isWalk = true;
        },
        ()=>
        {   
           
        });

        OnKey(KeyCode.S, ()=>
        {
            _anim.SetFloat("dir", 0);
            isWalk = true;
        },
        ()=>
        {

        });

        OnKey(KeyCode.A, ()=>
        {
            transform.localScale = new Vector3(1, 1, 1);
            isWalk = true;
        },
        ()=>
        {

        });

        OnKey(KeyCode.D, ()=>
        {
            transform.localScale = new Vector3(-1, 1, 1);
            isWalk = true;
        },
        ()=>
        {

        });

        if(Input.GetKeyDown(KeyCode.L))
        {
            // _anim.SetTrigger("isAttack");
            _anim.Play("Attack");
            // _anim.Update(0);
        }

        _anim.SetBool("isWalk", isWalk);
    }

    public void OnKey(KeyCode keyCode, Action downCallback, Action upCallback)
    {
        if(Input.GetKey(keyCode))
        {
            downCallback?.Invoke();
        }
        else if(Input.GetKeyUp(keyCode))
        {
            upCallback?.Invoke();
        }
    }
}
