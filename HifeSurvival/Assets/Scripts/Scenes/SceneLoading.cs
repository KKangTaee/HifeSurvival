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


    private const string    SHADER_PARAM_KEY_MASK_VALUE = "_MaskValue";
    private const int       SHADER_PARAM_MASK_VALUE_MAX = 15;

    private Material    _borderMat;
    private bool        _hide;


    //---------------
    // unity events
    //---------------

    private void Awake()
    {
        _borderMat = IMG_LoadingBorder.material;
        _borderMat.SetFloat(SHADER_PARAM_KEY_MASK_VALUE, 0);
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

            tweener = _borderMat.DOFloat(SHADER_PARAM_MASK_VALUE_MAX,
                                         SHADER_PARAM_KEY_MASK_VALUE,
                                         3f);
                                         
            
            break;

            case EAnim.HIDE:

            tweener = _borderMat.DOFloat(0,
                                         SHADER_PARAM_KEY_MASK_VALUE,
                                         1.5f);
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


    private async UniTaskVoid PlayLoading()
    {
        while(_hide == false)
        {
            // TODO@taeho.kang do something...
            await UniTask.Delay(33);
        }
    }


    public void Show(Action<bool> doneCallback)
    {
        _hide = false;
        PlayAnimation(EAnim.SHOW, doneCallback);
        PlayLoading().Forget();
    }

    public void Hide(Action<bool> doneCallback)
    {
        _hide = true;
        PlayAnimation(EAnim.HIDE, doneCallback);
    }
}