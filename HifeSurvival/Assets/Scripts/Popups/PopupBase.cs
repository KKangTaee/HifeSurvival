using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;


public abstract class PopupBase : MonoBehaviour
{

    //-------------------
    // enums
    //-------------------

    public enum EAnim
    {
        OPEN_SCALE_NORMAL,  
        CLOSE_SCALE_NORMAL
    }




    [Header("[PopupBase]")]

    [SerializeField] Canvas          _canvas;

    // 최상단 백그라운드
    [SerializeField] RectTransform  _background; 
    
    // 백딤드
    [SerializeField] Image          _backDimmed;



    //------------------
    // variables
    //------------------

    public bool _isAnimatingNow;      // 지금 애니메이션 진행중인지 체크



    //-------------------
    // virtuals
    //-------------------

    protected abstract void OnButtonEvent(Button inButton);



    //-------------------
    // unity events
    //-------------------

    private void Awake()
    {
        _canvas.renderMode  = RenderMode.ScreenSpaceCamera; 
        _canvas.worldCamera = Camera.main;
    }



    //-------------------
    // functions
    //-------------------

    public void Open(int inLayerOrder, Action<PopupBase> inOpenCallback = null)
    {
        _canvas.sortingOrder = inLayerOrder;

        PlayAnimation(EAnim.OPEN_SCALE_NORMAL, inOpenCallback);
    }


    public void Close(Action<PopupBase> inCloseCallback = null)
    {
        PlayAnimation(EAnim.OPEN_SCALE_NORMAL, inCloseCallback);
    }


    public void PlayAnimation(EAnim inAnim, Action<PopupBase> inDoneCallback = null)
    { 
        _isAnimatingNow = true;

        Tweener anim = null;

        switch(inAnim)
        {
            case EAnim.OPEN_SCALE_NORMAL:
            _background.localScale = Vector2.zero;
            anim = _background.DOScale(Vector3.one, 0.5f);
            break;

            case EAnim.CLOSE_SCALE_NORMAL:
            anim = _background.DOScale(Vector3.zero, 0.3f);
            break;
        }

        anim.onComplete = ()=>
        {
            inDoneCallback?.Invoke(this);
            _isAnimatingNow = false;
        };
    }


    public void OnButton(Button inButton)
    {
        if(_isAnimatingNow == true)
            return;

        OnButtonEvent(inButton);        
    }


#if UNITY_EDITOR   


#endif
}
