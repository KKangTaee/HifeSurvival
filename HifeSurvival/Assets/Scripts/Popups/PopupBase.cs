using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using UniRx.Async;


//-----------------
// interfaces
//-----------------
public interface IPopupOpenAsync
{
    UniTask PrevOpenAsync();
    UniTask PostOpenAsync();
}


public interface IPopupOpen
{
    void PrevOpen();
    void PostOpen();
}



[RequireComponent(typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster))]
public abstract class PopupBase : MonoBehaviour
{

    //-------------------
    // enums
    //-------------------

    public enum EAnim
    {
        OPEN_SCALE_NORMAL,  
        CLOSE_SCALE_NORMAL,
        NONE,
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

    public const string BACKDIMMED_OBJ_NAME = "BackDimmed";
    public const string BACKGROUND_OBJ_NAME = "Background";


    protected bool  _isAnimatingNow;      // 지금 애니메이션 진행중인지 체크

    protected EAnim _eOpenAnim  = EAnim.OPEN_SCALE_NORMAL;

    protected EAnim _eCloseAnim = EAnim.CLOSE_SCALE_NORMAL;
    



    //-------------------
    // virtuals
    //-------------------

    protected abstract void OnButtonEvent(Button inButton);



    //-------------------
    // unity events
    //-------------------

    private void Reset()
    {
        if(_backDimmed == null)
        {
            for(int i =0; i<transform.childCount; i++)
            {
                var child = transform.GetChild(i);

                if(child.name == BACKDIMMED_OBJ_NAME)
                {
                   _backDimmed = child.gameObject.AddComponent<Image>();
                   break;
                }
            }

            if(_backDimmed == null)
            {
                var emtpyDimmed = new GameObject();
                emtpyDimmed.name = BACKDIMMED_OBJ_NAME;
                emtpyDimmed.transform.parent = transform;
                _backDimmed = emtpyDimmed.AddComponent<Image>();
            }

            _backDimmed.color = new Color(0,0,0,0.5f);

            _backDimmed.rectTransform.anchorMin = Vector2.zero;
            _backDimmed.rectTransform.anchorMax = Vector2.one;
            _backDimmed.rectTransform.anchoredPosition = Vector3.zero;
            _backDimmed.rectTransform.sizeDelta = Vector2.zero;
        }

        if(_background == null)
        {
            for(int i =0; i<_backDimmed.transform.childCount; i++)
            {
                var child = _backDimmed.transform.GetChild(i);

                if(child.name == BACKGROUND_OBJ_NAME)
                {
                    _background = child.gameObject.GetComponent<RectTransform>();
                    break;
                }
            }

            if(_background == null)
            {
                var emptyBg = new GameObject();
                emptyBg.name = BACKGROUND_OBJ_NAME;
                emptyBg.transform.parent = _backDimmed.transform;

                _background = emptyBg.AddComponent<RectTransform>();

                _background.anchorMin = Vector2.zero;
                _background.anchorMax = Vector2.one;
                _background.anchoredPosition = Vector3.zero;
                _background.sizeDelta = Vector2.zero;
            }

        }

        _canvas = GetComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceCamera;
    }



    protected virtual void Awake()
    {
        _canvas.worldCamera = Camera.main;
    }



    //-------------------
    // functions
    //-------------------

    public void Open(int inLayerOrder, Action<PopupBase> inOpenCallback = null)
    {
        _canvas.sortingOrder = inLayerOrder;

        PlayAnimation(_eOpenAnim, inOpenCallback);
    }


    public void Close(Action<PopupBase> inCloseCallback = null)
    {
        PlayAnimation(_eCloseAnim, (popup) =>
        {
            inCloseCallback?.Invoke(popup);
            
            // NOTE@taeho.kang 더 좋은 방법이 있으면 수정.
            PopupManager.Instance.RemovePopup(this);

            Destroy(gameObject);
        });         
    }


    public void PlayAnimation(EAnim inAnim, Action<PopupBase> inDoneCallback = null)
    { 
        _isAnimatingNow = true;

        Tweener anim = null;

        switch(inAnim)
        {
            case EAnim.OPEN_SCALE_NORMAL:
            _background.localScale = Vector2.zero;
            anim = _background.DOScale(Vector3.one, 0.2f);
            break;

            case EAnim.CLOSE_SCALE_NORMAL:
            anim = _background.DOScale(Vector3.zero, 0.2f);
            break;
        }

        if(anim != null)
        {
            anim.onComplete = ()=>
            {
                inDoneCallback?.Invoke(this);
                _isAnimatingNow = false;
            };
        }
        else
        {
            inDoneCallback?.Invoke(this);
            _isAnimatingNow = false;
        }
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
