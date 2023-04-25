using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;


public class TopHUD : MonoBehaviour
{
    private static TopHUD _instance;

    public static TopHUD Instance
    {
        get
        {
            if(_instance == null)
            {
                var prefab = Resources.Load<TopHUD>(TOP_HUD_PREFAB_PATH);
                _instance = Instantiate<TopHUD>(prefab);
                _instance.gameObject.name = $"[Singleton Object][{nameof(TopHUD)}]";

                DontDestroyOnLoad(_instance);
            }

            return _instance;
        }
    }


    public enum EAnim
    {
        SHOW,
        HIDE,
    }

    [SerializeField] private RectTransform      RT_background;

    [SerializeField] private Button             BTN_Settings;

    [SerializeField] private HUD_CurrencyView   HUD_Coin;
    [SerializeField] private HUD_CurrencyView   HUD_Ticket;

    private const int ANIM_SHOW_POS_Y = 20;
    private const int ANIM_HIDE_POS_Y = 140;

    private const string TOP_HUD_PREFAB_PATH = "Prefabs/HUD/TopHUD";


    //---------------
    // unity events
    //---------------

    private void Awake()
    {
        Initialize();
    }



    //----------------
    // functions
    //----------------

    private void Initialize()
    {
        HUD_Coin.SetInfo(EGameCurrency.COIN,    1000);

        HUD_Coin.SetInfo(EGameCurrency.TICKET,  50);
    
        BTN_Settings.onClick.AddListener(()=>
        {
            // TODO@taeho.kang 설정팝업
        });

        RT_background.anchoredPosition = new Vector2(0, ANIM_HIDE_POS_Y);
    }


    private void Release()
    {
        _instance = null;
        Destroy(this);
    }


    public void PlayAnimation(EAnim inAnim, Action<bool> doneCallback = null)
    {
        Tweener tweener = null;

        switch(inAnim)
        {
            case EAnim.SHOW:
            tweener = RT_background.DOAnchorPosY(ANIM_SHOW_POS_Y, 1);
            break;
            
            case EAnim.HIDE:
            tweener = RT_background.DOAnchorPosY(ANIM_HIDE_POS_Y, 1);
            break;
        }

        if(tweener == null)
        {
            doneCallback?.Invoke(false);
        }
        else
        {
            tweener.onComplete= ()=>
            {
                doneCallback?.Invoke(true);
            };
        }
    }

    public void Show(Action<bool> doneCallback = null) =>
        PlayAnimation(EAnim.SHOW, doneCallback);


    public void Hide(Action<bool> doneCallback = null) =>
        PlayAnimation(EAnim.HIDE, doneCallback);
    
}
