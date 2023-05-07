using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UniRx.Async;
using DG.Tweening;

public class SceneLoading : MonoBehaviour
{
    public enum EAnim
    {
        SHOW,
        HIDE
    }


    [SerializeField] private Image IMG_LoadingBorder;
    [SerializeField] private TMPro.TextMeshProUGUI TXT_Loading;


    private const string    SHADER_PARAM_KEY_MASK_VALUE = "_Radius";
    private const int       SHADER_PARAM_MASK_VALUE_MAX = 1;

    private Material    _borderMat;
    private bool        _hide;
    private Sequence    _textSeq;


    //---------------
    // unity events
    //---------------

    private void Awake()
    {
        _borderMat = IMG_LoadingBorder.material;
        _borderMat.SetFloat(SHADER_PARAM_KEY_MASK_VALUE, 1);

        StopTextSequence();
    }


    //-----------------
    // functions
    //-----------------


    private void PlayAnimation(EAnim inAnim, Action<bool> doneCallback = null)
    {
        Tweener tweener = null;

        switch(inAnim)
        {
            case EAnim.SHOW:

            tweener = _borderMat.DOFloat(0,
                                         SHADER_PARAM_KEY_MASK_VALUE,
                                         0.7f)
                                .SetEase(Ease.OutCubic);
                                         
            
            break;

            case EAnim.HIDE:

            tweener = _borderMat.DOFloat(SHADER_PARAM_MASK_VALUE_MAX,
                                         SHADER_PARAM_KEY_MASK_VALUE,
                                         0.7f)
                                .SetEase(Ease.OutCubic);
            break;
        }

        if(tweener == null)
        {
            doneCallback?.Invoke(false);
        }
        else
        {
            tweener.onComplete = ()=>
            {
                doneCallback?.Invoke(true);
            };
        }
    }

    private void PlayTextSequence()
    {
        TXT_Loading.gameObject.SetActive(true);

        float duration = 0.5f;

        _textSeq = DOTween.Sequence();
        _textSeq.Append(TXT_Loading.DOFade(0.5f, duration))
                .Append(TXT_Loading.DOFade(1f, duration))
                .SetLoops(-1, LoopType.Restart);
    }

    private void StopTextSequence()
    {
        _textSeq?.Kill();
        TXT_Loading.gameObject.SetActive(false);
    }


    private async UniTaskVoid PlayLoading()
    {
        while(_hide == false)
        {
            //TODO@taeho.kang do something...
            await UniTask.Delay(33);
        }
    }


    public void Show(Action<bool> doneCallback)
    {
        _hide = false;

        PlayAnimation(EAnim.SHOW, isSuccess =>
        {
            PlayTextSequence();
            doneCallback?.Invoke(isSuccess);
        });
    }

    public void Hide(Action<bool> doneCallback)
    {
        _hide = true;

        StopTextSequence();
        PlayAnimation(EAnim.HIDE, doneCallback);
    }
}